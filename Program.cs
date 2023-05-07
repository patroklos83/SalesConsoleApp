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
        /// <summary>
        /// This console application reads sales .csv files.
        /// the csv file sales have to be sorted by Dates desc/asc order
        /// No Arrays or Lists are used to calculate the statistics
        /// Files has to be ordered as well and have specific continuity.
        /// see example files  [/ImportFiles]
        /// The main ideas is to Avoid large Lists [with millions of elements]
        /// which will result in MBS/GBs of memory used.In addition, Avoid as well
        /// O(n) time complexity for iterating through the large Lists/arrays.
        /// 
        /// see example of using a list of type decimal for storing 20 years
        /// of sales record in memory.
        /// 
        /// 20 years * 365 days * 300 sales/day = 2,190,000 sales records
        /// 2,190,000 sales records * 16 bytes (decimal type) = 35,040,000 bytes
        /// 35,040,000 bytes = 35.04 Mbytes
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("Hello, This is the Sales statistics console app! ");
            Console.WriteLine("------------------------------------------------ ");
            Console.WriteLine();

            try
            {
                var userInput = ConsoleInput.GetUserInput();
                SalesImportCsvResultDTO result = ReadSalesProcess.Read(userInput);
                ConsoleOutput.DisplayStatistics(userInput, result);
                userInput = ConsoleInput.GetYearRange(userInput, result);
                ConsoleOutput.DisplayStatisticsForYearRange(userInput, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Oops we have errors");
                Console.WriteLine("-------------------");
                Console.WriteLine();
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                if (ex is ArgumentException)
                {
                    Console.WriteLine();
                    Console.WriteLine("Please retry ...");
                    Console.WriteLine();
                    Main(new string[0]);
                }
            }
        }
    }
}
