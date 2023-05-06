using Microsoft.VisualBasic;
using SalesConsoleApp.DTO.Csv;
using SalesConsoleApp.Enums;
using SalesConsoleApp.Process;
using SalesConsoleApp.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesConsoleApp.UserInterface
{
    internal class ConsoleInput
    {
        internal static SalesImportCsvInputDTO GetUserInput()
        {
            SalesImportCsvInputDTO input = new SalesImportCsvInputDTO();

            input = GetFilePath(input);

            input = GetFormats(input);  

            string isDateRangeYesOrNo = null;
            while(!IsValidYesOrNo(isDateRangeYesOrNo))
            {
                Console.WriteLine();
                Console.WriteLine("Do you want to export statistics of a specific date range?");
                Console.WriteLine("[Y/N] ?: ");

                isDateRangeYesOrNo = Console.ReadLine();
            }

            if (IsYesOrNo(isDateRangeYesOrNo))
            {
                while (!IsDateFromBeforeDateAfter(input.Date.FromDate, input.Date.ToDate))
                {
                    input = CollectDateRangeInput(input);
                }
            }

            return input;
        }

        private static SalesImportCsvInputDTO CollectDateRangeInput(SalesImportCsvInputDTO input)
        {
            Console.WriteLine("");
            Console.WriteLine("Please specify the Date Range for the calculation of Sales statistics");
            Console.WriteLine("");

            string? fromDate = null;
            string dateFormatStr = DateTimeUtil.GetDateFormatByDateFormatEnum(input.Date.DateFormat.Value);
            while (!IsDateFromValid(fromDate, dateFormatStr))
            {
                Console.WriteLine();
                Console.WriteLine(string.Format("From Date: example format ({0})", dateFormatStr));
                Console.WriteLine();

                fromDate = Console.ReadLine();
            }

            Console.WriteLine();

            string? toDate = null;
            while (!IsDateToValid(toDate, dateFormatStr))
            {
                Console.WriteLine();
                Console.WriteLine(string.Format("To Date: example format ({0})", dateFormatStr));
                Console.WriteLine();

                toDate = Console.ReadLine();
            }

            input.Date.FromDate = DateTimeUtil.ConvertStringToDateTimeFormat(dateFormatStr, fromDate);
            input.Date.ToDate = DateTimeUtil.ConvertStringToDateTimeFormat(dateFormatStr, toDate);

            input.Date.IsExportResultsFromToDate = true;

            return input;
        }

        internal static SalesImportCsvInputDTO GetFormats(SalesImportCsvInputDTO input)
        {
            string? dateFormat = null;
            while (!IsDateFormatValid(dateFormat))
            {
                Console.WriteLine();
                Console.WriteLine("Please specify the Date Format as per the import CSV file(s):");
                Console.WriteLine("");
                Console.WriteLine(DateTimeUtil.GetDateFormatEnumDescriptions());

                dateFormat = Console.ReadLine();
            }

            int dateFormatInt = int.Parse(dateFormat, NumberStyles.Integer);
            input.Date = new DateDTO
            {
                DateFormat = (DateFormatEnum?)dateFormatInt
            };

            Console.WriteLine();

            string? amountFormat = null;
            while (!IsAmountFormatValid(amountFormat))
            {
                Console.WriteLine();
                Console.WriteLine("Please specify the Amount Format as per the import CSV file(s):");
                Console.WriteLine("");
                Console.WriteLine(AmountUtil.GetAmountFormatEnumDescriptions());

                amountFormat = Console.ReadLine();
            }

            Console.WriteLine();

            string? includeCurrencySign = null;
            while (!IsAmountCurrencyFormatValid(includeCurrencySign))
            {
                Console.WriteLine();
                Console.WriteLine("Please specify if the Amount Format includes a currency symbol as per the import CSV file(s):");
                Console.WriteLine("");
                Console.WriteLine(AmountUtil.GetAmountFormatCurrencySymbolsEnumDescriptions());

                includeCurrencySign = Console.ReadLine();
            }

            int amountFormatInt = int.Parse(amountFormat, NumberStyles.Integer);
            int currencySymbolInt = int.Parse(includeCurrencySign, NumberStyles.Integer);

            input.AmountFormat = new AmountFormatDTO()
            {
                Format = (AmountFormatEnum?)amountFormatInt,
                CurrencySymbol = (AmountCurrencyFormatEnum?)currencySymbolInt
            };

            string? numberOfDecimalPoints = null;
            if (input.AmountFormat.Format > AmountFormatEnum.AmountFormat1)
            {
                Console.WriteLine();

                while (!IsAmountDecimalPointsFormatValid(numberOfDecimalPoints))
                {
                    Console.WriteLine("Please specify the max number of decimal points of the Amount Format as per the import CSV file(s):");
                    Console.WriteLine(string.Format("Note: Max number of allowed decimal points is: {0}", AmountUtil.AMOUNT_DECIMAL_POINTS_MAX));

                    numberOfDecimalPoints = Console.ReadLine();
                }
            }

            int numberOfDecimalPointsInt = int.Parse(numberOfDecimalPoints ?? "0", NumberStyles.Integer);
            input.AmountFormat.NumberOfDecimalPoints = numberOfDecimalPointsInt;

            return input;
        }

        internal static SalesImportCsvInputDTO GetFilePath(SalesImportCsvInputDTO input)
        {
            string? inputFolderPath = null;
            while (!IsFilePathValid(inputFolderPath))
            {
                Console.WriteLine("Please specify the folder for the .CSV files(s):");
                Console.WriteLine("Note: if no path is specified, default [/ImportFiles] path will be used");
                inputFolderPath = Console.ReadLine();
            }

            input.FilesPath = !string.IsNullOrWhiteSpace(inputFolderPath) ? inputFolderPath : null;

            return input;
        }

        internal static SalesImportCsvInputDTO GetYearRange(SalesImportCsvInputDTO input, SalesImportCsvResultDTO result)
        {
            Console.WriteLine();

            string isYearRangeYesOrNo = null;
            while (!IsValidYesOrNo(isYearRangeYesOrNo))
            { 
                Console.WriteLine("Do you want to export statistics for a specific range of years");
                Console.WriteLine("[Y/N] ?: ");
                isYearRangeYesOrNo = Console.ReadLine();
            }

            if (IsYesOrNo(isYearRangeYesOrNo))
            {
                input = ConsoleInput.CollectYearRange(input, 
                    new SortedSet<string> ( result.StatisticPerYear.Keys.Select(y => y.ToString()) ));
            }

            return input;
        }

        internal static SalesImportCsvInputDTO CollectYearRange(SalesImportCsvInputDTO input, SortedSet<string> yearsAvailable)
        {
            Console.WriteLine("");
            Console.WriteLine("Please specify the Years Range for the calculation of Sales statistics");
            Console.WriteLine("");

            string? fromYear = null;
            while (!IsYearValid(fromYear, yearsAvailable))
            {
                Console.Write("From Year:");
                fromYear = Console.ReadLine();
            }

            Console.WriteLine("");

            string? toYear = null;
            while (!IsYearValid(toYear, yearsAvailable))
            {
                Console.Write("To Year:");
                toYear = Console.ReadLine();
            }

            input.Date.FromYear = fromYear;
            input.Date.ToYear = toYear;

            return input;
        }

        private static bool IsDateFromBeforeDateAfter(DateTime? from, DateTime? to)
        {
            if (!from.HasValue || !to.HasValue)
            {
                Console.WriteLine("FromDate or ToDate is empty!");
                return false;
            }

            if (from >= to)
            {
                Console.WriteLine("FromDate must have a date value before ToDate's date value");
                return false;
            }

            return true;
        }

        private static bool IsYearValid(string? year, SortedSet<string> yearsAvailable)
        {
            if (string.Empty.Equals(year))
            {
                Console.WriteLine("Please enter a valid year");
                return false;
            }

            if (!yearsAvailable.Contains(year))
            {
                Console.WriteLine(string.Format("Please enter a valid year from the available years [{0}]", string.Join("-", yearsAvailable)));
                return false;
            }

            return true;
        }

        internal static bool IsValidYesOrNo(string yesOrNo)
        {
            if (string.IsNullOrEmpty(yesOrNo))
            {
                return false;
            }

            if (!yesOrNo.Equals("Y", StringComparison.OrdinalIgnoreCase) &&
                !yesOrNo.Equals("N", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static bool IsYesOrNo(string yesOrNo)
        {
            if (!IsValidYesOrNo(yesOrNo))
            {
                throw new ArgumentException("Invalid Yes/No [Y/N]");
            }

            return yesOrNo.Equals("Y", StringComparison.OrdinalIgnoreCase) ? true : false;
        }

        private static bool IsFilePathValid(string? inputFolderPath)
        {
            if (string.Empty.Equals(inputFolderPath))
            {
                // valid case, default filepath will be used instead
                return true;
            }

            if (string.IsNullOrEmpty(inputFolderPath))
            {
                return false;
            }

            int i = 0;
            bool isNum = int.TryParse(inputFolderPath, out i);
            if (isNum)
            {
                Console.WriteLine("Not a valid path: This value is numeric");
                return false;
            }

            try
            {
                FileUtil.GetFilesFromPath(inputFolderPath, FileUtil.FILES_EXTENTION_COMMA_SEPARATED);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return false;
        }

        private static bool IsDateToValid(string? toDate, string format)
        {
            if (string.IsNullOrEmpty(toDate))
            {
                return false;
            }

            DateTime? result = DateTimeUtil.ConvertStringToDateTimeFormat(format, toDate);

            if (result != null)
                return true;
            else
            {
                Console.WriteLine("Invalid Date or format of Date");
                return false;
            }
        }

        private static bool IsDateFromValid(string? fromDate, string format)
        {
            if (string.IsNullOrEmpty(fromDate))
            {
                return false;
            }

            DateTime? result = DateTimeUtil.ConvertStringToDateTimeFormat(format, fromDate);

            if (result != null)
                return true;
            else
            {
                Console.WriteLine("Invalid Date or format of Date");
                return false;
            }
        }

        private static bool IsAmountDecimalPointsFormatValid(string? numberOfDecimalPoints)
        {
            if (string.IsNullOrEmpty(numberOfDecimalPoints))
            {
                return false;
            }

            bool isValid = int.TryParse(numberOfDecimalPoints, NumberStyles.Integer, CultureInfo.InvariantCulture, out int resultInt);
            if (!isValid)
            {
                Console.WriteLine("Not an integer number");
                return false;
            }

            if (resultInt <= 0)
            {
                Console.WriteLine("Invalid decimal points number");
                return false;
            }

            if (resultInt > AmountUtil.AMOUNT_DECIMAL_POINTS_MAX)
            {
                Console.WriteLine(string.Format("Maximum valid decimal points are {0}", AmountUtil.AMOUNT_DECIMAL_POINTS_MAX));
                return false;
            }

            return true;
        }

        private static bool IsDateFormatValid(string dateFormat)
        {
            if (string.IsNullOrEmpty(dateFormat))
            {
                return false;
            }

            bool isValid = int.TryParse(dateFormat, NumberStyles.Integer, CultureInfo.InvariantCulture, out int resultInt);
            if (!isValid)
            {
                Console.WriteLine("Not an integer number");
                return false;
            }

            DateFormatEnum dateFormatEnum = (DateFormatEnum)resultInt;
            if (dateFormatEnum == DateFormatEnum.None || dateFormatEnum > DateFormatEnum.DateFormat3)
            {
                Console.WriteLine("Not a valid choice");
                return false;
            }

            return true;
        }

        private static bool IsAmountFormatValid(string amountFormat)
        {
            if (string.IsNullOrEmpty(amountFormat))
            {
                return false;
            }

            bool isValid = int.TryParse(amountFormat, NumberStyles.Integer, CultureInfo.InvariantCulture, out int resultInt);
            if (!isValid)
            {
                Console.WriteLine("Not an integer number");
                return false;
            }

            AmountFormatEnum amountFormatEnum = (AmountFormatEnum)resultInt;
            if (amountFormatEnum == AmountFormatEnum.None || amountFormatEnum > AmountFormatEnum.AmountFormat4)
            {
                Console.WriteLine("Not a valid choice");
                return false;
            }

            return true;
        }

        private static bool IsAmountCurrencyFormatValid(string amountCurrencyFormat)
        {
            if (string.IsNullOrEmpty(amountCurrencyFormat))
            {
                return false;
            }

            bool isValid = int.TryParse(amountCurrencyFormat, NumberStyles.Integer, CultureInfo.InvariantCulture, out int resultInt);
            if (!isValid)
            {
                Console.WriteLine("Not an integer number");
                return false;
            }

            AmountCurrencyFormatEnum amountCurrencyFormatEnum = (AmountCurrencyFormatEnum)resultInt;
            if (amountCurrencyFormatEnum > AmountCurrencyFormatEnum.Dollar)
            {
                Console.WriteLine("Not a valid choice");
                return false;
            }

            return true;
        }
    }
}
