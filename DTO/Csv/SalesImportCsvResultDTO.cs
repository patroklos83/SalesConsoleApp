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
        public StatisticPerYearDTO StatisticPerRangeOfYears { get; set; } = new StatisticPerYearDTO();
        public DateTime? MinDateFound { get; internal set; }
        public DateTime? MaxDateFound { get; internal set; }
    }

    internal class StatisticPerYearDTO
    {
        public DateTime? MinDateFound { get; set; }
        public DateTime? MaxDateFound { get; set; }
        public int count { get; set; }
        public decimal sum { get; set; }
        public decimal sumd { get; set; }
        public decimal average { get; set; }
        public decimal mean { get; set; }

        public List<double> records { get; set; } = new List<double>();
    }

}
