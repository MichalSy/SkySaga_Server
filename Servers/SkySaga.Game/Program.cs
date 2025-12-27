// Validate RakNet DLL before starting
if (!File.Exists("RakNet.dll"))
{
    Console.WriteLine("""
        The RakNet DLL is missing.
        Press any key to continue . . .
        """);

    Console.ReadKey();

    return;
}

try
{
    _ = new RakString();
}
catch
{
    Console.WriteLine("""
        RakNet DLL issue.
        Most likely the provided DLL wasn't build with the C# wrapper file.
        Press any key to continue . . .
        """);

    Console.ReadKey();

    return;
}

// Create and configure the host
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register database services
        services.AddSingleton<DatabaseContext>(sp =>
            new DatabaseContext("Data/skysaga_worlds.db"));
        services.AddSingleton<IWorldRepository, WorldRepository>();

        // Register managers and services
        services.AddSingleton<PlayerConnectionManager>();
        services.AddSingleton<IWorldManager, WorldManager>();
        services.AddSingleton(sp => new PlayerInitializer(sp.GetRequiredService<IWorldManager>().EntityManager));

        // Register Game Server
        services.AddSingleton<Server>();

        // Register IRC Server
        services.AddSingleton<IrcServer>();

        // Register hosted services
        services.AddHostedService<ServerHostedService>();
        services.AddHostedService<IrcServerHostedService>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Debug);
    });

var host = builder.Build();

// Run the host
await host.RunAsync();

Console.WriteLine("""
        Server has stopped.
        Press any key to continue . . .
        """);

Console.ReadKey();