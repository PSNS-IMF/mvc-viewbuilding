using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Psns.Common.Mvc.ViewBuilding.Attributes;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    public class InputProperty : IVisitable
    {
        public InputPropertyType Type { get; set; }
        public string Label { get; set; }
        public string ModelName { get; set; }
        public object Value { get; set; }
        public object Source { get; set; }
    }
}
