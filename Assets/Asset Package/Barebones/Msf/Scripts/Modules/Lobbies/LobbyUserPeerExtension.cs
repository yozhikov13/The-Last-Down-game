using Barebones.Networking;

namespace Barebones.MasterServer
{
    public class LobbyUserPeerExtension : IPeerExtension
    {
        public IPeer Peer { get; private set; }

        /// <summary>
        /// Lobby, to which current peer belongs
        /// </summary>
        public ILobby CurrentLobby { get; set; }

        public LobbyUserPeerExtension(IPeer peer)
        {
            this.Peer = peer;
        }
    }
}