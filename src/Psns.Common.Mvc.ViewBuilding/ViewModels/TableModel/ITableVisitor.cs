using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel
{
    public interface ITableVisitor
    {
        void Visit(Table table);
        void Visit(Row row);
        void Visit(Column column);
    }
}
