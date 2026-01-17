using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SkySaga.Web.Endpoints;

namespace SkySaga.Web;

public static class AuthComponent
{
    public static void ConfigureApp(WebApplication app)
    {
        app.UseHttpLogging();

        app.MapGet("/ping", () => Results.Ok());

        app.MapAccountEndpoints();
        app.MapMatchMakingEndpoints();
        app.MapBinaryStorageEndpoint();
        app.MapGameConductorEndpoints();
        app.MapAuthenticationEndpoints();
        app.MapPersistentRecordEndpoints();
    }
}
