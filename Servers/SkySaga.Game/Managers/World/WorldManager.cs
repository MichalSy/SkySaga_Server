using System.Linq;

namespace SkySaga.Game.Managers.World;

/// <summary>
/// World manager that combines entity and chunk management.
/// Provides a single entry point for world state access.
/// </summary>
public sealed class WorldManager : IWorldManager
{
    private readonly MapChunkManager _chunkManager;

    public IMapEntityManager EntityManager { get; }
    public IMapChunkManager ChunkManager => _chunkManager;
    public MapDefinition Definition { get; set; }

    public WorldManager(PlayerConnectionManager playerConnectionManager)
    {
        ArgumentNullException.ThrowIfNull(playerConnectionManager);

        EntityManager = new EntityManager();
        _chunkManager = new MapChunkManager(playerConnectionManager);

        // Initialize with default map definition
        Definition = new MapDefinition
        {
            MapSizeChunks = new Vector3Int(4, 4, 4),
            BiomeType = Util.ComputeCrc32("Sky_Island"),
            GameMode = 1
        };

        // Generate starting island
        GenerateStartIsland();
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
                    chunk.SetVoxel(x, 0, z, 1);
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
                    chunk.SetVoxel(posX, 1, posZ, (byte)blockType);
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
                    chunk.SetVoxel(x, 0, z, 24);
                }
            }

            // Schachbrett-Pattern - Fortsetzung von Chunk (0,0,0)
            int blockType = 1;
            int posX = 0;
            int posZ = 0;

            // Springe bis zur Position wo wir im vorigen Chunk aufgehört haben
            // Chunk (0,0,0) kann max 16*16 = 256 Positionen mit 2er-Schritten haben
            // Das reicht für Blöcke 1-256, also brauchen wir nur 1-80
            // Berechne wie viele in Chunk 0,0,0 passen
            int positionsPerRow = 32 / 2; // 16 mit 2er-Schritten
            int maxInChunk0 = positionsPerRow * (32 / 2); // 16 * 16 = 256 möglich, aber nur 80 nötig

            // Finde die Position wo wir mit Block 1 in Chunk 0,0,0 angefangen haben
            // und berechne wo wir enden
            int nextBlockType = 1;
            int tempX = 0;
            int tempZ = 0;

            while (nextBlockType <= 80 && (tempX < 32 && tempZ < 32))
            {
                nextBlockType++;
                tempX += 2;
                if (tempX >= 32)
                {
                    tempX = 0;
                    tempZ += 2;
                }
            }

            // Jetzt setzen wir die verbleibenden Blöcke in Chunk 1,0,0
            blockType = nextBlockType;
            posX = 5;
            posZ = 5;

            while (blockType <= 255)
            {
                if (posX < 32 && posZ < 32)
                {
                    chunk.SetVoxel(posX, 1, posZ, (byte)blockType);
                    blockType++;
                    posX += 2;

                    if (posX >= 32)
                    {
                        posX = 5;
                        posZ += 2;
                    }
                }
                else
                {
                    break;
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
                    chunk.SetVoxel(x, 0, z, 24);
                }
            }

            // Decorative blocks - block type 20
            for (int i = 0; i < 14; i += 2)
            {
                chunk.SetVoxel(4 + i, 1, 14, 20);
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
                    chunk.SetVoxel(x, 0, z, 24);
                }
            }
        }

        {
            var chunk1 = ChunkManager.GetOrCreateChunk(new Vector3Int(1, 0, 0));

            chunk1.SetVoxel(5, 1, 31, blockType: 13); // Place a special block (block type 30) at (5,1,5)
            chunk1.SetVoxel(6, 2, 31, blockType: 13); // Place a special block (block type 30) at (5,1,5)

            var chunk3 = ChunkManager.GetOrCreateChunk(new Vector3Int(1, 0, 1));
            chunk3.SetVoxel(7, 2, 0, blockType: 13); // Place a special block (block type 30) at (5,1,5)
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
                transformComponent.Position = new Vector3(31.25f, 1.1f, 19.8f);
            }
        }

        // TimeOfDay
        if (EntityManager.TryCreateEntity("TimeOfDay", out var timeOfDay))
        {
            if (timeOfDay.TryGetComponent<ClientTimeOfDayComponent>(out var clientTimeOfDayComponent))
            {
                clientTimeOfDayComponent.StartTimeOfDay = 32768;
                clientTimeOfDayComponent.FixedTimeOfDay = true;
                clientTimeOfDayComponent.DayNightCycleDuration = 20;
                clientTimeOfDayComponent.RealWorldStartTime = RakNet.RakNet.GetTime();
                clientTimeOfDayComponent.TimeStretch = 8;
                clientTimeOfDayComponent.TimeOfDayOffset = 65536;
            }
        }

        // Sheep
        if (EntityManager.TryCreateEntity("Sheep", out var sheep))
        {
            if (sheep.TryGetComponent<SmoothedTransformComponent>(out var smoothedTransformComponent))
                smoothedTransformComponent.Position = new Vector3(31.25f, 1.1f, 9.8f);

            if (sheep.TryGetComponent<ClientHealthComponent>(out var clientHealthComponent))
                clientHealthComponent.HalfHearts = 50;

            if (sheep.TryGetComponent<ClientCharacterPhysicsComponent>(out var clientCharacterPhysicsComponent))
                clientCharacterPhysicsComponent.IsMoveable = true;
        }

        // Bear
        if (EntityManager.TryCreateEntity("Iron", out var bear))
        {
            if (bear.TryGetComponent<SmoothedTransformComponent>(out var smoothedTransformComponent))
                smoothedTransformComponent.Position = new Vector3(34.4f, 1.1f, 9.8f);
        }

        // Chicken
        if (EntityManager.TryCreateEntity("CactusBlast", out var chicken))
        {
            if (chicken.TryGetComponent<SmoothedTransformComponent>(out var smoothedTransformComponent))
                smoothedTransformComponent.Position = new Vector3(37.5f, 1.1f, 9.8f);
        }

        // Goat
        if (EntityManager.TryCreateEntity("Bush_Desert_A", out var goat))
        {
            if (goat.TryGetComponent<SmoothedTransformComponent>(out var smoothedTransformComponent))
                smoothedTransformComponent.Position = new Vector3(40.625f, 1.1f, 9.8f);
        }

        // Knight
        if (EntityManager.TryCreateEntity("Knight", out var knight))
        {
            if (knight.TryGetComponent<SmoothedTransformComponent>(out var smoothedTransformComponent))
                smoothedTransformComponent.Position = new Vector3(43.75f, 1.1f, 9.8f);
        }

        // Monkey
        if (EntityManager.TryCreateEntity("Monkey", out var monkey))
        {
            if (monkey.TryGetComponent<SmoothedTransformComponent>(out var smoothedTransformComponent))
                smoothedTransformComponent.Position = new Vector3(46.8f, 1.2f, 9.8f);
        }
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
    /// Resets the world to initial state (useful for testing/reloading).
    /// </summary>
    public void Reset()
    {
        ((EntityManager)EntityManager).Clear();
        ChunkManager.ClearAll();
        GenerateStartIsland();
    }
}
