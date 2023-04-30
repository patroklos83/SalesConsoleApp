// See https://aka.ms/new-console-template for more information
using System.Formats.Asn1;
using System.Globalization;
using System;
using CsvHelper;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using CsvHelper.Configuration;
using SalesConsoleApp.DTO;
using SalesConsoleApp.DTO.Csv;
using System.Collections.Generic;
using CsvHelper.Expressions;
using SalesConsoleApp.Process;
using SalesConsoleApp.Utility;
using SalesConsoleApp.Enums;
using System.Text;
using SalesConsoleApp.UserInterface;

namespace SalesConsoleApp
{
    class Program
    {
        public const string dateFormat = "dd/MM/yyyy";

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("Hello, This is the Sales statistics console app! ");
            Console.WriteLine("------------------------------------------------ ");
            Console.WriteLine();

            var userInput = ConsoleInput.GetUserInput();

            string importPath = @"C:\Users\patro\OneDrive\Desktop\sales"; //args[0];

            //List<double> intList = new List<double> { 1, 2, 3, 4, 5 };
            //double standard_deviation = standardDeviation(intList);
            //Console.WriteLine("Standard Deviation = {0}", standard_deviation);
            //Console.WriteLine("Standard Deviation = {0}", intList.standardDeviation());


            //https://www.csharp-examples.net/string-format-double/#:~:text=For%20two%20decimal%20places%20use,the%20number%20will%20be%20rounded.
            try
            {
                userInput.FilesPath = importPath;
                ReadSalesProcess.ExecuteProcess(userInput);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Oops we have errors");
                Console.WriteLine("-------------------");
                Console.WriteLine();
                Console.WriteLine(ex.Message);
            }

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

        static double standardDeviation(IEnumerable<double> sequence)
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
    }
}
