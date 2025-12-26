namespace SkySaga.Game.Managers.Entities;

/// <summary>
/// Manages entity creation, deletion, and retrieval within the game world.
/// </summary>
public interface IMapEntityManager
{
    /// <summary>
    /// Gets all entities currently in the world.
    /// </summary>
    IEnumerable<Entity> Entities { get; }

    /// <summary>
    /// Attempts to get an entity by its ID.
    /// </summary>
    bool TryGetEntity(int id, [NotNullWhen(true)] out Entity? entity);

    /// <summary>
    /// Attempts to create a new entity of the specified type (auto-assigns ID).
    /// </summary>
    bool TryCreateEntity(string name, [NotNullWhen(true)] out Entity? entity);

    /// <summary>
    /// Removes an entity from the world.
    /// </summary>
    void RemoveEntity(Entity entity);

    /// <summary>
    /// Removes an entity by its ID.
    /// </summary>
    void RemoveEntity(int id);
}
