using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    /// <summary>
    /// Defines a visitory for the IndexView model
    /// </summary>
    public interface IIndexViewVisitor : ITableVisitor
    {
        /// <summary>
        /// Defines a method that get the IndexView being visited
        /// </summary>
        /// <param name="indexView">The IndexView being visited</param>
        void Visit(IndexView indexView);
    }
}
