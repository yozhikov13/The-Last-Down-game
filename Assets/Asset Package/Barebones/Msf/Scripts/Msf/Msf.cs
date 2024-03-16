using Barebones.Networking;
using System;

namespace Barebones.MasterServer
{
    /// <summary>
    /// This class is a central class, which can be used by entities (clients and servers)
    /// that need to connect to master server, and access it's functionality
    /// </summary>
    public static class Msf
    {
        /// <summary>
        /// Version of the framework
        /// </summary>
        public static string Version => "v3.8.2";

        /// <summary>
        /// Just name of the framework
        /// </summary>
        public static string Name => "MASTER SERVER FRAMEWORK";

        /// <summary>
        /// Main connection to master server
        /// </summary>
        public static IClientSocket Connection { get; private set; }

        /// <summary>
        /// Advanced master server framework settings
        /// </summary>
        public static MsfAdvancedSettings Advanced { get; private set; }

        /// <summary>
        /// Collection of methods, that can be used BY CLIENT, connected to master server
        /// </summary>
        public static MsfClient Client { get; private set; }

        /// <summary>
        /// Collection of methods, that can be used from your servers
        /// </summary>
        public static MsfServer Server { get; private set; }

        /// <summary>
        /// Contains methods to help work with threads
        /// </summary>
        public static MsfConcurrency Concurrency { get; private set; }

        /// <summary>
        /// Contains methods for creating some of the common types
        /// (server sockets, messages and etc)
        /// </summary>
        public static MsfCreate Create { get; set; }

        /// <summary>
        /// Contains helper methods, that couldn't be added to any other
        /// object
        /// </summary>
        public static MsfHelper Helper { get; set; }

        /// <summary>
        /// Contains security-related stuff (encryptions, permission requests)
        /// </summary>
        public static MsfSecurity Security { get; private set; }

        /// <summary>
        /// Default events channel
        /// </summary>
        public static EventsChannel Events { get; private set; }

        /// <summary>
        /// Contains methods, that work with runtime data
        /// </summary>
        public static MsfRuntime Runtime { get; private set; }

        /// <summary>
        /// Contains command line / terminal values, which were provided
        /// when starting the process
        /// </summary>
        public static MsfArgs Args { get; private set; }

        public static DictionaryOptions Options { get; private set; }

        static Msf()
        {
            // Initialize helpers to work with MSF
            Helper = new MsfHelper();

            // Initialize advanced settings
            Advanced = new MsfAdvancedSettings();

            // Initialize runtime data
            Runtime = new MsfRuntime();

            // Initialize work with command line arguments
            Args = new MsfArgs();

            // List of options you can use in game
            Options = new DictionaryOptions();

            // Create a default connection
            Connection = Advanced.ClientSocketFactory();

            // Initialize parts of framework, that act as "clients"
            Client = new MsfClient(Connection);
            Server = new MsfServer(Connection);
            Security = new MsfSecurity(Connection);

            // Other stuff
            Create = new MsfCreate();
            Concurrency = new MsfConcurrency();
            Events = new EventsChannel("default", true);
        }
    }
}