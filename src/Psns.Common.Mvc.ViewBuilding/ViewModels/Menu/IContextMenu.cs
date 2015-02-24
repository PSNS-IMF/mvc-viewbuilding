using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.Menu
{
    /// <summary>
    /// Defines the application specific context menu area
    /// </summary>
    public interface IContextMenu
    {
        /// <summary>
        /// The title of the application
        /// </summary>
        string Title { get; }
    }

    public interface IContextMenuWithDropDowns : IContextMenu
    {
        /// <summary>
        /// Defines application specific drop-down menus
        /// </summary>
        IEnumerable<IMenuItem> MenuItems { get; }
    }
}
