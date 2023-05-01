using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.VisualBasic;
using SalesConsoleApp.DTO.Csv;
using SalesConsoleApp.DTO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SalesConsoleApp.Utility;
using SalesConsoleApp.Enums;

namespace SalesConsoleApp.Process
{
    internal class ReadSalesProcess
    {
        internal const string FILES_PATH = "./ImportFiles";

        internal static string DateFormat = DateTimeUtil.DATE_FORMAT;

        internal static AmountFormatDTO AmountFormat = new AmountFormatDTO
        {
            StrAmountFormat = AmountUtil.AMOUNT_FORMAT,
            CurrencySymbol = AmountCurrencyFormatEnum.None,
            NumberOfDecimalPoints = AmountUtil.AMOUNT_DECIMAL_POINTS_MAX
        };

        internal static SalesImportCsvResultDTO ExecuteProcess(SalesImportCsvInputDTO input)
        {
            DateFormat = DateTimeUtil.GetDateFormatByDateFormatEnum(input.Date.DateFormat.Value);
            AmountFormat.Format = input.AmountFormat.Format;
            AmountFormat.StrAmountFormat = AmountUtil.GetAmountFormatByAmountFormatEnum(input.AmountFormat.Format.Value);
            AmountFormat.CurrencySymbol = input.AmountFormat.CurrencySymbol;
            AmountFormat.NumberOfDecimalPoints = input.AmountFormat.NumberOfDecimalPoints;

            string amountFormat = string.Format("{0}{1} [Max decimal points: {2}]",
                AmountFormat.CurrencySymbol.HasValue && AmountFormat.CurrencySymbol != AmountCurrencyFormatEnum.None ? AmountFormat.CurrencySymbol.GetDescription() : string.Empty,
                AmountFormat.StrAmountFormat,
                AmountFormat.NumberOfDecimalPoints);

            Console.WriteLine();
            Console.WriteLine("Proceeding with formats");
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine(string.Format("Date Format: {0}", DateFormat));
            Console.WriteLine(string.Format("Amount Format: {0}", amountFormat));
            Console.WriteLine();

            string path = string.IsNullOrEmpty(input.FilesPath) ? FILES_PATH : input.FilesPath;

            Console.WriteLine(string.Format("CSV Import File(s) path: [{0}]", path));
            Console.WriteLine("");

            var watch = System.Diagnostics.Stopwatch.StartNew();
            var salesImportCsvResultDTO = ReadImportFiles(path);
            watch.Stop();

            Console.WriteLine($"Total Execution Time: {watch.ElapsedMilliseconds} ms");

            return salesImportCsvResultDTO;
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

    private static SalesImportCsvResultDTO ReadImportFiles(string importPath)
        {
            SalesImportCsvResultDTO salesImportCsvResultDTO = new SalesImportCsvResultDTO();

            var filesEnum = Directory.EnumerateFiles(importPath, "*.csv");
            List<string> files = new List<string>(filesEnum);

            if (files == null || !files.Any())
            {
                throw new ArgumentException(string.Format("No import csv files found in the specified path {0}", importPath));
            }

            if (files.Count() > 1)
            {
                int midPoint = files.Count() / 2;
                List<string> partition1 = files.GetRange(0, midPoint);
                List<string> partition2 = files.GetRange(midPoint, files.Count() - 1);

                SalesImportCsvResultDTO salesImportCsvResultDTO1 = new SalesImportCsvResultDTO();
                Task t1 = Task.Run(() =>
                {
                    foreach (var importCsvFile in partition1)
                    {
                        ReadCSV(importCsvFile, ref salesImportCsvResultDTO1);
                    }
                });

                SalesImportCsvResultDTO salesImportCsvResultDTO2 = new SalesImportCsvResultDTO();
                Task t2 = Task.Run(() =>
                {
                    foreach (var importCsvFile in partition2)
                    {
                        ReadCSV(importCsvFile, ref salesImportCsvResultDTO2);
                    }
                });

                Task.WaitAll(t1, t2);

                salesImportCsvResultDTO.count = salesImportCsvResultDTO2.count + salesImportCsvResultDTO1.count;
                salesImportCsvResultDTO.sum = salesImportCsvResultDTO2.sum + salesImportCsvResultDTO1.sum;
                salesImportCsvResultDTO.sumd = salesImportCsvResultDTO2.sumd + salesImportCsvResultDTO1.sumd;
            }
            else
            {
                foreach (var importCsvFile in files)
                {
                    ReadCSV(importCsvFile, ref salesImportCsvResultDTO);
                }
            }

            return salesImportCsvResultDTO;
        }
        private static SalesImportCsvResultDTO ReadCSV(string importCsvFile, ref SalesImportCsvResultDTO salesImportCsvResultDTO)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = "##",
                IgnoreBlankLines = true
            };

