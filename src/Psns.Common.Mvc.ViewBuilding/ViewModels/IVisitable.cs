using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    public interface IVisitable
    {
        /// <summary>
        /// The object from which the element was built
        /// </summary>
        object Source { get; }
    }
}
