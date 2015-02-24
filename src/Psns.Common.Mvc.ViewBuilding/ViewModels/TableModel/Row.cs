using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel
{
    public class Row : ITableElement
    {
        public Row()
        {
            Html = new AttributeDictionary();
            Columns = new Collection<Column>();
        }

        public int Id { get; set; }

        public AttributeDictionary Html { get; private set; }
        public Collection<Column> Columns { get; private set; }

        public void Accept(ITableVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
