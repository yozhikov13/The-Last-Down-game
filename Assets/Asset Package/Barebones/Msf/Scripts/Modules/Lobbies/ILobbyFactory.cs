using Barebones.Networking;
using System.Collections.Generic;

namespace Barebones.MasterServer
{
    public interface ILobbyFactory
    {
        string Id { get; }

        ILobby CreateLobby(Dictionary<string, string> properties, IPeer creator);
    }
}