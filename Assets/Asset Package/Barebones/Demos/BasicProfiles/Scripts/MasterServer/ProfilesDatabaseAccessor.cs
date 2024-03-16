#if (!UNITY_WEBGL && !UNITY_IOS) || UNITY_EDITOR

using Barebones.Logging;
using Barebones.MasterServer;
using LiteDB;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Barebones.MasterServer.Examples.BasicProfile
{
    public class ProfilesDatabaseAccessor : IProfilesDatabaseAccessor
    {
        private readonly ILiteCollection<ProfileInfoData> profiles;
        private readonly ILiteDatabase database;

        public ProfilesDatabaseAccessor(LiteDatabase database)
        {
            this.database = database;

            profiles = this.database.GetCollection<ProfileInfoData>("profiles");
            profiles.EnsureIndex(a => a.Username, true);
        }

        /// <summary>
        /// Get profile info from database
        /// </summary>
        /// <param name="profile"></param>
        public void RestoreProfile(ObservableServerProfile profile)
        {
            var data = FindOrCreateData(profile);
            profile.FromBytes(data.Data);
        }

        /// <summary>
        /// Get profile info from database
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="callback"></param>
        public async void RestoreProfileAsync(ObservableServerProfile profile, SuccessCallback callback)
        {
            try
            {
                await Task.Run(() => RestoreProfile(profile));
                callback?.Invoke(true, string.Empty);
            }
            catch (Exception e)
            {
                callback?.Invoke(false, e.Message);
            }
        }

        /// <summary>
        /// Update profile info in database
        /// </summary>
        /// <param name="profile"></param>
        public void UpdateProfile(ObservableServerProfile profile)
        {
            var data = FindOrCreateData(profile);
            data.Data = profile.ToBytes();
            profiles.Update(data);
        }

        /// <summary>
        /// Update profile info in database
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="callback"></param>
        public async void UpdateProfileAsync(ObservableServerProfile profile, SuccessCallback callback)
        {
            try
            {
                await Task.Run(() => UpdateProfile(profile));
                callback?.Invoke(true, string.Empty);
            }
            catch (Exception e)
            {
                callback?.Invoke(false, e.Message);
            }
        }

        /// <summary>
        /// Find profile data in database or create new data and insert them to database
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        private ProfileInfoData FindOrCreateData(ObservableServerProfile profile)
        {
            string username = profile.Username;
            var data = profiles.FindOne(a => a.Username == username);

            if (data == null)
            {
                data = new ProfileInfoData()
                {
                    Username = profile.Username,
                    Data = profile.ToBytes()
                };

                profiles.Insert(data);
            }

            return data;
        }

        /// <summary>
        /// LiteDB profile data implementation
        /// </summary>
        private class ProfileInfoData
        {
            [BsonId]
            public string Username { get; set; }
            public byte[] Data { get; set; }
        }
    }
}

#endif