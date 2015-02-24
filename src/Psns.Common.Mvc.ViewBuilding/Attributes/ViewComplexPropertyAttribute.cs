using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.Attributes
{
    /// <summary>
    /// Provides information needed by the CrudViewBuilder to map reference type attributes
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ViewComplexPropertyAttribute : Attribute
    {
        /// <summary>
        /// The name of the property that will be used as the label when mapping
        /// </summary>
        public readonly string LabelPropertyName;

        /// <summary>
        /// The name of the property that will be used as the value when mapping
        /// </summary>
        public readonly string ValuePropertyName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="labelPropertyName">The name of the property that will be used as the label when mapping</param>
        /// <param name="valuePropertyName">The name of the property that will be used as the value when mapping</param>
        public ViewComplexPropertyAttribute(string labelPropertyName, 
            string valuePropertyName)
        {
            LabelPropertyName = labelPropertyName;
            ValuePropertyName = valuePropertyName;
        }
    }
}
