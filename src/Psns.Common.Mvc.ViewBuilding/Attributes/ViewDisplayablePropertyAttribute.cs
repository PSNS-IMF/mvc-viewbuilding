using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.Attributes
{
    /// <summary>
    /// Used to annotate which view a property should be displayed on
    /// </summary>
    public enum DisplayViewTypes
    {
        Index,
        Details
    }

    /// <summary>
    /// Configure how a property should be displayed by the View Mapper
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ViewDisplayablePropertyAttribute : Attribute
    {
        public readonly DisplayViewTypes[] DisplayViewTypes;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="viewTypes">Which views this property should be displayed on</param>
        public ViewDisplayablePropertyAttribute(params DisplayViewTypes[] viewTypes)
        {
            DisplayViewTypes = viewTypes;
        }
    }
}
