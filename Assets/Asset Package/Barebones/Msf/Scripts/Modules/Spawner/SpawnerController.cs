﻿using Barebones.Logging;
using Barebones.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Barebones.MasterServer
{
    public class SpawnerController : ISpawnerController
    {
        /// <summary>
        /// Just <see cref="Process"/> lock
        /// </summary>
        protected static object processLock = new object();

        /// <summary>
        /// List of spawned processes
        /// </summary>
        protected Dictionary<int, Process> processes = new Dictionary<int, Process>();

        /// <summary>
        /// Current connection
        /// </summary>
        public IClientSocket Connection { get; protected set; }

        /// <summary>
        /// Id of this spawner controller that master server gives
        /// </summary>
        public int SpawnerId { get; protected set; }

        /// <summary>
        /// Settings, which are used by the default spawn handler
        /// </summary>
        public SpawnerConfig SpawnSettings { get; protected set; }

        public Logger Logger { get; protected set; }

        /// <summary>
        /// Fired when process is started
        /// </summary>
        public event Action OnProcessStartedEvent;

        /// <summary>
        /// Fired when process is killed
        /// </summary>
        public event Action OnProcessKilledEvent;

        /// <summary>
        /// Create new instance of spawner controller
        /// </summary>
        /// <param name="spawnerId"></param>
        /// <param name="connection"></param>
        /// <param name="options"></param>
        public SpawnerController(int spawnerId, IClientSocket connection, SpawnerOptions spawnerOptions)
        {
            Logger = Msf.Create.Logger(typeof(SpawnerController).Name, LogLevel.Info);

            Connection = connection;
            SpawnerId = spawnerId;

            SpawnSettings = new SpawnerConfig()
            {
                MasterIp = connection.ConnectionIp,
                MasterPort = connection.ConnectionPort,
                SpawnInBatchmode = false,
                MachineIp = spawnerOptions.MachineIp,
                Region = string.IsNullOrEmpty(spawnerOptions.Region) ? "International" : spawnerOptions.Region
            };

            // Add static handlers to listen one message for all controllers
            connection.SetHandler((short)MsfMessageCodes.SpawnProcessRequest, SpawnProcessRequestHandler);
            connection.SetHandler((short)MsfMessageCodes.KillProcessRequest, KillProcessRequestHandler);
        }

        /// <summary>
        /// Handles spawn request for all controllers filtered by ID
        /// </summary>
        /// <param name="message"></param>
        private static void SpawnProcessRequestHandler(IIncommingMessage message)
        {
            var data = message.Deserialize(new SpawnRequestPacket());
            var controller = Msf.Server.Spawners.GetController(data.SpawnerId) as SpawnerController;

            if (controller == null)
            {
                if (message.IsExpectingResponse)
                {
                    message.Respond("Couldn't find a spawn controller", ResponseStatus.NotHandled);
                }

                return;
            }

            controller.Logger.Debug($"Spawn process requested for spawn controller [{controller.SpawnerId}]");
            controller.SpawnRequestHandler(data, message);
        }

        /// <summary>
        /// Handles kill request for all controllers filtered by ID
        /// </summary>
        /// <param name="message"></param>
        private static void KillProcessRequestHandler(IIncommingMessage message)
        {
            var data = message.Deserialize(new KillSpawnedProcessRequestPacket());
            var controller = Msf.Server.Spawners.GetController(data.SpawnerId) as SpawnerController;

            if (controller == null)
            {
                if (message.IsExpectingResponse)
                {
                    message.Respond("Couldn't find a spawn controller", ResponseStatus.NotHandled);
                }

                return;
            }

            controller.Logger.Debug($"Kill process requested for spawn controller [{controller.SpawnerId}]");
            controller.KillRequestHandler(data.SpawnId);
            message.Respond(ResponseStatus.Success);
        }

        /// <summary>
        /// Notifies all listeners that process is started
        /// </summary>
        /// <param name="spawnId"></param>
        /// <param name="processId"></param>
        /// <param name="cmdArgs"></param>
        public void NotifyProcessStarted(int spawnId, int processId, string cmdArgs)
        {
            Msf.Server.Spawners.NotifyProcessStarted(spawnId, processId, cmdArgs, Connection);
            OnProcessStartedEvent?.Invoke();
        }

        /// <summary>
        /// Notifies all listeners that process is killed
        /// </summary>
        /// <param name="spawnId"></param>
        public void NotifyProcessKilled(int spawnId)
        {
            Msf.Server.Spawners.NotifyProcessKilled(spawnId);
            OnProcessKilledEvent?.Invoke();
        }

        /// <summary>
        /// Notifies master server, how many processes are running on a specified spawner
        /// </summary>
        /// <param name="count"></param>
        public void UpdateProcessesCount(int count)
        {
            Msf.Server.Spawners.UpdateProcessesCount(SpawnerId, count, Connection);
        }

        /// <summary>
        /// Default spawn spawned process request handler that will be used by controller if <see cref="spawnRequestHandler"/> is not overriden
        /// </summary>
        /// <param name="data"></param>
        /// <param name="message"></param>
        public virtual void SpawnRequestHandler(SpawnRequestPacket data, IIncommingMessage message)
        {
            Logger.Debug($"Default spawn handler started handling a request to spawn process for spawn controller [{SpawnerId}]");

            /************************************************************************/
            // Create process args string
            var processArguments = new DictionaryOptions();

            /************************************************************************/
            // Check if we're overriding an IP to master server
            var masterIpArgument = string.IsNullOrEmpty(SpawnSettings.MasterIp) ?
                Connection.ConnectionIp : SpawnSettings.MasterIp;

            // Create master IP arg
            processArguments.Set(Msf.Args.Names.MasterIp, masterIpArgument);

            /************************************************************************/
            /// Check if we're overriding a port to master server
            var masterPortArgument = SpawnSettings.MasterPort < 0 ?
                Connection.ConnectionPort : SpawnSettings.MasterPort;

            // Create master port arg
            processArguments.Set(Msf.Args.Names.MasterPort, masterPortArgument);

            /************************************************************************/
            // Room Name
            processArguments.Set(Msf.Args.Names.RoomName, $"\"{data.Options.AsString(MsfDictKeys.roomName, "Room_" + Msf.Helper.CreateRandomString(6))}\"");

            /************************************************************************/
            // Room Region
            processArguments.Set(Msf.Args.Names.RoomRegion, $"\"{SpawnSettings.Region}\"");

            /************************************************************************/
            // Room Max Connections
            if (data.Options.Has(MsfDictKeys.maxPlayers))
            {
                processArguments.Set(Msf.Args.Names.RoomMaxConnections, data.Options.AsString(MsfDictKeys.maxPlayers));
            }

            /************************************************************************/
            // Machine Ip
            processArguments.Set(Msf.Args.Names.RoomIp, SpawnSettings.MachineIp);

            /************************************************************************/
            // Create port for room arg
            int machinePortArgument = Msf.Server.Spawners.GetAvailablePort();
            processArguments.Set(Msf.Args.Names.RoomPort, machinePortArgument);

            /************************************************************************/
            // Get the scene name
            if (data.Options.Has(MsfDictKeys.sceneName))
            {
                processArguments.Set(Msf.Args.Names.LoadScene, data.Options.AsString(MsfDictKeys.sceneName));
            }

            /************************************************************************/
            // If spawn in batchmode was set and DontSpawnInBatchmode arg is not provided
            if (SpawnSettings.SpawnInBatchmode && !Msf.Args.DontSpawnInBatchmode)
            {
                processArguments.Set("-batchmode -nographics", string.Empty);
            }

            /************************************************************************/
            // Create use websockets arg
            if (SpawnSettings.UseWebSockets)
            {
                processArguments.Set(Msf.Args.Names.UseWebSockets, string.Empty);
            }

            /************************************************************************/
            // Create spawn id arg
            processArguments.Set(Msf.Args.Names.SpawnTaskId, data.SpawnTaskId);

            /************************************************************************/
            // Create spawn code arg
            processArguments.Set(Msf.Args.Names.SpawnTaskUniqueCode, data.SpawnTaskUniqueCode);

            /************************************************************************/
            // Create destroy ui arg
            if (Msf.Args.DestroyUi)
            {
                processArguments.Set(Msf.Args.Names.DestroyUi, string.Empty);
            }

            /************************************************************************/
            // Create custom args
            processArguments.Append(data.CustomOptions);

            /************************************************************************/
            // Path to executable
            var executablePath = SpawnSettings.ExecutablePath;

            if (string.IsNullOrEmpty(executablePath))
            {
                executablePath = File.Exists(Environment.GetCommandLineArgs()[0])
                    ? Environment.GetCommandLineArgs()[0]
                    : Process.GetCurrentProcess().MainModule.FileName;
            }

            // In case a path is provided with the request
            if (data.Options.Has(MsfDictKeys.executablePath))
            {
                executablePath = data.Options.AsString(MsfDictKeys.executablePath);
            }

            if (!string.IsNullOrEmpty(data.OverrideExePath))
            {
                executablePath = data.OverrideExePath;
            }

            /// Create info about starting process
            var startProcessInfo = new ProcessStartInfo(executablePath)
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                Arguments = processArguments.ToReadableString(" ", " ")
            };

            Logger.Debug("Starting process with args: " + startProcessInfo.Arguments);

            var processStarted = false;

            try
            {
                new Thread(() =>
                {
                    try
                    {
                        using (var process = Process.Start(startProcessInfo))
                        {
                            Logger.Debug("Process started. Spawn Id: " + data.SpawnTaskId + ", pid: " + process.Id);
                            processStarted = true;

                            lock (processLock)
                            {
                                // Save the process
                                processes[data.SpawnTaskId] = process;
                            }

                            var processId = process.Id;

                            // Notify server that we've successfully handled the request
                            MsfTimer.RunInMainThread(() =>
                            {
                                message.Respond(ResponseStatus.Success);
                                NotifyProcessStarted(data.SpawnTaskId, processId, startProcessInfo.Arguments);
                            });

                            process.WaitForExit();
                        }
                    }
                    catch (Exception e)
                    {
                        if (!processStarted)
                        {
                            MsfTimer.RunInMainThread(() => { message.Respond(ResponseStatus.Failed); });
                        }

                        Logger.Error("An exception was thrown while starting a process. Make sure that you have set a correct build path. " +
                                     $"We've tried to start a process at [{executablePath}]. You can change it at 'SpawnerBehaviour' component");
                        Logger.Error(e);
                    }
                    finally
                    {
                        lock (processLock)
                        {
                            // Remove the process
                            processes.Remove(data.SpawnTaskId);
                        }

                        MsfTimer.RunInMainThread(() =>
                        {
                            // Release the port number
                            Msf.Server.Spawners.ReleasePort(machinePortArgument);
                            Logger.Debug($"Notifying about killed process with spawn id [{data.SpawnTaskId}]");
                            NotifyProcessKilled(data.SpawnTaskId);
                        });
                    }

                }).Start();
            }
            catch (Exception e)
            {
                message.Respond(e.Message, ResponseStatus.Error);
                Logs.Error(e);
            }
        }

        /// <summary>
        /// Default kill spawned process request handler that will be used by controller if <see cref="killRequestHandler"/> is not overriden
        /// </summary>
        /// <param name="spawnId"></param>
        public virtual void KillRequestHandler(int spawnId)
        {
            Logger.Debug($"Default kill request handler started handling a request to kill a process with id [{spawnId}] for spawn controller with id [{SpawnerId}]");

            try
            {
                Process process;

                lock (processLock)
                {
                    processes.TryGetValue(spawnId, out process);
                    processes.Remove(spawnId);
                }

                if (process != null)
                {
                    process.Kill();
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Got error while killing a spawned process with id [{spawnId}]");
                Logger.Error(e);
            }
        }

        /// <summary>
        /// Kill all processes running in this controller
        /// </summary>
        public void KillProcesses()
        {
            var list = new List<Process>();

            lock (processLock)
            {
                foreach (var process in processes.Values)
                {
                    list.Add(process);
                }
            }

            foreach (var process in list)
            {
                process.Kill();
            }
        }

        /// <summary>
        /// Get the number of processes
        /// </summary>
        /// <returns></returns>
        public int ProcessesCount()
        {
            return processes != null ? processes.Count : 0;
        }
    }
}