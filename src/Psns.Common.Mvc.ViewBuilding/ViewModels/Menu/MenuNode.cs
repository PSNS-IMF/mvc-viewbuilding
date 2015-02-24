using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.Menu
{
    /// <summary>
    /// A node within a drop-down menu
    /// </summary>
    public class MenuNode
    {
        public MenuNode()
        {
            Children = new Collection<MenuNode>();
            Url = "#";
        }

        /// <summary>
        /// Text to be displayed
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Text for Html Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Url to link to
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Html classes used to display an icon instead of text
        /// </summary>
        public string IconHtmlClasses { get; set; }

        /// <summary>
        /// Child nodes to be display as a sub-menu to this node
        /// </summary>
        public Collection<MenuNode> Children { get; set; }
    }
}
