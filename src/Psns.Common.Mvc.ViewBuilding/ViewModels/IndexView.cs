using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    public class IndexView : View
    {
        public IndexView(string modelName)
        {
            Table = new Table();
            Pager = new Pager();
            SearchControl = new SearchControl();
            ModelName = modelName;
        }

        public readonly Table Table;
        public readonly Pager Pager;

        public readonly string ModelName;

        public readonly SearchControl SearchControl;
    }
}
