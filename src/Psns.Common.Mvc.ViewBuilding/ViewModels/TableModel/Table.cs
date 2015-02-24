using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel
{
    public class Table : ITableElement
    {
        public Table()
        {
            Rows = new Collection<Row>();
            Header = new Row();
        }

        public Row Header { get; set; }
        public Collection<Row> Rows { get; private set; }

        public void Accept(ITableVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
