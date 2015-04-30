using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Mvc;
using System.Web.Routing;

using Psns.Common.Mvc.ViewBuilding.Entities;
using Psns.Common.Web.Adapters;
using Psns.Common.Persistence.Definitions;

namespace Psns.Common.Mvc.ViewBuilding.Controllers
{
    /// <summary>
    /// Defines an IRepositoryController that can delete
    /// </summary>
    /// <typeparam name="T">A type that implements INameable and IIdentifiable</typeparam>
    public interface IDeleteable<T> : IRepositoryController<T>
        where T : class, 
        INameable,
        IIdentifiable
    {
        /// <summary>
        /// Defines a delete method
        /// </summary>
        /// <param name="id">The id of the object to delete</param>
        /// <returns>An ActionResult</returns>
        ActionResult Delete(int id);
        ActionResult Delete(T model);
    }

    /// <summary>
    /// Extensions for IDeleteable methods
    /// </summary>
    public static class DeleteableExtensions
    {
        /// <summary>
        /// Validates the AntiForgery token and calls delete for the id on the Repository
        /// </summary>
        /// <typeparam name="T">A type that implements INameable and IIdentifiable</typeparam>
        /// <param name="controller">The controller being extended</param>
        /// <param name="id">The id of the object to be deleted</param>
        /// <returns>A redirect to the Index</returns>
        public static ActionResult Delete<T>(this IDeleteable<T> controller, int id) 
            where T : class, 
            INameable, 
            IIdentifiable
        {
            return Delete(controller, controller.Repository.Find(id));
        }

        /// <summary>
        /// Validates the AntiForgery token and calls delete for the id on the Repository
        /// </summary>
        /// <typeparam name="T">A type that implements INameable and IIdentifiable</typeparam>
        /// <param name="controller">The controller being extended</param>
        /// <param name="id">The id of the object to be deleted</param>
        /// <returns>A redirect to the Index</returns>
        public static ActionResult Delete<T>(this IDeleteable<T> controller, T model)
            where T : class, 
            INameable,
            IIdentifiable
        {
            AntiForgeryHelperAdapter.Validate();

            controller.Repository.Delete(model);
            controller.Repository.SaveChanges();

            return new RedirectToRouteResult("Default",
                new RouteValueDictionary(new { action = "Index" }));
        }
    }
}
