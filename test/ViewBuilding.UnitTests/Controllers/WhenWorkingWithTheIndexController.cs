using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Psns.Common.Test.BehaviorDrivenDevelopment;
using Psns.Common.Mvc.ViewBuilding.Controllers;
using Psns.Common.Mvc.ViewBuilding.ViewBuilders;
using Psns.Common.Mvc.ViewBuilding.ViewModels;

using Moq;

namespace ViewBuilding.UnitTests.Controllers.Index
{
    public class WhenWorkingWithTheIndexController : BehaviorDrivenDevelopmentCaseTemplate
    {
        protected IndexController Controller;
        protected Mock<ICrudViewBuilder> MockViewBuilder;

        public override void Arrange()
        {
            base.Arrange();

            MockViewBuilder = new Mock<ICrudViewBuilder>();

            Controller = new IndexController(MockViewBuilder.Object);
        }
    }

    [TestClass]
    public class AndGettingFilterOptions : WhenWorkingWithTheIndexController
    {
        IEnumerable<FilterOption> _results;

        public override void Arrange()
        {
            base.Arrange();

            MockViewBuilder.Setup(b => b.GetIndexFilterOptions<TestEntity>(null)).Returns(new List<FilterOption>
            {
                new FilterOption  
                { 
                    Label = "Column Name", 
                    Children = new List<FilterOption> 
                    { 
                        new FilterOption { Label = "Option One" } 
                    } 
                }
            });
        }

        public override void Act()
        {
            base.Act();

            _results = Controller.GetFilterOptions(typeof(TestEntity).AssemblyQualifiedName);
        }

        [TestMethod]
        public void ThenTheFilterOptionsShouldBeReturned()
        {
            Assert.AreEqual("Column Name", _results.ElementAt(0).Label);
        }
    }
}
