using Barebones.Networking;
using System.Collections.Generic;

namespace Barebones.MasterServer
{
    public class PeerAccountInfoPacket : SerializablePacket
    {
        public int PeerId { get; set; }
        public string Username { get; set; }
        public DictionaryOptions CustomOptions { get; set; }

        public override void ToBinaryWriter(EndianBinaryWriter writer)
        {
            writer.Write(PeerId);
            writer.Write(Username);
            writer.Write(CustomOptions.ToDictionary());
        }

        public override void FromBinaryReader(EndianBinaryReader reader)
        {
            PeerId = reader.ReadInt32();
            Username = reader.ReadString();
            CustomOptions = new DictionaryOptions(reader.ReadDictionary());
        }

        public override string ToString()
        {
            return string.Format($"[Peer account info: Peer ID: {PeerId}, Username: {Username}, Properties: {CustomOptions.ToReadableString()}]");
        }
    }
}