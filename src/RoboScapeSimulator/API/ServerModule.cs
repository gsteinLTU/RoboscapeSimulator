using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.Utilities;
using EmbedIO.WebApi;

namespace RoboScapeSimulator.API;

/// <summary>
/// API module providing information about this server 
/// </summary>
public class ServerModule : WebApiModule
{

    public ServerModule(string baseRoute, ConcurrentDictionary<string, Room> rooms) : base(baseRoute)
    {
        AddHandler(HttpVerbs.Get, RouteMatcher.Parse("/status", false), (context, route) =>
        {
            string output = JsonSerializer.Serialize(new ServerStatus
            {
                activeRooms = rooms.Count(kvp => !kvp.Value.Hibernating),
                hibernatingRooms = rooms.Count(kvp => kvp.Value.Hibernating),
                maxRooms = SettingsManager.MaxRooms
            },
            new JsonSerializerOptions()
            {
                IncludeFields = true
            });

            return context.SendStringAsync(output, "application/json", Encoding.Default);
        });
    }

    [Serializable]
    public struct ServerStatus
    {
        public int activeRooms;
        public int hibernatingRooms;
        public int maxRooms;
    }
}