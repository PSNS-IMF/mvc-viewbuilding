using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    /// <summary>
    /// Contains the element for a common index view
    /// </summary>
    public class IndexView : View
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="modelName">The name of the model being represented in the index view</param>
        public IndexView(string modelName)
        {
            Table = new Table();
            Pager = new Pager();
            SearchControl = new SearchControl();
            ModelName = modelName;
        }

        /// <summary>
        /// Represents the Create New Item button on the Index View
        /// </summary>
        public ActionModel CreateButton { get; set; }

        public readonly Table Table;
        public readonly Pager Pager;

        public readonly string ModelName;

        public readonly SearchControl SearchControl;

        /// <summary>
        /// Accept an IIndexViewVisitor and pass it this
        /// </summary>
        /// <param name="visitor">IIndexViewVisitor</param>
        public void Accept(IIndexViewVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
