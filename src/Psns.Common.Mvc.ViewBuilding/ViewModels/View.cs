using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    public class View
    {
        Collection<Routeable> _contextItems;

        public string Title { get; set; }

        public Collection<Routeable> ContextItems
        {
            get
            {
                if(_contextItems == null)
                    _contextItems = new Collection<Routeable>();

                return _contextItems;
            }

            private set
            {
                _contextItems = value;
            }
        }
    }
}
