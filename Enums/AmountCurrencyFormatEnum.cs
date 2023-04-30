using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesConsoleApp.Enums
{
    internal enum AmountCurrencyFormatEnum
    {
        [Description("None")]
        None = 0,
        [Description(@"€")]
        Euro = 1,
        [Description(@"$")]
        Dollar = 2,
        [Description(@"£")]
        BritishPound = 3
    }
}
