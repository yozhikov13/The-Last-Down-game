using Aevien.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Barebones.Games
{
    public class PasswordResetView : UIView
    {
        private TMP_InputField resetCodeInputField;
        private TMP_InputField newPasswordInputField;
        private TMP_InputField newPasswordConfirmInputField;

        public string ResetCode
        {
            get
            {
                return resetCodeInputField != null ? resetCodeInputField.text : string.Empty;
            }
        }

        public string NewPassword
        {
            get
            {
                return newPasswordInputField != null ? newPasswordInputField.text : string.Empty;
            }
        }

        public string NewPasswordConfirm
        {
            get
            {
                return newPasswordConfirmInputField != null ? newPasswordConfirmInputField.text : string.Empty;
            }
        }

        protected override void Start()
        {
            base.Start();
            resetCodeInputField = ChildComponent<TMP_InputField>("resetCodeInputField");
            newPasswordInputField = ChildComponent<TMP_InputField>("newPasswordInputField");
            newPasswordConfirmInputField = ChildComponent<TMP_InputField>("newPasswordConfirmInputField");
        }
    }
}