using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesConsoleApp.Utility
{
    internal class FileUtil
    {
        internal const string DEFAULT_FILES_PATH = "./ImportFiles";

        internal const string FILES_EXTENTION_COMMA_SEPARATED = "*.csv";
        internal static List<string> GetFilesFromPath(string filePath, string fileExtention)
        {
            var filesEnum = Directory.EnumerateFiles(filePath, fileExtention);
            List<string> files = new List<string>(filesEnum);

            if (files == null || !files.Any())
            {
                throw new ArgumentException(string.Format("No import csv files found in the specified path {0}", filePath));
            }

            return files;
        }
    }
}
