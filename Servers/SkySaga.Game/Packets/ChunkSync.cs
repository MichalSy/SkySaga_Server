namespace SkySaga.Game.Packets;

public class ChunkSync : ISerializablePacket
{
    public Vector3Int Coords;

    public byte[]? Data1;
    public byte[]? Data2;

    public byte? AdjacentChunks;

    public BitStream Serialize()
    {
        var bitStream = new BitStream();

        bitStream.WritePacketId(PacketId.ChunkSync);

        bitStream.WriteBits(BitConverter.GetBytes(Coords.X), 32 - Util.NumBitsRequiredUInt32(32), true);
        bitStream.WriteBits(BitConverter.GetBytes(Coords.Y), 32 - Util.NumBitsRequiredUInt32(32), true);
        bitStream.WriteBits(BitConverter.GetBytes(Coords.Z), 32 - Util.NumBitsRequiredUInt32(32), true);

        bitStream.WriteOptional(Data1, (value) =>
        {
            bitStream.Write(value.Length);
            bitStream.WriteAlignedBytes(value, (uint)value.Length);
        });

        bitStream.WriteOptional(Data2, (value) =>
        {
            bitStream.Write(value.Length);
            bitStream.WriteAlignedBytes(value, (uint)value.Length);
        });

        bitStream.WriteOptional(AdjacentChunks, (value) =>
        {
            bitStream.WriteBits([value], 8 - Util.NumBitsRequiredByte(64), true);
        });

        return bitStream;
    }
}