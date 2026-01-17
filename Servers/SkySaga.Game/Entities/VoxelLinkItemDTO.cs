namespace SkySaga.Game.Entities;

public record VoxelLinkItemDTO
{
    public Vector3Int VoxelCoord { get; init; }
    public byte VoxelId { get; init; }
}
