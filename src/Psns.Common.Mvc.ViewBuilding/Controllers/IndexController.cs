using System;
using System.Collections.Generic;
using System.Web.Http;

using Psns.Common.Mvc.ViewBuilding.ViewBuilders;
using Psns.Common.Mvc.ViewBuilding.ViewModels;

namespace Psns.Common.Mvc.ViewBuilding.Controllers
{
    public class IndexController : ApiController, IIndexController
    {
        ICrudViewBuilder _viewBuilder;

        public IndexController(ICrudViewBuilder viewBuilder)
        {
            _viewBuilder = viewBuilder;
        }

        public IEnumerable<FilterOption> GetFilterOptions(string modelName)
        {
            return this.GetFilterOptions(modelName, _viewBuilder, null);
        }
    }
}
