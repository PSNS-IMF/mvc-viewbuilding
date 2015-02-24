using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    public class UpdateView : View
    {
        public Collection<InputProperty> InputProperties { get; private set; }

        public UpdateView()
        {
            InputProperties = new Collection<InputProperty>();
        }

        public Routeable Form { get; set; }
    }
}
