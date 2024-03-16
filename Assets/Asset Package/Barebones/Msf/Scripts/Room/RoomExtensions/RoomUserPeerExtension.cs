using Barebones.MasterServer;
using Barebones.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barebones.MasterServer
{
    public class RoomUserPeerExtension : IRoomUserPeerExtension
    {
        /// <summary>
        /// Instance of room server peer extension
        /// </summary>
        /// <param name="masterPeerId"></param>
        /// <param name="username"></param>
        /// <param name="roomPeer"></param>
        /// <param name="customOptions"></param>
        public RoomUserPeerExtension(int masterPeerId, string username, IPeer roomPeer, DictionaryOptions customOptions)
        {
            MasterPeerId = masterPeerId;
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Peer = roomPeer ?? throw new ArgumentNullException(nameof(roomPeer));
            Options = customOptions ?? throw new ArgumentNullException(nameof(customOptions));
        }

        /// <summary>
        /// Peer Id of master server client
        /// </summary>
        public int MasterPeerId { get; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Room server peer
        /// </summary>
        public IPeer Peer { get; }

        /// <summary>
        /// Account properties
        /// </summary>
        public DictionaryOptions Options { get; }
    }
}
