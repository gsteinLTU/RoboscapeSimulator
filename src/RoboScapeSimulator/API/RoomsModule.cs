using EmbedIO;
using EmbedIO.Routing;
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
        return context.SendAsJSON(Program.Rooms.Keys);
    }
}