using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Psns.Common.Persistence.Definitions;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel
{
    public class Row : ITableElement
    {
        public Row(IIdentifiable source)
        {
            Html = new AttributeDictionary();
            Columns = new Collection<Column>();
            Source = source;
        }

        public int Id { get; set; }

        public AttributeDictionary Html { get; private set; }
        public Collection<Column> Columns { get; private set; }

        public void Accept(ITableVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IIdentifiable Source { get; private set; }
    }
}
