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

        internal static SalesImportCsvResultDTO Read(SalesImportCsvInputDTO input)
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


            if (input.Date.FromDate.HasValue && input.Date.ToDate.HasValue && input.Date.FromDate.Value >= input.Date.ToDate)
            {
                throw new ArgumentException(string.Format("[From] Date: {0} is invalid (comes after to-date). [From] Date {1} > [To] Date {2}",
                    input.Date.FromDate.Value.ToString(DateFormat),
                    input.Date.FromDate.Value.ToString(DateFormat), 
                    input.Date.ToDate.Value.ToString(DateFormat)));
            }

            Console.WriteLine();
            Console.WriteLine("Proceeding with formats");
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine(string.Format("Date Format: {0}", DateFormat));
            Console.WriteLine(string.Format("Amount Format: {0}", amountFormat));
            Console.WriteLine();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            var salesImportCsvResultDTO = ReadImportFiles(input);
            watch.Stop();

            Console.WriteLine($"Total Execution Time: {watch.ElapsedMilliseconds} ms");

            return salesImportCsvResultDTO;
        }

    private static SalesImportCsvResultDTO ReadImportFiles(SalesImportCsvInputDTO input)
        {
            SalesImportCsvResultDTO salesImportCsvResultDTO = new SalesImportCsvResultDTO();

            string path = string.IsNullOrEmpty(input.FilesPath) ? FILES_PATH : input.FilesPath;

            Console.WriteLine(string.Format("CSV Import File(s) path: [{0}]", path));
            Console.WriteLine("");

            var filesEnum = Directory.EnumerateFiles(path, "*.csv");
            List<string> files = new List<string>(filesEnum);

            if (files == null || !files.Any())
            {
                throw new ArgumentException(string.Format("No import csv files found in the specified path {0}", path));
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
                        ReadCSV(importCsvFile, input, ref salesImportCsvResultDTO1);
                    }
                });

                SalesImportCsvResultDTO salesImportCsvResultDTO2 = new SalesImportCsvResultDTO();
                Task t2 = Task.Run(() =>
                {
                    foreach (var importCsvFile in partition2)
                    {
                        ReadCSV(importCsvFile, input, ref salesImportCsvResultDTO2);
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
                    ReadCSV(importCsvFile, input, ref salesImportCsvResultDTO);
                }
            }

            return salesImportCsvResultDTO;
        }
        private static SalesImportCsvResultDTO ReadCSV(string importCsvFile, SalesImportCsvInputDTO input, ref SalesImportCsvResultDTO salesImportCsvResultDTO)
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
                        record = CollectRecord(csv, importCsvFile);
                        CollectStatistics(record, input, ref salesImportCsvResultDTO);
                    }

                    Console.WriteLine(string.Format("Completed reading import file {0}", importCsvFile));
                }
            }

            return salesImportCsvResultDTO;
        }

        private static SalesDTO CollectRecord(CsvReader csv, string importCsvFile)
        {
            SalesDTO record = null;

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

            return record;
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

        private static void CollectStatistics(SalesDTO? record, SalesImportCsvInputDTO input, ref SalesImportCsvResultDTO salesImportCsvResultDTO)
        {
            //var statisticPerYear = new StatisticPerYearDTO();
            //statisticPerYear.count++;
            //statisticPerYear.sum += record.Amount.Value;
            //statisticPerYear.sumd += (decimal)Math.Pow((double)record.Amount.Value, 2);

            //statisticPerYear.records.Add((double)record.Amount); //test remove !!!!

            var currDate = record.Date.Value;
            var year = currDate.Year;

            // Store minimum date found in all csv files
            CollectMinMaxDateFound(currDate, ref salesImportCsvResultDTO);
            //if (!salesImportCsvResultDTO.MinDateFound.HasValue)
            //{
            //    salesImportCsvResultDTO.MinDateFound = currDate;
            //}
            //else if (salesImportCsvResultDTO.MinDateFound > currDate)
            //{
            //    salesImportCsvResultDTO.MinDateFound = currDate;
            //}

            //// Store maximum date found in all csv files
            //if (!salesImportCsvResultDTO.MaxDateFound.HasValue)
            //{
            //    salesImportCsvResultDTO.MaxDateFound = currDate;
            //}
            //else if (salesImportCsvResultDTO.MaxDateFound < currDate)
            //{
            //    salesImportCsvResultDTO.MaxDateFound = currDate;
            //}

            // Collect statistics per year
            CollectStatisticsPerYear(record, currDate, ref salesImportCsvResultDTO);
            //bool isFound = salesImportCsvResultDTO.StatisticPerYear.TryGetValue(year, out StatisticPerYearDTO existingStatisticPerYear);
            //if (isFound)
            //{
            //    existingStatisticPerYear.count++;
            //    existingStatisticPerYear.sum += record.Amount.Value;
            //    existingStatisticPerYear.sumd += (decimal)Math.Pow((double)record.Amount.Value, 2);

            //    existingStatisticPerYear.records.Add((double)record.Amount); //test remove !!!!

            //    salesImportCsvResultDTO.StatisticPerYear.TryAdd(year, existingStatisticPerYear);
            //}
            //else
            //    salesImportCsvResultDTO.StatisticPerYear.TryAdd(year, statisticPerYear);

            // Collect statistics for a range of dates [given input by the user]
            if (input.Date.FromDate.HasValue && input.Date.ToDate.HasValue)
            {
                CollectStatisticsPerDateRange(record, input.Date.FromDate.Value, input.Date.ToDate.Value, currDate, ref salesImportCsvResultDTO);
            }
            
            //if (input.Date.FromDate.Value <= currDate && input.Date.ToDate >= currDate)
            //{
            //    // Store minimum date found in all csv files
            //    if (!salesImportCsvResultDTO.StatisticPerRangeOfYears.MinDateFound.HasValue)
            //    {
            //        salesImportCsvResultDTO.StatisticPerRangeOfYears.MinDateFound = currDate;
            //    }
            //    else if (salesImportCsvResultDTO.StatisticPerRangeOfYears.MinDateFound > currDate)
            //    {
            //        salesImportCsvResultDTO.StatisticPerRangeOfYears.MinDateFound = currDate;
            //    }

            //    // Store maximum date found in all csv files
            //    if (!salesImportCsvResultDTO.StatisticPerRangeOfYears.MaxDateFound.HasValue)
            //    {
            //        salesImportCsvResultDTO.StatisticPerRangeOfYears.MaxDateFound = currDate;
            //    }
            //    else if (salesImportCsvResultDTO.StatisticPerRangeOfYears.MaxDateFound < currDate)
            //    {
            //        salesImportCsvResultDTO.StatisticPerRangeOfYears.MaxDateFound = currDate;
            //    }

            //    salesImportCsvResultDTO.StatisticPerRangeOfYears.count++;
            //    salesImportCsvResultDTO.StatisticPerRangeOfYears.sum += record.Amount.Value;
            //    salesImportCsvResultDTO.StatisticPerRangeOfYears.sumd += (decimal)Math.Pow((double)record.Amount.Value, 2);
            //    salesImportCsvResultDTO.StatisticPerRangeOfYears.records.Add((double)record.Amount); //test remove !!!!
            //}

            // Collect statistics for total rows read
            salesImportCsvResultDTO.count++;
            salesImportCsvResultDTO.sum += record.Amount.Value;
            salesImportCsvResultDTO.sumd += (decimal)Math.Pow((double)record.Amount.Value, 2);


            salesImportCsvResultDTO.records.Add((double)record.Amount); //test remove !!!!
        }

        private static void CollectMinMaxDateFound(DateTime currDate, ref SalesImportCsvResultDTO salesImportCsvResultDTO)
        {
            if (!salesImportCsvResultDTO.MinDateFound.HasValue)
            {
                salesImportCsvResultDTO.MinDateFound = currDate;
            }
            else if (salesImportCsvResultDTO.MinDateFound > currDate)
            {
                salesImportCsvResultDTO.MinDateFound = currDate;
            }

            // Store maximum date found in all csv files
            if (!salesImportCsvResultDTO.MaxDateFound.HasValue)
            {
                salesImportCsvResultDTO.MaxDateFound = currDate;
            }
            else if (salesImportCsvResultDTO.MaxDateFound < currDate)
            {
                salesImportCsvResultDTO.MaxDateFound = currDate;
            }
        }

        private static void CollectStatisticsPerYear(SalesDTO record, DateTime currDate, ref SalesImportCsvResultDTO salesImportCsvResultDTO)
        {
            var year = currDate.Year;

            var statisticPerYear = new StatisticPerYearDTO();
            statisticPerYear.count++;
            statisticPerYear.sum += record.Amount.Value;
            statisticPerYear.sumd += (decimal)Math.Pow((double)record.Amount.Value, 2);

            statisticPerYear.records.Add((double)record.Amount); //test remove !!!!

            if (salesImportCsvResultDTO.StatisticPerYear == null)
            {
                salesImportCsvResultDTO.StatisticPerYear = new SortedDictionary<int, StatisticPerYearDTO>();
            }

            bool isFound = salesImportCsvResultDTO.StatisticPerYear.TryGetValue(year, out StatisticPerYearDTO existingStatisticPerYear);
            if (isFound)
            {
                existingStatisticPerYear.count++;
                existingStatisticPerYear.sum += record.Amount.Value;
                existingStatisticPerYear.sumd += (decimal)Math.Pow((double)record.Amount.Value, 2);

                existingStatisticPerYear.records.Add((double)record.Amount); //test remove !!!!

                salesImportCsvResultDTO.StatisticPerYear.TryAdd(year, existingStatisticPerYear);
            }
            else
                salesImportCsvResultDTO.StatisticPerYear.TryAdd(year, statisticPerYear);

            TryAddUpdateMonthsDatesIterated(ref statisticPerYear, currDate);
        }

        private static void TryAddUpdateMonthsDatesIterated(ref StatisticPerYearDTO statisticPerYearDTO, DateTime currDate)
        {
            var month = currDate.Month;
            var day = currDate.Day;

            bool isFound = statisticPerYearDTO.MonthDaysIterated.TryGetValue(month, out byte[] existingMonthDays);
            if (isFound)
            {
                existingMonthDays[day] = 1;
                statisticPerYearDTO.MonthDaysIterated.TryAdd(month, existingMonthDays);
            }
            else
            {
                byte[] existingMonthDaysNew = new byte[32];
                existingMonthDaysNew[day] = 1;
                statisticPerYearDTO.MonthDaysIterated.TryAdd(month, existingMonthDaysNew);
            }
        }

        private static void CollectStatisticsPerDateRange(
            SalesDTO record, 
            DateTime from, DateTime to, 
            DateTime currDate, 
            ref SalesImportCsvResultDTO salesImportCsvResultDTO)
        {

            if (salesImportCsvResultDTO.StatisticSpecificDateRange == null)
            {
                salesImportCsvResultDTO.StatisticSpecificDateRange = new StatisticPerYearDTO();
            }

            if (from <= currDate && to >= currDate)
            {
                // Store minimum date found in all csv files
                if (!salesImportCsvResultDTO.StatisticSpecificDateRange.MinDateFound.HasValue)
                {
                    salesImportCsvResultDTO.StatisticSpecificDateRange.MinDateFound = currDate;
                }
                else if (salesImportCsvResultDTO.StatisticSpecificDateRange.MinDateFound > currDate)
                {
                    salesImportCsvResultDTO.StatisticSpecificDateRange.MinDateFound = currDate;
                }

                // Store maximum date found in all csv files
                if (!salesImportCsvResultDTO.StatisticSpecificDateRange.MaxDateFound.HasValue)
                {
                    salesImportCsvResultDTO.StatisticSpecificDateRange.MaxDateFound = currDate;
                }
                else if (salesImportCsvResultDTO.StatisticSpecificDateRange.MaxDateFound < currDate)
                {
                    salesImportCsvResultDTO.StatisticSpecificDateRange.MaxDateFound = currDate;
                }

                salesImportCsvResultDTO.StatisticSpecificDateRange.count++;
                salesImportCsvResultDTO.StatisticSpecificDateRange.sum += record.Amount.Value;
                salesImportCsvResultDTO.StatisticSpecificDateRange.sumd += (decimal)Math.Pow((double)record.Amount.Value, 2);
                salesImportCsvResultDTO.StatisticSpecificDateRange.records.Add((double)record.Amount); //test remove !!!!
            }
        }

    }
}
