using Aevien.Utilities;
using Barebones.Logging;
using Barebones.Networking;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Barebones.MasterServer
{
    [AddComponentMenu("MSF/Shared/ClientToMasterConnector")]
    /// <summary>
    /// Automatically connects to master server
    /// </summary>
    public class ClientToMasterConnector : ConnectionHelper<ClientToMasterConnector>
    {
        protected override void Awake()
        {
            base.Awake();

            // If master IP is provided via cmd arguments
            if (Msf.Args.IsProvided(Msf.Args.Names.MasterIp))
            {
                serverIp = Msf.Args.MasterIp;
            }

            // If master port is provided via cmd arguments
            if (Msf.Args.IsProvided(Msf.Args.Names.MasterPort))
            {
                serverPort = Msf.Args.MasterPort;
            }
        }
    }
}