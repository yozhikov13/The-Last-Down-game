using Barebones.Logging;
using Barebones.Networking;
using System.Collections.Generic;
using System.Linq;

namespace Barebones.MasterServer
{
    public delegate void FindGamesCallback(List<GameInfoPacket> games);

    public class MsfMatchmakerClient : MsfBaseClient
    {
        /// <summary>
        /// List of the last loaded games
        /// </summary>
        public List<GameInfoPacket> Games { get; private set; }

        public MsfMatchmakerClient(IClientSocket connection) : base(connection) { }

        /// <summary>
        /// Retrieves a list of all public games
        /// </summary>
        /// <param name="callback"></param>
        public void FindGames(FindGamesCallback callback)
        {
            FindGames(new Dictionary<string, string>(), callback, Connection);
        }

        /// <summary>
        /// Retrieves a list of public games, which pass a provided filter.
        /// (You can implement your own filtering by extending modules or "classes" 
        /// that implement <see cref="IGamesProvider"/>)
        /// </summary>
        public void FindGames(Dictionary<string, string> filter, FindGamesCallback callback)
        {
            FindGames(filter, callback, Connection);
        }

        /// <summary>
        /// Retrieves a list of public games, which pass a provided filter.
        /// (You can implement your own filtering by extending modules or "classes" 
        /// that implement <see cref="IGamesProvider"/>)
        /// </summary>
        public void FindGames(Dictionary<string, string> filter, FindGamesCallback callback, IClientSocket connection)
        {
            if (!connection.IsConnected)
            {
                Games = new List<GameInfoPacket>();
                Logs.Error("Not connected");
                callback.Invoke(Games);
                return;
            }

            connection.SendMessage((short)MsfMessageCodes.FindGamesRequest, filter.ToBytes(), (status, response) =>
            {
                if (status != ResponseStatus.Success)
                {
                    Games = new List<GameInfoPacket>();
                    Logs.Error(response.AsString("Unknown error while requesting a list of games"));
                    callback.Invoke(Games);
                    return;
                }

                Games = response.DeserializeList(() => new GameInfoPacket()).ToList();
                callback.Invoke(Games);
            });
        }
    }
}