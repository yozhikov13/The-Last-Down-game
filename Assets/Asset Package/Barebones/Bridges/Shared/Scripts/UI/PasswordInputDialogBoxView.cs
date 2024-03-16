using Aevien.UI;
using Barebones.MasterServer;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Barebones.Games
{
    [RequireComponent(typeof(UIView))]
    public class PasswordInputDialogBoxView : PopupViewComponent
    {
        private TMP_InputField passwordInputField;
        private UnityAction submitCallback;

        public override void OnOwnerStart()
        {
            passwordInputField = Owner.ChildComponent<TMP_InputField>("passwordInputField");
            Msf.Events.AddEventListener(MsfEventKeys.showPasswordDialogBox, OnShowPasswordDialogBoxEventHandler);
            Msf.Events.AddEventListener(MsfEventKeys.hidePasswordDialogBox, OnHidePasswordDialogBoxEventHandler);
        }

        private void OnHidePasswordDialogBoxEventHandler(EventMessage message)
        {
            Owner.Hide();
        }

        private void OnShowPasswordDialogBoxEventHandler(EventMessage message)
        {
            var messageData = message.GetData<PasswordInputDialoxBoxEventMessage>();

            SetLables(messageData.Message);
            submitCallback = messageData.OkCallback;
            Owner.Show();
        }

        public void Submit()
        {
            Msf.Options.Set(MsfDictKeys.roomPassword, passwordInputField.text);
            submitCallback?.Invoke();
            Owner.Hide();
        }
    }
}