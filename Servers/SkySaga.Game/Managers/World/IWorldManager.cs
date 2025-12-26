namespace SkySaga.Game.Managers.World;

/// <summary>
/// Interface for the world manager that provides access to entity and chunk management.
/// </summary>
public interface IWorldManager
{
    IMapEntityManager EntityManager { get; }
    IMapChunkManager ChunkManager { get; }
    MapDefinition Definition { get; set; }

    void SendInitialChunks(PlayerConnection connection);
    void SendInitialEntities(PlayerConnection connection, Entity player, PlayerInitializer playerInitializer);
    void Reset();
}
