using Barebones.Networking;

namespace Barebones.MasterServer
{
    public class KillSpawnedProcessRequestPacket : SerializablePacket
    {
        public int SpawnerId { get; set; }
        public int SpawnId { get; set; }

        public override void ToBinaryWriter(EndianBinaryWriter writer)
        {
            writer.Write(SpawnerId);
            writer.Write(SpawnId);
        }

        public override void FromBinaryReader(EndianBinaryReader reader)
        {
            SpawnerId = reader.ReadInt32();
            SpawnId = reader.ReadInt32();
        }
    }
}