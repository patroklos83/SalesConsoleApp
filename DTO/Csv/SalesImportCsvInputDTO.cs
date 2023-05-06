using SalesConsoleApp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesConsoleApp.DTO.Csv
{
    internal class SalesImportCsvInputDTO
    {
        public string? FilesPath { get; set; }
        public DateDTO Date { get; set; }
        public AmountFormatDTO AmountFormat { get; set; }
    }

    internal class DateDTO
    {
        public DateFormatEnum? DateFormat { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? FromYear { get; set; }
        public string? ToYear { get; set; }
        public bool IsExportResultsFromToDate { get; set; }
    }

    internal class AmountFormatDTO
    {
        public AmountFormatEnum? Format { get; set; }
        public string StrAmountFormat { get; set; }
        public int NumberOfDecimalPoints { get; set; }
        public AmountCurrencyFormatEnum? CurrencySymbol { get; set; }
    }
}
