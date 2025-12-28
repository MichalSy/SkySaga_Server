using RakNet;
using SkySaga.Game.Managers.Player;
using System.Collections.Frozen;

namespace SkySaga.Game;

public class Server : IDisposable
{
    private const int MaxConnections = 100;

    private readonly ushort _port;
    private readonly string _password;
    private readonly RakPeerInterface _peer;
    private readonly IWorldManager _worldManager;
    private readonly PlayerInitializer _playerInitializer;
    private readonly PlayerConnectionManager _playerConnectionManager;

    private readonly Dictionary<ulong, PlayerConnection> _connections = [];

    public Server(IWorldManager worldManager, PlayerInitializer playerInitializer, PlayerConnectionManager playerConnectionManager, string password = "Something about penguins\0", ushort port = 42069)
    {
        _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        _playerInitializer = playerInitializer ?? throw new ArgumentNullException(nameof(playerInitializer));
        _playerConnectionManager = playerConnectionManager ?? throw new ArgumentNullException(nameof(playerConnectionManager));
        _password = password ?? throw new ArgumentNullException(nameof(password));
        _port = port;

        _peer = RakPeerInterface.GetInstance();

        //var packetLogger = new PacketLogger();
        //_peer.AttachPlugin(packetLogger);

        _peer.SetIncomingPassword(_password, _password.Length);
        _peer.SetMaximumIncomingConnections(MaxConnections);
    }

    public bool Start()
    {
        var status = _peer.Startup(MaxConnections, new SocketDescriptor(_port, string.Empty), 1);

        return status == StartupResult.RAKNET_STARTED;
    }

    public void Tick()
    {
        ProcessPackets();

        UpdateAndSyncEntities();
        ProcessConnections();
    }

    private void ProcessPackets()
    {
        var packet = _peer.Receive();

        if (packet is null)
            return;

        var bitStream = new BitStream(packet.data, packet.length, false);

        var messageId = bitStream.ReadMessageId();

        if (!_connections.TryGetValue(packet.guid.g, out var connection)
            && messageId == (byte)DefaultMessageIDTypes.ID_NEW_INCOMING_CONNECTION)
        {
            connection = new PlayerConnection(this, packet.guid, _worldManager, _playerInitializer, _playerConnectionManager);

            _connections.TryAdd(packet.guid.g, connection);

            OnConnectionAdded(connection);

            goto Deallocate;
        }

        if (connection is null)
            goto Deallocate;

        if (messageId == (byte)DefaultMessageIDTypes.ID_CONNECTION_LOST ||
            messageId == (byte)DefaultMessageIDTypes.ID_DISCONNECTION_NOTIFICATION)
        {
            if (!_connections.Remove(packet.guid.g, out connection))
                throw new InvalidOperationException();

            OnConnectionRemoved(connection);

            goto Deallocate;
        }

        if (messageId >= (byte)DefaultMessageIDTypes.ID_USER_PACKET_ENUM)
        {
            var packetId = (PacketId)messageId - (byte)DefaultMessageIDTypes.ID_USER_PACKET_ENUM;

            var handled = connection.ProcessPacket(packetId, bitStream);

            if (!handled)
            {
                Debug.WriteLine($"Unhandled Packet. ( Length: {packet.length} )", packetId.ToString());

                //if (!bitStream.ReadBits(out int sourceEntityID))
                //    return false;

            }
        }

    Deallocate:
        _peer.DeallocatePacket(packet);
    }

    private void UpdateAndSyncEntities()
    {
        var entities = _worldManager.EntityManager.Entities;

        foreach (var entity in entities)
        {
            entity.TickComponents();

            if (!entity.SyncRequired)
                continue;

            var entitySync = new EntitySync
            {
                Id = entity.Id,
                SyncData = entity.GetSyncData(newEntity: false)
            };

            _playerConnectionManager.BroadcastToAll(entitySync);
        }
    }

    private void ProcessConnections()
    {
        foreach (var connection in _connections.ToFrozenSet())
        {
            connection.Value.Tick();
        }
    }

    public void Send(BitStream bitStream, AddressOrGUID systemIdentifier)
    {
        _peer.Send(bitStream, PacketPriority.HIGH_PRIORITY, PacketReliability.RELIABLE_ORDERED, (char)0, systemIdentifier, false);
    }


    private void OnConnectionAdded(PlayerConnection connection)
    {
        // Register connection with PlayerConnectionManager
        _playerConnectionManager.AddConnection(connection.Guid.g, connection);
        connection.OnConnected();
    }

    private void OnConnectionRemoved(PlayerConnection connection)
    {
        // Unregister connection from PlayerConnectionManager
        _playerConnectionManager.RemoveConnection(connection.Guid.g);

        _connections.Remove(connection.Guid.g);
        connection.OnDisconnected();
    }

    public void Dispose()
    {
        RakPeerInterface.DestroyInstance(_peer);
    }
}