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
            // double absoluteNumber = ((double)sumOfDerivationAverage - ((double)average * (double)average));


            var result = Math.Sqrt(Math.Abs((double)(sumd / count - average * average)));
            return Math.Round(result, AmountUtil.AMOUNT_DECIMAL_POINTS_MAX);
        }

        internal static decimal GetAverage(decimal sum, int count)
        {
            decimal average = sum / count;
            return Math.Round(average, AmountUtil.AMOUNT_DECIMAL_POINTS_MAX);
        }

        internal static double standardDeviation(IEnumerable<double> sequence, int? count = null)
        {
            double result = 0;

            if (sequence.Any())
            {
                int c = count.HasValue ? count.Value : sequence.Count();
                double average = sequence.Sum() / c;//sequence.Average();
                double sum = sequence.Sum(d => Math.Pow(d - average, 2));
                result = Math.Sqrt((sum) / c);
            }
            return result;
        }

        public class Test
        {
            public DateTime date { get; set; }
            public Double amount { get; set; }
        }


        internal static double StandardDeviation2(IEnumerable<Test> sequence)
        {
            double result = 0;

            if (sequence.Any())
            {
                int c = sequence.Count();
                double average = sequence.Select(s => s.amount).Sum() / c;//sequence.Average();
                double sum = sequence.Select(s => s.amount).Sum(d => Math.Pow(d - average, 2));
                result = Math.Sqrt((sum) / c);
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
