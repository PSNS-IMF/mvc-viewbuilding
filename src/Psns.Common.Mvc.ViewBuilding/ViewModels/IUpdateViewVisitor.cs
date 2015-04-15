using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psns.Common.Mvc.ViewBuilding.ViewModels
{
    public interface IUpdateViewVisitor
    {
        InputProperty Visit(InputProperty inputProperty);
    }
}
