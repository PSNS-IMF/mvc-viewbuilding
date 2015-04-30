using System;
using System.Web.Mvc;
using System.Reflection;

namespace Psns.Common.Mvc.ViewBuilding.Controllers
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class RequireRequestValueAttribute : ActionMethodSelectorAttribute
    {
        public readonly string ValueName;

        public RequireRequestValueAttribute(string valueName)
        {
            ValueName = valueName;
        }

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            return controllerContext.HttpContext.Request[ValueName] != null;
        }
    }
}
