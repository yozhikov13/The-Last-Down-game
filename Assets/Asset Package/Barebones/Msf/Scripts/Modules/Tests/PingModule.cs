using System;
using System.Collections;
using System.Collections.Generic;
using Barebones.Networking;
using UnityEngine;

namespace Barebones.MasterServer
{
    public class PingModule : BaseServerModule
    {
        [SerializeField, TextArea(3, 5)]
        private string pongMessage = "Hello, Pong!";

        public override void Initialize(IServer server)
        {
            server.SetHandler((short)MsfMessageCodes.Ping, OnPingRequestListener);
        }

        private void OnPingRequestListener(IIncommingMessage message)
        {
            message.Respond(pongMessage.ToBytes(), ResponseStatus.Success);
        }
    }
}