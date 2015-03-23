using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    public class FilterOption
    {
        public string Label { get; set; }
        public IEnumerable<FilterOption> Children { get; set; }
    }
}