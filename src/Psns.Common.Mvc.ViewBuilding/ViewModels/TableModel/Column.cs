using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

using Psns.Common.Mvc.ViewBuilding.ViewModels;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel
{
    public class Column : ITableElement
    {
        public Column(PropertyInfo source)
        {
            Html = new AttributeDictionary();
            Source = source;
        }

        public object Value { get; set; }

        public AttributeDictionary Html { get; private set; }

        public void Accept(ITableVisitor visitor)
        {
            visitor.Visit(this);
        }

        public PropertyInfo Source { get; private set; }
    }
}
