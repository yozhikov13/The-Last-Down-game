using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barebones.MasterServer
{
    public class RoomAccessProviderCheck
    {
        public int PeerId { get; set; }
        public string Username { get; set; }
        public DictionaryOptions CustomOptions { get; set; }
    }
}