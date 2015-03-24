[assembly: WebActivatorEx.PostApplicationStartMethod(typeof($rootnamespace$.App_Start.ViewBuildingActivator), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof($rootnamespace$.App_Start.ViewBuildingActivator), "Stop")]

namespace $rootnamespace$.App_Start
{
    using System.Web.Http;

    using Newtonsoft.Json.Serialization;

    public static class ViewBuildingActivator
    {
        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            Psns.Common.Mvc.ViewBuilding.Infrastructure.WebApiConfig.Register(GlobalConfiguration.Configuration);
            
            System.Web.Mvc.ControllerBuilder.Current.DefaultNamespaces.Add("Psns.Common.Mvc.ViewBuilding.Controllers");

            GlobalConfiguration.Configuration
                .Formatters
                .JsonFormatter
                .SerializerSettings
                .ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {

        }
    }
}
