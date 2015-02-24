using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.Attributes
{
    /// <summary>
    /// Represents types of properties that will be displayed on an update view
    /// </summary>
    public enum InputPropertyType
    {
        /// <summary>
        /// An example is a bool
        /// </summary>
        Basic,
        /// <summary>
        /// Represents a simple text box input
        /// </summary>
        String
    }
}
