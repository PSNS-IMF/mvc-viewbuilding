using System;
using System.Collections.Generic;
using System.Web.Http;

using Psns.Common.Mvc.ViewBuilding.ViewBuilders;
using Psns.Common.Mvc.ViewBuilding.ViewModels;

namespace Psns.Common.Mvc.ViewBuilding.Controllers
{
    /// <summary>
    /// An Api controller for the Index view
    /// </summary>
    public class IndexController : ApiController, IIndexController
    {
        ICrudViewBuilder _viewBuilder;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="viewBuilder">The viewBuilder to retrieve the view models from</param>
        public IndexController(ICrudViewBuilder viewBuilder)
        {
            _viewBuilder = viewBuilder;
        }

        /// <summary>
        /// Returns the options used to populate the filter drop downs on the Index view
        /// </summary>
        /// <param name="modelName">The Fully Qualified Assembly Name of the class whose properties are used in the Index View</param>
        /// <returns>A collection of FilterOption objects</returns>
        public virtual IEnumerable<FilterOption> GetFilterOptions(string modelName)
        {
            return this.GetFilterOptions(modelName, _viewBuilder, null);
        }
    }
}
