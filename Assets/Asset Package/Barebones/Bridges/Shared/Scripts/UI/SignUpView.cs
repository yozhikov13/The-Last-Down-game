using Aevien.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Barebones.Games
{
    public class SignUpView : UIView
    {
        private TMP_InputField usernameInputField;
        private TMP_InputField emailInputField;
        private TMP_InputField passwordInputField;
        private TMP_InputField confirmPasswordInputField;

        public string Username
        {
            get
            {
                return usernameInputField != null ? usernameInputField.text : string.Empty;
            }
        }

        public string Email
        {
            get
            {
                return emailInputField != null ? emailInputField.text : string.Empty;
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
            emailInputField = ChildComponent<TMP_InputField>("emailInputField");
            passwordInputField = ChildComponent<TMP_InputField>("passwordInputField");
            confirmPasswordInputField = ChildComponent<TMP_InputField>("confirmPasswordInputField");
        }

        public void SetInputFieldsValues(string username, string email, string password)
        {
            usernameInputField.text = username;
            emailInputField.text = email;
            passwordInputField.text = password;
            confirmPasswordInputField.text = password;
        }
    }
}