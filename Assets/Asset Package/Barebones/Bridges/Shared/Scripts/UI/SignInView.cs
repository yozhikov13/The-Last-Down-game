using Aevien.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Barebones.Games
{
    public class SignInView : UIView
    {
        private TMP_InputField usernameInputField;
        private TMP_InputField passwordInputField;

        public string Username
        {
            get
            {
                return usernameInputField != null ? usernameInputField.text : string.Empty;
            }
        }

        public string Password
        {
            get
            {
                return passwordInputField != null ? passwordInputField.text : string.Empty;
            }
        }

        protected override void Start()
        {
            base.Start();

            usernameInputField = ChildComponent<TMP_InputField>("usernameInputField");
            passwordInputField = ChildComponent<TMP_InputField>("passwordInputField");
        }

        public void SetInputFieldsValues(string username, string password)
        {
            if (usernameInputField)
                usernameInputField.text = username;

            if (passwordInputField)
                passwordInputField.text = password;
        }
    }
}