using Barebones.MasterServer;
using Barebones.Networking;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Barebones.MasterServer
{
    public class RoomClientBehaviour : BaseClientBehaviour
    {
        #region INSPECTOR

        [Header("Master Connection Settings")]
        [SerializeField]
        private string masterIp = "127.0.0.1";
        [SerializeField]
        private int masterPort = 5000;

        [Header("Room Connection Settings"), SerializeField]
        private string roomServerIp = "127.0.0.1";
        [SerializeField]
        private int roomServerPort = 7777;
        [SerializeField]
        private string offlineScene = "";
        [SerializeField]
        private string onlineScene = "";

        [Header("Editor Settings"), SerializeField]
        private HelpBox roomClientInfoEditor = new HelpBox()
        {
            Text = "Editor settings are used only while running in editor",
            Type = HelpBoxType.Warning
        };

        [SerializeField]
        protected bool autoStartInEditor = true;

        [SerializeField]
        protected bool signInAsGuest = true;

        [SerializeField]
        protected string username = "qwerty";

        [SerializeField]
        protected string password = "qwerty12345";

        #endregion 

        /// <summary>
        /// This socket connects room server to master as client
        /// </summary>
        protected IClientSocket roomServerConnection;

        /// <summary>
        /// Room access that client gets from master server
        /// </summary>
        private RoomAccessPacket roomServerAccessInfo;

        /// <summary>
        /// Fires when room server has given an access to us
        /// </summary>
        public event Action OnAccessGrantedEvent;

        /// <summary>
        /// Fires when room server has rejected an access to us
        /// </summary>
        public event Action OnAccessDiniedEvent;

        protected override void Awake()
        {
            base.Awake();

            // Create room client connection
            roomServerConnection = Msf.Create.ClientSocket();

            // Listen toconnection statuses
            roomServerConnection.AddConnectionListener(OnClientConnectedToRoomServer, false);
            roomServerConnection.AddDisconnectionListener(OnClientDisconnectedFromRoomServer, false);

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

            // Set the scene that player will be sent to is disconnected from server
            if (Msf.Options.Has(MsfDictKeys.offlineSceneName))
            {
                offlineScene = Msf.Options.AsString(MsfDictKeys.offlineSceneName);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (roomServerConnection != null)
            {
                roomServerConnection.Disconnect();
            }
        }

        /// <summary>
        /// Check if room client behaviour is in test mode
        /// </summary>
        /// <returns></returns>
        public virtual bool IsInTestMode()
        {
            return Msf.Runtime.IsEditor && autoStartInEditor && !Msf.Options.Has(MsfDictKeys.autoStartRoomClient);
        }

        /// <summary>
        /// Clears connection and all its handlers if <paramref name="clearHandlers"/> is true
        /// </summary>
        public override void ClearConnection(bool clearHandlers = true)
        {
            base.ClearConnection(clearHandlers);

            // If we are in test mode we need to be disconnected
            if (IsInTestMode() && Connection != null)
            {
                Connection.Disconnect();
            }
        }

        protected override void OnInitialize()
        {
            // Listen to disconnection from master
            Connection.AddDisconnectionListener(OnDisconnectedFromMasterEvent, false);

            MsfTimer.WaitForSeconds(1f, () =>
            {
                if (IsInTestMode())
                {
                    StartRoomClient(true);
                }

                if (Msf.Options.Has(MsfDictKeys.autoStartRoomClient) || Msf.Args.StartClientConnection)
                {
                    StartRoomClient();
                }
            });
        }

        /// <summary>
        /// Start room client
        /// </summary>
        /// <param name="ignoreForceClientMode"></param>
        public void StartRoomClient(bool ignoreForceClientMode = false)
        {
            if (!Msf.Client.Rooms.ForceClientMode && !ignoreForceClientMode) return;

            logger.Info($"Starting Room Client... {Msf.Version}. Multithreading is: {(Msf.Runtime.SupportsThreads ? "On" : "Off")}");
            logger.Info($"Start parameters are: {Msf.Args}");

            // Start connecting room server to master server
            ConnectToMaster();
        }

        /// <summary>
        /// Starting connection to master server as client to be able to register room later after successful connection
        /// </summary>
        private void ConnectToMaster()
        {
            // Start client connection
            if (!Connection.IsConnected)
            {
                Connection.Connect(masterIp, masterPort);
            }

            // Wait a result of client connection
            Connection.WaitForConnection((clientSocket) =>
            {
                if (!clientSocket.IsConnected)
                {
                    logger.Error("Failed to connect room client to master server");
                }
                else
                {
                    logger.Info($"Successfully connected to {Connection.ConnectionIp}:{Connection.ConnectionPort}");

                    // For the test purpose only
                    if (IsInTestMode())
                    {
                        if (signInAsGuest)
                        {
                            // Sign in client as guest
                            Msf.Client.Auth.SignInAsGuest(SignInCallback);
                        }
                        else
                        {
                            // Sign in client using credentials
                            Msf.Client.Auth.SignIn(username, password, SignInCallback);
                        }
                    }
                    else
                    {
                        // If we have option with room id
                        // this approach can be used when you have come to this scene from another one.
                        // Set this option before this room client controller is connected to master server
                        if (Msf.Options.Has(MsfDictKeys.roomId))
                        {
                            // Let's try to get access data for room we want to connect to
                            GetRoomAccess(Msf.Options.AsInt(MsfDictKeys.roomId));
                        }
                        else
                        {
                            logger.Error($"You have no room id in this options: {Msf.Options}");
                        }
                    }
                }
            }, 4f);
        }

        /// <summary>
        /// Test sign in callback
        /// </summary>
        /// <param name="accountInfo"></param>
        /// <param name="error"></param>
        private void SignInCallback(AccountInfoPacket accountInfo, string error)
        {
            if (accountInfo == null)
            {
                logger.Error(error);
                return;
            }

            logger.Debug($"Signed in successfully as {accountInfo.Username}");
            logger.Debug("Finding games...");

            Msf.Client.Matchmaker.FindGames((games) =>
            {
                if (games.Count == 0)
                {
                    logger.Error("No test game found");
                    return;
                }

                logger.Debug($"Found {games.Count} games");

                // Get first game fromlist
                GameInfoPacket firstGame = games.First();

                // Let's try to get access data for room we want to connect to
                GetRoomAccess(firstGame.Id);
            });
        }

        /// <summary>
        /// Tries to get access data for room we want to connect to
        /// </summary>
        /// <param name="roomId"></param>
        private void GetRoomAccess(int roomId)
        {
            logger.Debug($"Getting access to room {roomId}");

            Msf.Client.Rooms.GetAccess(roomId, (access, error) =>
            {
                if (access == null)
                {
                    logger.Error(error);
                    OnAccessDiniedEvent?.Invoke();
                    return;
                }

                // Save gotten room access
                roomServerAccessInfo = access;

                // Let's set the IP before we start connection
                roomServerIp = roomServerAccessInfo.RoomIp;

                // Let's set the port before we start connection
                roomServerPort = roomServerAccessInfo.RoomPort;

                logger.Debug($"Access to room {roomId} received");
                logger.Debug(access);
                logger.Debug("Connecting to room server...");

                // Start client connection
                roomServerConnection.Connect(roomServerIp, roomServerPort);

                // Wait a result of client connection
                roomServerConnection.WaitForConnection((clientSocket) =>
                {
                    if (!clientSocket.IsConnected)
                    {
                        logger.Error("Connection attempts to room server timed out");
                        return;
                    }
                }, 4f);
            });
        }

        /// <summary>
        /// Fires when client connected to room server
        /// </summary>
        protected virtual void OnClientConnectedToRoomServer()
        {
            logger.Info("We have successfully connected to the room server");

            roomServerConnection.RemoveConnectionListener(OnClientConnectedToRoomServer);
            roomServerConnection.SendMessage((short)MsfMessageCodes.ValidateRoomAccessRequest, roomServerAccessInfo.Token, (status, response) =>
            {
                // If access denied
                if (status != ResponseStatus.Success)
                {
                    logger.Error(response.AsString());
                    OnAccessDiniedEvent?.Invoke();
                    return;
                }

                // If access granted
                OnAccessGrantedEvent?.Invoke();
            });
        }

        /// <summary>
        /// Fires when client disconnected from room server
        /// </summary>
        protected virtual void OnClientDisconnectedFromRoomServer()
        {
            roomServerConnection.RemoveDisconnectionListener(OnClientDisconnectedFromRoomServer);
            logger.Error("We have lost the connection to room server");

            if (!string.IsNullOrEmpty(offlineScene))
            {
                SceneManager.LoadScene(offlineScene);
            }
        }


        protected virtual void OnDisconnectedFromMasterEvent()
        {
            if (Connection != null)
                Connection.RemoveDisconnectionListener(OnDisconnectedFromMasterEvent);

            if (roomServerConnection != null)
                roomServerConnection.Disconnect();
        }
    }
}