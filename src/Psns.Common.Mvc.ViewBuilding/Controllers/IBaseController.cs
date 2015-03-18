using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using Psns.Common.Mvc.ViewBuilding.ViewBuilders;
using Psns.Common.Persistence.Definitions;

namespace Psns.Common.Mvc.ViewBuilding.Controllers
{
    public interface IBaseController
    {
        ICrudViewBuilder Builder { get; }
    }

    public interface IRepositoryController<T> : IBaseController where T : class, IIdentifiable
    {
        IRepository<T> Repository { get; }
        ModelStateDictionary ModelState { get; }
        ControllerContext ControllerContext { get; }
        ViewDataDictionary ViewData { get; }
        TempDataDictionary TempData { get; }
        ViewEngineCollection ViewEngineCollection { get; }
    }
}