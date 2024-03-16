﻿using Barebones.Networking;
using System.Collections.Generic;

namespace Barebones.MasterServer
{
    public class ChatUserPeerExtension : IPeerExtension
    {
        public HashSet<ChatChannel> CurrentChannels { get; private set; }

        public ChatChannel DefaultChannel { get; set; }

        public string Username { get; private set; }

        public IPeer Peer { get; private set; }

        public ChatUserPeerExtension(IPeer peer, string username)
        {
            Peer = peer;
            Username = username;
            CurrentChannels = new HashSet<ChatChannel>();
        }
    }
}