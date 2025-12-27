using System.Threading;
using System.Threading.Tasks;

namespace SkySaga.Game;

public class ServerHostedService(
    Server server,
    ILogger<ServerHostedService> logger,
    IHostApplicationLifetime lifetime,
    IWorldManager worldManager) : IHostedService
{
    private readonly Server _server = server ?? throw new ArgumentNullException(nameof(server));
    private readonly ILogger<ServerHostedService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHostApplicationLifetime _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
    private readonly IWorldManager _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
    private Task? _tickTask;
    private CancellationTokenSource? _tickCancellation;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting server...");

        if (!_server.Start())
        {
            _logger.LogError("Failed to start server");
            _lifetime.StopApplication();
            return Task.CompletedTask;
        }

        _logger.LogInformation("Server started successfully");

        // Start the tick loop in a background task
        _tickCancellation = new CancellationTokenSource();
        _tickTask = Task.Run(() => TickLoop(_tickCancellation.Token), cancellationToken);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping server...");

        // Save world state to database
        if (_worldManager is WorldManager wm)
        {
            _logger.LogInformation("Saving world to database...");
            await wm.SaveAllChunksAsync();
        }

        // Signal the tick loop to stop
        _tickCancellation?.Cancel();

        // Wait for the tick task to complete
        if (_tickTask != null)
        {
            await _tickTask;
        }

        _logger.LogInformation("Server stopped");
    }

    private void TickLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _server.Tick();
                Thread.Sleep(1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in server tick loop");
            }
        }
    }
}
