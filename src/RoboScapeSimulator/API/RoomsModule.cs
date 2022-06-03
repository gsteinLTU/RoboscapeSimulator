using System.Net;
using System.Net.Sockets;
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
        AddHandler(HttpVerbs.Post, RouteMatcher.Parse("/create", false), PostCreate);
    }

    private Task GetList(IHttpContext context, RouteMatch route)
    {
        // Allow query of specific-user-created rooms
        if (context.GetRequestQueryData().ContainsKey("user") && context.GetRequestQueryData()["user"] != null)
        {
            var username = context.GetRequestQueryData()["user"];

            if (username != null)
            {
                return context.SendAsJSON(Program.Rooms.Where(kvp => kvp.Value.Visitors.Contains(username)).Select(kvp => kvp.Key));
            }
        }

        return context.SendAsJSON(Program.Rooms.Keys);
    }

    private Task PostCreate(IHttpContext context, RouteMatch route)
    {

        return context.SendAsJSON(new Dictionary<string, string>() { { "server", Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString() }, { "room", "" } });
    }
}