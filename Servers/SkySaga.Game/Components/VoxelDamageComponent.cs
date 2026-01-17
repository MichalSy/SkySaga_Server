namespace SkySaga.Game.Components;

public class VoxelDamageComponent : Component
{
    private const int DamagedVoxelListDefaultCount = 6;
    public List<DamagedVoxel> DamagedVoxelList { get; set => SetIfChanged(ref field, value); } = [];

    public override bool TrySync(string parameterName, BitStream bitStream)
    {
        if (parameterName.Equals("DamagedVoxels", StringComparison.OrdinalIgnoreCase))
        {
            // Count is optimised
            if (DamagedVoxelList.Count < DamagedVoxelListDefaultCount)
            {
                bitStream.WriteBits(BitConverter.GetBytes(DamagedVoxelList.Count), 32 - Util.NumBitsRequiredUInt32(DamagedVoxelListDefaultCount), true);
            }
            else
            {
                bitStream.WriteBits(BitConverter.GetBytes(DamagedVoxelListDefaultCount), 32 - Util.NumBitsRequiredUInt32(DamagedVoxelListDefaultCount), true);

                bitStream.Write1();
                bitStream.Write(DamagedVoxelList.Count);
            }

            foreach (var damagedVoxel in DamagedVoxelList)
                damagedVoxel.Serialize(bitStream);

            return true;
        }

        return false;
    }
}
