using SalesConsoleApp.DTO.Csv;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesConsoleApp.Utility
{
    internal class StatisticsUtil
    {
        //List<double> intList = new List<double> { 1, 2, 3, 4, 5 };
        //double standard_deviation = standardDeviation(intList);
        //Console.WriteLine("Standard Deviation = {0}", standard_deviation);
        //Console.WriteLine("Standard Deviation = {0}", intList.standardDeviation());
        internal static double GetStandardDeviation(decimal sumd, decimal sum, int count)
        {
            decimal sumOfDerivationAverage = sumd / count;
            decimal average = sum / count;
            var result = Math.Sqrt(Math.Abs((double)(sumd / count - average * average)));
            return Math.Round(result, AmountUtil.AMOUNT_DECIMAL_POINTS_MAX);
        }

        internal static decimal GetAverage(decimal sum, int count)
        {
            decimal average = sum / count;
            return Math.Round(average, AmountUtil.AMOUNT_DECIMAL_POINTS_MAX);
        }
    }
}
