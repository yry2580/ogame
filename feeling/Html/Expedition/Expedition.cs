using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace feeling
{
    class Expedition
    {
        public static List<ShipType> ShipOptions = new List<ShipType> {
            ShipType.SC,
            ShipType.LC,
            ShipType.LF,
            ShipType.DD,
            ShipType.BC
        };

        public static List<string> GetShipOptions()
        {
            return (from type in ShipOptions select Ship.GetShipName(type)).ToList();
        }

        public HtmlParser Parser = new HtmlParser();

        public bool ParseFleetQueue(string source, out FleetQueue fleetQueue)
        {
            fleetQueue = null;

            if (string.IsNullOrWhiteSpace(source)) return false;
            var doc = Parser.ParseDocument(source);
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

            return false;
        }

        public bool ParseShip(string source, string shipId, out int total)
        {
            total = 0;

            if (string.IsNullOrWhiteSpace(source)) return false;
            var doc = Parser.ParseDocument(source);
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

        public bool ParseSuccess(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;
            var doc = Parser.ParseDocument(source);
            var node = doc?.QuerySelector(".success");
            if (null == node) return false;
            if (node.TextContent.Trim() == "派遣舰队") return true;
            return false;
        }
    }
}
