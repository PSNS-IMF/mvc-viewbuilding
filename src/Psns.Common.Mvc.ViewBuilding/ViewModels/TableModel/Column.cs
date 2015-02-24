using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Psns.Common.Mvc.ViewBuilding.ViewModels;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel
{
    public class Column : ITableElement
    {
        public Column()
        {
            Html = new AttributeDictionary();
        }

        public object Value { get; set; }

        public AttributeDictionary Html { get; private set; }

        public void Accept(ITableVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
