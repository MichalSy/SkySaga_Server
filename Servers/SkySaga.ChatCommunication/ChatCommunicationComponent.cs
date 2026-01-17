using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkySaga.Game;

namespace SkySaga.ChatCommunication;

public static class ChatCommunicationComponent
{
    public static void Configure(IServiceCollection services)
    {
        services.AddSingleton<IrcServer>();

        // Register hosted services
        services.AddHostedService<IrcServerHostedService>();
    }
}
