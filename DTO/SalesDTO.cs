using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesConsoleApp.DTO
{
    public class SalesDTO
    {
        public DateTime? Date { get; set; }
        public decimal? Amount { get; set; }
    }
}
