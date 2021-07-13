using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace feeling
{
    class HtmlUtil
    {
        public static string HtmlSpace = "&nbsp;";

        public static string ParseText(string source, string separator = "")
        {
            if (string.IsNullOrWhiteSpace(source)) return "";

            if (HtmlSpace == source) return "";

            if (!string.IsNullOrEmpty(separator))
            {
                int idx = source.IndexOf(separator);
                if (idx >= 0)
                {
                    source = source.Substring(0, idx);
                }
            }
            return source.Trim();
        }

        public static bool IsHomeUrl(string url = "")
        {
            return url.Contains("www.cicihappy.com");
        }

        public static bool IsGameUrl(string url = "")
        {
            return url.Contains(".cicihappy.com/ogame/frames.php");
        }

        public static bool IsInGame(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;

            var parser = new HtmlParser();
            var doc = parser.ParseDocument(source);
            var logout = doc.QuerySelector("#header_top a[accesskey=s]");
            if (null != logout) return true;

            var home = doc.QuerySelector("#menuTable .menubutton_table a[target=Hauptframe]");
            if (null != home) return true;

            return false;
        }

        public static bool HasLogoutBtn(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;

            var parser = new HtmlParser();
            var doc = parser.ParseDocument(source);
            var logout = doc.QuerySelector("#header_top a[accesskey=s]");
            return null != logout;
        }

        public static bool HasTutorial(string source, HtmlParser parser = null)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;
            parser = parser??new HtmlParser();
            var doc = parser.ParseDocument(source);
            var node = doc.QuerySelector("#tutorial .tutorial_buttons a");
            return null != node;
        }

        public static bool HasFleetSuccess(string source, HtmlParser parser = null)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;
            parser = parser??new HtmlParser();
            var doc = parser.ParseDocument(source);
            var node = doc.QuerySelector(".success");
            if (null == node) return false;
            if (node.TextContent.Trim() == "派遣舰队") return true;
            return false;
        }

        public static bool ParseFleetQueue(string source, out FleetQueue fleetQueue)
        {
            fleetQueue = null;

            if (string.IsNullOrWhiteSpace(source)) return false;
            var parser = new HtmlParser();
            var doc = parser.ParseDocument(source);
            var node = doc?.QuerySelector("#fleetdelaybox");
            if (null == node) return false;

            var trList = doc.QuerySelectorAll("center table tr").ToList();
            var idx = trList.FindIndex(e => e.Id == "fleetdelaybox");
            var tr = trList[idx + 1];

            var mat = Regex.Match(tr.TextContent, @"舰队 (?<jd>\d{1,2}) / (?<jdMax>\d{1,2})探险 (?<tx>\d) / (?<txMax>\d)");
            if (mat.Success)
            {
                fleetQueue = new FleetQueue
                {
                    Count = int.Parse(mat.Groups["jd"].Value),
                    MaxCount = int.Parse(mat.Groups["jdMax"].Value),
                    ExCount = int.Parse(mat.Groups["tx"].Value),
                    ExMaxCount = int.Parse(mat.Groups["txMax"].Value),
                };
                return true;
            }

            mat = Regex.Match(tr.TextContent, @"舰队 (?<jd>\d{1,2}) / (?<jdMax>\d{1,2})探险");
            if (mat.Success)
            {
                fleetQueue = new FleetQueue
                {
                    Count = int.Parse(mat.Groups["jd"].Value),
                    MaxCount = int.Parse(mat.Groups["jdMax"].Value),
                    ExCount = 0,
                    ExMaxCount = 0,
                };
                return true;
            }

            return false;
        }

        public static bool ParseShip(string source, string shipId, out int total)
        {
            total = 0;

            if (string.IsNullOrWhiteSpace(source)) return false;
            var parser = new HtmlParser();
            var doc = parser.ParseDocument(source);
            var node = doc?.QuerySelector("#fleetdelaybox");
            if (null == node) return false;

            var el = doc.QuerySelector($".l input[name={shipId}]");
            if (null == el) return false;

            var alt = el.GetAttribute("alt") ?? "";
            alt.Trim();
            var arr = alt.Split(' ');
            if (arr.Length < 2) return false;
            total = int.Parse(arr[1]);
            return true;
        }

        public static bool ParseSuccess(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;
            var parser = new HtmlParser();
            var doc = parser.ParseDocument(source);
            var node = doc?.QuerySelector(".success");
            if (null == node) return false;
            if (node.TextContent.Trim() == "派遣舰队") return true;
            return false;
        }
    }
}
