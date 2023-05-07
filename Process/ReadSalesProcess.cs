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
using System.Collections.Concurrent;
using static SalesConsoleApp.Utility.StatisticsUtil;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;

namespace SalesConsoleApp.Process
{
    internal class ReadSalesProcess
    {
        internal static string DateFormat = DateTimeUtil.DATE_FORMAT;

        internal static AmountFormatDTO AmountFormat = new AmountFormatDTO
        {
            StrAmountFormat = AmountUtil.AMOUNT_FORMAT,
            CurrencySymbol = AmountCurrencyFormatEnum.None,
            NumberOfDecimalPoints = AmountUtil.AMOUNT_DECIMAL_POINTS_MAX
        };

        private static SalesDTO previousSalesRecord = new SalesDTO { Amount = 0 };

        private static int csvRowCounter = 1;

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

            string path = string.IsNullOrEmpty(input.FilesPath) ? FileUtil.DEFAULT_FILES_PATH : input.FilesPath;

            Console.WriteLine(string.Format("CSV Import File(s) path: [{0}]", path));
            Console.WriteLine("");

            var files = FileUtil.GetFilesFromPath(path, FileUtil.FILES_EXTENTION_COMMA_SEPARATED);
            if (files.Any())
            {
                foreach (var importCsvFile in files)
                {
                    ReadCSV(importCsvFile, input, ref salesImportCsvResultDTO);
                }
            }

            // salesImportCsvResultDTO = GollectLastRecord(input, salesImportCsvResultDTO);
            CollectStatistics(previousSalesRecord, input, true, ref salesImportCsvResultDTO);
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
                        if (csvRowCounter== 1)
                        {
                            // Init temporary variable previousSalesRecord on first record from all files
                            previousSalesRecord.Date = record.Date;
                        }
                        CollectStatistics(record, input, false, ref salesImportCsvResultDTO);
                        csvRowCounter++;
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


