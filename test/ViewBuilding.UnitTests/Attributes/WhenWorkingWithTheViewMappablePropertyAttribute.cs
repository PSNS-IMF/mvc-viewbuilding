using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Psns.Common.Test.BehaviorDrivenDevelopment;

using Psns.Common.Mvc.ViewBuilding.Attributes;

namespace ViewBuilding.UnitTests.Attributes.ViewMappablePropertyAttributes
{
    public class WhenWorkingWithTheViewMappablePropertyAttribute : BehaviorDrivenDevelopmentCaseTemplate
    {
        protected ViewDisplayablePropertyAttribute Attribute;

        public override void Arrange()
        {
            base.Arrange();

            Attribute = new ViewDisplayablePropertyAttribute(new DisplayViewTypes[] { DisplayViewTypes.Index });
        }
    }

    [TestClass]
    public class AndConstructing : WhenWorkingWithTheViewMappablePropertyAttribute
    {
        [TestMethod]
        public void ThenThePublicAttributesShouldBeInitializedProperly()
        {
            Assert.AreEqual<DisplayViewTypes>(DisplayViewTypes.Index, Attribute.DisplayViewTypes[0]);
        }
    }
}
