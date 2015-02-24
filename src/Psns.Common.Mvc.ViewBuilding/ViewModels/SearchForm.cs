using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    public class SearchControl
    {
        AttributeDictionary _html;

        public SearchControl()
        {
            Button = new ActionModel();
        }

        public string Query { get; set; }

        public ActionModel Button { get; set; }

        public AttributeDictionary InputHtml
        {
            get
            {
                if(_html == null)
                    _html = new AttributeDictionary();

                return _html;
            }
            set
            {
                _html = value;
            }
        }
    }
}
