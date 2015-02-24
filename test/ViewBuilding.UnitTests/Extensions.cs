using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewBuilding.UnitTests
{
    public static class Extensions
    {
        public static int Count(this IEnumerable items)
        {
            int total = 0;
            foreach(var item in items)
            {
                total++;
            }

            return total;
        }
    }
}
