using Dapper;
using SkySaga.Game.Data.Models;
using System.Linq;

namespace SkySaga.Game.Data.Repositories;

/// <summary>
/// Repository implementation using Dapper for world and chunk persistence.
/// </summary>
public class WorldRepository(DatabaseContext dbContext) : IWorldRepository
{
    private readonly DatabaseContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    // ============= WORLD OPERATIONS =============

    public async Task<WorldDBO?> GetWorldByIdAsync(Guid worldId)
    {
        const string sql = "SELECT * FROM Worlds WHERE Id = @Id";
        using var connection = _dbContext.GetConnection();

        var result = await connection.QueryFirstOrDefaultAsync<WorldDboDto>(sql, new { Id = worldId.ToString() });
        return result?.ToModel();
    }

    public async Task<WorldDBO?> GetWorldByNameAsync(string name)
    {
        const string sql = "SELECT * FROM Worlds WHERE Name = @Name";
        using var connection = _dbContext.GetConnection();

        var result = await connection.QueryFirstOrDefaultAsync<WorldDboDto>(sql, new { Name = name });
        return result?.ToModel();
    }

    public async Task<IEnumerable<WorldDBO>> GetAllWorldsAsync()
    {
        const string sql = "SELECT * FROM Worlds ORDER BY CreatedAt DESC";
        using var connection = _dbContext.GetConnection();

        var results = await connection.QueryAsync<WorldDboDto>(sql);
        return results.Select(dto => dto.ToModel());
    }

    public async Task<Guid> CreateWorldAsync(WorldDBO world)
    {
        ArgumentNullException.ThrowIfNull(world);

        const string sql = """
            INSERT INTO Worlds (Id, Name, CreatedAt, LastModifiedAt,
                MapSizeChunksX, MapSizeChunksY, MapSizeChunksZ, BiomeType, GameMode)
            VALUES (@Id, @Name, @CreatedAt, @LastModifiedAt,
                @MapSizeChunksX, @MapSizeChunksY, @MapSizeChunksZ, @BiomeType, @GameMode)
            """;

        using var connection = _dbContext.GetConnection();
        await connection.ExecuteAsync(sql, new
        {
            Id = world.Id.ToString(),
            world.Name,
            CreatedAt = world.CreatedAt.ToString("O"),
            LastModifiedAt = world.LastModifiedAt.ToString("O"),
            world.MapSizeChunksX,
            world.MapSizeChunksY,
            world.MapSizeChunksZ,
            BiomeType = world.BiomeType.HasValue ? (int)world.BiomeType.Value : (int?)null,
            world.GameMode
        });

        return world.Id;
    }

    public async Task UpdateWorldAsync(WorldDBO world)
    {
        ArgumentNullException.ThrowIfNull(world);

        const string sql = """
            UPDATE Worlds
            SET LastModifiedAt = @LastModifiedAt,
                MapSizeChunksX = @MapSizeChunksX,
                MapSizeChunksY = @MapSizeChunksY,
                MapSizeChunksZ = @MapSizeChunksZ,
                BiomeType = @BiomeType,
                GameMode = @GameMode
            WHERE Id = @Id
            """;

        using var connection = _dbContext.GetConnection();
        await connection.ExecuteAsync(sql, new
        {
            Id = world.Id.ToString(),
            LastModifiedAt = DateTime.UtcNow.ToString("O"),
            world.MapSizeChunksX,
            world.MapSizeChunksY,
            world.MapSizeChunksZ,
            BiomeType = world.BiomeType.HasValue ? (int)world.BiomeType.Value : (int?)null,
            world.GameMode
        });
    }

    public async Task DeleteWorldAsync(Guid worldId)
    {
        const string sql = "DELETE FROM Worlds WHERE Id = @Id";
        using var connection = _dbContext.GetConnection();
        await connection.ExecuteAsync(sql, new { Id = worldId.ToString() });
    }

    // ============= CHUNK OPERATIONS =============

    public async Task<WorldChunkDBO?> GetChunkAsync(Guid worldId, Vector3Int position)
    {
        const string sql = """
            SELECT * FROM WorldChunks
            WHERE WorldId = @WorldId AND ChunkX = @ChunkX AND ChunkY = @ChunkY AND ChunkZ = @ChunkZ
            """;

        using var connection = _dbContext.GetConnection();
        var result = await connection.QueryFirstOrDefaultAsync<WorldChunkDboDto>(sql, new
        {
            WorldId = worldId.ToString(),
            ChunkX = position.X,
            ChunkY = position.Y,
            ChunkZ = position.Z
        });

        return result?.ToModel();
    }

    public async Task<IEnumerable<WorldChunkDBO>> GetAllChunksAsync(Guid worldId)
    {
        const string sql = "SELECT * FROM WorldChunks WHERE WorldId = @WorldId";
        using var connection = _dbContext.GetConnection();

        var results = await connection.QueryAsync<WorldChunkDboDto>(sql, new { WorldId = worldId.ToString() });
        return results.Select(dto => dto.ToModel());
    }

