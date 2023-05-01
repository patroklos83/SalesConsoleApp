using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesConsoleApp.DTO.Csv
{
    internal class SalesImportCsvResultDTO
    {
        public int count { get; set; }
        public decimal sum { get; set; }
        public decimal sumd { get; set; }
        public decimal average { get; set; }
        public decimal mean { get; set; }

        public List<double> records { get; set; } = new List<double>();

        public SortedDictionary<int, StatisticPerYearDTO> StatisticPerYear { get; set; } = new SortedDictionary<int, StatisticPerYearDTO>();
    }

    internal class StatisticPerYearDTO
    {
        public int count { get; set; }
        public decimal sum { get; set; }
        public decimal sumd { get; set; }
        public decimal average { get; set; }
        public decimal mean { get; set; }

        public List<double> records { get; set; } = new List<double>();
    }

}
