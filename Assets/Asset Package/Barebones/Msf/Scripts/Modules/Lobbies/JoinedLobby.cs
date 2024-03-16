using Barebones.Networking;
using System;
using System.Collections.Generic;

namespace Barebones.MasterServer
{
    /// <summary>
    /// Represents a joined lobby. When player joins a lobby,
    /// an instance of this class is created. It acts as a convenient way
    /// to manage lobby state from player perspective
    /// </summary>
    public class JoinedLobby
    {
        public delegate void PlayerPropertyChangedHandler(LobbyMemberData member, string propertyKey, string propertyValue);

        public delegate void LobbyPropertyChangeHandler(string property, string key);

        public LobbyDataPacket Data { get; private set; }
        private readonly IClientSocket _connection;

        public Dictionary<string, string> Properties { get; private set; }
        public Dictionary<string, LobbyMemberData> Members { get; private set; }
        public Dictionary<string, LobbyTeamData> Teams { get; private set; }

        private ILobbyListener _listener;

        public JoinedLobby(LobbyDataPacket data, IClientSocket connection)
        {
            Data = data;
            _connection = connection;
            connection.SetHandler((short)MsfMessageCodes.LobbyMemberPropertyChanged, HandleMemberPropertyChanged);
            connection.SetHandler((short)MsfMessageCodes.LeftLobby, HandleLeftLobbyMsg);
            connection.SetHandler((short)MsfMessageCodes.LobbyChatMessage, HandleLobbyChatMessageMsg);
            connection.SetHandler((short)MsfMessageCodes.LobbyMemberJoined, HandleLobbyMemberJoinedMsg);
            connection.SetHandler((short)MsfMessageCodes.LobbyMemberLeft, HandleLobbyMemberLeftMsg);
            connection.SetHandler((short)MsfMessageCodes.LobbyStateChange, HandleLobbyStateChangeMsg);
            connection.SetHandler((short)MsfMessageCodes.LobbyStatusTextChange, HandleLobbyStatusTextChangeMsg);
            connection.SetHandler((short)MsfMessageCodes.LobbyMemberChangedTeam, HandlePlayerTeamChangeMsg);
            connection.SetHandler((short)MsfMessageCodes.LobbyMemberReadyStatusChange, HandleLobbyMemberReadyStatusChangeMsg);
            connection.SetHandler((short)MsfMessageCodes.LobbyMasterChange, HandleLobbyMasterChangeMsg);
            connection.SetHandler((short)MsfMessageCodes.LobbyPropertyChanged, HandleLobbyPropertyChanged);

            Properties = data.LobbyProperties;
            Members = data.Players;
            Teams = data.Teams;
        }

        public ILobbyListener Listener { get { return _listener; } }

        public string LobbyName { get { return Data.LobbyName; } }

        public int Id { get { return Data.LobbyId; } }

        public LobbyState State { get { return Data.LobbyState; } }

        public bool HasLeft { get; private set; }

        /// <summary>
        /// Leaves this lobby
        /// </summary>
        public void Leave()
        {
            Msf.Client.Lobbies.LeaveLobby(Id, () => { }, _connection);
        }

        /// <summary>
        /// Leaves this lobby
        /// </summary>
        /// <param name="callback"></param>
        public void Leave(Action callback)
        {
            Msf.Client.Lobbies.LeaveLobby(Id, callback, _connection);
        }

        /// <summary>
        /// Sets a lobby property to a specified value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetLobbyProperty(string key, string value)
        {
            SetLobbyProperty(key, value, (successful, error) => { });
        }

        /// <summary>
        /// Sets a lobby property to a specified value
        /// </summary>
        public void SetLobbyProperty(string key, string value, SuccessCallback callback)
        {
            var data = new Dictionary<string, string>()
            {
                {key, value }
            };

            Msf.Client.Lobbies.SetLobbyProperties(Id, data, callback, _connection);
        }

        /// <summary>
        /// Sets a lobby properties to values, provided within a dictionary
        /// </summary>
        public void SetLobbyProperties(Dictionary<string, string> properties, SuccessCallback callback)
        {
            Msf.Client.Lobbies.SetLobbyProperties(Id, properties, callback, _connection);
        }

        /// <summary>
        /// Sets current player's properties
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetMyProperty(string key, string value)
        {
            SetMyProperty(key, value, (successful, error) => { });
        }


        /// <summary>
        /// Set's current player's properties
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="callback"></param>
        public void SetMyProperty(string key, string value, SuccessCallback callback)
        {
            var data = new Dictionary<string, string>()
            {
                {key, value }
            };

            Msf.Client.Lobbies.SetMyProperties(data, callback, _connection);
        }

        /// <summary>
        /// Set's current player's properties
        /// </summary>
        public void SetMyProperties(Dictionary<string, string> properties, SuccessCallback callback)
        {
            Msf.Client.Lobbies.SetMyProperties(properties, callback, _connection);
        }

        /// <summary>
        /// Set's current player's ready status
        /// </summary>
        /// <param name="isReady"></param>
        public void SetReadyStatus(bool isReady)
        {
            Msf.Client.Lobbies.SetReadyStatus(isReady, (successful, error) => { }, _connection);
        }

        /// <summary>
        /// Set's current player's ready status
        /// </summary>
        public void SetReadyStatus(bool isReady, SuccessCallback callback)
        {
            Msf.Client.Lobbies.SetReadyStatus(isReady, callback, _connection);
        }

