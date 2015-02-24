using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    public enum ActionType
    {
        Button,
        Link
    }

    public class ActionModel : Routeable
    {
        Collection<string> _iconHtmlClasses;

        public ActionType Type { get; set; }
        public string Text { get; set; }

        public Collection<string> IconHtmlClasses
        {
            get
            {
                if(_iconHtmlClasses == null)
                    _iconHtmlClasses = new Collection<string>();

                return _iconHtmlClasses;
            }

            private set
            {
                _iconHtmlClasses = value;
            }
        }
    }
}
