using Barebones.MasterServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Barebones.MasterServer.Examples.BasicAuthorization;

#if (!UNITY_WEBGL && !UNITY_IOS) || UNITY_EDITOR
using LiteDB;
#endif

namespace Barebones.MasterServer.Examples.BasicProfile
{
    public class DatabaseFactoryModule : BaseServerModule
    {
        public HelpBox _header = new HelpBox()
        {
            Text = "This script is a custom module, which sets up database accessors for the game"
        };

        public override void Initialize(IServer server)
        {
#if (!UNITY_WEBGL && !UNITY_IOS) || UNITY_EDITOR
            try
            {
                Msf.Server.DbAccessors.SetAccessor<IAccountsDatabaseAccessor>(new AccountsDatabaseAccessor(new LiteDatabase(@"accounts.db")));
                Msf.Server.DbAccessors.SetAccessor<IProfilesDatabaseAccessor>(new ProfilesDatabaseAccessor(new LiteDatabase(@"profiles.db")));
            }
            catch (Exception e)
            {
                logger.Error("Failed to setup LiteDB");
                logger.Error(e);
            }
#endif
        }
    }
}