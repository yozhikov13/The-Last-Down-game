using Barebones.Bridges.Mirror.Character;
using Barebones.MasterServer;
using Barebones.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barebones.Bridges.Mirror.Character
{
    public class PlayerCharacterProfile : PlayerCharacterBehaviour
    {
        protected MirrorRoomPlayer roomPlayer;

        public override void OnStartServer()
        {
            base.OnStartServer();

            // Get room server player by mirror peer id
            roomPlayer = MirrorRoomServer.Instance.GetRoomPlayerByMirrorPeer(netIdentity.connectionToClient);

            if(roomPlayer == null)
            {
                Debug.LogError($"Player {netIdentity.connectionToClient.connectionId} could not get its room playerinfo");
                netIdentity.connectionToClient.Disconnect();
                return;
            }
        }
    }
}