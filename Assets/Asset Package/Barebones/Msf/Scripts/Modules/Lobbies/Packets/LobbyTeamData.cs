using Barebones.Networking;
using System.Collections.Generic;

namespace Barebones.MasterServer
{
    public class LobbyTeamData : SerializablePacket
    {
        public string Name { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public override void ToBinaryWriter(EndianBinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(MinPlayers);
            writer.Write(MaxPlayers);
            writer.WriteDictionary(Properties);
        }

        public override void FromBinaryReader(EndianBinaryReader reader)
        {
            Name = reader.ReadString();
            MinPlayers = reader.ReadInt32();
            MaxPlayers = reader.ReadInt32();
            Properties = reader.ReadDictionary();
        }
    }
}