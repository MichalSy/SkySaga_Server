namespace SkySaga.Game.Packets.Common;

public class DamagedVoxel : ISerializableType
{
    public Vector3Int VoxelPosition;

    public int Unknown;

    public ItemSpec ItemSpec = ItemSpec.Empty;

    public void Serialize(BitStream bitStream)
    {
        bitStream.WriteBits(BitConverter.GetBytes(VoxelPosition.X), 32 - Util.NumBitsRequiredUInt32(1024), true);
        bitStream.WriteBits(BitConverter.GetBytes(VoxelPosition.Y), 32 - Util.NumBitsRequiredUInt32(1024), true);
        bitStream.WriteBits(BitConverter.GetBytes(VoxelPosition.Z), 32 - Util.NumBitsRequiredUInt32(1024), true);

        bitStream.WriteBits(BitConverter.GetBytes(Unknown), 32 - Util.NumBitsRequiredUInt32(512), true);

        ItemSpec.Serialize(bitStream);
    }
}
