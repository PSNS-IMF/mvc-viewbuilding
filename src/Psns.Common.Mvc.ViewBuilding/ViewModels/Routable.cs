using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    public class Routeable
    {
        private AttributeDictionary _html, _routeValues;

        public Routeable()
        {
            RouteName = string.Empty;
        }

        public string RouteName { get; set; }
        public FormMethod FormMethod { get; set; }

        public AttributeDictionary RouteValues
        {
            get
            {
                if (_routeValues == null)
                    _routeValues = new AttributeDictionary();

                return _routeValues;
            }
            set
            {
                _routeValues = value;
            }
        }
        public AttributeDictionary Html
        {
            get
            {
                if (_html == null)
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