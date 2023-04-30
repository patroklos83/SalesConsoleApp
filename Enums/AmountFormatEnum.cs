using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesConsoleApp.Enums
{
    internal enum AmountFormatEnum
    {
        None = 0,
        [Description("XXXXX")]
        AmountFormat1 = 1,
        [Description("XXXXX.XX")]
        AmountFormat2 = 2,
        [Description("X,XXXXX.XX")]
        AmountFormat3 = 3,
        [Description("XXXXX,XX")]
        AmountFormat4 = 4
    }
}
