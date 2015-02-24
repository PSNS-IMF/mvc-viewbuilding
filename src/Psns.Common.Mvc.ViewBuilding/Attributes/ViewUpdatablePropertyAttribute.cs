using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.Attributes
{
    /// <summary>
    /// Used to display a property on create and update views
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ViewUpdatablePropertyAttribute : Attribute
    {
        public readonly InputPropertyType InputPropertyType;

        public ViewUpdatablePropertyAttribute(InputPropertyType inputPropertyType = InputPropertyType.Basic)
        {
            InputPropertyType = inputPropertyType;
        }
    }
}
