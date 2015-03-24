using System;
using System.Collections.Generic;
using System.Web.Http;

using Psns.Common.Mvc.ViewBuilding.ViewBuilders;
using Psns.Common.Mvc.ViewBuilding.ViewModels;

namespace Psns.Common.Mvc.ViewBuilding.Controllers
{
    /// <summary>
    /// Defines an api controller for an Index view
    /// </summary>
    public interface IIndexController
    {
        /// <summary>
        /// Defines a method that returns filter options for an Index view
        /// </summary>
        /// <param name="modelName">The name of the model from which the filter options are generated</param>
        /// <returns>The filter options</returns>
        IEnumerable<FilterOption> GetFilterOptions(string modelName);
    }

    /// <summary>
    /// Extensions for IIndexController to enable re-usability
    /// </summary>
    public static class IIndexControllerExtensions
    {
        /// <summary>
        /// Calls the GetIndexFilterOptions on the ViewBuilder and returns the results
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="modelName">The Fully Qualified Assembly Name of the model</param>
        /// <param name="viewBuilder">An ICrudViewBuilder implementation</param>
        /// <param name="visitors">Any IFilterOptionVisitors to be passed to the ViewBuilder</param>
        /// <returns></returns>
        public static IEnumerable<FilterOption> GetFilterOptions(this IIndexController controller,
            string modelName,
            ICrudViewBuilder viewBuilder,
            params IFilterOptionVisitor[] visitors)
        {
            var modelType = Type.GetType(modelName, false, true);

            var buildIndexMethod = viewBuilder.GetType().GetMethod("GetIndexFilterOptions");
            var genericMethod = buildIndexMethod.MakeGenericMethod(modelType);

            var filterOptions = genericMethod.Invoke(viewBuilder, new object[] { visitors }) as IEnumerable<FilterOption>;

            return filterOptions;
        }
    }
}
