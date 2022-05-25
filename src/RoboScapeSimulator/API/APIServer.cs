using System.Diagnostics;
using System.Text;
using EmbedIO;

namespace RoboScapeSimulator.API;

/// <summary>
/// Provides functionality for instantiating API server
/// </summary>
public class APIServer
{
    /// <summary>
    /// Creates a WebServer listening on a specific port, hosting this server's API content
    /// </summary>
    /// <returns>The WebServer created</returns>
    public static WebServer CreateWebServer()
    {
        var server = new WebServer(SettingsManager.APIPort)
            .WithModule(new ServerModule("/server"))
            .WithModule(new EnvironmentsModule("/environments"))
            .WithModule(new RoomsModule("/rooms"));

        // Listen for state changes.
        server.StateChanged += (s, e) => Debug.WriteLine($"WebServer New State - {e.NewState}");

        server.HandleHttpException(async (context, exception) =>
        {
            context.Response.StatusCode = exception.StatusCode;

            switch (exception.StatusCode)
            {
                case 404:
                    await context.SendStringAsync("Not found", "text/html", Encoding.UTF8);
                    break;
                default:
                    await HttpExceptionHandler.Default(context, exception);
                    break;
            }
        });

        return server;
    }
}