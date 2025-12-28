using System.Linq;

namespace SkySaga.Game.Managers.World;

/// <summary>
/// World manager that combines entity and chunk management with database persistence.
/// Provides a single entry point for world state access.
/// </summary>
public sealed class WorldManager : IWorldManager
{
    private readonly MapChunkManager _chunkManager;
    private readonly IWorldRepository _worldRepository;
    private readonly ILogger<WorldManager>? _logger;

    public IMapEntityManager EntityManager { get; }
    public IMapChunkManager ChunkManager => _chunkManager;
    public MapDefinition Definition { get; set; }
    public Guid WorldId { get; private set; }

    public WorldManager(
        PlayerConnectionManager playerConnectionManager,
        IWorldRepository worldRepository,
        ILogger<WorldManager>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(playerConnectionManager);
        ArgumentNullException.ThrowIfNull(worldRepository);

        EntityManager = new EntityManager();
        _chunkManager = new MapChunkManager(playerConnectionManager, this);
        _worldRepository = worldRepository;
        _logger = logger;

        // Initialize world asynchronously
        InitializeWorldAsync("DefaultWorld").GetAwaiter().GetResult();
    }

    /// <summary>
    /// Initializes or loads a world from the database.
    /// </summary>
    private async Task InitializeWorldAsync(string worldName)
    {
        _logger?.LogInformation("Initializing world '{WorldName}'...", worldName);

        // Try to load existing world from database
        var existingWorld = await _worldRepository.GetWorldByNameAsync(worldName);

        if (existingWorld != null)
        {
            // Load from database
            _logger?.LogInformation("Loading existing world '{WorldName}' (ID: {WorldId})", worldName, existingWorld.Id);

            WorldId = existingWorld.Id;
            Definition = existingWorld.ToMapDefinition();

            // Load all chunks from database
            await LoadChunksFromDatabaseAsync();
        }
        else
        {
            // Create new world
            _logger?.LogInformation("Creating new world '{WorldName}'...", worldName);

            WorldId = Guid.NewGuid();

            // Initialize with default map definition
            Definition = new MapDefinition
            {
                MapSizeChunks = new Vector3Int(2, 2, 2),
                BiomeType = Util.ComputeCrc32("Sky_Island"),
                GameMode = 1
            };

            // Save world to database
            var worldModel = WorldDBO.FromMapDefinition(WorldId, worldName, Definition);
            await _worldRepository.CreateWorldAsync(worldModel);

            // Generate starting island
            GenerateStartIsland();

            // Save generated chunks to database
            await SaveAllChunksAsync();

            _logger?.LogInformation("New world '{WorldName}' created with ID: {WorldId}", worldName, WorldId);
        }

        // Initialize world entities (not persisted)
        GenerateWorldEntities();
    }

    /// <summary>
    /// Loads all chunks for this world from the database.
    /// </summary>
    private async Task LoadChunksFromDatabaseAsync()
    {
        var chunks = await _worldRepository.GetAllChunksAsync(WorldId);
        int chunkCount = 0;

        foreach (var chunkModel in chunks)
        {
            var chunk = chunkModel.ToWorldChunk();
            _chunkManager.SetChunk(chunk.Position, chunk);
            chunkCount++;
        }

        _logger?.LogInformation("Loaded {ChunkCount} chunks from database", chunkCount);
    }

