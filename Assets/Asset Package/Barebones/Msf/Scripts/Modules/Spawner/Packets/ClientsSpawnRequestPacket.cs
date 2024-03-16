using Barebones.Networking;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barebones.MasterServer
{
    public class ClientsSpawnRequestPacket : SerializablePacket
    {
        public DictionaryOptions Options { get; set; }
        public DictionaryOptions CustomOptions { get; set; }

        public ClientsSpawnRequestPacket()
        {
            Options = new DictionaryOptions();
            CustomOptions = new DictionaryOptions();
        }

        public override void ToBinaryWriter(EndianBinaryWriter writer)
        {
            writer.Write(Options.ToDictionary());
            writer.WriteDictionary(CustomOptions.ToDictionary());
        }

        public override void FromBinaryReader(EndianBinaryReader reader)
        {
            Options = new DictionaryOptions(reader.ReadDictionary());
            CustomOptions = new DictionaryOptions(reader.ReadDictionary());
        }

        public override string ToString()
        {
            var options = new DictionaryOptions(Options);

            if (options.IsValueEmpty(MsfDictKeys.region))
            {
                options.Set(MsfDictKeys.region, "International");
            }

            return options.ToReadableString() + " " + CustomOptions.ToReadableString();
        }
    }
}