using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Text.RegularExpressions;

namespace ViewBuilding.UnitTests
{
    public class IdRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, 
            Route route, 
            string parameterName, 
            RouteValueDictionary values, 
            RouteDirection routeDirection)
        {
            return values.ContainsKey("id") &&
                Regex.IsMatch(values["id"].ToString(), @"^([1-9][0-9]*)$");
        }
    }

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Details",
                url: "{controller}/{id}/{action}",
                defaults: new { controller = "Home", action = "Details" },
                constraints: new { id = new IdRouteConstraint() }
            );

            routes.Remove(routes["Default"]);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}