    /// <summary>
    /// Saves all loaded chunks to the database.
    /// </summary>
    public async Task SaveAllChunksAsync()
    {
        var chunks = _chunkManager.GetLoadedChunks()
            .Select(chunk => WorldChunkDBO.FromWorldChunk(WorldId, chunk))
            .ToList();

        if (chunks.Count > 0)
        {
            try
            {
                await _worldRepository.SaveChunksBatchAsync(chunks);
                _logger?.LogDebug("Saved {ChunkCount} chunks to database", chunks.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save chunks to database");
            }
        }
    }

    /// <summary>
    /// Saves a single chunk to the database.
    /// </summary>
    public async Task SaveChunkAsync(Vector3Int position)
    {
        try
        {
            if (_chunkManager.TryGetChunk(position, out var chunk))
            {
                var chunkModel = WorldChunkDBO.FromWorldChunk(WorldId, chunk);
                await _worldRepository.SaveChunkAsync(chunkModel);
                _logger?.LogDebug("Saved chunk at {Position}", position);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save chunk at {Position}", position);
        }
    }

    /// <summary>
    /// Generates the starting island with clean, readable code.
    /// </summary>
    private void GenerateStartIsland()
    {
        // Chunk (0, 0, 0) - Grundebene + Schachbrett mit allen Blöcken (1-80)
        {
            var chunk = ChunkManager.GetOrCreateChunk(new Vector3Int(0, 0, 0));

            // Grundebene auf Y=0 mit Dirt (block type 24)
            for (int z = 0; z < 32; z++)
            {
                for (int x = 0; x < 32; x++)
                {
                    chunk.SetVoxel(x, 10, z, 1);
                }
            }

            // Schachbrett-Pattern mit allen Blöcken 1-80 auf Y=1
            // Mit 1 Block Abstand = 2 Positionen pro Block
            // Starte bei (5, 5) statt (0, 0)
            int blockType = 1;
            int posX = 5;
            int posZ = 5;

            while (blockType <= 255)
            {
                if (posX < 32 && posZ < 32)
                {
                    chunk.SetVoxel(posX, 11, posZ, (byte)blockType);
                    blockType++;
                    posX += 2; // 1 Block + 1 Abstand

                    if (posX >= 32)
                    {
                        posX = 5;
                        posZ += 2;
                    }
                }
                else
                {
                    break; // Chunk 0,0,0 voll, weiter in nächstem Chunk
                }
            }
        }

        // Chunk (1, 0, 0) - Grundebene + Fortsetzung des Schachbretts (restliche Blöcke)
        {
            var chunk = ChunkManager.GetOrCreateChunk(new Vector3Int(1, 0, 0));

            // Grundebene auf Y=0 mit Dirt (block type 24)
            for (int z = 0; z < 32; z++)
            {
                for (int x = 0; x < 32; x++)
                {
                    chunk.SetVoxel(x, 10, z, 1);
                }
            }
        }

        // Chunk (1, 0, 1) - Grass field with decorations
        {
            var chunk = ChunkManager.GetOrCreateChunk(new Vector3Int(1, 0, 1));

            // Fill bottom layer with dirt
            for (int z = 0; z < 32; z++)
            {
                for (int x = 0; x < 32; x++)
                {
                    chunk.SetVoxel(x, 10, z, 1);
                }
            }

            // Decorative blocks - block type 20
            for (int i = 0; i < 14; i += 2)
            {
                chunk.SetVoxel(4 + i, 11, 14, 20);
            }
        }

        // Chunk (0, 0, 1) - Empty land
        {
            var chunk = ChunkManager.GetOrCreateChunk(new Vector3Int(0, 0, 1));

            // Fill bottom layer with dirt
            for (int z = 0; z < 32; z++)
            {
                for (int x = 0; x < 32; x++)
                {
                    chunk.SetVoxel(x, 10, z, 1);
                }
            }
        }



        // Initialize world entities
        GenerateWorldEntities();
    }

    /// <summary>
    /// Initializes world entities like NPCs, animals, and environmental objects.
    /// </summary>
    private void GenerateWorldEntities()
    {
        // AirShip
        if (EntityManager.TryCreateEntity("AirShip", out var airShip))
        {
            if (airShip.TryGetComponent<TransformComponent>(out var transformComponent))
            {
                transformComponent.Position = new Vector3(31.25f, 11f, 19.8f);
                transformComponent.YawDegrees = 45;
            }
        }

        // TimeOfDay
        if (EntityManager.TryCreateEntity("TimeOfDay", out var timeOfDay))
        {
            if (timeOfDay.TryGetComponent<ClientTimeOfDayComponent>(out var clientTimeOfDayComponent))
            {
                clientTimeOfDayComponent.StartTimeOfDay = 0.5f;
                clientTimeOfDayComponent.FixedTimeOfDay = false;
                clientTimeOfDayComponent.DayNightCycleDuration = 1;
                clientTimeOfDayComponent.RealWorldStartTime = RakNet.RakNet.GetTime();
                clientTimeOfDayComponent.TimeStretch = 0;
                clientTimeOfDayComponent.TimeOfDayOffset = 0.5f;
            }
        }

        // SeaLevel
        if (EntityManager.TryCreateEntity("SeaLevel", out var sea))
        {
            if (sea.TryGetComponent<SeaLevelComponent>(out var seaLevelComponent))
            {
                seaLevelComponent.SeaFloorLevel = 0;
                seaLevelComponent.SeaLevel = 1;
            }
        }

        if (EntityManager.TryCreateEntity("Christmas_Tree", out var tree))
        {
            if (tree.TryGetComponent<TransformComponent>(out var treeComp))
            {
                treeComp.Position = new Vector3(36.25f, 11.0f, 25.8f);
                treeComp.YawDegrees = 45;
            }

        }

        if (EntityManager.TryCreateEntity("Camp_Fire", out var fire))
        {
            if (fire.TryGetComponent<TransformComponent>(out var smoothedTransformComponent))
                smoothedTransformComponent.Position = new Vector3(39.25f, 11.0f, 25.8f);

            if (fire.TryGetComponent<ClientCampFireComponent>(out var campFire))
            {
                campFire.EnemiesTooClose = false;
            }
        }

        // HomeTeleporter
        if (EntityManager.TryCreateEntity("HomeTeleporter", out var teleport))
        {
            if (teleport.TryGetComponent<TransformComponent>(out var smoothedTransformComponent))
            {
                smoothedTransformComponent.Position = new Vector3(50.25f, 11.0f, 25.8f);
                smoothedTransformComponent.YawDegrees = 90;
            }
        }

        //// Bear
        //if (EntityManager.TryCreateEntity("Iron", out var bear))
        //{
        //    if (bear.TryGetComponent<SmoothedTransformComponent>(out var smoothedTransformComponent))
        //        smoothedTransformComponent.Position = new Vector3(34.4f, 1.1f, 9.8f);
        //}

        //// Chicken
        //if (EntityManager.TryCreateEntity("CactusBlast", out var chicken))
        //{
        //    if (chicken.TryGetComponent<SmoothedTransformComponent>(out var smoothedTransformComponent))
        //        smoothedTransformComponent.Position = new Vector3(37.5f, 1.1f, 9.8f);
        //}

        //// Goat
        //if (EntityManager.TryCreateEntity("Bush_Desert_A", out var goat))
        //{
        //    if (goat.TryGetComponent<SmoothedTransformComponent>(out var smoothedTransformComponent))
        //        smoothedTransformComponent.Position = new Vector3(40.625f, 1.1f, 9.8f);
        //}

        //// Knight
        //if (EntityManager.TryCreateEntity("Knight", out var knight))
        //{
        //    if (knight.TryGetComponent<SmoothedTransformComponent>(out var smoothedTransformComponent))
        //        smoothedTransformComponent.Position = new Vector3(43.75f, 1.1f, 9.8f);
        //}

        //// Monkey
        //if (EntityManager.TryCreateEntity("Monkey", out var monkey))
        //{
        //    if (monkey.TryGetComponent<SmoothedTransformComponent>(out var smoothedTransformComponent))
        //        smoothedTransformComponent.Position = new Vector3(46.8f, 1.2f, 9.8f);
        //}
    }


    /// <summary>
    /// Sends initial chunk data to a client connection.
    /// </summary>
    public void SendInitialChunks(PlayerConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        var chunkSyncData = ChunkManager.GetAllChunkSyncData().ToList();

        var beginSync = new BeginSync
        {
            NumChunksToSync = chunkSyncData.Count
        };

        connection.Send(beginSync);

        foreach (var chunkSync in chunkSyncData)
        {
            connection.Send(chunkSync);
        }
    }

    /// <summary>
    /// Sends initial entity data to a client connection.
    /// </summary>
    public void SendInitialEntities(PlayerConnection connection, Entity player, PlayerInitializer playerInitializer)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(playerInitializer);

        // Initialize the player with default components and inventory
        playerInitializer.InitializePlayer(player);

        // Send all entities to the client
        foreach (var entity in EntityManager.Entities)
        {
            var entityAdd = new EntityAdd
            {
                Id = entity.Id,
                NameHash = Util.ComputeCrc32(entity.Name),
                SyncData = entity.GetSyncData(newEntity: true)
            };

            connection.Send(entityAdd);
        }

        connection.Send(new ClientEntitiesSyncFinished());
    }

    /// <summary>
    /// Resets the world to its initial state.
    /// </summary>
    public void Reset()
    {
        _chunkManager.ClearAll();
        GenerateStartIsland();

        SaveAllChunksAsync().GetAwaiter().GetResult();
        _logger?.LogInformation("World reset completed");
    }

}
