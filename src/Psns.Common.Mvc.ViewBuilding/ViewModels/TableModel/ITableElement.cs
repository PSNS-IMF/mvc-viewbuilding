using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel
{
    public interface ITableElement
    {
        /// <summary>
        /// The object from which the element was built
        /// </summary>
        object Source { get; }

        void Accept(ITableVisitor visitor);
    }
}
