using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

using Psns.Common.Test.BehaviorDrivenDevelopment;
using Psns.Common.Mvc.ViewBuilding.Controllers;

using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Moq;

namespace ViewBuilding.UnitTests.Controllers
{
    public class WhenWorkingWorkingWithTheRequireRequestValueAttribute : BehaviorDrivenDevelopmentCaseTemplate
    {
        protected RequireRequestValueAttribute Attribute;
        protected bool IsValidForRequest;
        protected ControllerContext Context;
        protected Mock<HttpContextBase> MockHttpContext;

        protected const string ValueName = "RequestedValue";

        public override void Arrange()
        {
            base.Arrange();

            MockHttpContext = new Mock<HttpContextBase>();
            MockHttpContext.Setup(ctx => ctx.Request[ValueName]).Returns(() => null);

            Context = new ControllerContext(MockHttpContext.Object, 
                new RouteData(), 
                new Mock<ControllerBase>().Object);

            Attribute = new RequireRequestValueAttribute(ValueName);
        }

        public override void Act()
        {
            base.Act();

            IsValidForRequest = Attribute.IsValidForRequest(Context, null);
        }
    }

    [TestClass]
    public class AndTheRequestDoesNotContainTheValueName : WhenWorkingWorkingWithTheRequireRequestValueAttribute
    {
        [TestMethod]
        public void ThenRequestShouldNotBeValid()
        {
            Assert.IsFalse(IsValidForRequest);
        }
    }

    [TestClass]
    public class AndTheRequestDoesContainTheValueName : WhenWorkingWorkingWithTheRequireRequestValueAttribute
    {
        public override void Arrange()
        {
            base.Arrange();

            MockHttpContext.Setup(ctx => ctx.Request[ValueName]).Returns("value");
        }

        [TestMethod]
        public void ThenRequestShouldBeValid()
        {
            Assert.IsTrue(IsValidForRequest);
        }
    }
}
