using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    /// <summary>
    /// A View with a Table that is used for a typical details view page
    /// </summary>
    public class DetailsView : View
    {
        public DetailsView()
        {
            Table = new Table(null);
        }

        public readonly Table Table;

        /// <summary>
        /// Accept an IDetailsViewVisitor and pass it this
        /// </summary>
        /// <param name="visitor">IDetailsViewVisitor</param>
        public void Accept(IDetailsViewVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
