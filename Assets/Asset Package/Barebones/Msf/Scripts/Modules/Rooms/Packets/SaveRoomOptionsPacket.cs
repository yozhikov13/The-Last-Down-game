using Barebones.Networking;

namespace Barebones.MasterServer
{
    public class SaveRoomOptionsPacket : SerializablePacket
    {
        public int RoomId;
        public RoomOptions Options;

        public override void ToBinaryWriter(EndianBinaryWriter writer)
        {
            writer.Write(RoomId);
            writer.Write(Options);
        }

        public override void FromBinaryReader(EndianBinaryReader reader)
        {
            RoomId = reader.ReadInt32();
            Options = reader.ReadPacket(new RoomOptions());
        }
    }
}