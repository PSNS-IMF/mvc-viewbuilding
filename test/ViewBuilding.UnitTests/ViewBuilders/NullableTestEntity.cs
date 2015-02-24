using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel.DataAnnotations;

using Psns.Common.Mvc.ViewBuilding.Attributes;
using Psns.Common.Mvc.ViewBuilding.Entities;
using Psns.Common.Persistence.Definitions;

namespace ViewBuilding.UnitTests.ViewBuilders
{
    public class NullableTestEntity : IIdentifiable
    {
        [ViewDisplayableProperty(new [] { DisplayViewTypes.Index })]
        [Display(Order = 1)]
        public int Id { get; set; }

        [ViewDisplayableProperty(new[] { DisplayViewTypes.Index })]
        [Display(Order = 0)]
        public string Name { get; set; }

        public string NotMapped { get; set; }
    }
}
