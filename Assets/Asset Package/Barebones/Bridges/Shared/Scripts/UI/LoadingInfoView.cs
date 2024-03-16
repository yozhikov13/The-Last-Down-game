using Aevien.UI;
using Barebones.MasterServer;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Barebones.Games
{
    [RequireComponent(typeof(UIView))]
    public class LoadingInfoView : PopupViewComponent
    {
        public override void OnOwnerStart()
        {
            Msf.Events.AddEventListener(MsfEventKeys.showLoadingInfo, OnShowLoadingInfoEventHandler);
            Msf.Events.AddEventListener(MsfEventKeys.hideLoadingInfo, OnHideLoadingInfoEventHandler);
        }

        private void OnShowLoadingInfoEventHandler(EventMessage message)
        {
            SetLables(message.GetData<string>());
            Owner.Show();
        }

        private void OnHideLoadingInfoEventHandler(EventMessage message)
        {
            Owner.Hide();
        }
    }
}