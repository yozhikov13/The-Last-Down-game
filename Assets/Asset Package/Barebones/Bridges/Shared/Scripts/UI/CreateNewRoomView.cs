using Aevien.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Barebones.Games
{
    public class CreateNewRoomView : UIView
    {
        private TMP_InputField roomNameInputField;
        private TMP_InputField roomMaxConnectionsInputField;
        private TMP_InputField roomRegionNameInputField;
        private TMP_InputField roomPasswordInputField;

        protected override void Start()
        {
            base.Start();

            roomNameInputField = ChildComponent<TMP_InputField>("roomNameInputField");
            roomMaxConnectionsInputField = ChildComponent<TMP_InputField>("roomMaxConnectionsInputField");
            roomRegionNameInputField = ChildComponent<TMP_InputField>("roomRegionNameInputField");
            roomPasswordInputField = ChildComponent<TMP_InputField>("roomPasswordInputField");
        }

        public string RoomName
        {
            get
            {
                return roomNameInputField != null ? roomNameInputField.text : string.Empty;
            }

            set
            {
                if (roomNameInputField)
                    roomNameInputField.text = value;
            }
        }

        public string MaxConnections
        {
            get
            {
                return roomMaxConnectionsInputField != null ? roomMaxConnectionsInputField.text : string.Empty;
            }

            set
            {
                if (roomMaxConnectionsInputField)
                    roomMaxConnectionsInputField.text = value;
            }
        }

        public string RegionName
        {
            get
            {
                return roomRegionNameInputField != null ? roomRegionNameInputField.text : string.Empty;
            }

            set
            {
                if (roomRegionNameInputField)
                    roomRegionNameInputField.text = value;
            }
        }

        public string Password
        {
            get
            {
                return roomPasswordInputField != null ? roomPasswordInputField.text : string.Empty;
            }

            set
            {
                if (roomPasswordInputField)
                    roomPasswordInputField.text = value;
            }
        }
    }
}