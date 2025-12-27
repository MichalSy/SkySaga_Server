namespace SkySaga.Game.Data.Models;

/// <summary>
/// Database object representing a chunk in the WorldChunks table.
/// </summary>
public class WorldChunkDBO(
    Guid worldId,
    int chunkX,
    int chunkY,
    int chunkZ,
    byte[] voxelData,
    byte[] metadataBlob,
    DateTime lastModifiedAt)
{
    public Guid WorldId { get; init; } = worldId;
    public int ChunkX { get; init; } = chunkX;
    public int ChunkY { get; init; } = chunkY;
    public int ChunkZ { get; init; } = chunkZ;
    public byte[] VoxelData { get; init; } = voxelData;
    public byte[] MetadataBlob { get; init; } = metadataBlob;
    public DateTime LastModifiedAt { get; set; } = lastModifiedAt;

    public Vector3Int Position => new(ChunkX, ChunkY, ChunkZ);

    /// <summary>
    /// Converts this DB object to a WorldChunk.
    /// </summary>
    public WorldChunk ToWorldChunk()
    {
        var chunk = new WorldChunk(Position);
        chunk.SetVoxelData(VoxelData);
        chunk.SetMetadataData(MetadataBlob);
        return chunk;
    }

    /// <summary>
    /// Creates a DB object from a WorldChunk.
    /// </summary>
    public static WorldChunkDBO FromWorldChunk(Guid worldId, WorldChunk chunk) => new(
        worldId: worldId,
        chunkX: chunk.Position.X,
        chunkY: chunk.Position.Y,
        chunkZ: chunk.Position.Z,
        voxelData: chunk.GetVoxelData(),
        metadataBlob: chunk.GetMetadataForNetwork(),
        lastModifiedAt: DateTime.UtcNow
    );
}
