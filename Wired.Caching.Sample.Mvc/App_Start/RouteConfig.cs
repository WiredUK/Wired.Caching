using System.Web.Mvc;
using System.Web.Routing;

namespace Wired.Caching.Sample.Mvc
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "AttributeDemo",
                "{controller}/{action}/{id}/{someOtherParameter}",
                new { controller = "Home", action = "Index" }
            );

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}
