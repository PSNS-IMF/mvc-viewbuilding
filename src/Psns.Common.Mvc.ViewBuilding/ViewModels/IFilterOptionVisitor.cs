using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Psns.Common.Persistence.Definitions;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    public interface IFilterOptionVisitor
    {
        /// <summary>
        /// Defines a method that visits an IIdentifiable type
        /// </summary>
        /// <param name="item">The item to visit</param>
        /// <returns>Returns a potentially modified version of item</returns>
        IIdentifiable Visit(IIdentifiable item);
    }
}
