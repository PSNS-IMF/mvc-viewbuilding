using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Routing;
using System.Web.Helpers;
using System.Web.Mvc;

using Psns.Common.Mvc.ViewBuilding.Entities;
using Psns.Common.Web.Adapters;
using Psns.Common.Persistence.Definitions;

namespace Psns.Common.Mvc.ViewBuilding.Controllers
{
    public interface IUpdatable<T> : IRepositoryController<T>
         where T : class, 
        INameable,
        IIdentifiable
    {
        ActionResult Update(int? id);
        ActionResult Update(T model);
    }

    public static class UpdatableExtensions
    {
        public static ActionResult Update<T>(this IUpdatable<T> controller, int? id)
            where T : class, 
            INameable, 
            IIdentifiable
        {
            return new ViewResult
            {
                ViewData = new ViewDataDictionary(controller.Builder.BuildUpdateView<T>(id))
            };
        }

        public static ActionResult Update<T>(this IUpdatable<T> controller, T model) 
            where T : class, 
            INameable, 
            IIdentifiable
        {
            AntiForgeryHelperAdapter.Validate();

            if(controller.ModelState.IsValid)
            {
                T updated;

                if(IsCreate(controller, model))
                    updated = controller.Repository.Create(model);
                else
                {
                    var includes = CrudEntityExtensions.GetComplexPropertyNames(typeof(T).GetUpdateProperties());
                    updated = controller.Repository.Update(model, includes);
                }

                controller.Repository.SaveChanges();

                return new RedirectToRouteResult("Details",
                    new RouteValueDictionary(new { action = "Details", id = updated.Id }));
            }
            else
            {
                controller.ViewData.Model = controller.Builder.BuildUpdateView<T>(model);

                return new ViewResult
                {
                    ViewData = controller.ViewData,
                    TempData = controller.TempData,
                    ViewEngineCollection = controller.ViewEngineCollection
                };
            }
        }

        public static bool IsCreate<T>(this IUpdatable<T> updatable, T model)
            where T : class, IIdentifiable, INameable
        {
            return model.Id == 0;
        }
    }
}
