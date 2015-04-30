using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Mvc;

using Psns.Common.Mvc.ViewBuilding.Entities;
using Psns.Common.Persistence.Definitions;

namespace Psns.Common.Mvc.ViewBuilding.Controllers
{
    public interface IDetailable<T> : IBaseController
        where T : class, 
        INameable,
        IIdentifiable
    {
        ActionResult Details(int id);
        ActionResult Details(T model);
    }

    public static class DetailableExtensions
    {
        public static ActionResult Details<T>(this IDetailable<T> controller, int id) 
            where T : class, 
            INameable, 
            IIdentifiable
        {
            return new ViewResult
            {
                ViewData = new ViewDataDictionary(controller.Builder.BuildDetailsView<T>(id))
            };
        }

        public static ActionResult Details<T>(this IDetailable<T> controller, T model)
            where T : class, 
            INameable,
            IIdentifiable
        {
            return new ViewResult
            {
                ViewData = new ViewDataDictionary(controller.Builder.BuildDetailsView<T>(model))
            };
        }
    }
}
