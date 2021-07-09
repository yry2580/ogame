using Spire.Xls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeling
{
    class IoUtil
    {
        public static bool CsvToXlsx(string fileName, string newFileName)
        {
            try
            {
                if (!fileName.EndsWith(".csv")) return false;
                if (!newFileName.EndsWith(".xlsx") && !newFileName.EndsWith(".xls")) return false;

                Workbook workbook = new Workbook();
                workbook.LoadFromFile(fileName, ",", 1, 1);
                // Worksheet sheet = workbook.Worksheets[0];
                workbook.SaveToFile(newFileName, FileFormat.Version2016);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"XlsxToCsv \n old: {fileName} \n new: {newFileName}) \n catch: {ex.Message}");
                return false;
            }
        }

        public static bool XlsxToCsv(string fileName, string newFileName)
        {
            try
            {
                if (!fileName.EndsWith(".xlsx") && !fileName.EndsWith(".xls")) return false;
                if (!newFileName.EndsWith(".csv")) return false;

                Workbook workbook = new Workbook();
                workbook.LoadFromFile(fileName);
                // Worksheet sheet = workbook.Worksheets[0];
                workbook.SaveToFile(newFileName, ",");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"XlsxToCsv \n old: {fileName} \n new: {newFileName}) \n catch: {ex.Message}");
                return false;
            }
        }
    }
}
