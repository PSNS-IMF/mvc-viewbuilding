using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.Menu
{
    /// <summary>
    /// Defines a drop-down menu
    /// </summary>
    public interface IMenuItem
    {
        /// <summary>
        /// The root of a drop-down menu
        /// </summary>
        MenuNode RootNode { get; }
    }
}
