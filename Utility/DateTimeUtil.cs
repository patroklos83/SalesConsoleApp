using SalesConsoleApp.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesConsoleApp.Utility
{
    internal class DateTimeUtil
    {
        internal const string DATE_FORMAT = "dd/MM/yyyy";
        internal const string DATE_FORMAT2 = "dd-MM-yyyy";
        internal const string DATE_FORMAT3 = "yyyy-MM-dd";

        public static bool IsValidDateFormat(string dateFormat)
        {
            string s = DateTime.Now.ToString(dateFormat, CultureInfo.InvariantCulture);
            var result = ConvertStringToDateTimeFormat(dateFormat, s);
            return result != null ? true : false;
        }

        public static DateTime? ConvertStringToDateTimeFormat(string dateFormat, string date)
        {
            DateTime result;

            bool isValid = DateTime.TryParseExact(date, dateFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out result);

            return isValid ? result : null;
        }

        public static string GetDateFormatEnumDescriptions()
        {
            var stringBuilder = new StringBuilder();

            foreach (DateFormatEnum i in DateFormatEnum.GetValues(typeof(DateFormatEnum)))
            {
                if (i == DateFormatEnum.None) continue;

                string description = i.GetDescription();
                stringBuilder.Append(string.Format("{0} = {1}", (int)i, description));
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        internal static string GetDateFormatByDateFormatEnum(DateFormatEnum dateFormatEnum)
        {
            string? dateFormatVal = null;

            switch (dateFormatEnum)
            {
                case DateFormatEnum.None:
                    dateFormatVal = string.Empty;
                    break;
                case DateFormatEnum.DateFormat1:
                    dateFormatVal = DATE_FORMAT;
                    break;
                case DateFormatEnum.DateFormat2:
                    dateFormatVal = DATE_FORMAT2;
                    break;
                case DateFormatEnum.DateFormat3:
                    dateFormatVal = DATE_FORMAT3;
                    break;
                default:  break;

            }

            if (string.IsNullOrEmpty(dateFormatVal))
            {
                throw new ArgumentException(string.Format("Date Format provided is invalid and cannot be mapped: value {0}", dateFormatEnum.GetDescription()));
            }

            return dateFormatVal;
        }
    }
}
