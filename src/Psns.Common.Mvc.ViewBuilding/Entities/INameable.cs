using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.Entities
{
    /// <summary>
    /// Represents an object with a string property called Name
    /// </summary>
    public interface INameable
    {
        /// <summary>
        /// The Name of the INameable
        /// </summary>
        string Name { get; }
    }
}