            using (var reader = new StreamReader(importCsvFile))
            {
                using (var csv = new CsvReader(reader, configuration))
                {
                    Console.WriteLine(string.Format("Reading import file {0} ...  ", importCsvFile));

                    csv.Context.RegisterClassMap<SalesDTOMap>();

                    SalesDTO? record = null;

                    while (csv.Read())
                    {
                        try
                        {
                            record = csv.GetRecord<SalesDTO>();
                            ValidateCsvRow(record);
                        }
                        catch (CsvHelperException e)
                        {
                            throw new ArgumentException(string.Format("{0} {1}", string.Format("Import File {0} Line {1}:",
                                importCsvFile, csv.Context.Parser.RawRow), 
                                (e.InnerException == null ? string.Empty : e.InnerException.Message)));
                        }
                        catch (ArgumentException e)
                        {
                            throw new ArgumentException(string.Format("{0} {1}", string.Format("Import File {0} Line {1}:",
                                importCsvFile, csv.Context.Parser.RawRow), 
                                e.Message));
                        }
                        
                        CollectStatistics(record, ref salesImportCsvResultDTO);
                        
                        //var statisticPerYear = new StatisticPerYearDTO();
                        //statisticPerYear.count++;
                        //statisticPerYear.sum += record.Amount.Value;
                        //statisticPerYear.sumd += (decimal)Math.Pow((double)record.Amount.Value, 2);

                        //var year = record.Date.Value.Year;
                        //bool isFound = salesImportCsvResultDTO.StatisticPerYear.TryGetValue(year, out StatisticPerYearDTO existingStatisticPerYear);
                        //if (isFound)
                        //{
                        //    existingStatisticPerYear.count++;
                        //    existingStatisticPerYear.sum += record.Amount.Value;
                        //    existingStatisticPerYear.sumd += (decimal)Math.Pow((double)record.Amount.Value, 2);
                        //    salesImportCsvResultDTO.StatisticPerYear.TryAdd(year, existingStatisticPerYear);
                        //}
                        //else
                        // salesImportCsvResultDTO.StatisticPerYear.TryAdd(year, statisticPerYear);


                        //salesImportCsvResultDTO.count++;
                        //salesImportCsvResultDTO.sum += record.Amount.Value;
                        //salesImportCsvResultDTO.sumd += (decimal)Math.Pow((double)record.Amount.Value, 2);

                        // remove !!!!! salesImportCsvResultDTO.records.Add((double)record.Amount);
                    }

                    Console.WriteLine(string.Format("Completed reading import file {0}", importCsvFile));
                }
            }

            return salesImportCsvResultDTO;
        }

        private static void ValidateCsvRow(SalesDTO? record)
        {
            if (record == null)
            {
                throw new ArgumentException("Record is null");
            }
            if (!string.IsNullOrEmpty(record.Redundant))
            {
                throw new ArgumentException(string.Format("Invalid data: Found extra info [{0}]", record.Redundant));
            }
            if (!record.Date.HasValue)
            {
                throw new ArgumentException("Empty Date");
            }
            if (!record.Amount.HasValue)
            {
                throw new ArgumentException("Empty Amount");
            }
        }

        private static void CollectStatistics(SalesDTO? record, ref SalesImportCsvResultDTO salesImportCsvResultDTO)
        {
            var statisticPerYear = new StatisticPerYearDTO();
            statisticPerYear.count++;
            statisticPerYear.sum += record.Amount.Value;
            statisticPerYear.sumd += (decimal)Math.Pow((double)record.Amount.Value, 2);

            var year = record.Date.Value.Year;
            bool isFound = salesImportCsvResultDTO.StatisticPerYear.TryGetValue(year, out StatisticPerYearDTO existingStatisticPerYear);
            if (isFound)
            {
                existingStatisticPerYear.count++;
                existingStatisticPerYear.sum += record.Amount.Value;
                existingStatisticPerYear.sumd += (decimal)Math.Pow((double)record.Amount.Value, 2);
                salesImportCsvResultDTO.StatisticPerYear.TryAdd(year, existingStatisticPerYear);
            }
            else
                salesImportCsvResultDTO.StatisticPerYear.TryAdd(year, statisticPerYear);


            salesImportCsvResultDTO.count++;
            salesImportCsvResultDTO.sum += record.Amount.Value;
            salesImportCsvResultDTO.sumd += (decimal)Math.Pow((double)record.Amount.Value, 2);

        }

    }
}
