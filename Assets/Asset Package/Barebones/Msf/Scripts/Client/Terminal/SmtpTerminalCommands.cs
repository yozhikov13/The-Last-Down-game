﻿using Barebones.MasterServer;
using CommandTerminal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barebones.Client.Utilities
{
    public class SmtpTerminalCommands
    {
        private void Start()
        {
            Terminal.Shell.AddCommand("smtp.send", ClientAuthSignInAsGuest, 1, 1, "Sends E-Mail message to given address");
        }

        [RegisterCommand(Name = "master.smtp.send", Help = "Sends E-Mail message to given address. 1 Email, 2 Message", MinArgCount = 2)]
        private static void ClientAuthSignInAsGuest(CommandArg[] args)
        {
            var mailer = Object.FindObjectOfType<Mailer>();
            var message = Msf.Helper.JoinCommandArgs(args, 1);
            mailer.SendMail(args[0].String, "Test Message", message);
        }
    }
}