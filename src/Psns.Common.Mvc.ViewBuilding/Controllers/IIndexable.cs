using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Mvc;

using Psns.Common.Mvc.ViewBuilding.Entities;
using Psns.Common.Persistence.Definitions;

namespace Psns.Common.Mvc.ViewBuilding.Controllers
{
    public interface IIndexable : IBaseController
    {
        ActionResult Index();
    }

    public static class IndexableExtensions
    {
        public static ActionResult Index<T>(this IIndexable controller) 
            where T : class, 
            INameable, 
            IIdentifiable
        {
            return new ViewResult
            {
                ViewData = new ViewDataDictionary(controller.Builder.BuildIndexView<T>())
            };
        }
    }
}
