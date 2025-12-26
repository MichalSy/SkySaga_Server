namespace SkySaga.Game.Packets;

public class MapDefinition : ISerializablePacket
{
    /// <summary>
    /// Map size in chunks (x, y, z). Max 32,32,32
    /// </summary>
    public Vector3Int MapSizeChunks;

    /// <summary>
    /// Crc32 Hash of biome name
    /// </summary>
    /// <remarks>
    /// <c>GeoData.json > Biomes > Name</c>
    /// </remarks>
    public uint? BiomeType;

    /// <summary>
    /// 1 - 
    /// 2 - 
    /// 3 - 
    /// 4 - 
    /// </summary>
    public int GameMode;

    public BitStream Serialize()
    {
        var bitStream = new BitStream();

        bitStream.WritePacketId(PacketId.MapDefinition);

        bitStream.WriteBits(BitConverter.GetBytes(MapSizeChunks.X), 32 - Util.NumBitsRequiredUInt32(32), true);
        bitStream.WriteBits(BitConverter.GetBytes(MapSizeChunks.Y), 32 - Util.NumBitsRequiredUInt32(32), true);
        bitStream.WriteBits(BitConverter.GetBytes(MapSizeChunks.Z), 32 - Util.NumBitsRequiredUInt32(32), true);

        bitStream.WriteOptional(BiomeType, (value) =>
        {
            bitStream.Write((int)value);
        });

        bitStream.WriteBits(BitConverter.GetBytes(GameMode), 32 - Util.NumBitsRequiredUInt32(4), true);

        return bitStream;
    }
}