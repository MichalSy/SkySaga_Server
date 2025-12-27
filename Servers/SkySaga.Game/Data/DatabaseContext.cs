using Microsoft.Data.Sqlite;
using System.Data;

namespace SkySaga.Game.Data;

/// <summary>
/// Manages SQLite database connection and schema initialization.
/// </summary>
public class DatabaseContext : IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _connection;

    public DatabaseContext(string databasePath = "Data/skysaga_worlds.db")
    {
        ArgumentNullException.ThrowIfNull(databasePath);

        // Create Data directory if it doesn't exist
        string? directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = $"Data Source={databasePath}";
        InitializeSchema();
    }

    /// <summary>
    /// Gets or creates a database connection.
    /// </summary>
    public IDbConnection GetConnection()
    {
        if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
        {
            _connection = new SqliteConnection(_connectionString);
            _connection.Open();
        }
        return _connection;
    }

    /// <summary>
    /// Initializes the database schema if it doesn't exist.
    /// </summary>
    private void InitializeSchema()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        const string createWorldsTable = """
            CREATE TABLE IF NOT EXISTS Worlds (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                LastModifiedAt TEXT NOT NULL,
                MapSizeChunksX INTEGER NOT NULL,
                MapSizeChunksY INTEGER NOT NULL,
                MapSizeChunksZ INTEGER NOT NULL,
                BiomeType INTEGER,
                GameMode INTEGER NOT NULL
            );
            """;

        const string createChunksTable = """
            CREATE TABLE IF NOT EXISTS WorldChunks (
                WorldId TEXT NOT NULL,
                ChunkX INTEGER NOT NULL,
                ChunkY INTEGER NOT NULL,
                ChunkZ INTEGER NOT NULL,
                VoxelData BLOB NOT NULL,
                MetadataBlob BLOB NOT NULL,
                LastModifiedAt TEXT NOT NULL,
                PRIMARY KEY (WorldId, ChunkX, ChunkY, ChunkZ),
                FOREIGN KEY (WorldId) REFERENCES Worlds(Id) ON DELETE CASCADE
            );
            """;

        const string createIndex = """
            CREATE INDEX IF NOT EXISTS idx_worldchunks_worldid
            ON WorldChunks(WorldId);
            """;

        using var command = connection.CreateCommand();
        command.CommandText = createWorldsTable;
        command.ExecuteNonQuery();

        command.CommandText = createChunksTable;
        command.ExecuteNonQuery();

        command.CommandText = createIndex;
        command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}
