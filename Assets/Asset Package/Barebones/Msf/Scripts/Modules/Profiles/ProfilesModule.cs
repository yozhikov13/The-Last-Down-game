using Barebones.Logging;
using Barebones.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Barebones.MasterServer
{
    public delegate ObservableServerProfile ProfileFactory(string username, IPeer clientPeer);

    /// <summary>
    /// Handles player profiles within master server.
    /// Listens to changes in player profiles, and sends updates to
    /// clients of interest.
    /// Also, reads changes from game server, and applies them to players profile
    /// </summary>
    public class ProfilesModule : BaseServerModule
    {
        private AuthModule authModule;
        private HashSet<string> debouncedSaves;
        private HashSet<string> debouncedClientUpdates;

        /// <summary>
        /// Time to pass after logging out, until profile
        /// will be removed from the lookup. Should be enough for game
        /// server to submit last changes
        /// </summary>
        public float unloadProfileAfter = 20f;

        /// <summary>
        /// Interval, in which updated profiles will be saved to database
        /// </summary>
        public float saveProfileInterval = 1f;

        /// <summary>
        /// Interval, in which profile updates will be sent to clients
        /// </summary>
        public float clientUpdateInterval = 0f;

        /// <summary>
        /// Permission user need to have to edit profile
        /// </summary>
        public int editProfilePermissionLevel = 0;

        /// <summary>
        /// DB to work with profile data
        /// </summary>
        public IProfilesDatabaseAccessor profileDatabaseAccessor;

        /// <summary>
        /// Ignore errors occurred when profile data mismatch
        /// </summary>
        public bool ignoreProfileMissmatchError = false;

        /// <summary>
        /// By default, profiles module will use this factory to create a profile for users.
        /// If you're using profiles, you will need to change this factory to construct the
        /// structure of a profile.
        /// </summary>
        public ProfileFactory ProfileFactory { get; set; }

        public Dictionary<string, ObservableServerProfile> ProfilesList { get; protected set; }

        protected override void Awake()
        {
            base.Awake();

            if (DestroyIfExists())
            {
                return;
            }

            AddOptionalDependency<AuthModule>();

            ProfilesList = new Dictionary<string, ObservableServerProfile>();
            debouncedSaves = new HashSet<string>();
            debouncedClientUpdates = new HashSet<string>();
        }

        public override void Initialize(IServer server)
        {
            profileDatabaseAccessor = Msf.Server.DbAccessors.GetAccessor<IProfilesDatabaseAccessor>();

            if (profileDatabaseAccessor == null)
            {
                logger.Error("Profiles database implementation was not found");
            }

            // Auth dependency setup
            authModule = server.GetModule<AuthModule>();

            if (authModule != null)
            {
                authModule.OnUserLoggedInEvent += OnUserLoggedInEventHandler;
            }

            // Games dependency setup
            server.SetHandler((short)MsfMessageCodes.ServerProfileRequest, GameServerProfileRequestHandler);
            server.SetHandler((short)MsfMessageCodes.UpdateServerProfile, ProfileUpdateHandler);
            server.SetHandler((short)MsfMessageCodes.ClientProfileRequest, ClientProfileRequestHandler);
        }

        /// <summary>
        /// Invoked, when user logs into the master server
        /// </summary>
        /// <param name="session"></param>
        /// <param name="accountData"></param>
        private void OnUserLoggedInEventHandler(IUserPeerExtension user)
        {
            user.Peer.OnPeerDisconnectedEvent += OnPeerPlayerDisconnectedEventHandler;

            // Create a profile
            ObservableServerProfile profile;

            if (ProfilesList.ContainsKey(user.Username))
            {
                // There's a profile from before, which we can use
                profile = ProfilesList[user.Username];
                profile.ClientPeer = user.Peer;
            }
            else
            {
                // We need to create a new one
                profile = CreateProfile(user.Username, user.Peer);
                ProfilesList.Add(user.Username, profile);
            }

            // Restore profile data from database (only if not a guest)
            if (!user.Account.IsGuest)
            {
                profileDatabaseAccessor.RestoreProfile(profile);
            }

            // Save profile property
            user.Peer.AddExtension(new ProfilePeerExtension(profile, user.Peer));

            // Listen to profile events
            profile.OnModifiedInServerEvent += OnProfileChangedEventHandler;
        }

        /// <summary>
        /// Creates an observable profile for a client.
        /// Override this, if you want to customize the profile creation
        /// </summary>
        /// <param name="username"></param>
        /// <param name="clientPeer"></param>
        /// <returns></returns>
        protected virtual ObservableServerProfile CreateProfile(string username, IPeer clientPeer)
        {
            if (ProfileFactory != null)
            {
                return ProfileFactory(username, clientPeer);
            }

            return new ObservableServerProfile(username, clientPeer);
        }

        /// <summary>
        /// Invoked, when profile is changed
        /// </summary>
        /// <param name="profile"></param>
        private void OnProfileChangedEventHandler(ObservableServerProfile profile)
        {
            // Debouncing is used to reduce a number of updates per interval to one
            // TODO make debounce lookup more efficient than using string hashet

            if (!debouncedSaves.Contains(profile.Username) && profile.ShouldBeSavedToDatabase)
            {
                // If profile is not already waiting to be saved
                debouncedSaves.Add(profile.Username);
                StartCoroutine(SaveProfile(profile, saveProfileInterval));
            }

            if (!debouncedClientUpdates.Contains(profile.Username))
            {
                // If it's a master server
                debouncedClientUpdates.Add(profile.Username);
                StartCoroutine(SendUpdatesToClient(profile, clientUpdateInterval));
            }
        }

        /// <summary>
        /// Invoked, when user logs out (disconnects from master)
        /// </summary>
        /// <param name="session"></param>
        private void OnPeerPlayerDisconnectedEventHandler(IPeer peer)
        {
            peer.OnPeerDisconnectedEvent -= OnPeerPlayerDisconnectedEventHandler;

            var profileExtension = peer.GetExtension<ProfilePeerExtension>();

            if (profileExtension == null)
            {
                return;
            }

            // Unload profile
            StartCoroutine(UnloadProfile(profileExtension.Username, unloadProfileAfter));
        }

        /// <summary>
        /// Saves a profile into database after delay
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator SaveProfile(ObservableServerProfile profile, float delay)
        {
            // Wait for the delay
            yield return new WaitForSecondsRealtime(delay);

            // Remove value from debounced updates
            debouncedSaves.Remove(profile.Username);

            profileDatabaseAccessor.UpdateProfile(profile);

            profile.UnsavedProperties.Clear();
        }

        /// <summary>
        /// Collets changes in the profile, and sends them to client after delay
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator SendUpdatesToClient(ObservableServerProfile profile, float delay)
        {
            // Wait for the delay
            if (delay > 0.01f)
            {
                yield return new WaitForSecondsRealtime(delay);
            }
            else
            {
                // Wait one frame, so that we don't send multiple packets
                // in case we update multiple values
                yield return null;
            }

            // Remove value from debounced updates
            debouncedClientUpdates.Remove(profile.Username);

            if (profile.ClientPeer == null || !profile.ClientPeer.IsConnected)
            {
                // If client is not connected, and we don't need to send him profile updates
                profile.ClearUpdates();
                yield break;
            }

            using (var ms = new MemoryStream())
            {
                using (var writer = new EndianBinaryWriter(EndianBitConverter.Big, ms))
                {
                    profile.GetUpdates(writer);
                    profile.ClearUpdates();
                }

                profile.ClientPeer.SendMessage(MessageHelper.Create((short)MsfMessageCodes.UpdateClientProfile, ms.ToArray()),
                    DeliveryMethod.ReliableSequenced);
            }
        }

        /// <summary>
        /// Coroutine, which unloads profile after a period of time
        /// </summary>
        /// <param name="username"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator UnloadProfile(string username, float delay)
        {
            // Wait for the delay
            yield return new WaitForSecondsRealtime(delay);

            // If user is not actually logged in, remove the profile
            if (authModule.IsUserLoggedIn(username))
            {
                yield break;
            }

            ProfilesList.TryGetValue(username, out ObservableServerProfile profile);

            if (profile == null)
            {
                yield break;
            }

            // Remove profile
            ProfilesList.Remove(username);

            // Remove listeners
            profile.OnModifiedInServerEvent -= OnProfileChangedEventHandler;
        }

        protected virtual bool HasPermissionToEditProfiles(IPeer messagePeer)
        {
            var securityExtension = messagePeer.GetExtension<SecurityInfoPeerExtension>();

            return securityExtension != null
                   && securityExtension.PermissionLevel >= editProfilePermissionLevel;
        }

        #region Handlers

        /// <summary>
        /// Handles a message from game server, which includes player profiles updates
        /// </summary>
        /// <param name="message"></param>
        protected virtual void ProfileUpdateHandler(IIncommingMessage message)
        {
            if (!HasPermissionToEditProfiles(message.Peer))
            {
                Logs.Error("Master server received an update for a profile, but peer who tried to " +
                           "update it did not have sufficient permissions");
                return;
            }

            var data = message.AsBytes();

            using (var ms = new MemoryStream(data))
            {
                using (var reader = new EndianBinaryReader(EndianBitConverter.Big, ms))
                {
                    // Read profiles count
                    var count = reader.ReadInt32();

                    for (var i = 0; i < count; i++)
                    {
                        // Read username
                        var username = reader.ReadString();

                        // Read updates length
                        var updatesLength = reader.ReadInt32();

                        // Read updates
                        var updates = reader.ReadBytes(updatesLength);

                        try
                        {
                            if (ProfilesList.TryGetValue(username, out ObservableServerProfile profile))
                            {
                                profile.ApplyUpdates(updates);
                            }
                        }
                        catch (Exception e)
                        {
                            Logs.Error("Error while trying to handle profile updates from master server");
                            Logs.Error(e);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles a request from client to get profile
        /// </summary>
        /// <param name="message"></param>
        protected virtual void ClientProfileRequestHandler(IIncommingMessage message)
        {
            var clientPropCount = message.AsInt();

            var profileExt = message.Peer.GetExtension<ProfilePeerExtension>();

            if (profileExt == null)
            {
                message.Respond("Profile not found", ResponseStatus.Failed);
                return;
            }

            profileExt.Profile.ClientPeer = message.Peer;

            if (!ignoreProfileMissmatchError && clientPropCount != profileExt.Profile.PropertyCount)
            {
                logger.Error(string.Format($"Client requested a profile with {clientPropCount} properties, but server " +
                                           $"constructed a profile with {profileExt.Profile.PropertyCount}. Make sure that you've changed the " +
                                           "profile factory on the ProfilesModule"));
            }

            message.Respond(profileExt.Profile.ToBytes(), ResponseStatus.Success);
        }

        /// <summary>
        /// Handles a request from game server to get a profile
        /// </summary>
        /// <param name="message"></param>
        protected virtual void GameServerProfileRequestHandler(IIncommingMessage message)
        {
            if (!HasPermissionToEditProfiles(message.Peer))
            {
                message.Respond("Invalid permission level", ResponseStatus.Unauthorized);
                return;
            }

            var username = message.AsString();

            ObservableServerProfile profile;
            ProfilesList.TryGetValue(username, out profile);

            if (profile == null)
            {
                message.Respond(ResponseStatus.Failed);
                return;
            }

            message.Respond(profile.ToBytes(), ResponseStatus.Success);
        }

        #endregion
    }
}