namespace Barebones.MasterServer
{
    public interface IRoomUserPeerExtension : IPeerExtension
    {
        /// <summary>
        /// Peer Id that master server gives to client
        /// </summary>
        int MasterPeerId { get; }

        /// <summary>
        /// Username that client has
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Properties of client account
        /// </summary>
        DictionaryOptions Options { get; }
    }
}