    public async Task SaveChunkAsync(WorldChunkDBO chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        const string sql = """
            INSERT INTO WorldChunks (WorldId, ChunkX, ChunkY, ChunkZ, VoxelData, MetadataBlob, LastModifiedAt)
            VALUES (@WorldId, @ChunkX, @ChunkY, @ChunkZ, @VoxelData, @MetadataBlob, @LastModifiedAt)
            ON CONFLICT(WorldId, ChunkX, ChunkY, ChunkZ)
            DO UPDATE SET
                VoxelData = @VoxelData,
                MetadataBlob = @MetadataBlob,
                LastModifiedAt = @LastModifiedAt
            """;

        using var connection = _dbContext.GetConnection();
        await connection.ExecuteAsync(sql, new
        {
            WorldId = chunk.WorldId.ToString(),
            chunk.ChunkX,
            chunk.ChunkY,
            chunk.ChunkZ,
            chunk.VoxelData,
            chunk.MetadataBlob,
            LastModifiedAt = DateTime.UtcNow.ToString("O")
        });
    }

    public async Task SaveChunksBatchAsync(IEnumerable<WorldChunkDBO> chunks)
    {
        ArgumentNullException.ThrowIfNull(chunks);

        const string sql = """
            INSERT INTO WorldChunks (WorldId, ChunkX, ChunkY, ChunkZ, VoxelData, MetadataBlob, LastModifiedAt)
            VALUES (@WorldId, @ChunkX, @ChunkY, @ChunkZ, @VoxelData, @MetadataBlob, @LastModifiedAt)
            ON CONFLICT(WorldId, ChunkX, ChunkY, ChunkZ)
            DO UPDATE SET
                VoxelData = @VoxelData,
                MetadataBlob = @MetadataBlob,
                LastModifiedAt = @LastModifiedAt
            """;

        using var connection = _dbContext.GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            foreach (var chunk in chunks)
            {
                await connection.ExecuteAsync(sql, new
                {
                    WorldId = chunk.WorldId.ToString(),
                    chunk.ChunkX,
                    chunk.ChunkY,
                    chunk.ChunkZ,
                    chunk.VoxelData,
                    chunk.MetadataBlob,
                    LastModifiedAt = DateTime.UtcNow.ToString("O")
                }, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task DeleteChunkAsync(Guid worldId, Vector3Int position)
    {
        const string sql = """
            DELETE FROM WorldChunks
            WHERE WorldId = @WorldId AND ChunkX = @ChunkX AND ChunkY = @ChunkY AND ChunkZ = @ChunkZ
            """;

        using var connection = _dbContext.GetConnection();
        await connection.ExecuteAsync(sql, new
        {
            WorldId = worldId.ToString(),
            ChunkX = position.X,
            ChunkY = position.Y,
            ChunkZ = position.Z
        });
    }

    public async Task<int> GetChunkCountAsync(Guid worldId)
    {
        const string sql = "SELECT COUNT(*) FROM WorldChunks WHERE WorldId = @WorldId";
        using var connection = _dbContext.GetConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { WorldId = worldId.ToString() });
    }

    // ============= HELPER DTO CLASSES =============

    /// <summary>
    /// DTO for Dapper mapping (DateTime and Guid as strings from DB).
    /// </summary>
    private class WorldDboDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? CreatedAt { get; set; }
        public string? LastModifiedAt { get; set; }
        public int MapSizeChunksX { get; set; }
        public int MapSizeChunksY { get; set; }
        public int MapSizeChunksZ { get; set; }
        public int? BiomeType { get; set; }
        public int GameMode { get; set; }

        public WorldDBO ToModel() => new(
            id: Guid.Parse(Id!),
            name: Name!,
            createdAt: DateTime.Parse(CreatedAt!),
            lastModifiedAt: DateTime.Parse(LastModifiedAt!),
            mapSizeChunksX: MapSizeChunksX,
            mapSizeChunksY: MapSizeChunksY,
            mapSizeChunksZ: MapSizeChunksZ,
            biomeType: BiomeType.HasValue ? (uint)BiomeType.Value : null,
            gameMode: GameMode
        );
    }

    private class WorldChunkDboDto
    {
        public string? WorldId { get; set; }
        public int ChunkX { get; set; }
        public int ChunkY { get; set; }
        public int ChunkZ { get; set; }
        public byte[]? VoxelData { get; set; }
        public byte[]? MetadataBlob { get; set; }
        public string? LastModifiedAt { get; set; }

        public WorldChunkDBO ToModel() => new(
            worldId: Guid.Parse(WorldId!),
            chunkX: ChunkX,
            chunkY: ChunkY,
            chunkZ: ChunkZ,
            voxelData: VoxelData!,
            metadataBlob: MetadataBlob!,
            lastModifiedAt: DateTime.Parse(LastModifiedAt!)
        );
    }
}
