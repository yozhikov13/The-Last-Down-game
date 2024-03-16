using Barebones.MasterServer;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barebones.Bridges.Mirror
{
    public enum RoomAlertMessageCode
    {
        Default, Warning, Error
    }

    public class RoomAlertMessage : IMessageBase
    {
        public string Message { get; set; }
        public RoomAlertMessageCode Code { get; set; }

        public void Deserialize(NetworkReader reader)
        {
            Message = reader.ReadString();
            Code = (RoomAlertMessageCode)reader.ReadUInt16();
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteString(Message);
            writer.WriteUInt16((ushort)Code);
        }

        public override string ToString()
        {
            var options = new DictionaryOptions();
            options.Add("Code", Code.ToString());
            options.Add("Message", Message);

            return options.ToReadableString();
        }
    }
}