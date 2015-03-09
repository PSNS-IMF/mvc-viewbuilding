using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel
{
    public class Table : ITableElement
    {
        public Table(object source)
        {
            Rows = new Collection<Row>();
            Header = new Row(source);
            Source = source;
        }

        public Row Header { get; set; }
        public Collection<Row> Rows { get; private set; }

        public void Accept(ITableVisitor visitor)
        {
            visitor.Visit(this);
        }

        public object Source { get; private set; }
    }
}
