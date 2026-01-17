namespace SkySaga.Game;

public static class ServerGameComponent
{
    public static void Configure(IServiceCollection services)
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

        services.AddHostedService<ServerHostedService>();
    }
}
