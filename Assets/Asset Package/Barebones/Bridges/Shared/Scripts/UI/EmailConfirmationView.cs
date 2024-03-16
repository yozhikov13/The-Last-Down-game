using Aevien.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Barebones.Games
{
    public class EmailConfirmationView : UIView
    {
        private TMP_InputField confirmationCodeInputField;

        public string ConfirmationCode
        {
            get
            {
                return confirmationCodeInputField != null ? confirmationCodeInputField.text : string.Empty;
            }
        }

        protected override void Start()
        {
            base.Start();
            confirmationCodeInputField = ChildComponent<TMP_InputField>("confirmationCodeInputField");
        }
    }
}