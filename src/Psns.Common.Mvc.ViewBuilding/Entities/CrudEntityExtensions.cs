using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using System.ComponentModel.DataAnnotations;

using Psns.Common.Persistence.Definitions;
using Psns.Common.Mvc.ViewBuilding.Attributes;

namespace Psns.Common.Mvc.ViewBuilding.Entities
{
    public static class CrudEntityExtensions
    {
        /// <summary>
        /// Gets all PropertyInfos of an object that implement IEnumerable
        /// </summary>
        /// <param name="obj">The object</param>
        /// <returns>IEnumerable<PropertyInfo></returns>
        public static IEnumerable<PropertyInfo> GetEnumerableProperties(this object obj)
        {
            return obj
                .GetType()
                .GetProperties()
                .Where(p => p.PropertyType.IsGenericType &&
                    p.PropertyType.GetInterfaces().Any(x => x == typeof(IEnumerable)));
        }

        /// <summary>
        /// Gets all property names of an object that are decorated with ViewComplexPropertyAttribute or ViewComplexPropertyForeignKeyAttribute.
        /// </summary>
        /// <param name="properties">A list of PropertyInfo object to query</param>
        /// <returns>string[]</returns>
        public static string[] GetComplexPropertyNames(IOrderedEnumerable<PropertyInfo> properties)
        {
            return properties.Aggregate<PropertyInfo, List<string>>(new List<string>(), (List<string> sum, PropertyInfo property) =>
            {
                if(property.GetCustomAttributes(typeof(ViewComplexPropertyAttribute), false).Length > 0)
                {
                    var fkAttribute = (property.GetCustomAttributes(typeof(ViewComplexPropertyForeignKeyAttribute), false) as ViewComplexPropertyForeignKeyAttribute[]).SingleOrDefault();
                    if(fkAttribute != null)
                        sum.Add(fkAttribute.ForPropertyName);
                    else
                        sum.Add(property.Name);
                }

                return sum;
            }).ToArray();
        }

        /// <summary>
        /// Get PropertInfos of a Type that are decorated with ViewUpdatablePropertyAttribute.
        /// </summary>
        /// <param name="type">The Type to be tested</param>
        /// <returns>IOrderedEnumerable<PropertyInfo></returns>
        public static IOrderedEnumerable<PropertyInfo> GetUpdateProperties(this Type type)
        {
            int displayOrderCount = 0;

            var indexProperties = type
                .GetProperties()
                .Where(p => (p.GetCustomAttributes(typeof(ViewUpdatablePropertyAttribute), false) as ViewUpdatablePropertyAttribute[])
                            .Any())
                .OrderBy(p => GetPropertyOrder(p, displayOrderCount));
            return indexProperties;
        }

        /// <summary>
        /// Gets the order of a given PropertyInfo as annotated by a DisplayAttribute.Order.
        /// </summary>
        /// <param name="property">The given PropertyInfo</param>
        /// <param name="displayOrderCount">An integer with a pre-existing order value.</param>
        /// <returns>int</returns>
        public static int GetPropertyOrder(this PropertyInfo property, int displayOrderCount)
        {
            var displayAttribute = property.GetCustomAttributes(typeof(DisplayAttribute), false).SingleOrDefault();

            if(displayAttribute != null)
                displayOrderCount = (displayAttribute as DisplayAttribute).Order;

            return displayOrderCount++;
        }
    }
}
