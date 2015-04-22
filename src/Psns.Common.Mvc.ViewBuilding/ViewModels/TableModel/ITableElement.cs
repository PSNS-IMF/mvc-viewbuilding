using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel
{
    public interface ITableElement
    {
        void Accept(ITableVisitor visitor);
    }
}
