using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesConsoleApp.Enums
{
    internal enum DateFormatEnum
    {
        None = 0,
        [Description("dd/MM/yyyy")]
        DateFormat1 = 1,
        [Description("dd-MM-yyyy")]
        DateFormat2 = 2,
        [Description("yyyy-MM-dd")]
        DateFormat3 = 3
    }
}