        /// <summary>
        /// Get last row from the last csv file read
        /// </summary>
        /// <param name="salesImportCsvInputDTO"></param>
        /// <param name="salesImportCsvResultDTO"></param>
        /// <returns></returns>
        private static SalesImportCsvResultDTO GollectLastRecord(SalesImportCsvInputDTO salesImportCsvInputDTO, SalesImportCsvResultDTO salesImportCsvResultDTO)
        {
            decimal sum = previousSalesRecord.Amount.Value;
            decimal sumd = (decimal)Math.Pow((double)previousSalesRecord.Amount.Value, 2);

            // Collect statistics for total rows read
            salesImportCsvResultDTO.Metrics.count++;
            salesImportCsvResultDTO.Metrics.sum += sum;
            salesImportCsvResultDTO.Metrics.sumd += sumd;
            salesImportCsvResultDTO.Metrics = CollectMinMaxDateFound(previousSalesRecord.Date.Value, salesImportCsvResultDTO.Metrics);

            // Collect statistics per year
            UpdateStatisticsPerYear(new SalesDTO { Amount = 0, Date = previousSalesRecord.Date },
                previousSalesRecord.Date.Value,
                sum,
                sumd,
                ref salesImportCsvResultDTO);

            // Collect statistics for a range of dates [given input by the user]
            if (salesImportCsvInputDTO.Date.IsExportResultsFromToDate)
            {
                UpdateStatisticsPerDateRange(sum, sumd,
                    salesImportCsvInputDTO.Date.FromDate.Value,
                    salesImportCsvInputDTO.Date.ToDate.Value,
                    previousSalesRecord.Date.Value, ref salesImportCsvResultDTO);
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

        /// <summary>
        /// Checks if current row/record has a different Date
        /// than the previous record
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private static bool IsDateAlreadyIterated(SalesDTO? record)
        {
            if (!previousSalesRecord.Date.HasValue)
                return false;
            else if (record.Date.HasValue && record.Date.Value == previousSalesRecord.Date.Value)
                return true;

            return false;
        }

        //private static bool IsFirstRow()
        //{
        //    if (!previousSalesRecord.Date.HasValue)
        //        return true;

        //    return false;
        //}


        private static void CollectStatistics(
            SalesDTO? record,
            SalesImportCsvInputDTO input,
            bool isLastRecord,
            ref SalesImportCsvResultDTO salesImportCsvResultDTO)
        {
            if (salesImportCsvResultDTO.Metrics == null)
                salesImportCsvResultDTO.Metrics = new Metrics();

            var currDate = record.Date.Value;
            var year = currDate.Year;

            bool isDateAlreadyIterated = IsDateAlreadyIterated(record);
            if (!isDateAlreadyIterated || isLastRecord)
            {
                // Collect statistics for total rows read
                decimal sum = previousSalesRecord.Amount.Value;
                decimal sumd = (decimal)Math.Pow((double)previousSalesRecord.Amount.Value, 2);

                salesImportCsvResultDTO.Metrics.count++;
                salesImportCsvResultDTO.Metrics.sum += sum;
                salesImportCsvResultDTO.Metrics.sumd += sumd;

                // Store minimum date found in all csv files
                salesImportCsvResultDTO.Metrics = CollectMinMaxDateFound(previousSalesRecord.Date.Value, salesImportCsvResultDTO.Metrics);

                // Collect statistics per year
                var recordTemp = isLastRecord? new SalesDTO { Amount = 0, Date = previousSalesRecord.Date } : record;
                UpdateStatisticsPerYear(recordTemp, previousSalesRecord.Date.Value, sum, sumd, ref salesImportCsvResultDTO);

                // Collect statistics for a range of dates [given input by the user]
                if (input.Date.IsExportResultsFromToDate)
                {
                    UpdateStatisticsPerDateRange(
                        sum, sumd, 
                        input.Date.FromDate.Value, input.Date.ToDate.Value,
                        previousSalesRecord.Date.Value, 
                        ref salesImportCsvResultDTO);
                }

                previousSalesRecord.Amount = 0;
            }


            previousSalesRecord.Amount += record.Amount;
            previousSalesRecord.Date = record.Date;

            //test!!!!!!
            if (salesImportCsvResultDTO.records2 == null)
                salesImportCsvResultDTO.records2 = new List<Test>();


            if (salesImportCsvResultDTO.records2.Exists(r => r.date == record.Date))
            {
                salesImportCsvResultDTO.records2.FirstOrDefault(r => r.date == record.Date)
                    .amount += (double)record.Amount;
            }
            else
            {
                salesImportCsvResultDTO.records2.Add(new Test
                {
                    date = (DateTime)record.Date,
                    amount = (double)record.Amount
                });
            }
        }

        private static Metrics CollectMinMaxDateFound(DateTime date, Metrics metrics)
        {
            // Store minumimum date found in all csv files
            if (!metrics.MinDateFound.HasValue)
            {
                metrics.MinDateFound = date;
            }
            else if (metrics.MinDateFound > date)
            {
                metrics.MinDateFound = date;
            }

            // Store maximum date found in all csv files
            if (!metrics.MaxDateFound.HasValue)
            {
                metrics.MaxDateFound = date;
            }
            else if (metrics.MaxDateFound < date)
            {
                metrics.MaxDateFound = date;
            }

            return metrics;
        }

        private static void UpdateStatisticsPerYear(SalesDTO record, DateTime date, decimal sum, decimal sumd, ref SalesImportCsvResultDTO salesImportCsvResultDTO)
        {
            int year = date.Year;

            var statisticPerYear = new StatisticPerYearDTO();
            statisticPerYear.Metrics = new Metrics();
            statisticPerYear.Metrics.TotalRowCount++;

            if (salesImportCsvResultDTO.StatisticPerYear == null)
                salesImportCsvResultDTO.StatisticPerYear = new SortedDictionary<int, StatisticPerYearDTO>();
            //test!!!!!!
            if (statisticPerYear.records2 == null)
                statisticPerYear.records2 = new List<Test>();


            if (statisticPerYear.records2.Exists(r => r.date == record.Date))
            {
                statisticPerYear.records2.FirstOrDefault(r => r.date == record.Date)
                    .amount += (double)previousSalesRecord.Amount;
            }
            else
            {
                statisticPerYear.records2.Add(new Test
                {
                    date = (DateTime)previousSalesRecord.Date,
                    amount = (double)previousSalesRecord.Amount
                });
            }

            bool isFound = salesImportCsvResultDTO.StatisticPerYear.TryGetValue(year, out StatisticPerYearDTO existingStatisticPerYear);
            statisticPerYear.Metrics.count++;
            statisticPerYear.Metrics.sum += sum;
            statisticPerYear.Metrics.sumd += sumd;

            if (isFound)
            {
                existingStatisticPerYear.Metrics.count++;
                existingStatisticPerYear.Metrics.sum += sum;
                existingStatisticPerYear.Metrics.sumd += sumd;
                existingStatisticPerYear.Metrics = CollectMinMaxDateFound(previousSalesRecord.Date.Value, existingStatisticPerYear.Metrics);
                //existingStatisticPerYear = GetMinMaxDatePerYear(existingStatisticPerYear, date);

                //test!!!!!!
                if (existingStatisticPerYear.records2 == null)
                    existingStatisticPerYear.records2 = new List<Test>();


                if (existingStatisticPerYear.records2.Exists(r => r.date == record.Date))
                {
                    existingStatisticPerYear.records2.FirstOrDefault(r => r.date == record.Date)
                        .amount += (double)previousSalesRecord.Amount;
                }
                else
                {
                    existingStatisticPerYear.records2.Add(new Test
                    {
                        date = (DateTime)previousSalesRecord.Date,
                        amount = (double)previousSalesRecord.Amount
                    });
                }

                salesImportCsvResultDTO.StatisticPerYear.TryAdd(year, existingStatisticPerYear);
            }
            else
            {
                statisticPerYear.Metrics = CollectMinMaxDateFound(previousSalesRecord.Date.Value, statisticPerYear.Metrics);
                // statisticPerYear = GetMinMaxDatePerYear(statisticPerYear, date);
                salesImportCsvResultDTO.StatisticPerYear.TryAdd(year, statisticPerYear);
            }
        }

        //private static StatisticPerYearDTO GetMinMaxDatePerYear(StatisticPerYearDTO statisticPerYear, DateTime date)
        //{
        //    // Store minumimum date found in all csv files
        //    if (!statisticPerYear.MinDateFound.HasValue)
        //    {
        //        statisticPerYear.MinDateFound = date;
        //    }
        //    else if (statisticPerYear.MinDateFound > date)
        //    {
        //        statisticPerYear.MinDateFound = date;
        //    }

        //    // Store maximum date found in all csv files
        //    if (!statisticPerYear.MaxDateFound.HasValue)
        //    {
        //        statisticPerYear.MaxDateFound = date;
        //    }
        //    else if (statisticPerYear.MaxDateFound < date)
        //    {
        //        statisticPerYear.MaxDateFound = date;
        //    }

        //    return statisticPerYear;
        //}

        private static void UpdateStatisticsPerDateRange(
        decimal sum,
        decimal sumd,
        DateTime from,
        DateTime to,
        DateTime date,
        ref SalesImportCsvResultDTO salesImportCsvResultDTO)
        {
            if (salesImportCsvResultDTO.StatisticSpecificDateRange == null)
            {
                salesImportCsvResultDTO.StatisticSpecificDateRange = new StatisticPerYearDTO();
                salesImportCsvResultDTO.StatisticSpecificDateRange.Metrics = new Metrics();
            }

            if (from <= date && to >= date)
            {
                salesImportCsvResultDTO.StatisticSpecificDateRange.Metrics = CollectMinMaxDateFound(date, salesImportCsvResultDTO.StatisticSpecificDateRange.Metrics);
                //// Store minimum date found in all csv files
                //if (!salesImportCsvResultDTO.StatisticSpecificDateRange.MinDateFound.HasValue)
                //{
                //    salesImportCsvResultDTO.StatisticSpecificDateRange.MinDateFound = date;
                //}
                //else if (salesImportCsvResultDTO.StatisticSpecificDateRange.MinDateFound > date)
                //{
                //    salesImportCsvResultDTO.StatisticSpecificDateRange.MinDateFound = date;
                //}

                //// Store maximum date found in all csv files
                //if (!salesImportCsvResultDTO.StatisticSpecificDateRange.MaxDateFound.HasValue)
                //{
                //    salesImportCsvResultDTO.StatisticSpecificDateRange.MaxDateFound = date;
                //}
                //else if (salesImportCsvResultDTO.StatisticSpecificDateRange.MaxDateFound < date)
                //{
                //    salesImportCsvResultDTO.StatisticSpecificDateRange.MaxDateFound = date;
                //}

                salesImportCsvResultDTO.StatisticSpecificDateRange.Metrics.count++;
                salesImportCsvResultDTO.StatisticSpecificDateRange.Metrics.sum += sum;
                salesImportCsvResultDTO.StatisticSpecificDateRange.Metrics.sumd += sumd;

                //test
                if (salesImportCsvResultDTO.StatisticSpecificDateRange.records2 == null)
                    salesImportCsvResultDTO.StatisticSpecificDateRange.records2 = new List<Test>();
                if (salesImportCsvResultDTO.StatisticSpecificDateRange.records2.Exists(r => r.date == date))
                {
                    salesImportCsvResultDTO.StatisticSpecificDateRange.records2.FirstOrDefault(r => r.date == date)
                        .amount += (double)previousSalesRecord.Amount;
                }
                else
                {
                    salesImportCsvResultDTO.StatisticSpecificDateRange.records2.Add(new Test
                    {
                        date = (DateTime)previousSalesRecord.Date,
                        amount = (double)previousSalesRecord.Amount
                    });
                }
            }
        }

    }
}
