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

            if (salesImportCscResult.StatisticPerRangeOfYears.MinDateFound.HasValue &&
              salesImportCscResult.StatisticPerRangeOfYears.MaxDateFound.HasValue)
            {
                Console.WriteLine(string.Format("Standard Deviation for range of Dates {0} - {1}",
                    salesImportCscResult.StatisticPerRangeOfYears.MinDateFound.Value.ToString(ReadSalesProcess.DateFormat),
                    salesImportCscResult.StatisticPerRangeOfYears.MaxDateFound.Value.ToString(ReadSalesProcess.DateFormat)));
                Console.WriteLine("=============================================================");
                standardDeviation = StatisticsUtil.GetStandardDeviation(salesImportCscResult.StatisticPerRangeOfYears.sumd,
                        salesImportCscResult.StatisticPerRangeOfYears.sum,
                        salesImportCscResult.StatisticPerRangeOfYears.count);
                Console.WriteLine("Standard Deviation = {0}", standardDeviation);
                //TEST!!!!!
                Console.WriteLine("Standard Deviation TEST!!!!! = {0}", StatisticsUtil.standardDeviation(salesImportCscResult.StatisticPerRangeOfYears.records));
            }
            else
            {
                Console.WriteLine("No dates found in between the given input [From] - [To] Dates");
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
    }
}
