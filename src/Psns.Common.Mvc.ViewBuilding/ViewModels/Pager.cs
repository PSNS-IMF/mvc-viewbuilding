using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    public class Pager
    {
        AttributeDictionary _html;

        public ActionModel First { get; set; }
        public ActionModel Previous { get; set; }
        public ActionModel PagerState { get; set; }
        public ActionModel Next { get; set; }
        public ActionModel Last { get; set; }

        public AttributeDictionary Html
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
