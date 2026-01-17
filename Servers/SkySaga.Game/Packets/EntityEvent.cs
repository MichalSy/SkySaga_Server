namespace SkySaga.Game.Packets;

public class EntityEvent : ISerializablePacket
{
    public byte EventId { get; set; }
    public int EntityId { get; set; }
    public int? UnknownParameter { get; set; }

    public BitStream Serialize()
    {
        var bitStream = new BitStream();

        bitStream.WritePacketId(PacketId.EntityEvent);

        // EventId is required, max value 96
        bitStream.WriteByte(EventId, 96);

        // EntityId is required
        bitStream.Write(EntityId);

        // UnknownParameter is optional
        if (UnknownParameter.HasValue)
        {
            bitStream.Write1();
            bitStream.Write(UnknownParameter.Value);
        }
        else
        {
            bitStream.Write0();
        }

        return bitStream;
    }
}
