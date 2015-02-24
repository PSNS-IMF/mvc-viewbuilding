using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    public class DetailsView : View
    {
        public DetailsView()
        {
            Table = new Table();
        }

        public readonly Table Table;
    }
}
