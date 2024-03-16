using Barebones.Networking;
using System.Collections.Generic;

namespace Barebones.MasterServer
{
    public class SpawnFinalizationPacket : SerializablePacket
    {
        public int SpawnTaskId { get; set; }
        public DictionaryOptions FinalizationData { get; set; }

        public override void ToBinaryWriter(EndianBinaryWriter writer)
        {
            writer.Write(SpawnTaskId);
            writer.Write(FinalizationData.ToDictionary());
        }

        public override void FromBinaryReader(EndianBinaryReader reader)
        {
            SpawnTaskId = reader.ReadInt32();
            FinalizationData = new DictionaryOptions(reader.ReadDictionary());
        }
    }
}