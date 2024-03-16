using Aevien.UI;
using Barebones.MasterServer;
using UnityEngine;

namespace Barebones.Games
{
    [RequireComponent(typeof(UIView))]
    public class OkDialogBoxView : PopupViewComponent
    {
        public override void OnOwnerStart()
        {
            Msf.Events.AddEventListener(MsfEventKeys.showOkDialogBox, OnShowOkDialogBoxEventHandler);
            Msf.Events.AddEventListener(MsfEventKeys.hideOkDialogBox, OnHideOkDialogBoxEventHandler);
        }

        private void OnShowOkDialogBoxEventHandler(EventMessage message)
        {
            var messageData = message.GetData<OkDialogBoxViewEventMessage>();

            SetLables(messageData.Message);

            if (messageData.OkCallback != null)
            {
                SetButtonsClick(messageData.OkCallback);
            }
            else
            {
                SetButtonsClick(() =>
                {
                    Owner.Hide();
                });
            }

            Owner.Show();
        }

        private void OnHideOkDialogBoxEventHandler(EventMessage message)
        {
            Owner.Hide();
        }
    }
}
