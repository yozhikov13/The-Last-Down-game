using Aevien.UI;
using Barebones.MasterServer;
using Barebones.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Barebones.MasterServer
{
    public class RoomServerBehaviour : ServerBehaviour
    {
        #region INSPECTOR

        [Header("Room Settings"), SerializeField]
        private HelpBox roomServerInfoEditor = new HelpBox()
        {
            Text = "Room server component is responsable for connection to master server and starting room server",
            Type = HelpBoxType.Info
        };

        /// <summary>
        /// The max number of connections to room
        /// </summary>
        [SerializeField]
        protected int maxConnections = 4;

        [Header("Master Connection Settings")]
        [SerializeField]
        private string masterIp = "127.0.0.1";
        [SerializeField]
        private int masterPort = 5000;

        #endregion

        /// <summary>
        /// This socket connects room server to master as client
        /// </summary>
        private IClientSocket masterConnection;

        /// <summary>
        /// Options of this room we must share with clients
        /// </summary>
        private RoomOptions roomOptions;

        /// <summary>
        /// Controller of the room
        /// </summary>
        public RoomController CurrentRoomController { get; private set; }

        /// <summary>
        /// Fires when server room is successfully registered
        /// </summary>
        public UnityEvent OnRoomServerRegisteredEvent;

        protected override void Awake()
        {
            base.Awake();

            // If master IP is provided via cmd arguments
            if (Msf.Args.IsProvided(Msf.Args.Names.MasterIp))
            {
                masterIp = Msf.Args.MasterIp;
            }

            // If master port is provided via cmd arguments
            if (Msf.Args.IsProvided(Msf.Args.Names.MasterPort))
            {
                masterPort = Msf.Args.MasterPort;
            }
        }

        protected override void Start()
        {
            if (Msf.Runtime.IsEditor && autoStartInEditor && !Msf.Options.Has(MsfDictKeys.autoStartRoomClient))
            {
                MsfTimer.WaitForEndOfFrame(() => {
                    StartRoomServer(true);
                });
            }

            // Start room server at start
            if (Msf.Args.StartClientConnection && !Msf.Runtime.IsEditor)
            {
                MsfTimer.WaitForEndOfFrame(() => {
                    StartRoomServer();
                });
            }
        }

        protected virtual void OnApplicationQuit()
        {
            if (masterConnection != null)
                masterConnection.Disconnect();
        }

        /// <summary>
        /// Set this room options
        /// </summary>
        /// <returns></returns>
        protected virtual RoomOptions SetRoomOptions()
        {
            return new RoomOptions
            {
                // Let's make this room as private until it is successfully registered. 
                // This is useful to prevent players connection to this room before registration process finished.
                IsPublic = false,

                // This is for controlling max number of players that may be connected
                MaxConnections = Msf.Args.ExtractValueInt(Msf.Args.Names.RoomMaxConnections, maxConnections),

                // Just the name of the room
                Name = Msf.Args.ExtractValue(Msf.Args.Names.RoomName, "Room_" + Msf.Helper.CreateRandomString(5)),

                // If room uses the password
                Password = Msf.Args.ExtractValue(Msf.Args.Names.RoomPassword, string.Empty),

                // Room IP that will be used by players to connect to this room
                RoomIp = Msf.Args.ExtractValue(Msf.Args.Names.RoomIp, serverIP),

                // Room port that will be used by players to connect to this room
                RoomPort = Msf.Args.ExtractValueInt(Msf.Args.Names.RoomPort, serverPort),

                // Region that this room may use to filter it in games list
                Region = Msf.Args.ExtractValue(Msf.Args.Names.RoomRegion, string.Empty)
            };
        }

        /// <summary>
        /// Let's create new connection to master server
        /// </summary>
        /// <returns></returns>
        protected virtual IClientSocket ConnectionFactory()
        {
            return Msf.Create.ClientSocket();
        }

        /// <summary>
        /// Start connection to master
        /// </summary>
        private void ConnectToMaster()
        {
            // Start room connection to master
            if (!masterConnection.IsConnected)
            {
                masterConnection.Connect(masterIp, masterPort);
            }

            // Wait a result of client connection
            masterConnection.WaitForConnection((clientSocket) =>
            {
                if (!clientSocket.IsConnected)
                {
                    logger.Error("Failed to connect room server to master server");
                }
                else
                {
                    logger.Info("Room server is successfuly connected to master server");

                    // Start the server on next frame
                    MsfTimer.WaitForEndOfFrame(() =>
                    {
                        StartServer(roomOptions.RoomIp, roomOptions.RoomPort);
                    });
                }
            }, 4f);
        }

        /// <summary>
        /// Fires when room server is started
        /// </summary>
        protected override void OnStartedServer()
        {
            base.OnStartedServer();

            // If this room was spawned
            if (Msf.Server.Spawners.IsSpawnedProccess)
            {
                // Try to register spawned process first
                RegisterSpawnedProcess();
            }
            else
            {
                RegisterRoomServer(() =>
                {
                    logger.Info("Ok!");
                    OnRoomServerRegisteredEvent?.Invoke();
                });
            }
        }

        /// <summary>
        /// Before we register our room we need to register spawned process if required
        /// </summary>
        protected void RegisterSpawnedProcess()
        {
            // Let's register this process
            Msf.Server.Spawners.RegisterSpawnedProcess(Msf.Args.SpawnTaskId, Msf.Args.SpawnTaskUniqueCode, (taskController, error) =>
            {
                if (taskController == null)
                {
                    logger.Error($"Room server process cannot be registered. The reason is: {error}");
                    return;
                }

                // Then start registering our room server
                RegisterRoomServer(() =>
                {
                    logger.Info("Finalizing registration task");

                    // Create finalization options
                    var options = new DictionaryOptions();
                    options.Add(MsfDictKeys.roomId, CurrentRoomController.RoomId);

                    // Send finilization request
                    taskController.FinalizeTask(options, () =>
                    {
                        logger.Info("Ok!");
                        OnRoomServerRegisteredEvent?.Invoke();
                    });
                });
            });
        }

        /// <summary>
        /// Fired when this room server is disconnected from master as client
        /// </summary>
        protected virtual void OnDisconnectedFromMasterHandler()
        {
            // Remove listener after disconnection
            masterConnection.RemoveDisconnectionListener(OnDisconnectedFromMasterHandler);

            // Quit the room. Master Server will restart the room
            Msf.Runtime.Quit();
        }

        /// <summary>
        /// Start registering our room server
        /// </summary>
        protected virtual void RegisterRoomServer(UnityAction successCallback = null)
        {
            logger.Info($"Registering room to list with options: {roomOptions}");

            Msf.Server.Rooms.RegisterRoom(roomOptions, (controller, error) =>
            {
                if (controller == null)
                {
                    logger.Error(error);
                    return;
                }

                // When registration process is successfully finished we can change options of the registered room
                roomOptions.IsPublic = !Msf.Args.IsProvided(Msf.Args.Names.RoomIsPrivate);

                // And save them
                controller.SaveOptions();

                // Registered room controller
                CurrentRoomController = controller;

                logger.Info($"Room Created successfully. Room ID: {controller.RoomId}, {roomOptions}");

                successCallback?.Invoke();
            });
        }

        /// <summary>
        /// Start room server
        /// </summary>
        /// <param name="ignoreForceClientMode"></param>
        public virtual void StartRoomServer(bool ignoreForceClientMode = false)
        {
            if (Msf.Client.Rooms.ForceClientMode && !ignoreForceClientMode) return;

            // Set connection of the room server
            masterConnection = ConnectionFactory();

            // Register disconnection listener
            masterConnection.AddDisconnectionListener(OnDisconnectedFromMasterHandler, false);

            // Set this connection to services we want to use
            Msf.Server.Rooms.ChangeConnection(masterConnection);
            Msf.Server.Spawners.ChangeConnection(masterConnection);
            Msf.Server.Auth.ChangeConnection(masterConnection);
            Msf.Server.Profiles.ChangeConnection(masterConnection);

            // Set room oprions
            roomOptions = SetRoomOptions();

            logger.Info($"Starting Room Server... {Msf.Version}. Multithreading is: {(Msf.Runtime.SupportsThreads ? "On" : "Off")}");
            logger.Info($"Start parameters are: {Msf.Args}");

            // Start connecting room server to master server
            ConnectToMaster();
        }
    }
}
