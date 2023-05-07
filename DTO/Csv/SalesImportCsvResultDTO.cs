using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SalesConsoleApp.Utility.StatisticsUtil;

namespace SalesConsoleApp.DTO.Csv
{
    internal class SalesImportCsvResultDTO
    {
        public Metrics Metrics { get; set; }
        public SortedDictionary<int, StatisticPerYearDTO> StatisticPerYear { get; set; }
        public StatisticPerYearDTO StatisticSpecificDateRange { get; set; }
    }

    internal class StatisticPerYearDTO
    {
        public Metrics Metrics { get; set; }
    }

    public class Metrics
    {
        public DateTime? MinDateFound { get; set; }
        public DateTime? MaxDateFound { get; set; }
        public int count { get; set; }
        public int TotalRowCount { get; set; }
        public decimal sum { get; set; }
        public decimal sumd { get; set; }
    }
}
