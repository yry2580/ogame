using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Parser;

namespace feeling
{
    class Galaxy
    {
        public const int MaxX = 9;
        public const int MaxY = 499;

        public int X { get; private set; } = 0;
        public int Y { get; private set; } = 0;

        public int NextX { get; private set; } = 0;
        public int NextY { get; private set; } = 0;

        public int OperCount { get; private set; } = 0;
        public string UniverseName = "";

        public int Count => mGalaxyDict.Count;
        IDictionary<string, string> mGalaxyDict = new ConcurrentDictionary<string, string>();

        HtmlParser mHtmlParser = new HtmlParser();

        public bool AddPage(string source)
        {
            if (string.IsNullOrEmpty(source)) return false;

            if (source.IndexOf("id=\"galaxypage\"") < 0)
            {
                return false;
            }

            if (ParsePage(source))
            {
                Move(out int x, out int y);
                X = x;
                Y = y;
            }

            return true;
        }

        public bool TryMove()
        {
            if (Move(out int x, out int y))
            {
                NextX = x;
                NextY = y;
                return true;
            }

            return false;
        }

        protected bool Move(out int x, out int y)
        {
            x = X;
            y = Y;
            if (x >= MaxX && y >= MaxY) return false;

            var _x = X <= 0 ? 1 : X;
            var _y = Y + 1;
            _y = _y <= 0 ? 1 : _y;

            if (_y > MaxY)
            {
                _y = 1;
                _x += 1;
            }

            x = _x;
            y = _y;
            return true;
        }

        public void Clear()
        {
            X = 0;
            Y = 0;
            NextX = 1;
            NextY = 1;
            OperCount = 0;
            mGalaxyDict.Clear();
        }

        protected bool ParsePage(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;

            var doc = mHtmlParser.ParseDocument(source);
            var galaxypage = doc?.QuerySelector("#galaxypage");
            var list = galaxypage?.QuerySelectorAll("tbody tr");
            if (null == list) return false;

            var pagexy = list[0];
            var xy = pagexy?.TextContent.Replace("太阳系", "").Split(':');
            if (null == xy) return false;
            int x = int.Parse(xy[0]);
            int y = int.Parse(xy[1]);

            string key = $"{x}:{y:d3}";

            for (int z = 1; z < 16; z++)
            {
                int idx = z + 1;
                if (idx >= list.Length) break;

                var e = list[idx];
                var childs = e.Children;

                var name = HtmlUtil.ParseText(childs[5].TextContent, "(");
                if (string.IsNullOrWhiteSpace(name)) continue;

                var rno = HtmlUtil.ParseText(childs[0].TextContent);
                var union = HtmlUtil.ParseText(childs[6].TextContent);
                var rank = "";
                var mat = Regex.Match(childs[5].InnerHtml, $@"玩家 (?<name>\S*) 排名 (?<rank>\d*)");
                if (mat.Success)
                {
                    name = mat.Groups["name"].Value;
                    rank = mat.Groups["rank"].Value;
                }

                mGalaxyDict[key] = $"{x}:{y}:{rno},{name},{union},{rank}";
            }

            return true;
        }

        public void Save()
        {
            try
            {
                string name = NativeConst.FileDirectory + $"{UniverseName}_{DateTime.Now:yyyyMMdd}.csv";

                if (mGalaxyDict.Count <= 0)
                {
                    return;
                }

                var sb = new StringBuilder();

                var ret = from obj in mGalaxyDict orderby obj.Key ascending select obj.Value;

                foreach(var str in ret)
                {
                    sb.AppendLine(str);
                }

                File.WriteAllText(name, sb.ToString(), Encoding.Default);
                string xlsx = NativeConst.FileDirectory + $"{UniverseName}_{DateTime.Now:yyyyMMdd}.xlsx";
                IoUtil.CsvToXlsx(name, xlsx);

                sb = null;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Save galaxy catch {ex.Message}");
            }
        }
    }
}
