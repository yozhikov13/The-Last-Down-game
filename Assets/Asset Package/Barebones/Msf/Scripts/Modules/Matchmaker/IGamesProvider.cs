using Barebones.Networking;
using System.Collections.Generic;

namespace Barebones.MasterServer
{
    public interface IGamesProvider
    {
        IEnumerable<GameInfoPacket> GetPublicGames(IPeer peer, Dictionary<string, string> filters);
    }
}