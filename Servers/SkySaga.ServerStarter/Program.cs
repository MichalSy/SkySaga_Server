// Validate RakNet DLL before starting
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpLogging;
using SkySaga.Web;

if (!File.Exists(Path.Combine(AppContext.BaseDirectory, "RakNet.dll")))
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
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
});

builder.Services.AddLogging(logging =>
{
    //logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});

ServerGameComponent.Configure(builder.Services);
ChatCommunicationComponent.Configure(builder.Services);

builder.WebHost.UseUrls("http://localhost:5164");

var host = builder.Build();

AuthComponent.ConfigureApp(host);

// Run the host
await host.RunAsync();

Console.WriteLine("""
        Server has stopped.
        Press any key to continue . . .
        """);

Console.ReadKey();