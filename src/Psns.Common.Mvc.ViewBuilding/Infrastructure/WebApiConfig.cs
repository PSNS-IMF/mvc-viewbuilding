using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Psns.Common.Mvc.ViewBuilding.Infrastructure
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                "ApiAction",
                "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
