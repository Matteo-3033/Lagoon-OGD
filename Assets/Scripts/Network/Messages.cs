using MasterServerToolkit.Extensions;
using MasterServerToolkit.Networking;
using Mirror;

namespace Network.Messages
{
    public struct OpCodes
    {
        public static ushort GetMatch = nameof(GetMatch).ToUint16Hash();   
    }
    
    public struct ValidateRoomAccessResultMessage : NetworkMessage
    {
        public string Error { get; set; }
        public ResponseStatus Status { get; set; }
    }

    public static class ValidateRoomAccessResultMessageExtension
    {
        public static void Serialize(this NetworkWriter writer, ValidateRoomAccessResultMessage value)
        {
            writer.WriteString(value.Error);
            writer.WriteInt((int)value.Status);
        }

        public static ValidateRoomAccessResultMessage Deserialize(this NetworkReader reader)
        {
            ValidateRoomAccessResultMessage value = new ValidateRoomAccessResultMessage()
            {
                Error = reader.ReadString(),
                Status = (ResponseStatus)reader.ReadInt()
            };

            return value;
        }
    }
    
    public struct ValidateRoomAccessRequestMessage : NetworkMessage
    {
        public string Token { get; set; }
    }

    public static class ValidateRoomAccessRequestMessageExtension
    {
        public static void Serialize(this NetworkWriter writer, ValidateRoomAccessRequestMessage value)
        {
            writer.WriteString(value.Token);
        }

        public static ValidateRoomAccessRequestMessage Deserialize(this NetworkReader reader)
        {
            ValidateRoomAccessRequestMessage value = new ValidateRoomAccessRequestMessage()
            {
                Token = reader.ReadString()
            };

            return value;
        }
    }
}