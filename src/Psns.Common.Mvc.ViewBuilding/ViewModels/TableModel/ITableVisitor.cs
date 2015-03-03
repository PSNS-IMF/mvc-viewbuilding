using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel
{
    /// <summary>
    /// Defines a visitor for the Table view model
    /// </summary>
    public interface ITableVisitor
    {
        /// <summary>
        /// Defines a method that visits the Table
        /// </summary>
        /// <param name="table">Table view model</param>
        void Visit(Table table);

        /// <summary>
        /// Defines a method that visits each Row of a Table
        /// </summary>
        /// <param name="row">A Row of the Table</param>
        void Visit(Row row);

        /// <summary>
        /// Defines a method that visits each Column of each Row of a Table
        /// </summary>
        /// <param name="column">A Column of a Row of a Table</param>
        void Visit(Column column);
    }
}
