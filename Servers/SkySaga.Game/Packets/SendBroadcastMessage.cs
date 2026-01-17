namespace SkySaga.Game.Packets;

public class SendBroadcastMessage : ISerializablePacket
{
    public required string Message { get; init; }

    public BitStream Serialize()
    {
        var bitStream = new BitStream();

        bitStream.WritePacketId(PacketId.SendBroadcastMessage);

        bitStream.WriteString(Message);

        return bitStream;
    }
}