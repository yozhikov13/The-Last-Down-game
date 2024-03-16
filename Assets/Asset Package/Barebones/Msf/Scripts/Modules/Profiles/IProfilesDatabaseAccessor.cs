using System;

namespace Barebones.MasterServer
{
    /// <summary>
    /// Represents generic database for profiles
    /// </summary>
    public interface IProfilesDatabaseAccessor
    {
        /// <summary>
        /// Should restore all values of the given profile, 
        /// or not change them, if there's no entry in the database
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        void RestoreProfile(ObservableServerProfile profile);

        /// <summary>
        /// Should restore all values of the given profile, 
        /// or not change them, if there's no entry in the database
        /// </summary>
        /// <param name="profile"></param>
        void RestoreProfileAsync(ObservableServerProfile profile, SuccessCallback callback);

        /// <summary>
        /// Should save updated profile into database
        /// </summary>
        /// <param name="profile"></param>
        void UpdateProfile(ObservableServerProfile profile);

        /// <summary>
        /// Should save updated profile into database
        /// </summary>
        /// <param name="profile"></param>
        void UpdateProfileAsync(ObservableServerProfile profile, SuccessCallback callback);
    }
}