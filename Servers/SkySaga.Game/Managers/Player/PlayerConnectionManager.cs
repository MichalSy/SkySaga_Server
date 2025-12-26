namespace SkySaga.Game.Managers.Player;

/// <summary>
/// Manager for all player connections.
/// </summary>
public sealed class PlayerConnectionManager
{
    private readonly Dictionary<ulong, PlayerConnection> _connections = [];

    /// <summary>
    /// Adds a player connection to the manager.
    /// </summary>
    public void AddConnection(ulong guid, PlayerConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        _connections.TryAdd(guid, connection);
    }

    /// <summary>
    /// Removes a player connection from the manager.
    /// </summary>
    public void RemoveConnection(ulong guid)
    {
        _connections.Remove(guid);
    }

    /// <summary>
    /// Gets a player connection by GUID.
    /// </summary>
    public bool TryGetConnection(ulong guid, [NotNullWhen(true)] out PlayerConnection? connection)
    {
        return _connections.TryGetValue(guid, out connection);
    }

    /// <summary>
    /// Gets all active player connections.
    /// </summary>
    public IEnumerable<PlayerConnection> GetAllConnections()
    {
        return _connections.Values;
    }

    /// <summary>
    /// Broadcasts a packet to all connected players.
    /// </summary>
    public void BroadcastToAll(ISerializablePacket packet)
    {
        foreach (var connection in _connections.Values)
        {
            connection.Send(packet);
        }
    }

    /// <summary>
    /// Broadcasts a packet to all connected players except one.
    /// </summary>
    public void BroadcastToAllExcept(ulong excludeGuid, ISerializablePacket packet)
    {
        foreach (var kvp in _connections)
        {
            if (kvp.Key != excludeGuid)
            {
                kvp.Value.Send(packet);
            }
        }
    }
}

