using Aevien.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Barebones.Games
{
    public class PasswordResetCodeView : UIView
    {
        private TMP_InputField emailInputField;

        public string Email
        {
            get
            {
                return emailInputField != null ? emailInputField.text : string.Empty;
            }
        }

        protected override void Start()
        {
            base.Start();
            emailInputField = ChildComponent<TMP_InputField>("emailInputField");
        }
    }
}
