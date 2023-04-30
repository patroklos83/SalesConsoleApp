using Microsoft.VisualBasic;
using SalesConsoleApp.DTO.Csv;
using SalesConsoleApp.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SalesConsoleApp.Utility
{
    internal class AmountUtil
    {
        internal const string AMOUNT_FORMAT = "XXXXX";
        internal const string AMOUNT_FORMAT2 = "XXXXX.XX";
        internal const string AMOUNT_FORMAT3 = "X,XXXX.XX";
        internal const string AMOUNT_FORMAT4 = "XXXXX,XX";

        internal const int AMOUNT_DECIMAL_POINTS_MAX = 6;

        //internal const string REGX_AMOUNT_FORMAT = @"^[1-9]+[0-9]*$";
        //internal const string REGX_AMOUNT_FORMAT2 = @"^(([1-9]\d*(\d+)*(\.\d{1,4})?)|(0\.\d{1,4})|0)$";
        //internal const string REGX_AMOUNT_FORMAT3 = @"^(([1-9]\d*(\,\d+)*(\.\d{1,4})?)|(0\.\d{1,4})|0)$";
        //internal const string REGX_AMOUNT_FORMAT4 = @"^(([1-9]\d*(\d+)*(\,\d{1,4})?)|(0\.\d{1,4})|0)$";


        internal const string REGX_AMOUNT_FORMAT = @"^{currenySymbol}(([1-9]\d*(\{thousandsComma}\d+)*(\{decimalPointSeparator}\d{{decPoints}})?)|(0\.\d{1,4})|0)$";

        internal static bool ValidateAmountFormat(string decimalFormat, string amountString, AmountFormatDTO formatDTO = null)
        {
            string? regx = null;

            switch (decimalFormat)
            {
                case AMOUNT_FORMAT:
                    regx = REGX_AMOUNT_FORMAT;
                    regx = regx.Replace("{decPoints}", "0");
                    break;
                case AMOUNT_FORMAT2:
                    regx = REGX_AMOUNT_FORMAT;
                    regx = regx.Replace("{thousandsComma}", string.Empty);
                    break;
                case AMOUNT_FORMAT3:
                    regx = REGX_AMOUNT_FORMAT;
                    break;
                case AMOUNT_FORMAT4:
                    regx = REGX_AMOUNT_FORMAT;
                    regx = regx.Replace("{thousandsComma}", string.Empty).Replace("{decimalPointSeparator}", ",");
                    break;
                default: break;
            }

            if (string.IsNullOrEmpty(regx))
            {
                throw new ArgumentException("Amount Format cannot be mapped and calculated!");
            }

            int? decimalPoints = null;
            string? currencySymbol = null;
            if (formatDTO != null)
            {
                decimalPoints = formatDTO.NumberOfDecimalPoints;
                currencySymbol = formatDTO.CurrencySymbol != AmountCurrencyFormatEnum.None ? formatDTO.CurrencySymbol.GetDescription() : null;
               
                if (currencySymbol != null)
                    regx = regx.Replace("{currenySymbol}", @"(\" + currencySymbol + "){1}");

                regx = regx.Replace("{decPoints}", "1," + decimalPoints);
            }

            regx = regx.Replace("{currenySymbol}", string.Empty)
                .Replace("{thousandsComma}", ",")
                .Replace("{decimalPointSeparator}", ".")
                .Replace("{decPoints}", "1,4");

            var isValidCurency = Regex.IsMatch(amountString, regx);

            if (!isValidCurency)
            {
                string error = string.Format("Expected format: {0} {1} {2}", decimalFormat,
                    currencySymbol != null ? ", Info: expected CurrencySymbol = " + currencySymbol : string.Empty,
                    decimalPoints != null ? ", Info: expected " + decimalPoints + " decimal points" : string.Empty);
                throw new ArgumentException(error);
            }

            return true;
        }
        internal static decimal? ConvertStringToDecimalFormat(string amountFormat, string amount, AmountFormatDTO formatDTO = null)
        {
            try
            {
                ValidateAmountFormat(amountFormat, amount, formatDTO);

                if (amountFormat.Equals(AMOUNT_FORMAT4))
                {
                    amount = amount.Replace(",", ".");
                }
                else if (amountFormat.Equals(AMOUNT_FORMAT))
                {
                    amount = amount.Replace(",", string.Empty);
                }

                if (formatDTO != null)
                {
                    if (formatDTO.CurrencySymbol != AmountCurrencyFormatEnum.None)
                    {
                        amount = amount.Replace(formatDTO.CurrencySymbol.GetDescription(), string.Empty);
                    }
                }

                return decimal.Parse(amount, NumberStyles.AllowDecimalPoint);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Unable to parse number {0}: error {1}", amount, ex.Message));
            }
        }

        public static string GetAmountFormatEnumDescriptions()
        {
            var stringBuilder = new StringBuilder();

            foreach (AmountFormatEnum i in AmountFormatEnum.GetValues(typeof(AmountFormatEnum)))
            {
                if (i == AmountFormatEnum.None) continue;

                string description = i.GetDescription();
                stringBuilder.Append(string.Format("{0} = {1}", (int)i, description));
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        public static string GetAmountFormatCurrencySymbolsEnumDescriptions()
        {
            var stringBuilder = new StringBuilder();
            var numberFormatInfo = CultureInfo.CurrentCulture.NumberFormat;
            foreach (AmountCurrencyFormatEnum i in AmountCurrencyFormatEnum.GetValues(typeof(AmountCurrencyFormatEnum)))
            {
                string description = string.Format(numberFormatInfo, "{0:c}", i.GetDescription());
                stringBuilder.Append(string.Format("{0} = {1}", (int)i, description));
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        internal static string GetAmountFormatByAmountFormatEnum(AmountFormatEnum amountFormatEnum)
        {
            string? amountFormatVal = null;

            switch (amountFormatEnum)
            {
                case AmountFormatEnum.None:
                    amountFormatVal = string.Empty;
                    break;
                case AmountFormatEnum.AmountFormat1:
                    amountFormatVal = AMOUNT_FORMAT;
                    break;
                case AmountFormatEnum.AmountFormat2:
                    amountFormatVal = AMOUNT_FORMAT2;
                    break;
                case AmountFormatEnum.AmountFormat3:
                    amountFormatVal = AMOUNT_FORMAT3;
                    break;
                case AmountFormatEnum.AmountFormat4:
                    amountFormatVal = AMOUNT_FORMAT4;
                    break;
                default: break;
            }

            if (string.IsNullOrEmpty(amountFormatVal))
            {
                throw new ArgumentException(string.Format("Amount Format provided is invalid and cannot be mapped: value {0}", amountFormatEnum.GetDescription()));
            }

            return amountFormatVal;
        }

    }
}
