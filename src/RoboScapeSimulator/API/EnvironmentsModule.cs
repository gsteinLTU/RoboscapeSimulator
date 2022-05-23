using System.Text;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.Utilities;
using EmbedIO.WebApi;

namespace RoboScapeSimulator.API;

public class EnvironmentsModule : WebApiModule
{
    public EnvironmentsModule(string baseRoute) : base(baseRoute)
    {
        AddHandler(HttpVerbs.Get, RouteMatcher.Parse("/hello", false), GetHello);
    }

    private Task GetHello(IHttpContext context, RouteMatch route)
    {
        var queryData = context.GetRequestQueryData();

        if (queryData.ContainsKey("username"))
        {
            return context.SendStringAsync("Hello " + queryData["username"] + "!", "text/plain", Encoding.Default);
        }
        else
        {
            return context.SendStringAsync("Hello world!", "text/plain", Encoding.Default);
        }
    }


}