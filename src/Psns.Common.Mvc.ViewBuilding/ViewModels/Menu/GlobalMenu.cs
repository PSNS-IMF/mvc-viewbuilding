using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.Menu
{
    /// <summary>
    /// Provides top-level menu content to be rendered in PSNSLayout
    /// </summary>
    public static class GlobalMenu
    {
        private static List<IMenuItem> _globalMenuItems;

        /// <summary>
        /// Provides content for top-level drop-down menus
        /// </summary>
        public static ICollection<IMenuItem> GlobalMenuItems
        {
            get
            {
                if(_globalMenuItems == null)
                    _globalMenuItems = new List<IMenuItem>();

                return _globalMenuItems;
            }
        }

        /// <summary>
        /// Provides application specific menu items to be rendered in PSNSLayout ContextActions section
        /// </summary>
        public static IContextMenu ContextMenu { get; set; }
    }
}
