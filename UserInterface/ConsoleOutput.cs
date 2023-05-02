using SalesConsoleApp.DTO.Csv;
using SalesConsoleApp.Process;
using SalesConsoleApp.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesConsoleApp.UserInterface
{
    internal class ConsoleOutput
    {
        internal static void DisplayStatistics(SalesImportCsvResultDTO salesImportCscResult)
        {
            double standardDeviation = StatisticsUtil.GetStandardDeviation(salesImportCscResult.sumd, salesImportCscResult.sum, salesImportCscResult.count);

            //decimal sumOfDerivationAverage = salesImportCsvResultDTO.sumd / salesImportCsvResultDTO.count;
            //decimal average = salesImportCsvResultDTO.sum / salesImportCsvResultDTO.count;
            //var result = Math.Sqrt((double)(sumOfDerivationAverage - (average * average)));
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Sales Import CSV files Statistics");
            Console.WriteLine("=======================================================");
            Console.WriteLine();
            if (salesImportCscResult.MinDateFound.HasValue && salesImportCscResult.MaxDateFound.HasValue)
            {
                Console.WriteLine("Minimum Date: [{0}]. Maximum Date: [{1}]",
                    salesImportCscResult.MinDateFound.Value.ToString(ReadSalesProcess.DateFormat),
                    salesImportCscResult.MaxDateFound.Value.ToString(ReadSalesProcess.DateFormat));
                Console.WriteLine();
            }

            string rangeOfYears = string.Join("/", salesImportCscResult.StatisticPerYear.Keys);
            Console.WriteLine("Total Standard Deviation = {0} [For years: {1}]", standardDeviation, rangeOfYears);
            //TEST!!!!!
            Console.WriteLine("Standard Deviation TEST!!!!! = {0} [For years: {1}]", StatisticsUtil.standardDeviation(salesImportCscResult.records), rangeOfYears);
            Console.WriteLine("=============================================================");

            //double standard_deviation = standardDeviation(salesImportCsvResultDTO.records);
            //Console.WriteLine("Standard Deviation using standard method = {0}", standard_deviation);

            //Total Execution Time: 446 ms
            //Standard Deviation = 43.30127018922193
            //Standard Deviation using standard method = 43.30127018922193

            Console.WriteLine();
            Console.WriteLine("Standard Deviation per year");
            Console.WriteLine("=============================================================");
            foreach (var yearEntry in salesImportCscResult.StatisticPerYear)
            {
                var year = yearEntry.Key;
                var statistics = yearEntry.Value;
                standardDeviation = StatisticsUtil.GetStandardDeviation(statistics.sumd, statistics.sum, statistics.count);
                Console.WriteLine("Standard Deviation = {0} [For year: {1}]", standardDeviation, year);

                //TEST!!!!!
                Console.WriteLine("Standard Deviation TEST!!!!! = {0} [For year: {1}]", StatisticsUtil.standardDeviation(statistics.records), year);
            }

            Console.WriteLine();
            

            string strDateRangeTitle = "Statistics for a specific of Dates";

            if (salesImportCscResult.StatisticSpecificDateRange != null
                && salesImportCscResult.StatisticSpecificDateRange.MinDateFound.HasValue &&
              salesImportCscResult.StatisticSpecificDateRange.MaxDateFound.HasValue)
            {
                Console.WriteLine(string.Format("{0} {1} - {2}",
                    strDateRangeTitle,
                    salesImportCscResult.StatisticSpecificDateRange.MinDateFound.Value.ToString(ReadSalesProcess.DateFormat),
                    salesImportCscResult.StatisticSpecificDateRange.MaxDateFound.Value.ToString(ReadSalesProcess.DateFormat)));
                Console.WriteLine("=============================================================");
                standardDeviation = StatisticsUtil.GetStandardDeviation(salesImportCscResult.StatisticSpecificDateRange.sumd,
                        salesImportCscResult.StatisticSpecificDateRange.sum,
                        salesImportCscResult.StatisticSpecificDateRange.count);
                Console.WriteLine("Standard Deviation = {0}", standardDeviation);
                //TEST!!!!!
                Console.WriteLine("Standard Deviation TEST!!!!! = {0}", StatisticsUtil.standardDeviation(salesImportCscResult.StatisticSpecificDateRange.records));
            }
            else
            {
                Console.WriteLine(strDateRangeTitle);
                Console.WriteLine("=============================================================");
                Console.WriteLine("No specific date range entered by the user");
            }


            Console.WriteLine();
            Console.WriteLine("Average Earnings per year");
            Console.WriteLine("=============================================================");
            foreach (var yearEntry in salesImportCscResult.StatisticPerYear)
            {
                var year = yearEntry.Key;
                var statistics = yearEntry.Value;
                decimal averageEarnings = StatisticsUtil.GetAverage(statistics.sum, statistics.count);
                Console.WriteLine("Average Sales Earnings = {0} [For year: {1}]", averageEarnings, year);
            }
        }

        internal static void DisplayStatisticsForYearRange(SalesImportCsvInputDTO input, SalesImportCsvResultDTO result)
        {
            if (string.IsNullOrEmpty(input.Date.FromYear) || string.IsNullOrEmpty(input.Date.ToYear))
            {
                return;
            }

            Console.WriteLine();
            Console.WriteLine(string.Format("Statistics for year range [{0} - {1}]", input.Date.FromYear, input.Date.ToYear));
            Console.WriteLine("=============================================================");
            int count = 0;
            decimal sum = 0;
            decimal sumd = 0;
            var record = new List<double>();
            foreach (var yearEntry in result.StatisticPerYear)
            {
                var year = yearEntry.Key;
                if (year > int.Parse(input.Date.ToYear) || year < int.Parse(input.Date.FromYear))
                    continue;

                var statistics = yearEntry.Value;

                count += statistics.count;
                sumd += statistics.sumd;
                sum += statistics.sum;

                record.AddRange(statistics.records);
            }
            double sd = StatisticsUtil.GetStandardDeviation(sumd, sum, count);
            Console.WriteLine("Standard Deviation = {0}", sd);

            double sd1 = StatisticsUtil.standardDeviation(record);
            Console.WriteLine("Standard Deviation = {0} // test!!!!!", sd1);

            double average = (double)StatisticsUtil.GetAverage(sum, count);
            Console.WriteLine("Average Sales Earnings = {0}", average);
        }
    }
}
