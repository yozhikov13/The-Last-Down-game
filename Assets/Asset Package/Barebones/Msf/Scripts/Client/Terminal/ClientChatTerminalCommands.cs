﻿using Barebones.Logging;
using Barebones.MasterServer;
using CommandTerminal;
using System;
using System.Text;
using UnityEngine;

namespace Barebones.Client.Utilities
{
    public static class ClientChatTerminalCommands
    {
        static string tempMessage = string.Empty;

        [RegisterCommand(Name = "client.chat.msg", Help = "Send the chat message to all clients. 1 Message", MinArgCount = 1)]
        private static void SendMessage(CommandArg[] args)
        {
            tempMessage = Msf.Helper.JoinCommandArgs(args, 1);
            Msf.Client.Chat.SendPrivateMessage(args[0].String, tempMessage, OnSuccess);
        }

        [RegisterCommand(Name = "client.chat.msgto", Help = "Send the chat message to client. 1 Username, 2 Message", MinArgCount = 2)]
        private static void SendPrivateMessage(CommandArg[] args)
        {
            tempMessage = Msf.Helper.JoinCommandArgs(args, 1);
            Msf.Client.Chat.SendPrivateMessage(args[0].String, tempMessage, OnSuccess);
        }

        private static void OnSuccess(bool isSuccessful, string error)
        {
            if (isSuccessful)
            {
                Logs.Info($"Message: {tempMessage}");
            }
        }
    }
}