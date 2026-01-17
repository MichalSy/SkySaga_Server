namespace SkySaga.Game.Components;

public class ClientVoxelLinkComponent : Component
{
    public List<VoxelLinkItemDTO> Voxels { get; set => SetIfChanged(ref field, value); } = [];
    public int CanReplaceVoxelsOfEntityID { get; set => SetIfChanged(ref field, value); } = 0;

    public override bool TrySync(string parameterName, BitStream bitStream)
    {
        if (parameterName.Equals(nameof(CanReplaceVoxelsOfEntityID), StringComparison.OrdinalIgnoreCase))
        {
            if (CanReplaceVoxelsOfEntityID > 0)
            {
                bitStream.WriteInt32(CanReplaceVoxelsOfEntityID);
                return true;
            }
        }
        else if (parameterName.Equals(nameof(Voxels), StringComparison.OrdinalIgnoreCase))
        {
            if (Voxels.Count < 1)
                return false;

            bitStream.WriteInt32(199, 199);

            bitStream.Write1();
            bitStream.WriteInt32(Voxels.Count);

            foreach (var currentVoxel in Voxels)
            {
                bitStream.WriteInt32(currentVoxel.VoxelCoord.X + 15, 30);
                bitStream.WriteInt32(currentVoxel.VoxelCoord.Y + 15, 30);
                bitStream.WriteInt32(currentVoxel.VoxelCoord.Z + 15, 30);

                bitStream.Write(currentVoxel.VoxelId);
            }

            return true;
        }


        return false;
    }
}
