using Barebones.Networking;
using System;
using System.Collections.Generic;

namespace Barebones.MasterServer
{
    public class UsersRoomModule : BaseServerModule
    {
        /// <summary>
        /// List of players filtered by master peer id
        /// </summary>
        private Dictionary<int, IRoomUserPeerExtension> roomPlayersByMsfPeerId;

        /// <summary>
        /// List of players filtered by room peer id
        /// </summary>
        private Dictionary<int, IRoomUserPeerExtension> roomPlayersByRoomPeerId;

        /// <summary>
        /// List of players filtered by username
        /// </summary>
        private Dictionary<string, IRoomUserPeerExtension> roomPlayersByUsername;

        /// <summary>
        /// Fires when new player joined room
        /// </summary>
        public event Action<IRoomUserPeerExtension> OnPlayerJoinedEvent;

        /// <summary>
        /// Fires when new player left room
        /// </summary>
        public event Action<IRoomUserPeerExtension> OnPlayerLeftEvent;

        private void OnDestroy()
        {
            if (Server != null)
                Server.OnPeerDisconnectedEvent -= Server_OnPeerDisconnectedEvent;
        }

        public override void Initialize(IServer server)
        {
            // Initialize lists
            roomPlayersByMsfPeerId = new Dictionary<int, IRoomUserPeerExtension>();
            roomPlayersByRoomPeerId = new Dictionary<int, IRoomUserPeerExtension>();
            roomPlayersByUsername = new Dictionary<string, IRoomUserPeerExtension>();

            Server.OnPeerDisconnectedEvent += Server_OnPeerDisconnectedEvent;

            // Register handler to handle validate access request
            server.SetHandler((short)MsfMessageCodes.ValidateRoomAccessRequest, ValidateRoomAccessRequestHandler);
        }

        /// <summary>
        /// Fires when room player is disconnected
        /// </summary>
        /// <param name="peer"></param>
        private void Server_OnPeerDisconnectedEvent(IPeer peer)
        {
            var roomUserExtension = peer.GetExtension<RoomUserPeerExtension>();

            if (roomUserExtension != null)
            {
                roomPlayersByMsfPeerId.Remove(roomUserExtension.MasterPeerId);
                roomPlayersByRoomPeerId.Remove(peer.Id);
                roomPlayersByUsername.Remove(roomUserExtension.Username);

                var roomServer = Server as RoomServerBehaviour;
                roomServer.CurrentRoomController.NotifyPlayerLeft(roomUserExtension.MasterPeerId);

                OnPlayerLeftEvent?.Invoke(roomUserExtension);
            }
        }

        /// <summary>
        /// Fired when player requested access to this room
        /// </summary>
        /// <param name="message"></param>
        private void ValidateRoomAccessRequestHandler(IIncommingMessage message)
        {
            var token = message.AsString();
            var roomServer = Server as RoomServerBehaviour;

            logger.Debug($"Client {message.Peer.Id} requested access validation");

            // Trying to validate room client connection access
            Msf.Server.Rooms.ValidateAccess(roomServer.CurrentRoomController.RoomId, token, (usernameAndPeerId, error) =>
            {
                if (usernameAndPeerId == null)
                {
                    logger.Error(error);
                    message.Peer.Disconnect("Invalid room access token");
                    return;
                }

                // Let's get account info of the connected peer from master server
                Msf.Server.Auth.GetPeerAccountInfo(usernameAndPeerId.PeerId, (info, infoError) =>
                {
                    if (info == null)
                    {
                        logger.Error(infoError);
                        return;
                    }

                    logger.Debug($"Room got peer account info. {info}");

                    // Create new room player info
                    var roomUserExtension = new RoomUserPeerExtension(info.PeerId, info.Username, message.Peer, info.CustomOptions);

                    // Add extension to peer
                    message.Peer.AddExtension(roomUserExtension);

                    // Inform all listeners
                    OnPlayerJoinedEvent?.Invoke(roomUserExtension);
                });

                message.Respond(ResponseStatus.Success);
            });
        }

        /// <summary>
        /// Get connected user by its msf peer id
        /// </summary>
        /// <param name="peerId"></param>
        /// <returns></returns>
        public IRoomUserPeerExtension GetUserByMsfPeerId(int peerId)
        {
            roomPlayersByMsfPeerId.TryGetValue(peerId, out IRoomUserPeerExtension user);
            return user;
        }

        /// <summary>
        /// Get connected user by its room peer id
        /// </summary>
        /// <param name="peerId"></param>
        /// <returns></returns>
        public IRoomUserPeerExtension GetUserByRoomPeerId(int peerId)
        {
            roomPlayersByRoomPeerId.TryGetValue(peerId, out IRoomUserPeerExtension user);
            return user;
        }

        /// <summary>
        /// Get connected user by its username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public IRoomUserPeerExtension GetUserByUsername(string username)
        {
            roomPlayersByUsername.TryGetValue(username, out IRoomUserPeerExtension user);
            return user;
        }
    }
}
