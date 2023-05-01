using Microsoft.VisualBasic;
using SalesConsoleApp.DTO.Csv;
using SalesConsoleApp.Enums;
using SalesConsoleApp.Process;
using SalesConsoleApp.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesConsoleApp.UserInterface
{
    internal class ConsoleInput
    {
        internal static SalesImportCsvInputDTO GetUserInput()
        {
            SalesImportCsvInputDTO result = new SalesImportCsvInputDTO();

            string? inputFolderPath = null;
            while (!IsFilePathValid(inputFolderPath))
            {
                Console.WriteLine("Please specify the folder for the .CSV files(s):");
                Console.WriteLine("Note: if no path is specified, default [/ImportFiles] path will be used");
                inputFolderPath = Console.ReadLine();
            }

            result.FilesPath = !string.IsNullOrWhiteSpace(inputFolderPath) ? inputFolderPath : null;

            string? dateFormat = null;
            while (!IsDateFormatValid(dateFormat))
            {
                Console.WriteLine("Please specify the Date Format as per the import CSV file(s):");
                Console.WriteLine("");
                Console.WriteLine(DateTimeUtil.GetDateFormatEnumDescriptions());

                dateFormat = Console.ReadLine();
            }

            int dateFormatInt = int.Parse(dateFormat, NumberStyles.Integer);
            result.Date = new DateDTO
            {
                DateFormat = (DateFormatEnum?)dateFormatInt
            };

            Console.WriteLine("");

            string? amountFormat = null;
            while (!IsAmountFormatValid(amountFormat))
            {
                Console.WriteLine("Please specify the Amount Format as per the import CSV file(s):");
                Console.WriteLine("");
                Console.WriteLine(AmountUtil.GetAmountFormatEnumDescriptions());

                amountFormat = Console.ReadLine();
            }

            Console.WriteLine("");

            string? includeCurrencySign = null;
            while (!IsAmountCurrencyFormatValid(includeCurrencySign))
            {
                Console.WriteLine("Please specify if the Amount Format includes a currency symbol as per the import CSV file(s):");
                Console.WriteLine("");
                Console.WriteLine(AmountUtil.GetAmountFormatCurrencySymbolsEnumDescriptions());

                includeCurrencySign = Console.ReadLine();
            }

            int amountFormatInt = int.Parse(amountFormat, NumberStyles.Integer);
            int currencySymbolInt = int.Parse(includeCurrencySign, NumberStyles.Integer);

            result.AmountFormat = new AmountFormatDTO()
            {
                Format = (AmountFormatEnum?)amountFormatInt,
                CurrencySymbol = (AmountCurrencyFormatEnum?)currencySymbolInt
            };

            string? numberOfDecimalPoints = null;
            if (result.AmountFormat.Format > AmountFormatEnum.AmountFormat1)
            {
                Console.WriteLine("");

                while (!IsAmountDecimalPointsFormatValid(numberOfDecimalPoints))
                {
                    Console.WriteLine("Please specify the max number of decimal points of the Amount Format as per the import CSV file(s):");
                    Console.WriteLine(string.Format("Note: Max number of allowed decimal points is: {0}", AmountUtil.AMOUNT_DECIMAL_POINTS_MAX));

                    numberOfDecimalPoints = Console.ReadLine();
                }
            }

            Console.WriteLine("Do you want to export statistics of a specific date range? [Y/N]:?");
            bool isDateRange = false;
            while(!IsYesOrNo(isDateRange))
            {


            }


            Console.WriteLine("");
            Console.WriteLine("Please specify the Date Range for the calculation of Sales statistics");
            Console.WriteLine("");

            int numberOfDecimalPointsInt = int.Parse(numberOfDecimalPoints ?? "0", NumberStyles.Integer);
            result.AmountFormat.NumberOfDecimalPoints = numberOfDecimalPointsInt;

            string? fromDate = null;
            string dateFormatStr = DateTimeUtil.GetDateFormatByDateFormatEnum(result.Date.DateFormat.Value);
            while (!IsDateFromValid(fromDate, dateFormatStr))
            {
                Console.WriteLine(string.Format("From Date: example format ({0})", dateFormatStr));
                Console.WriteLine("");

                fromDate = Console.ReadLine();
            }

            Console.WriteLine("");

            string? toDate = null;
            while (!IsDateToValid(toDate, dateFormatStr))
            {
                Console.WriteLine(string.Format("To Date: example format ({0})", dateFormatStr));
                Console.WriteLine("");

                toDate = Console.ReadLine();
            }

            result.Date.FromDate = DateTimeUtil.ConvertStringToDateTimeFormat(dateFormatStr, fromDate);
            result.Date.ToDate = DateTimeUtil.ConvertStringToDateTimeFormat(dateFormatStr, toDate);

            return result;
        }

        private static bool IsYesOrNo(bool isDateRange)
        {
            throw new NotImplementedException();
        }

        private static bool IsFilePathValid(string? inputFolderPath)
        {
            if (string.Empty.Equals(inputFolderPath))
            {
                // valid case, default filepath will be used instead
                return true;
            }

            int i = 0;
            bool isNum = int.TryParse(inputFolderPath, out i);
            if (isNum)
            {
                Console.WriteLine("Not a valid path: This value is numeric");
                return false;
            }

            if (!string.IsNullOrEmpty(inputFolderPath))
            {
                return true;
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
