namespace SkySaga.Game.Data.Repositories;

/// <summary>
/// Repository interface for world and chunk persistence.
/// </summary>
public interface IWorldRepository
{
    // World operations
    Task<WorldDBO?> GetWorldByIdAsync(Guid worldId);
    Task<WorldDBO?> GetWorldByNameAsync(string name);
    Task<IEnumerable<WorldDBO>> GetAllWorldsAsync();
    Task<Guid> CreateWorldAsync(WorldDBO world);
    Task UpdateWorldAsync(WorldDBO world);
    Task DeleteWorldAsync(Guid worldId);

    // Chunk operations
    Task<WorldChunkDBO?> GetChunkAsync(Guid worldId, Vector3Int position);
    Task<IEnumerable<WorldChunkDBO>> GetAllChunksAsync(Guid worldId);
    Task SaveChunkAsync(WorldChunkDBO chunk);
    Task SaveChunksBatchAsync(IEnumerable<WorldChunkDBO> chunks);
    Task DeleteChunkAsync(Guid worldId, Vector3Int position);
    Task<int> GetChunkCountAsync(Guid worldId);
}
