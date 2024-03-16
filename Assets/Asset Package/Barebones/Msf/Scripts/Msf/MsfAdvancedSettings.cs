using Barebones.Logging;
using Barebones.Networking;
using System;

namespace Barebones.MasterServer
{
    /// <summary>
    /// Advanced settings wrapper
    /// </summary>
    public class MsfAdvancedSettings
    {
        /// <summary>
        /// Factory, used to create client sockets
        /// </summary>
        public Func<IClientSocket> ClientSocketFactory = () => new ClientSocketWs();

        /// <summary>
        /// Factory, used to create server sockets
        /// </summary>
        public Func<IServerSocket> ServerSocketFactory = () => new ServerSocketWs();

        /// <summary>
        /// Message factory
        /// </summary>
        public IMessageFactory MessageFactory = new MessageFactory();

        /// <summary>
        /// Global logging settings
        /// </summary>
        public MsfLogController Logging { get; private set; }

        public MsfAdvancedSettings()
        {
            Logging = new MsfLogController(LogLevel.All);
        }
    }
}