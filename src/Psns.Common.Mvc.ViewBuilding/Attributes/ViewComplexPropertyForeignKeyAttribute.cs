using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.Attributes
{
    /// <summary>
    /// Annotates the foreign key id used to represent a singular navigation property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ViewComplexPropertyForeignKeyAttribute : Attribute
    {
        /// <summary>
        /// The name of the property that this is a foreign key for
        /// </summary>
        public readonly string ForPropertyName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="forPropertyName">The name of the property that this is a foreign key for</param>
        public ViewComplexPropertyForeignKeyAttribute(string forPropertyName)
        {
            ForPropertyName = forPropertyName;
        }
    }
}
