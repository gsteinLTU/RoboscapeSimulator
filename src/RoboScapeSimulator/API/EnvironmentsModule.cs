using System.Text;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.Utilities;
using EmbedIO.WebApi;

namespace RoboScapeSimulator.API;

/// <summary>
/// API module providing information about the environments available on this server 
/// </summary>
public class EnvironmentsModule : WebApiModule
{
    public EnvironmentsModule(string baseRoute) : base(baseRoute)
    {
        AddHandler(HttpVerbs.Get, RouteMatcher.Parse("/list", false), GetList);
    }

    private Task GetList(IHttpContext context, RouteMatch route)
    {
        return context.SendAsJSON(Room.ListEnvironments());
    }
}