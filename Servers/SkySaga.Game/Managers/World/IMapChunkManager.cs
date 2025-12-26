namespace SkySaga.Game.Managers.World;

/// <summary>
/// Manages chunk loading, unloading, and voxel access.
/// </summary>
public interface IMapChunkManager
{
    /// <summary>
    /// Attempts to get a chunk at the specified world chunk coordinates.
    /// </summary>
    bool TryGetChunk(Vector3Int chunkPosition, [NotNullWhen(true)] out WorldChunk? chunk);

    /// <summary>
    /// Gets a chunk at the specified coordinates, creating it if necessary.
    /// </summary>
    WorldChunk GetOrCreateChunk(Vector3Int chunkPosition);

    /// <summary>
    /// Sets a chunk at the specified coordinates.
    /// </summary>
    void SetChunk(Vector3Int chunkPosition, WorldChunk chunk);

    /// <summary>
    /// Removes a chunk from memory.
    /// </summary>
    void RemoveChunk(Vector3Int chunkPosition);

    /// <summary>
    /// Gets the voxel at the specified position.
    /// </summary>
    byte GetVoxel(Vector3Int chunkPosition, Vector3Int voxelPosition);

    /// <summary>
    /// Sets the voxel at the specified position.
    /// </summary>
    void SetVoxel(Vector3Int chunkPosition, Vector3Int voxelPosition, byte blockType, byte metaValue = 0x00);

    /// <summary>
    /// Gets all currently loaded chunks.
    /// </summary>
    IEnumerable<WorldChunk> GetLoadedChunks();

    /// <summary>
    /// Generates initial chunk data (hardcoded for now).
    /// </summary>
    void GenerateInitialChunks();

    /// <summary>
    /// Clears all chunks.
    /// </summary>
    void ClearAll();

    /// <summary>
    /// Converts a WorldChunk to ChunkSync packet format.
    /// </summary>
    ChunkSync GetChunkSyncData(Vector3Int chunkPosition);

    /// <summary>
    /// Gets all loaded chunks as ChunkSync packets.
    /// </summary>
    IEnumerable<ChunkSync> GetAllChunkSyncData();
}
