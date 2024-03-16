using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Barebones.MasterServer;
using Newtonsoft.Json;
using System.Reflection;
using System;

namespace GameData
{
    [DynamoDBTable("Profiles")]
    public class Profile
    {
        [DynamoDBHashKey]
        public string Username { get; set; }
        [DynamoDBProperty]
        public string Account { get; set; }
        [DynamoDBProperty]
        public Dictionary<int, WeaponSet> Sets { get; set; }
        [DynamoDBProperty]
        public List<Item> Inventory { get; set; }
        [DynamoDBProperty]
        public List<string> Characters { get; set; }
        [DynamoDBProperty]
        public int Experience { get; set; }
        [DynamoDBProperty]
        public int Money { get; set; }
        [DynamoDBProperty]
        public int PremiumMoney { get; set; }
        [DynamoDBProperty]
        public int KillsCount { get; set; }
        [DynamoDBProperty]
        public int DeathsCount { get; set; }
        [DynamoDBProperty]
        public int MathcesCount { get; set; }

        public Profile()
        {
            Experience = 0;
            Money = 0;
            PremiumMoney = 0;
            Inventory = new List<Item>();
            Characters = new List<string>();
            KillsCount = 0;
            DeathsCount = 0;
            MathcesCount = 0;
        }
    }

    public enum ProfileCodes
    {
        Account,
        Avatar2D,
        Avatar3D,
        Experience,
        Money,
        PremiumMoney,
        Sets,
        Inventory,
        Characters,
        KillsCount,
        DeathsCount,
        MatchesCount 
    }

    public class Item
    {
        public string Name { get; set; }
        public List<string> Improvements { get; set; }
    }

    public class WeaponSet
    {
        public string PrimaryWeapon { get; set; }
        public string SecondaryWeapon { get; set; }
        public string TertiaryWeapon { get; set; }
        public string Equipment { get; set; }
    }
}