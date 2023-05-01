using SalesConsoleApp.DTO.Csv;
using System;
using System.Collections.Generic;
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
            var result = Math.Sqrt((double)(sumOfDerivationAverage - (average * average)));
            // Console.WriteLine("Standard Deviation = {0}", result);
            return Math.Round(result, AmountUtil.AMOUNT_DECIMAL_POINTS_MAX);
        }

        internal static decimal GetAverage(decimal sum, int count)
        {
            decimal average = sum / count;
            return Math.Round(average, AmountUtil.AMOUNT_DECIMAL_POINTS_MAX);
        }

        internal static double standardDeviation(IEnumerable<double> sequence)
        {
            double result = 0;

            if (sequence.Any())
            {
                double average = sequence.Average();
                double sum = sequence.Sum(d => Math.Pow(d - average, 2));
                result = Math.Sqrt((sum) / sequence.Count());
            }
            return result;
        }

        private double getStandardDeviation(List<double> doubleList)
        {
            double average = doubleList.Average();
            double sumOfDerivation = 0;
            foreach (double value in doubleList)
            {
                sumOfDerivation += (value) * (value);
            }
            double sumOfDerivationAverage = sumOfDerivation / (doubleList.Count - 1);
            return Math.Sqrt(sumOfDerivationAverage - (average * average));
        }
    }
}