        /// <summary>
        /// Set's a lobby event listener
        /// </summary>
        /// <param name="listener"></param>
        public void SetListener(ILobbyListener listener)
        {
            _listener = listener;

            if (listener != null)
            {
                _listener.Initialize(this);
            }
        }

        /// <summary>
        /// Send's a lobby chat message
        /// </summary>
        /// <param name="message"></param>
        public void SendChatMessage(string message)
        {
            Msf.Client.Lobbies.SendChatMessage(message, _connection);
        }

        /// <summary>
        /// Switches current user to another team (if available)
        /// </summary>
        /// <param name="teamName"></param>
        /// <param name="callback"></param>
        public void JoinTeam(string teamName, SuccessCallback callback)
        {
            Msf.Client.Lobbies.JoinTeam(Id, teamName, callback, _connection);
        }

        /// <summary>
        /// Sends a request to server to start a match
        /// </summary>
        /// <param name="callback"></param>
        public void StartGame(SuccessCallback callback)
        {
            Msf.Client.Lobbies.StartGame(callback, _connection);
        }

        /// <summary>
        /// Retrieves an access to room, which is assigned to this lobby
        /// </summary>
        /// <param name="callback"></param>
        public void GetLobbyRoomAccess(RoomAccessCallback callback)
        {
            Msf.Client.Lobbies.GetLobbyRoomAccess(new Dictionary<string, string>(), callback, _connection);
        }

        /// <summary>
        /// Retrieves an access to room, which is assigned to this lobby
        /// </summary>
        public void GetLobbyRoomAccess(Dictionary<string, string> properties, RoomAccessCallback callback)
        {
            Msf.Client.Lobbies.GetLobbyRoomAccess(properties, callback, _connection);
        }

        #region Handlers

        private void HandleMemberPropertyChanged(IIncommingMessage message)
        {
            var data = message.Deserialize(new LobbyMemberPropChangePacket());

            if (Id != data.LobbyId)
            {
                return;
            }

            LobbyMemberData member;
            Members.TryGetValue(data.Username, out member);

            if (member == null)
            {
                return;
            }

            member.Properties[data.Property] = data.Value;

            if (_listener != null)
            {
                _listener.OnMemberPropertyChanged(member, data.Property, data.Value);
            }
        }

        private void HandleLeftLobbyMsg(IIncommingMessage message)
        {
            var id = message.AsInt();

            // Check the id in case there's something wrong with message order
            if (Id != id)
            {
                return;
            }

            HasLeft = true;

            if (_listener != null)
            {
                _listener.OnLobbyLeft();
            }
        }

        private void HandleLobbyChatMessageMsg(IIncommingMessage message)
        {
            var msg = message.Deserialize(new LobbyChatPacket());

            if (_listener != null)
            {
                _listener.OnChatMessageReceived(msg);
            }
        }

        private void HandleLobbyMemberLeftMsg(IIncommingMessage message)
        {
            var username = message.AsString();

            LobbyMemberData member;
            Members.TryGetValue(username, out member);

            if (member == null)
            {
                return;
            }

            if (_listener != null)
            {
                _listener.OnMemberLeft(member);
            }
        }

        private void HandleLobbyMemberJoinedMsg(IIncommingMessage message)
        {
            var data = message.Deserialize(new LobbyMemberData());
            Members[data.Username] = data;

            if (_listener != null)
            {
                _listener.OnMemberJoined(data);
            }
        }

        private void HandleLobbyMasterChangeMsg(IIncommingMessage message)
        {
            var masterUsername = message.AsString();

            Data.GameMaster = masterUsername;

            if (_listener != null)
            {
                _listener.OnMasterChanged(masterUsername);
            }
        }

        private void HandleLobbyMemberReadyStatusChangeMsg(IIncommingMessage message)
        {
            var data = message.Deserialize(new StringPairPacket());

            LobbyMemberData member;
            Members.TryGetValue(data.A, out member);

            if (member == null)
            {
                return;
            }

            member.IsReady = bool.Parse(data.B);

            if (_listener != null)
            {
                _listener.OnMemberReadyStatusChanged(member, member.IsReady);
            }
        }

        private void HandlePlayerTeamChangeMsg(IIncommingMessage message)
        {
            var data = message.Deserialize(new StringPairPacket());

            LobbyMemberData member;
            Members.TryGetValue(data.A, out member);

            if (member == null)
            {
                return;
            }

            LobbyTeamData newTeam;
            Teams.TryGetValue(data.B, out newTeam);

            if (newTeam == null)
            {
                return;
            }

            member.Team = newTeam.Name;

            if (_listener != null)
            {
                _listener.OnMemberTeamChanged(member, newTeam);
            }
        }

        private void HandleLobbyStatusTextChangeMsg(IIncommingMessage message)
        {
            var text = message.AsString();

            Data.StatusText = text;

            if (_listener != null)
            {
                _listener.OnLobbyStatusTextChanged(text);
            }
        }

        private void HandleLobbyStateChangeMsg(IIncommingMessage message)
        {
            var newState = (LobbyState)message.AsInt();

            Data.LobbyState = newState;

            if (_listener != null)
            {
                _listener.OnLobbyStateChange(newState);
            }
        }

        private void HandleLobbyPropertyChanged(IIncommingMessage message)
        {
            var data = message.Deserialize(new StringPairPacket());
            Properties[data.A] = data.B;

            if (_listener != null)
            {
                _listener.OnLobbyPropertyChanged(data.A, data.B);
            }
        }

        #endregion
    }
}