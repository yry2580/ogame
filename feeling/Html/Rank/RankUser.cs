using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeling
{
    class RankUser
    {
        public string Rank = "";
        public string Name = "";
        public string Union = "";
        public string CrossName = "";
        public string Score = "";
        public string Universe = "";

        public static void Save(List<RankUser> lists)
        {
            try
            {
                string name = NativeConst.FileDirectory + $"rank_user_{DateTime.Now:yyyyMMdd}";

                if (lists.Count <= 0)
                {
                    return;
                }

                var sb = new StringBuilder();
                string str = "";
                foreach (var item in lists)
                {
                    str = $"{item.Universe},{item.Rank},{item.Name},{item.Union},{item.CrossName},{item.Score}";
                    sb.AppendLine(str);
                }

                var csvName = $"{name}.csv";
                File.WriteAllText(csvName, sb.ToString(), Encoding.Default);
                IoUtil.CsvToXlsx(csvName, $"{name}.xlsx");

                NativeLog.Error($"RankUser save {csvName}");
                sb = null;
            }
            catch (Exception ex)
            {
                NativeLog.Error($"RankUser save catch {ex.Message}");
            }
        }
    }
}
