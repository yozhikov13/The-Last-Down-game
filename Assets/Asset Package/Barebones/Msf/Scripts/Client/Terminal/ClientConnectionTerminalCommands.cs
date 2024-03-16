using Barebones.MasterServer;
using CommandTerminal;
using UnityEngine;

namespace Barebones.Client.Utilities
{
    public class ClientConnectionTerminalCommands
    {
        [RegisterCommand(Name = "client.ping", Help = "Send ping to master server")]
        private static void SendPingCmd(CommandArg[] args)
        {
            Msf.Connection.SendMessage((short)MsfMessageCodes.Ping, (status, response) =>
            {
                Debug.Log($"Message: {response.AsString()}, Status: {response.Status.ToString()}");
            });
        }

        [RegisterCommand(Name = "client.connect", Help = "Connects the client to master. 1 Server IP address, 2 Server port", MinArgCount = 1)]
        private static void ClientConnect(CommandArg[] args)
        {
            Msf.Connection.Connect(args[0].String, Mathf.Clamp(args[1].Int, 0, ushort.MaxValue));
        }

        [RegisterCommand(Name = "client.disconnect", Help = "Disconnects the client from master")]
        private static void ClientDisconnect(CommandArg[] args)
        {
            Msf.Connection.Disconnect();
        }
    }
}