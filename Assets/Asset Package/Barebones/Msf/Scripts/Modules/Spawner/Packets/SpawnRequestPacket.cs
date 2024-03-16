using Barebones.Networking;
using System.Collections.Generic;

namespace Barebones.MasterServer
{
    public class SpawnRequestPacket : SerializablePacket
    {
        public int SpawnerId { get; set; }
        public int SpawnTaskId { get; set; }
        public string SpawnTaskUniqueCode { get; set; } = string.Empty;
        public string OverrideExePath { get; set; } = string.Empty;
        public DictionaryOptions Options { get; set; }
        public DictionaryOptions CustomOptions { get; set; }

        public SpawnRequestPacket()
        {
            Options = new DictionaryOptions();
            CustomOptions = new DictionaryOptions();
        }

        public override void ToBinaryWriter(EndianBinaryWriter writer)
        {
            writer.Write(SpawnerId);
            writer.Write(SpawnTaskId);
            writer.Write(SpawnTaskUniqueCode);
            writer.Write(OverrideExePath);
            writer.Write(Options.ToDictionary());
            writer.Write(CustomOptions.ToDictionary());
        }

        public override void FromBinaryReader(EndianBinaryReader reader)
        {
            SpawnerId = reader.ReadInt32();
            SpawnTaskId = reader.ReadInt32();
            SpawnTaskUniqueCode = reader.ReadString();
            OverrideExePath = reader.ReadString();
            Options = new DictionaryOptions(reader.ReadDictionary());
            CustomOptions = new DictionaryOptions(reader.ReadDictionary());
        }
    }
}