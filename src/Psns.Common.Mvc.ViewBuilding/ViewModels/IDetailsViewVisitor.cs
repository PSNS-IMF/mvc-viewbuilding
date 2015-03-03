using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    /// <summary>
    /// Defines a visitor for the DetailsView model
    /// </summary>
    public interface IDetailsViewVisitor : ITableVisitor
    {
        /// <summary>
        /// Defines a method that receives the DetailsView being visited
        /// </summary>
        /// <param name="indexView">The DetailsView being visited</param>
        void Visit(DetailsView view);
    }
}
