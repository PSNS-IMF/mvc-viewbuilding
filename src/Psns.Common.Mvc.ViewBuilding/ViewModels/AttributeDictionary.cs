using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Reflection;
using System.Security.Permissions;
using System.Runtime.Serialization;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    public class AttributeDictionary : RouteValueDictionary
    {
        public AttributeDictionary() { }

        public AttributeDictionary(object values)
        {
            Add(values);
        }

        public new object this[string val]
        {
            get
            {
                if (this.ContainsKey(val))
                    return base[val];
                else
                    return string.Empty;
            }

            set
            {
                base[val] = value;
            }
        }

        public void Add(object values)
        {
            var propertyInfo = values.GetType().GetProperties();

            foreach (var property in propertyInfo)
                Add(property.Name, property.GetValue(values, null));
        }

        public override bool Equals(object obj)
        {
            var iterator = ((AttributeDictionary)obj).GetEnumerator();
            while (iterator.MoveNext())
            {
                if (!this.ContainsKey(iterator.Current.Key) ||
                    !this[iterator.Current.Key].Equals(iterator.Current.Value))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            if (this.Count < 1)
                return string.Empty;

            var builder = new StringBuilder();
            builder.Append("{ ");

            var iterator = this.GetEnumerator();
            while (iterator.MoveNext())
            {
                if (iterator.Current.Value.ToString() != string.Empty)
                {
                    builder.AppendFormat("{0} = {1}", iterator.Current.Key, iterator.Current.Value);
                    if (!iterator.Current.Equals(this.Last()))
                        builder.Append(", ");
                }
            }

            builder.Append(" }");
            return builder.ToString();
        }

        public string ToHtmlString()
        {
            if (this.Count < 1)
                return string.Empty;

            var builder = new StringBuilder();

            var iterator = this.GetEnumerator();
            while (iterator.MoveNext())
            {
                if ( iterator.Current.Value != null && iterator.Current.Value.ToString() != string.Empty)
                {
                    builder.AppendFormat("{0}=\"{1}\"", iterator.Current.Key, iterator.Current.Value);
                    if (!iterator.Current.Equals(this.Last()))
                        builder.Append(" ");
                }
            }

            return builder.ToString();
        }
    }
}