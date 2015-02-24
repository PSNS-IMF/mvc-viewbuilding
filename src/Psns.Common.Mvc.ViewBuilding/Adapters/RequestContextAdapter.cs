using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Psns.Common.Mvc.ViewBuilding.Adapters
{
    /// <summary>
    /// An adapter for the HttpContext.Current.Request.RequestContext to assist with testing
    /// </summary>
    public class RequestContextAdapter
    {
        static RequestContext _context;

        /// <summary>
        /// Unless explicitly set, HttpContext.Current.Request.RequestContext is returned
        /// </summary>
        public static RequestContext Context
        {
            get
            {
                return _context ?? HttpContext.Current.Request.RequestContext;
            }

            set { _context = value; }
        }
    }
}
