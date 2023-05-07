using SalesConsoleApp.DTO.Csv;
using SalesConsoleApp.Process;
using SalesConsoleApp.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SalesConsoleApp.UserInterface
{
    internal class ConsoleOutput
    {
        internal static void DisplayStatistics(SalesImportCsvInputDTO salesImportCsvInputDTO, SalesImportCsvResultDTO salesImportCscResult)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Sales Import CSV files Statistics");
            Console.WriteLine("=======================================================");
            Console.WriteLine();

            DisplayTotalStandardDeviation(salesImportCscResult);
            
            Console.WriteLine();

            DisplayStandardDeviationPerYear(salesImportCscResult);

            Console.WriteLine();

            if (salesImportCsvInputDTO.Date.IsExportResultsFromToDate)
                DisplayStatisticsForDateRange(salesImportCsvInputDTO, salesImportCscResult);

            DisplayAverageEarningsPerYear(salesImportCscResult);
        }

        private static void DisplayTotalStandardDeviation(SalesImportCsvResultDTO salesImportCscResult)
        {
            string fromToDate = string.Empty;
            if (salesImportCscResult.Metrics.MinDateFound.HasValue && salesImportCscResult.Metrics.MaxDateFound.HasValue)
            {
                 fromToDate  = string.Format("From Date: [{0}]. To Date: [{1}]",
                    salesImportCscResult.Metrics.MinDateFound.Value.ToString(ReadSalesProcess.DateFormat),
                    salesImportCscResult.Metrics.MaxDateFound.Value.ToString(ReadSalesProcess.DateFormat));
            }

            Console.WriteLine();
            Console.WriteLine("Standard deviation of earnings [{0}]", fromToDate);
            double standardDeviation = StatisticsUtil.GetStandardDeviation(salesImportCscResult.Metrics.sumd, salesImportCscResult.Metrics.sum, salesImportCscResult.Metrics.count);

            string rangeOfYears = string.Join("/", salesImportCscResult.StatisticPerYear.Keys);
            Console.WriteLine(string.Format("Standard Deviation = {0} [For years: {1}] [Unique Dates Count: {2}]", standardDeviation, rangeOfYears, salesImportCscResult.Metrics.count));           
        }

        private static void DisplayStandardDeviationPerYear(SalesImportCsvResultDTO salesImportCscResult)
        {
            double standardDeviation = 0;

            Console.WriteLine();
            Console.WriteLine("Standard Deviation per year");
            Console.WriteLine("=============================================================");
            foreach (var yearEntry in salesImportCscResult.StatisticPerYear)
            {
                var year = yearEntry.Key;
                var statistics = yearEntry.Value;
                standardDeviation = StatisticsUtil.GetStandardDeviation(statistics.Metrics.sumd, statistics.Metrics.sum, statistics.Metrics.count);
                Console.WriteLine(string.Format("Standard Deviation = {0} [For year: {1}] [Unique Dates Count: {2}]", standardDeviation, year, statistics.Metrics.count));
            }
        }

        private static void DisplayAverageEarningsPerYear(SalesImportCsvResultDTO salesImportCscResult)
        {
            Console.WriteLine();
            Console.WriteLine("Average Earnings per year");
            Console.WriteLine("=============================================================");
            foreach (var yearEntry in salesImportCscResult.StatisticPerYear)
            {
                var year = yearEntry.Key;
                var statistics = yearEntry.Value;
                decimal averageEarnings = StatisticsUtil.GetAverage(statistics.Metrics.sum, statistics.Metrics.count);
                Console.WriteLine(string.Format("Average Sales Earnings = {0} [For year: {1}] [Unique Dates Count: {2}]", averageEarnings, year, statistics.Metrics.count));
            }
        }

        private static void DisplayStatisticsForDateRange(SalesImportCsvInputDTO salesImportCsvInputDTO, SalesImportCsvResultDTO salesImportCscResult)
        {
            string strDateRangeTitle = "Statistics for a specific range of Dates";

            var fromDateUserInput = salesImportCsvInputDTO.Date.FromDate.Value.ToString(DateTimeUtil.DATE_FORMAT);
            var toDateUserInput = salesImportCsvInputDTO.Date.ToDate.Value.ToString(DateTimeUtil.DATE_FORMAT);
            string userInputFromToDate = string.Format("{0} - {1}", fromDateUserInput, toDateUserInput);

            if (salesImportCscResult.StatisticSpecificDateRange != null
                && salesImportCscResult.StatisticSpecificDateRange.Metrics.MinDateFound.HasValue &&
              salesImportCscResult.StatisticSpecificDateRange.Metrics.MaxDateFound.HasValue)
            {
                double standardDeviation = 0;

                Console.WriteLine("{0} {1} - {2} [User Date range input: {3}] [Unique Dates count: {4}]",
                    strDateRangeTitle,
                    salesImportCscResult.StatisticSpecificDateRange.Metrics.MinDateFound.Value.ToString(ReadSalesProcess.DateFormat),
                    salesImportCscResult.StatisticSpecificDateRange.Metrics.MaxDateFound.Value.ToString(ReadSalesProcess.DateFormat),
                    userInputFromToDate,
                    salesImportCscResult.StatisticSpecificDateRange.Metrics.count);

                Console.WriteLine("=============================================================");
               
                standardDeviation = StatisticsUtil.GetStandardDeviation(salesImportCscResult.StatisticSpecificDateRange.Metrics.sumd,
                        salesImportCscResult.StatisticSpecificDateRange.Metrics.sum,
                        salesImportCscResult.StatisticSpecificDateRange.Metrics.count);
                Console.WriteLine("Standard Deviation = {0}", standardDeviation);
            }
            else
            {
                Console.WriteLine(strDateRangeTitle);
                Console.WriteLine("=============================================================");
                Console.WriteLine("The specific date range entered by the user could not be processed based on the date range from the import files: User Date range input: {0}", userInputFromToDate);
            }

        }

        internal static void DisplayStatisticsForYearRange(SalesImportCsvInputDTO input, SalesImportCsvResultDTO result)
        {
            if (string.IsNullOrEmpty(input.Date.FromYear) || string.IsNullOrEmpty(input.Date.ToYear))
            {
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Statistics for year range [{0} - {1}]", input.Date.FromYear, input.Date.ToYear);
            Console.WriteLine("=============================================================");
            int count = 0;
            decimal sum = 0;
            decimal sumd = 0;
            var record = new List<double>();
            DateTime? minDate = null;
            DateTime? maxDate = null;
            int countEntries = 0;
            foreach (var yearEntry in result.StatisticPerYear)
            {
                var year = yearEntry.Key;
                if (year > int.Parse(input.Date.ToYear) || year < int.Parse(input.Date.FromYear))
                    continue;

                var statistics = yearEntry.Value;

                // Store minimum/maximum date
                if (countEntries == 0)
                {
                    minDate = statistics.Metrics.MinDateFound;
                }
                maxDate = statistics.Metrics.MaxDateFound;

                count += statistics.Metrics.count;
                sumd += statistics.Metrics.sumd;
                sum += statistics.Metrics.sum;

                countEntries++;
            }

            Console.WriteLine("Unique Dates count: {0}", count);
            Console.WriteLine("From Date: {0} - To Date: {1}", minDate.Value.ToString(DateTimeUtil.DATE_FORMAT), maxDate.Value.ToString(DateTimeUtil.DATE_FORMAT));
            Console.WriteLine();

            double sd = StatisticsUtil.GetStandardDeviation(sumd, sum, count);
            Console.WriteLine("Standard Deviation of earnings = {0}", sd);

            double average = (double)StatisticsUtil.GetAverage(sum, count);
            Console.WriteLine("Average Sales Earnings = {0}", average);
        }
    }
}
