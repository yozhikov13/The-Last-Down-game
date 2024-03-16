using Barebones.Games;
using Barebones.Logging;
using Barebones.MasterServer;
using Barebones.Networking;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Barebones.Bridges.Mirror
{
    public class MirrorRoomClient : BaseClientBehaviour
    {
        #region INSPECTOR

        /// <summary>
        /// Master server IP address to connect room server to master server as client
        /// </summary>
        [Header("Master Connection Settings"), SerializeField, Tooltip("Master server IP address to connect room server to master server as client")]
        protected string masterIp = "127.0.0.1";

        /// <summary>
        /// Master server port to connect room server to master server as client
        /// </summary>
        [SerializeField, Tooltip("Master server port to connect room server to master server as client")]
        protected int masterPort = 5000;

        /// <summary>
        /// Time of waiting the connection to mirror server
        /// </summary>
        [SerializeField, Tooltip("Time of waiting the connection to mirror server")]
        protected int roomConnectionTimeout = 10;

        [Header("Editor Settings"), SerializeField]
        private HelpBox editorHelp = new HelpBox()
        {
            Text = "This settings works only in editor. They are for test purpose only",
            Type = HelpBoxType.Info
        };

        /// <summary>
        /// This will start client in editor automatically
        /// </summary>
        [SerializeField, Tooltip("This will start client in editor automatically")]
        protected bool autoStartInEditor = true;

        /// <summary>
        /// If true system will try to sign in as guest in test mode
        /// </summary>
        [SerializeField, Tooltip("If true system will try to sign in as guest in test mode")]
        protected bool signInAsGuest = true;

        /// <summary>
        /// If <see cref="signInAsGuest"/> is not true system will try sign in as registereg user in test mode using this username
        /// </summary>
        [SerializeField, Tooltip("If signInAsGuest is not true system will try sign in as registereg user in test mode using this username")]
        protected string username = "qwerty";

        /// <summary>
        /// If <see cref="signInAsGuest"/> is not true system will try sign in as registereg user in test mode using this password
        /// </summary>
        [SerializeField, Tooltip("If signInAsGuest is not true system will try sign in as registereg user in test mode using this password")]
        protected string password = "qwerty12345";

        #endregion

        /// <summary>
        /// Room access data received when getting access to room
        /// </summary>
        protected RoomAccessPacket roomAccess;

        /// <summary>
        /// The instance of the <see cref="MirrorRoomServer"/>
        /// </summary>
        public static MirrorRoomClient Instance { get; protected set; }

        /// <summary>
        /// Mirror network manager
        /// </summary>
        public NetworkManager MirrorNetworkManager { get; set; }

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
            // Only one room server can exist in scene
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            // Create simple singleton
            Instance = this;
            DontDestroyOnLoad(gameObject);

            base.Awake();

            // If master IP is provided via cmd arguments
            masterIp = Msf.Args.ExtractValue(Msf.Args.Names.MasterIp, masterIp);

            // If master port is provided via cmd arguments
            masterPort = Msf.Args.ExtractValueInt(Msf.Args.Names.MasterPort, masterPort);
        }

        protected virtual void OnValidate()
        {
            roomConnectionTimeout = Mathf.Clamp(roomConnectionTimeout, 4, 60);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            NetworkClient.UnregisterHandler<ValidateRoomAccessResultMessage>();

            // Stop listening to OnServerStartedEvent of our MirrorNetworkManager
            if (NetworkManager.singleton is MirrorNetworkManager manager)
            {
                manager.OnClientStartedEvent -= OnMirrorClientStartedEventHandler;
            }

            // Remove master server connection and disconnection listener
            Connection?.RemoveDisconnectionListener(OnDisconnectedFromMasterServerEventHandler);
        }

        protected override void OnInitialize()
        {
            // Get mirror network manager
            MirrorNetworkManager = NetworkManager.singleton;

            // If we hav offline scene in global options
            if (Msf.Options.Has(MsfDictKeys.offlineSceneName))
            {
                MirrorNetworkManager.offlineScene = Msf.Options.AsString(MsfDictKeys.offlineSceneName);
            }

            // Start listening to OnServerStartedEvent of our MirrorNetworkManager
            if (NetworkManager.singleton is MirrorNetworkManager manager)
            {
                manager.OnClientStartedEvent += OnMirrorClientStartedEventHandler;
            }
            else
            {
                logger.Error("Before using MirrorNetworkManager add it to scene");
            }

            // Add master server connection and disconnection listeners
            Connection.AddDisconnectionListener(OnDisconnectedFromMasterServerEventHandler, false);

            // If connection to master server is not established
            if (!Connection.IsConnected && !Connection.IsConnecting)
            {
                Connection.Connect(masterIp, masterPort);
            }
        }

        /// <summary>
        /// Fired when this room client is disconnected from master as client.
        /// </summary>
        protected virtual void OnDisconnectedFromMasterServerEventHandler()
        {
            logger.Debug("Room client was disconnected to master server");

            NetworkClient.UnregisterHandler<ValidateRoomAccessResultMessage>();

            // Stop mirror client
            MirrorNetworkManager.StopClient();
        }

        #region TEST MODE

        /// <summary>
        /// Check is this module is allowed to be started in editor. This feature is for testing purpose only
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsAllowedToBeStartedInEditor()
        {
            return !Msf.Client.Rooms.ForceClientMode
                && Msf.Runtime.IsEditor
                   && autoStartInEditor
                   && Msf.Client.Rooms.LastReceivedAccess == null;
        }

        /// <summary>
        /// Fired when mirror client is started.
        /// This is usefull in test mode.
        /// </summary>
        protected virtual void OnMirrorClientStartedEventHandler()
        {
            // If we are in test mode
            if (IsAllowedToBeStartedInEditor())
            {
                StartClient();
            }
        }

        /// <summary>
        /// Fired when msf user is successfully signed in in test mode
        /// </summary>
        /// <param name="accountInfo"></param>
        /// <param name="error"></param>
        protected virtual void OnSignInCallbackHandler(AccountInfoPacket accountInfo, string error)
        {
            if (accountInfo == null)
            {
                logger.Error(error);
                return;
            }

            StartClient();
        }

        #endregion

        #region MIRROR EVENTS

        /// <summary>
        /// Invokes when room client is successfully connected to mirror server
        /// </summary>
        protected virtual void OnConnectedToMirrorServerEventHandler(NetworkConnection connection)
        {
            logger.Debug($"Validating access to room server with token [{roomAccess.Token}]");

            // Register listener for access validation message from mirror room server
            NetworkClient.RegisterHandler<ValidateRoomAccessResultMessage>(ValidateRoomAccessResultHandler, false);

            // Send validation message to room server
            connection.Send(new ValidateRoomAccessRequestMessage(roomAccess.Token));
        }

        #endregion

        /// <summary>
        /// Tries to get access data for room we want to connect to
        /// </summary>
        /// <param name="roomId"></param>
        protected virtual void GetRoomAccess(int roomId)
        {
            logger.Debug($"Getting access to room {roomId}");
            Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, $"Getting access to room {roomId}... Please wait!");

            Msf.Client.Rooms.GetAccess(roomId, Msf.Options.AsString(MsfDictKeys.roomPassword, string.Empty), (access, error) =>
            {
                if (access == null)
                {
                    Msf.Events.Invoke(MsfEventKeys.hideLoadingInfo);
                    logger.Error(error);
                    Msf.Events.Invoke(MsfEventKeys.showOkDialogBox,
                        new OkDialogBoxViewEventMessage("We could not get access to room. Please try again later or contact to administrator",
                        () =>
                        {
                            if (IsAllowedToBeStartedInEditor())
                            {
                                Msf.Runtime.Quit();
                            }
                            else
                            {
                                SceneManager.LoadScene(MirrorNetworkManager.offlineScene);
                            }
                        }));
                    return;
                }

                logger.Debug($"Access to room {roomId} received");
                logger.Debug(access);

                // Save gotten room access
                roomAccess = access;

                Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, $"Joinig room {roomId}... Please wait!");

                // Wait for connection to mirror server
                MsfTimer.WaitWhile(() => !NetworkClient.isConnected, isSuccessful =>
                {
                    Msf.Events.Invoke(MsfEventKeys.hideLoadingInfo);

                    if (!isSuccessful)
                    {
                        logger.Error("We could not connect to room. Please try again later or contact to administrator");
                        MirrorNetworkManager.StopClient();
                    }
                    else
                    {
                        OnConnectedToMirrorServerEventHandler(NetworkClient.connection);
                    }
                }, roomConnectionTimeout);

                // If we are not connected to mirror server
                if (!NetworkClient.isConnected)
                {
                    // Let's set the IP before we start connection
                    SetAddress(roomAccess.RoomIp);

                    // Let's set the port before we start connection
                    SetPort(roomAccess.RoomPort);

                    logger.Debug("Connecting to mirror server...");

                    // Start mirror client
                    MirrorNetworkManager.StartClient();
                }
            });
        }

        /// <summary>
        /// Fires when room server send message about access validation result
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="msg"></param>
        protected virtual void ValidateRoomAccessResultHandler(NetworkConnection conn, ValidateRoomAccessResultMessage msg)
        {
            if (msg.Status != ResponseStatus.Success)
            {
                logger.Error(msg.Error);

                OnAccessDenied(conn);
                OnAccessDiniedEvent?.Invoke();

                return;
            }

            logger.Debug("Access to server room is successfully validated");

            OnAccessGranted(conn);
            OnAccessGrantedEvent?.Invoke();
        }

        /// <summary>
        /// Fires when access to room server granted
        /// </summary>
        /// <param name="conn"></param>
        protected virtual void OnAccessGranted(NetworkConnection conn)
        {
            CreatePlayer();
        }

        /// <summary>
        /// Fires when access to room server denied
        /// </summary>
        /// <param name="conn"></param>
        protected virtual void OnAccessDenied(NetworkConnection conn) { }

        /// <summary>
        /// Start room client
        /// </summary>
        public void StartClient()
        {
            if (!Msf.Client.Auth.IsSignedIn)
            {
                Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, "Signing in... Please wait!");

                if (signInAsGuest)
                {
                    Msf.Client.Auth.SignInAsGuest(OnSignInCallbackHandler);
                }
                else
                {
                    Msf.Client.Auth.SignIn(username, password, OnSignInCallbackHandler);
                }
            }
            else
            {
                if (!Msf.Options.Has(MsfDictKeys.roomId))
                {
                    Msf.Client.Matchmaker.FindGames((games) =>
                    {

                        if (games != null && games.Count > 0)
                        {
                            // Save room id to global options just for test purpose only
                            Msf.Options.Set(MsfDictKeys.roomId, games.First().Id);
                            StartClient();
                        }
                        else
                        {
                            Msf.Events.Invoke(MsfEventKeys.showOkDialogBox, new OkDialogBoxViewEventMessage("We could not get access to room. Please try again later or contact to administrator", () => { Msf.Runtime.Quit(); }));
                        }
                    });
                }
                else
                {
                    // Let's get access to room
                    GetRoomAccess(Msf.Options.AsInt(MsfDictKeys.roomId));
                }
            }
        }

        /// <summary>
        /// Create the network player in mirror networking
        /// </summary>
        public virtual void CreatePlayer()
        {
            NetworkClient.Send(new CreatePlayerMessage());
        }

        /// <summary>
        /// Sets an address 
        /// </summary>
        /// <param name="roomAddress"></param>
        public void SetAddress(string roomAddress)
        {
            NetworkManager.singleton.networkAddress = roomAddress;
        }

        /// <summary>
        /// Gets an address
        /// </summary>
        /// <param name="roomIp"></param>
        public string GetAddress()
        {
            return NetworkManager.singleton.networkAddress;
        }

        /// <summary>
        /// Set network transport port
        /// </summary>
        /// <param name="port"></param>
        public virtual void SetPort(int port)
        {
            if (Transport.activeTransport is TelepathyTransport transport)
            {
                transport.port = (ushort)port;
            }
            else
            {
                logger.Error("You are trying to use TelepathyTransport. But it is not found on the scene. Try to override this method to create you own implementation");
            }
        }

        /// <summary>
        /// Get network transport port
        /// </summary>
        /// <returns></returns>
        public virtual int GetPort()
        {
            if (Transport.activeTransport is TelepathyTransport transport)
            {
                return (int)transport.port;
            }
            else
            {
                logger.Error("You are trying to use TelepathyTransport. But it is not found on the scene. Try to override this method to create you own implementation");
                return 0;
            }
        }
    }
}