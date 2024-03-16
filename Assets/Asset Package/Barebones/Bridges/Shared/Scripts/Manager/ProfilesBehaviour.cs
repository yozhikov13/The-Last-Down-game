using Barebones.MasterServer;
using Barebones.MasterServer.Examples.BasicProfile;
using Barebones.Networking;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Barebones.Games
{
    [AddComponentMenu("MSF/Shared/ProfilesBehaviour")]
    public class ProfilesManager : BaseClientBehaviour
    {
        #region INSPECTOR

        /// <summary>
        /// Invokes when profile is loaded
        /// </summary>
        public UnityEvent OnProfileLoadedEvent;

        /// <summary>
        /// Invokes when profile is not loaded successfully
        /// </summary>
        public UnityEvent OnProfileLoadFailedEvent;

        #endregion

        /// <summary>
        /// The loaded profile of client
        /// </summary>
        public ObservableProfile Profile { get; protected set; }

        protected override void OnInitialize()
        {
            Profile = new ObservableProfile();
        }

        /// <summary>
        /// Invokes when user profile is loaded
        /// </summary>
        public virtual void OnProfileLoaded() { }

        /// <summary>
        /// Get profile data from master
        /// </summary>
        public void LoadProfile()
        {
            Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, "Loading profile... Please wait!");

            MsfTimer.WaitForSeconds(1f, () =>
            {
                Msf.Client.Profiles.GetProfileValues(Profile, (isSuccessful, error) =>
                {
                    Msf.Events.Invoke(MsfEventKeys.hideLoadingInfo);

                    if (isSuccessful)
                    {
                        OnProfileLoadedEvent?.Invoke();
                    }
                    else
                    {
                        logger.Error("Could not load user profile");
                        OnProfileLoadFailedEvent?.Invoke();
                    }
                });
            });
        }
    }
}