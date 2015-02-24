using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel.DataAnnotations;

using Psns.Common.Mvc.ViewBuilding.Attributes;
using Psns.Common.Mvc.ViewBuilding.Entities;
using Psns.Common.Persistence.Definitions;

namespace ViewBuilding.UnitTests
{
    public class AnotherEntity : IIdentifiable
    {
        public int Id { get; set; }

        public string StringProperty { get; set; }
    }

    public class TestEntity : IIdentifiable, INameable
    {
        public TestEntity()
        {
            AnotherEntities = new List<AnotherEntity>();
        }

        [ViewDisplayableProperty(new [] { DisplayViewTypes.Index, DisplayViewTypes.Details })]
        [Display(Order = 1)]
        public int Id { get; set; }

        [Required]
        [ViewDisplayableProperty(new[] { DisplayViewTypes.Index, DisplayViewTypes.Details })]
        [ViewUpdatableProperty(InputPropertyType.String)]
        [Display(Order = 0)]
        public string Name { get; set; }

        [ViewDisplayableProperty(new[] { DisplayViewTypes.Index, DisplayViewTypes.Details })]
        [Display(Order = 2, Name = "Named Name")]
        [ViewUpdatableProperty(InputPropertyType.String)]
        public string NamedName { get; set; }

        [Display(Order = 3, Name = "Is In Use")]
        [ViewUpdatableProperty]
        public bool IsInUse { get; set; }

        public string NotMapped { get; set; }

        [Required]
        [Display(Order = 4, Name = "Another Entity")]
        [ViewComplexProperty("StringProperty", "Id")]
        [ViewDisplayableProperty(new [] { DisplayViewTypes.Index, DisplayViewTypes.Details })]
        [ViewUpdatableProperty]
        public ICollection<AnotherEntity> AnotherEntities { get; set; }

        [Display(Order = 5)]
        [ViewDisplayableProperty(new DisplayViewTypes[] { DisplayViewTypes.Index, DisplayViewTypes.Details })]
        [ViewComplexProperty("StringProperty", "Id")]
        [ViewUpdatableProperty]
        public AnotherEntity AnotherEntity { get; set; }

        [Display(Order = 6, Name = "Other Another Entity")]
        [ViewComplexProperty("StringProperty", "Id")]
        [ViewDisplayableProperty(new[] { DisplayViewTypes.Index, DisplayViewTypes.Details })]
        [ViewUpdatableProperty]
        public ICollection<AnotherEntity> OtherAnotherEntities { get; set; }

        [Required]
        [Display(Order = 7)]
        [ViewDisplayableProperty(new DisplayViewTypes[] { DisplayViewTypes.Index })]
        [ViewComplexProperty("StringProperty", "Id")]
        [ViewUpdatableProperty]
        public AnotherEntity RequiredEntity { get; set; }

        [Display(Order = 8)]
        [ViewComplexPropertyForeignKey("ForeignKey")]
        [ViewDisplayableProperty(new DisplayViewTypes[] { DisplayViewTypes.Index, DisplayViewTypes.Details })]
        [ViewComplexProperty("StringProperty", "Id")]
        [ViewUpdatableProperty]
        public int? ForeignKeyId { get; set; }
        public AnotherEntity ForeignKey { get; set; }
    }
}
