using CsvHelper.Configuration;
using CsvHelper;
using SalesConsoleApp.DTO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesConsoleApp.Process;
using SalesConsoleApp.Utility;

namespace SalesConsoleApp.DTO.Csv
{
    public sealed class SalesDTOMap : ClassMap<SalesDTO>
    {
        public SalesDTOMap()
        {
            Map(m => m.Date).Convert(NullDateTimeParser);

            Map(m => m.Amount).Convert(NullDecimalParser);

            Map(m => m.Redundant).Convert(NullStringParser); // Used to catch any additional reduntant field-string on the same row
        }

        private DateTime? NullDateTimeParser(ConvertFromStringArgs arg)
        {
            var rawValue = arg.Row.GetField(0);

            if (string.IsNullOrEmpty(rawValue) || rawValue == "NULL")
                return null;
            else
            {
                DateTime? result = DateTimeUtil.ConvertStringToDateTimeFormat(ReadSalesProcess.DateFormat, rawValue);

                if (result != null)
                    return result;
                else
                    throw new ArgumentException(string.Format("Invalid Date provided [{0}]. Date format should be '{1}'", 
                        rawValue, ReadSalesProcess.DateFormat));
            }
        }

        private decimal? NullDecimalParser(ConvertFromStringArgs arg)
        {
            var rawValue = arg.Row.GetField(1);

            if (string.IsNullOrEmpty(rawValue) || rawValue == "NULL")
                return null;
            else
            {
                decimal? result = AmountUtil.ConvertStringToDecimalFormat(ReadSalesProcess.AmountFormat.StrAmountFormat, rawValue, ReadSalesProcess.AmountFormat);
                if (result != null)
                    return result;
                else
                    throw new ArgumentException(string.Format("Invalid Amount provided {0}. Amount format should be '{1}'",
                        rawValue, ReadSalesProcess.AmountFormat.StrAmountFormat));
            }
        }

        private string? NullStringParser(ConvertFromStringArgs arg)
        {
            arg.Row.TryGetField(typeof(string), 2, out object? rawValue);

            if (rawValue == null || string.IsNullOrEmpty(rawValue.ToString()) || rawValue.ToString() == "NULL")
                return null;
            else
                return rawValue.ToString();
        }

    }
}

