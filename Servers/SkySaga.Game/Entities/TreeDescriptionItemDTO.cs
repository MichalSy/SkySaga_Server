namespace SkySaga.Game.Entities;

public record TreeDescriptionItemDTO
{
    public byte HasDestoryEffect { get; init; }
    public TreeDescriptionPositionOffset OffsetDirection { get; init; }
    public byte NextNodeIndex { get; init; }
    public bool DestroyOnAnyDamage { get; init; }
    public byte VoxelIndex { get; init; }
}
