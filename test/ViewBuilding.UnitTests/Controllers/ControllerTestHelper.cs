using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.IO;

using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Moq;

namespace ViewBuilding.UnitTests.Controllers
{
    public class ControllerTestHelper
    {
        public static void SetContext(Controller controller, params string[] viewNamesInContext)
        {
            SetupViewContent(viewNamesInContext);

            var mockControllerContext = new Mock<HttpContextBase>();
            var httpRequestBase = new Mock<HttpRequestBase>();
            var response = new Mock<HttpResponseBase>();
            var session = new Mock<HttpSessionStateBase>();

            //var routes = new RouteCollection();
            //RouteConfigurator.RegisterRoutes(routes);

            //if(isAjax)
            //    httpRequestBase.SetupGet(x => x.Headers).Returns(new System.Net.WebHeaderCollection { "X-Requested-With:XMLHttpRequest" });
            //else
            //    httpRequestBase.SetupGet(x => x.Headers).Returns(new System.Net.WebHeaderCollection { "X-Requested-With:NoAjax" });
            //httpRequestBase.SetupGet(x => x.QueryString).Returns(new NameValueCollection());

            mockControllerContext.Setup(x => x.Response).Returns(response.Object);
            mockControllerContext.Setup(x => x.Request).Returns(httpRequestBase.Object);
            mockControllerContext.Setup(x => x.Session).Returns(session.Object);
            //Mock<IIdentity> mockIdentity = new Mock<IIdentity>();
            //mockIdentity.Setup(i => i.Name).Returns("domain\\username");
            //Mock<IPrincipal> mockPrincipal = new Mock<IPrincipal>();
            //mockPrincipal.Setup(p => p.Identity).Returns(mockIdentity.Object);
            //mockControllerContext.Setup(x => x.User).Returns(mockPrincipal.Object);

            //session.Setup(x => x["somesessionkey"]).Returns("value");
            httpRequestBase.Setup(x => x.Form).Returns(new NameValueCollection());
            httpRequestBase.Setup(x => x.Params).Returns(new NameValueCollection());
            httpRequestBase.Setup(x => x.Url).Returns(new Uri("http://test.com:80"));
            httpRequestBase.Setup(x => x.UrlReferrer).Returns(new Uri("http://test.com"));
            controller.ControllerContext = new ControllerContext(mockControllerContext.Object, new RouteData(), controller);
            //controller.ControllerContext.RouteData.Values.Add("controller", "Users");
            //controller.Url = new UrlHelper(new RequestContext(controller.HttpContext, new RouteData()), routes);
        }

        public static ViewEngineResult SetupViewContent(params string[] viewNamesInContext)
        {
            var mockedViewEngine = new Mock<IViewEngine>();
            var resultView = new Mock<IView>();

            resultView.Setup(x => x.Render(It.IsAny<ViewContext>(), It.IsAny<TextWriter>()))
                .Callback<ViewContext, TextWriter>((v, t) =>
                {
                    t.Write(string.Empty);
                });

            var viewEngineResult = new ViewEngineResult(resultView.Object, mockedViewEngine.Object);

            foreach(var name in viewNamesInContext)
            {
                mockedViewEngine.Setup(x => x.FindPartialView(It.IsAny<ControllerContext>(), name, It.IsAny<bool>()))
                    .Returns<ControllerContext, string, bool>((controller, view, useCache) =>
                    {
                        return viewEngineResult;
                    });

                mockedViewEngine.Setup(x => x.FindView(It.IsAny<ControllerContext>(), name, It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<ControllerContext, string, string, bool>((controller, view, masterName, useCache) =>
                    {
                        return viewEngineResult;
                    });
            }

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(mockedViewEngine.Object);
            return viewEngineResult;
        }
    }
}
