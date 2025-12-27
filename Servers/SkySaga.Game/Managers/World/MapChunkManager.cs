namespace SkySaga.Game.Managers.World;

public class MapChunkManager(PlayerConnectionManager playerConnectionManager, IWorldManager? worldManager = null) : IMapChunkManager
{
    private readonly Dictionary<Vector3Int, WorldChunk> _chunks = [];
    private readonly PlayerConnectionManager _playerConnectionManager = playerConnectionManager ?? throw new ArgumentNullException(nameof(playerConnectionManager));
    private readonly IWorldManager? _worldManager = worldManager;

    public bool TryGetChunk(Vector3Int chunkPosition, [NotNullWhen(true)] out WorldChunk? chunk)
    {
        return _chunks.TryGetValue(chunkPosition, out chunk);
    }

    public WorldChunk GetOrCreateChunk(Vector3Int chunkPosition)
    {
        if (!_chunks.TryGetValue(chunkPosition, out var chunk))
        {
            chunk = new WorldChunk(chunkPosition);
            _chunks[chunkPosition] = chunk;
        }
        return chunk;
    }

    public void SetChunk(Vector3Int chunkPosition, WorldChunk chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        _chunks[chunkPosition] = chunk;
    }

    public void RemoveChunk(Vector3Int chunkPosition)
    {
        _chunks.Remove(chunkPosition);
    }

    public byte GetVoxel(Vector3Int chunkPosition, Vector3Int voxelPosition)
    {
        if (!TryGetChunk(chunkPosition, out var chunk))
            return 0; // Return air if chunk doesn't exist

        return chunk.GetVoxel(voxelPosition);
    }

    public void SetVoxel(Vector3Int chunkPosition, Vector3Int voxelPosition, byte blockType, byte metaValue = 0x00)
    {
        var chunk = GetOrCreateChunk(chunkPosition);

        // Get the old block before changing it
        byte oldBlockType = chunk.GetVoxel(voxelPosition);

        // Set the new block
        chunk.SetVoxel(voxelPosition, blockType, metaValue);

        // Broadcast only the changed voxel to all connected players (much more efficient than full ChunkSync)
        var partialSync = GetPartialChunkEditSync(chunkPosition, voxelPosition, oldBlockType, blockType);
        _playerConnectionManager.BroadcastToAll(partialSync);

        // Save chunk async (fire-and-forget) to database
        if (_worldManager is WorldManager wm)
        {
            _ = Task.Run(async () => await wm.SaveChunkAsync(chunkPosition));
        }
    }


    public static PartialChunkEditsSync GetPartialChunkEditSync(Vector3Int chunkPosition, Vector3Int voxelPosition, byte oldBlockType, byte newBlockType)
    {
        if (newBlockType == 0)
        {
            newBlockType = 255; // Convert air to empty space for network format
        }

        return new PartialChunkEditsSync
        {
            ChunkCoord = chunkPosition,
            ChunkEditList =
            [
                new ChunkEdit
                {
                    VoxelIndex = newBlockType,
                    VoxelCoordList =
                    [
                        voxelPosition
                    ]
                }
            ]
        };
    }

    public IEnumerable<WorldChunk> GetLoadedChunks() => _chunks.Values;

    public void GenerateInitialChunks()
    {
        // This is now handled by WorldManager.GenerateStartIsland()
        // Keeping this empty method for backwards compatibility
    }

    public void ClearAll()
    {
        _chunks.Clear();
    }

    public ChunkSync GetChunkSyncData(Vector3Int chunkPosition)
    {
        if (!TryGetChunk(chunkPosition, out var chunk))
            throw new InvalidOperationException($"Chunk at {chunkPosition} not loaded");

        var voxelData = chunk.GetVoxelData();

        const int chunkSize = 32;
        const int layerSize = chunkSize * chunkSize; // 1024 bytes per layer
        const int layersToSync = chunkSize; // Send all 32 layers for all chunks

        int dataSize = layerSize * layersToSync;


        //var metaData = chunk.GetMetadataForNetwork();
        // Create ChunkSync packet: 1 byte header + voxel data
        var syncData = new byte[dataSize + 1];
        var syncMeta = new byte[dataSize + 1];

        

        // Convert voxel data: air (0) becomes empty space (255) in the network format
        for (int i = 0; i < dataSize; i++)
        {
            syncData[1 + i] = (voxelData[i] == 0) ? (byte)255 : voxelData[i];
        }

        //metaData.CopyTo(syncMeta, 1);
        Array.Fill<byte>(syncMeta, 0x00);

        return new ChunkSync
        {
            Coords = new Vector3Int(chunkPosition.X, chunkPosition.Y, chunkPosition.Z),
            Data1 = syncData,
            //Data2 = new byte[dataSize+1],
        };
    }

    public IEnumerable<ChunkSync> GetAllChunkSyncData()
    {
        foreach (var chunk in _chunks.Values)
        {
            yield return GetChunkSyncData(chunk.Position);
        }
    }
}
