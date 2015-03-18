using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Mvc;

using Psns.Common.Mvc.ViewBuilding.Entities;
using Psns.Common.Persistence.Definitions;

namespace Psns.Common.Mvc.ViewBuilding.Controllers
{
    public interface IDetailable : IBaseController
    {
        ActionResult Details(int id);
    }

    public static class DetailableExtensions
    {
        public static ActionResult Details<T>(this IDetailable controller, int id) 
            where T : class, 
            INameable, 
            IIdentifiable
        {
            return new ViewResult
            {
                ViewData = new ViewDataDictionary(controller.Builder.BuildDetailsView<T>(id))
            };
        }
    }
}
