using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Barebones.Games
{
    public class OkDialogBoxViewEventMessage
    {
        public OkDialogBoxViewEventMessage() { }

        public OkDialogBoxViewEventMessage(string message)
        {
            Message = message;
            OkCallback = null;
        }

        public OkDialogBoxViewEventMessage(string message, UnityAction okCallback)
        {
            Message = message;
            OkCallback = okCallback;
        }

        public string Message { get; set; }
        public UnityAction OkCallback { get; set; }
    }
}
