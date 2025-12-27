namespace SkySaga.Game.Data.Models;

/// <summary>
/// Database object representing a world in the Worlds table.
/// </summary>
public class WorldDBO(
    Guid id,
    string name,
    DateTime createdAt,
    DateTime lastModifiedAt,
    int mapSizeChunksX,
    int mapSizeChunksY,
    int mapSizeChunksZ,
    uint? biomeType,
    int gameMode)
{
    public Guid Id { get; init; } = id;
    public string Name { get; init; } = name;
    public DateTime CreatedAt { get; init; } = createdAt;
    public DateTime LastModifiedAt { get; set; } = lastModifiedAt;

    // MapDefinition properties
    public int MapSizeChunksX { get; init; } = mapSizeChunksX;
    public int MapSizeChunksY { get; init; } = mapSizeChunksY;
    public int MapSizeChunksZ { get; init; } = mapSizeChunksZ;
    public uint? BiomeType { get; init; } = biomeType;
    public int GameMode { get; init; } = gameMode;

    /// <summary>
    /// Converts this DB object to MapDefinition.
    /// </summary>
    public MapDefinition ToMapDefinition() => new()
    {
        MapSizeChunks = new Vector3Int(MapSizeChunksX, MapSizeChunksY, MapSizeChunksZ),
        BiomeType = BiomeType,
        GameMode = GameMode
    };

    /// <summary>
    /// Creates a DB object from MapDefinition.
    /// </summary>
    public static WorldDBO FromMapDefinition(Guid id, string name, MapDefinition definition) => new(
        id: id,
        name: name,
        createdAt: DateTime.UtcNow,
        lastModifiedAt: DateTime.UtcNow,
        mapSizeChunksX: definition.MapSizeChunks.X,
        mapSizeChunksY: definition.MapSizeChunks.Y,
        mapSizeChunksZ: definition.MapSizeChunks.Z,
        biomeType: definition.BiomeType,
        gameMode: definition.GameMode
    );
}
