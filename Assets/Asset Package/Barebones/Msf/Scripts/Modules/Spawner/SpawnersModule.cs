﻿using Barebones.Logging;
using Barebones.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Barebones.MasterServer
{
    public class SpawnersModule : BaseServerModule
    {
        public delegate void SpawnedProcessRegistrationHandler(SpawnTask task, IPeer peer);

        private int _spawnerId = 0;
        private int _spawnTaskId = 0;

        protected Dictionary<int, RegisteredSpawner> spawnersList;
        protected Dictionary<int, SpawnTask> spawnTasksList;

        [Header("Permissions"), SerializeField, Tooltip("Minimal permission level, necessary to register a spanwer")]
        protected int createSpawnerPermissionLevel = 0;

        [Tooltip("How often spawner queues are updated"), SerializeField]
        protected float queueUpdateFrequency = 0.1f;

        [Tooltip("If true, clients will be able to request spawns"), SerializeField]
        protected bool enableClientSpawnRequests = true;

        public event Action<RegisteredSpawner> OnSpawnerRegisteredEvent;
        public event Action<RegisteredSpawner> OnSpawnerDestroyedEvent;
        public event SpawnedProcessRegistrationHandler OnSpawnedProcessRegisteredEvent;

        public override void Initialize(IServer server)
        {
            spawnersList = new Dictionary<int, RegisteredSpawner>();
            spawnTasksList = new Dictionary<int, SpawnTask>();

            // Add handlers
            server.SetHandler((short)MsfMessageCodes.RegisterSpawner, RegisterSpawnerRequestHandler);
            server.SetHandler((short)MsfMessageCodes.ClientsSpawnRequest, ClientsSpawnRequestHandler);
            server.SetHandler((short)MsfMessageCodes.RegisterSpawnedProcess, RegisterSpawnedProcessRequestHandler);
            server.SetHandler((short)MsfMessageCodes.CompleteSpawnProcess, CompleteSpawnProcessRequestHandler);
            server.SetHandler((short)MsfMessageCodes.ProcessStarted, SetProcessStartedRequestHandler);
            server.SetHandler((short)MsfMessageCodes.ProcessKilled, SetProcessKilledRequestHandler);
            server.SetHandler((short)MsfMessageCodes.AbortSpawnRequest, AbortSpawnRequestHandler);
            server.SetHandler((short)MsfMessageCodes.GetSpawnFinalizationData, GetCompletionDataRequestHandler);
            server.SetHandler((short)MsfMessageCodes.UpdateSpawnerProcessesCount, SetSpawnedProcessesCountRequestHandler);

            // Coroutines
            StartCoroutine(StartQueueUpdater());
        }

        public virtual RegisteredSpawner CreateSpawner(IPeer peer, SpawnerOptions options)
        {
            var spawner = new RegisteredSpawner(GenerateSpawnerId(), peer, options);

            Dictionary<int, RegisteredSpawner> peerSpawners = peer.GetProperty((int)MsfPeerPropertyCodes.RegisteredSpawners) as Dictionary<int, RegisteredSpawner>;

            // If this is the first time registering a spawners
            if (peerSpawners == null)
            {
                // Save the dictionary
                peerSpawners = new Dictionary<int, RegisteredSpawner>();
                peer.SetProperty((int)MsfPeerPropertyCodes.RegisteredSpawners, peerSpawners);

                peer.OnPeerDisconnectedEvent += OnRegisteredPeerDisconnect;
            }

            // Add a new spawner
            peerSpawners[spawner.SpawnerId] = spawner;

            // Add the spawner to a list of all spawners
            spawnersList[spawner.SpawnerId] = spawner;

            // Invoke the event
            if (OnSpawnerRegisteredEvent != null)
            {
                OnSpawnerRegisteredEvent.Invoke(spawner);
            }

            return spawner;
        }

        private void OnRegisteredPeerDisconnect(IPeer peer)
        {
            var peerSpawners = peer.GetProperty((int)MsfPeerPropertyCodes.RegisteredSpawners) as Dictionary<int, RegisteredSpawner>;

            if (peerSpawners == null)
            {
                return;
            }

            // Create a copy so that we can iterate safely
            var registeredSpawners = peerSpawners.Values.ToList();

            foreach (var registeredSpawner in registeredSpawners)
            {
                DestroySpawner(registeredSpawner);
            }
        }

        public void DestroySpawner(RegisteredSpawner spawner)
        {
            var peer = spawner.Peer;

            if (peer != null)
            {
                var peerRooms = peer.GetProperty((int)MsfPeerPropertyCodes.RegisteredSpawners) as Dictionary<int, RegisteredSpawner>;

                // Remove the spawner from peer
                if (peerRooms != null)
                {
                    peerRooms.Remove(spawner.SpawnerId);
                }
            }

            // Remove the spawner from all spawners
            spawnersList.Remove(spawner.SpawnerId);

            // Invoke the event
            if (OnSpawnerDestroyedEvent != null)
            {
                OnSpawnerDestroyedEvent.Invoke(spawner);
            }
        }

        public int GenerateSpawnerId()
        {
            return _spawnerId++;
        }

        public int GenerateSpawnTaskId()
        {
            return _spawnTaskId++;
        }

        /// <summary>
        /// Start process on spawner side with given spawn options
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public SpawnTask Spawn(DictionaryOptions options)
        {
            return Spawn(options, string.Empty, new DictionaryOptions());
        }

        /// <summary>
        /// Start process on spawner side with given spawn <paramref name="options"/> and <paramref name="region"/>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        public SpawnTask Spawn(DictionaryOptions options, string region)
        {
            return Spawn(options, region, new DictionaryOptions());
        }

        /// <summary>
        /// Start process on spawner side with given spawn <paramref name="options"/>, <paramref name="region"/> and <paramref name="customOptions"/>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="region"></param>
        /// <param name="customOptions"></param>
        /// <returns></returns>
        public virtual SpawnTask Spawn(DictionaryOptions options, string region, DictionaryOptions customOptions)
        {
            // Get registered spawner by options and region
            var spawners = GetFilteredSpawners(options, region);

            if (spawners.Count == 0)
            {
                logger.Warn($"No spawner was returned after filtering. Region: {options.AsString(MsfDictKeys.region, "International")}");
                return null;
            }

            // Order from least busy server
            var orderedSpawners = spawners.OrderByDescending(s => s.CalculateFreeSlotsCount());
            var availableSpawner = orderedSpawners.FirstOrDefault(s => s.CanSpawnAnotherProcess());

            // Ignore, if all of the spawners are busy
            if (availableSpawner == null)
            {
                return null;
            }

            return Spawn(options, customOptions, availableSpawner);
        }

        /// <summary>
        /// Start process on spawner side with given spawn <paramref name="options"/>, <paramref name="customOptions"/> and <paramref name="spawner"/>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="customOptions"></param>
        /// <param name="spawner"></param>
        /// <returns></returns>
        public virtual SpawnTask Spawn(DictionaryOptions options, DictionaryOptions customOptions, RegisteredSpawner spawner)
        {
            // Create new spawn task
            var task = new SpawnTask(GenerateSpawnTaskId(), spawner, options, customOptions);

            // List this task
            spawnTasksList[task.Id] = task;

            // Add this task to queue
            spawner.AddTaskToQueue(task);

            logger.Debug("Spawner was found, and spawn task created: " + task);

            return task;
        }

        /// <summary>
        /// Retrieves a list of spawner that can be used with given properties and region name
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        public virtual List<RegisteredSpawner> GetFilteredSpawners(DictionaryOptions properties, string region)
        {
            return GetSpawners(region);
        }

        public virtual List<RegisteredSpawner> GetSpawners()
        {
            return GetSpawners(null);
        }

        public virtual List<RegisteredSpawner> GetSpawners(string region)
        {
            // If region is not provided, retrieve all spawners
            if (string.IsNullOrEmpty(region))
            {
                return spawnersList.Values.ToList();
            }

            return GetSpawnersInRegion(region);
        }

        public virtual List<RegisteredSpawner> GetSpawnersInRegion(string region)
        {
            return spawnersList.Values
                .Where(s => s.Options.Region == region)
                .ToList();
        }

        /// <summary>
        /// Returns true, if peer has permissions to register a spawner
        /// </summary>
        /// <param name="peer"></param>
        /// <returns></returns>
        protected virtual bool HasCreationPermissions(IPeer peer)
        {
            var extension = peer.GetExtension<SecurityInfoPeerExtension>();

            return extension.PermissionLevel >= createSpawnerPermissionLevel;
        }

        protected virtual bool CanClientSpawn(IPeer peer, ClientsSpawnRequestPacket data)
        {
            return enableClientSpawnRequests;
        }

        protected virtual IEnumerator StartQueueUpdater()
        {
            while (true)
            {
                yield return new WaitForSeconds(queueUpdateFrequency);

                foreach (var spawner in spawnersList.Values)
                {
                    try
                    {
                        spawner.UpdateQueue();
                    }
                    catch (Exception e)
                    {
                        logger.Error(e);
                    }
                }
            }
        }

        #region Message Handlers

        /// <summary>
        /// Fired whe connected client has made request to spawn process
        /// </summary>
        /// <param name="message"></param>
        protected virtual void ClientsSpawnRequestHandler(IIncommingMessage message)
        {
            // Parse data from message
            var spawnRequestData = message.Deserialize(new ClientsSpawnRequestPacket());
            var peer = message.Peer;

            logger.Info($"Client {peer.Id} requested to spawn room with options: {spawnRequestData}");

            if (spawnersList.Count == 0)
            {
                logger.Error("But no registered spawner was found!");
                message.Respond("No registered spawner was found", ResponseStatus.Failed);
                return;
            }

            // Check if current request is authorized
            if (!CanClientSpawn(peer, spawnRequestData))
            {
                logger.Error("Unauthorized request");
                // Client can't spawn
                message.Respond("Unauthorized", ResponseStatus.Unauthorized);
                return;
            }

            // Try to find existing request to prevent new one
            SpawnTask prevRequest = peer.GetProperty((int)MsfPeerPropertyCodes.ClientSpawnRequest) as SpawnTask;

            if (prevRequest != null && !prevRequest.IsDoneStartingProcess)
            {
                logger.Warn("And he already has an active request");
                // Client has unfinished request
                message.Respond("You already have an active request", ResponseStatus.Failed);
                return;
            }

            // Create a new spawn task
            var task = Spawn(spawnRequestData.Options, spawnRequestData.Options.AsString(MsfDictKeys.region), spawnRequestData.CustomOptions);

            // If spawn task is not created
            if (task == null)
            {
                logger.Warn("But all the servers are busy. Let him try again later");
                message.Respond("All the servers are busy. Try again later".ToBytes(), ResponseStatus.Failed);
                return;
            }

            // Save spawn task requester
            task.Requester = message.Peer;

            // Save the task as peer property
            peer.SetProperty((int)MsfPeerPropertyCodes.ClientSpawnRequest, task);

            // Listen to status changes
            task.OnStatusChangedEvent += (status) =>
            {
                // Send status update
                var msg = Msf.Create.Message((short)MsfMessageCodes.SpawnRequestStatusChange, new SpawnStatusUpdatePacket()
                {
                    SpawnId = task.Id,
                    Status = status
                });

                if (task.Requester != null && task.Requester.IsConnected)
                {
                    message.Peer.SendMessage(msg);
                }
            };

            message.Respond(task.Id, ResponseStatus.Success);
        }

        private void AbortSpawnRequestHandler(IIncommingMessage message)
        {
            var prevRequest = message.Peer.GetProperty((int)MsfPeerPropertyCodes.ClientSpawnRequest) as SpawnTask;

            if (prevRequest == null)
            {
                message.Respond("There's nothing to abort", ResponseStatus.Failed);
                return;
            }

            if (prevRequest.Status >= SpawnStatus.Finalized)
            {
                message.Respond("You can't abort a completed request", ResponseStatus.Failed);
                return;
            }

            if (prevRequest.Status <= SpawnStatus.None)
            {
                message.Respond("Already aborting", ResponseStatus.Success);
                return;
            }

            logger.Debug($"Client [{message.Peer.Id}] requested to terminate process [{prevRequest.Id}]");

            prevRequest.Abort();

            message.Respond(ResponseStatus.Success);
        }

        protected virtual void GetCompletionDataRequestHandler(IIncommingMessage message)
        {
            var spawnId = message.AsInt();

            if (!spawnTasksList.TryGetValue(spawnId, out SpawnTask task))
            {
                message.Respond("Invalid request", ResponseStatus.Failed);
                return;
            }

            if (task.Requester != message.Peer)
            {
                message.Respond("You're not the requester", ResponseStatus.Unauthorized);
                return;
            }

            if (task.FinalizationPacket == null)
            {
                message.Respond("Task has no completion data", ResponseStatus.Failed);
                return;
            }

            // Respond with data (dictionary of strings)
            message.Respond(task.FinalizationPacket.FinalizationData.ToBytes(), ResponseStatus.Success);
        }

        protected virtual void RegisterSpawnerRequestHandler(IIncommingMessage message)
        {
            logger.Debug($"Client [{message.Peer.Id}] requested to be registered as spawner");

            if (!HasCreationPermissions(message.Peer))
            {
                message.Respond("Insufficient permissions", ResponseStatus.Unauthorized);
                return;
            }

            var options = message.Deserialize(new SpawnerOptions());
            var spawner = CreateSpawner(message.Peer, options);

            logger.Debug($"Client [{message.Peer.Id}] was successfully registered as spawner [{spawner.SpawnerId}] with options: {options}");

            // Respond with spawner id
            message.Respond(spawner.SpawnerId, ResponseStatus.Success);
        }

        /// <summary>
        /// Handles a message from spawned process. Spawned process send this message
        /// to notify server that it was started
        /// </summary>
        /// <param name="message"></param>
        protected virtual void RegisterSpawnedProcessRequestHandler(IIncommingMessage message)
        {
            var data = message.Deserialize(new RegisterSpawnedProcessPacket());

            // Try get psawn task by ID
            if (!spawnTasksList.TryGetValue(data.SpawnId, out SpawnTask task))
            {
                message.Respond("Invalid spawn task", ResponseStatus.Failed);
                logger.Error("Process tried to register to an unknown task");
                return;
            }

            // Check spawn task unique code
            if (task.UniqueCode != data.SpawnCode)
            {
                message.Respond("Unauthorized", ResponseStatus.Unauthorized);
                logger.Error("Spawned process tried to register, but failed due to mismaching unique code");
                return;
            }

            // Set task as registered
            task.OnRegistered(message.Peer);

            // Invoke event
            OnSpawnedProcessRegisteredEvent?.Invoke(task, message.Peer);

            // Respon to requester
            message.Respond(task.Options.ToDictionary().ToBytes(), ResponseStatus.Success);
        }

        protected virtual void CompleteSpawnProcessRequestHandler(IIncommingMessage message)
        {
            var data = message.Deserialize(new SpawnFinalizationPacket());

            if (spawnTasksList.TryGetValue(data.SpawnTaskId, out SpawnTask task))
            {
                if (task.RegisteredPeer != message.Peer)
                {
                    message.Respond("Unauthorized", ResponseStatus.Unauthorized);
                    logger.Error("Spawned process tried to complete spawn task, but it's not the same peer who registered to the task");
                }
                else
                {
                    task.OnFinalized(data);
                    message.Respond(ResponseStatus.Success);
                }
            }
            else
            {
                message.Respond("Invalid spawn task", ResponseStatus.Failed);
                logger.Error("Process tried to complete to an unknown task");
            }
        }

        protected virtual void SetProcessKilledRequestHandler(IIncommingMessage message)
        {
            var spawnId = message.AsInt();

            if (spawnTasksList.TryGetValue(spawnId, out SpawnTask task))
            {
                task.OnProcessKilled();
                task.Spawner.OnProcessKilled();
            }
        }

        protected virtual void SetProcessStartedRequestHandler(IIncommingMessage message)
        {
            var spawnId = message.AsInt();

            if (spawnTasksList.TryGetValue(spawnId, out SpawnTask task))
            {
                task.OnProcessStarted();
                task.Spawner.OnProcessStarted();
            }
        }

        private void SetSpawnedProcessesCountRequestHandler(IIncommingMessage message)
        {
            var packet = message.Deserialize(new IntPairPacket());

            if (spawnersList.TryGetValue(packet.A, out RegisteredSpawner spawner))
            {
                spawner.UpdateProcessesCount(packet.B);
            }
        }

        #endregion
    }
}