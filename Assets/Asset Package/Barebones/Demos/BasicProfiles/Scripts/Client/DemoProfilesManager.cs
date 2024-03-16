using Aevien.UI;
using Barebones.Games;
using Barebones.MasterServer.Examples.BasicProfile;
using Barebones.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Barebones.MasterServer.Examples.BasicProfile
{
    public class DemoProfilesManager : ProfilesManager
    {
        private ProfileView profileView;
        private ProfileSettingsView profileSettingsView;

        public event Action<short, IObservableProperty> OnPropertyUpdatedEvent;
        public UnityEvent OnProfileSavedEvent;

        protected override void OnInitialize()
        {
            profileView = ViewsManager.GetView<ProfileView>("ProfileView");
            profileSettingsView = ViewsManager.GetView<ProfileSettingsView>("ProfileSettingsView");

            Profile = new ObservableProfile
            {
                new ObservableString((short)ObservablePropertiyCodes.DisplayName),
                new ObservableString((short)ObservablePropertiyCodes.Avatar),
                new ObservableFloat((short)ObservablePropertiyCodes.Bronze),
                new ObservableFloat((short)ObservablePropertiyCodes.Silver),
                new ObservableFloat((short)ObservablePropertiyCodes.Gold)
            };

            Profile.OnPropertyUpdatedEvent += OnPropertyUpdatedEventHandler;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Profile.OnPropertyUpdatedEvent -= OnPropertyUpdatedEventHandler;
        }

        private void OnPropertyUpdatedEventHandler(short key, IObservableProperty property)
        {
            OnPropertyUpdatedEvent?.Invoke(key, property);

            logger.Debug($"Property with code: {key} were updated: {property.Serialize()}");
        }

        public void UpdateProfile()
        {
            Msf.Events.Invoke(MsfEventKeys.showLoadingInfo, "Saving profile data... Please wait!");

            MsfTimer.WaitForSeconds(1f, () =>
            {
                var data = new Dictionary<string, string>
                {
                    { "displayName", profileSettingsView.DisplayName },
                    { "avatarUrl", profileSettingsView.AvatarUrl }
                };

                Connection.SendMessage((short)MsfMessageCodes.UpdateDisplayNameRequest, data.ToBytes(), OnSaveProfileResponseCallback);
            });
        }

        private void OnSaveProfileResponseCallback(ResponseStatus status, IIncommingMessage response)
        {
            Msf.Events.Invoke(MsfEventKeys.hideLoadingInfo);

            if(status == ResponseStatus.Success)
            {
                OnProfileSavedEvent?.Invoke();

                logger.Debug("Your profile is successfuly updated and saved");
            }
            else
            {
                Msf.Events.Invoke(MsfEventKeys.showOkDialogBox, new OkDialogBoxViewEventMessage(response.AsString()));
                logger.Error(response.AsString());
            }
        }
    }
}
