using System.IO;
using System.Text;

namespace SkySaga.Game.Packets;

public class SendChatChannelData : ISerializablePacket
{
    public BitStream Serialize()
    {
        var bitStream = new BitStream();

        bitStream.WritePacketId(PacketId.SendChatChannelData);

        bitStream.WriteBits(BitConverter.GetBytes(8), 32 - Util.NumBitsRequiredUInt32(8));
        bitStream.Write1();
        bitStream.Write(1); // Number of channels


        bitStream.WriteBits(BitConverter.GetBytes(1), 32 - Util.NumBitsRequiredUInt32(8));
        bitStream.WriteString("WorldChat");
        bitStream.Write1(); // Unknown Status flag


        bitStream.WriteString("System"); // unknown
        bitStream.WriteString("Global"); // unknown

        return bitStream;
    }
}
