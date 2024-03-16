using Barebones.Networking;

namespace Barebones.MasterServer
{
    public interface IPeerExtension
    {
        IPeer Peer { get; }
    }
}