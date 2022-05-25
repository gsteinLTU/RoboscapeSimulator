using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.Utilities;
using EmbedIO.WebApi;

namespace RoboScapeSimulator.API;

/// <summary>
/// API module providing information about rooms available on this server 
/// </summary>
public class RoomsModule : WebApiModule
{
    public RoomsModule(string baseRoute) : base(baseRoute)
    {
        AddHandler(HttpVerbs.Get, RouteMatcher.Parse("/list", false), GetList);
    }

    private Task GetList(IHttpContext context, RouteMatch route)
    {
        // Allow query of specific-user-created rooms
        if (context.GetRequestQueryData().ContainsKey("user"))
        {
            return context.SendAsJSON(Program.Rooms.Where(kvp => kvp.Value.Creator == context.GetRequestQueryData()["user"]).Select(kvp => kvp.Key));
        }

        return context.SendAsJSON(Program.Rooms.Keys);
    }
}