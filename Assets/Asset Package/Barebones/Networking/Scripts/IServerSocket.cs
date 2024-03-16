namespace Barebones.Networking
{
    public delegate void PeerActionHandler(IPeer peer);

    public interface IServerSocket
    {
        /// <summary>
        /// Invoked, when a client connects to this socket
        /// </summary>
        event PeerActionHandler OnClientConnectedEvent;

        /// <summary>
        /// Invoked, when client disconnects from this socket
        /// </summary>
        event PeerActionHandler OnClientDisconnectedEvent;

        /// <summary>
        /// Opens the socket and starts listening to a given port. IP is 127.0.0.1
        /// </summary>
        /// <param name="port"></param>
        void Listen(int port);

        /// <summary>
        /// Opens the socket and starts listening to a given port and IP
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        void Listen(string ip, int port);

        /// <summary>
        /// Stops listening
        /// </summary>
        void Stop();
    }
}