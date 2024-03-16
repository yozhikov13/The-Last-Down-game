using Barebones.MasterServer;
using Barebones.Networking;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barebones.Bridges.Mirror
{
    public class MirrorRoomClientStarter : BaseClientBehaviour
    {
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Connection.RemoveConnectionListener(OnConnectedToMasterServerEventHandler);
        }

        protected override void OnInitialize()
        {
            if (Msf.Options.Has(MsfDictKeys.autoStartRoomClient))
            {
                Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, "Connecting to room... Please wait!");
                Connection.AddConnectionListener(OnConnectedToMasterServerEventHandler);
            }
        }

        protected virtual void OnConnectedToMasterServerEventHandler()
        {
            MsfTimer.WaitForEndOfFrame(() =>
            {
                MirrorRoomClient.Instance.StartClient();
            });
        }
    }
}
