using System.Diagnostics;
using System.Text;
using EmbedIO;
using EmbedIO.Actions;

namespace RoboScapeSimulator.API;

public class APIServer
{
    public static WebServer CreateWebServer(int port)
    {
        var server = new WebServer(port).WithModule(new EnvironmentsModule("/environments"));

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