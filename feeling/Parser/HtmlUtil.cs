﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace feeling
{
    class HtmlUtil
    {
        public static string HtmlSpace = "&nbsp;";
        public static string Space = " ";

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

            var parser = new OgameParser();
            parser.LoadHtml(source);
#if !NET45
            var logout = parser.QuerySelector("#header_top a[accesskey=s]");
#else
            var logout = parser.QuerySelector("//*[@id='header_top']//a[@accesskey='s']");
#endif
            if (null != logout) return true;

#if !NET45
            var home = parser.QuerySelector("#menuTable .menubutton_table a[target=Hauptframe]");
#else
            var home = parser.QuerySelector("//*[@id='menuTable'//*[@class='menubutton_table']//a[@target='Hauptframe']");
#endif
            if (null != home) return true;

            return false;
        }

        public static bool ParseOwnerPlanets(string source, out List<string> result, OgameParser parser = null)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(source)) return false;

            if (source.IndexOf("id=\"header_top\"") < 0) return false;

            parser = parser??new OgameParser();
            parser.LoadHtml(source);

#if !NET45
            var list = parser?.QuerySelectorAll("#header_top option");
            if (null == list || list.Length <= 0) return false;
            result = list.Where(e => e.TextContent.Contains("[") && e.TextContent.Contains("]")).Select(e => e.TextContent.Trim()).ToList();
#else
            var list = parser?.QuerySelectorAll("//*[@id='header_top']//option");
            if (null == list || list.Count <= 0) return false;
            result = list.Where(e => e.InnerText.Contains("[") && e.InnerText.Contains("]")).Select(e => e.InnerText.Replace(HtmlSpace, Space).Trim()).ToList();
#endif
            return true;
        }

        public static bool HasLogoutBtn(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;

            var parser = new OgameParser();
            parser.LoadHtml(source);
#if !NET45
            var logout = parser.QuerySelector("#header_top a[accesskey=s]");
#else
            var logout = parser.QuerySelectorAll("//*[@id='header_top]//a[@accesskey='s']");
#endif
            return null != logout;
        }

        public static bool HasTutorial(string source, OgameParser parser = null)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;
            parser = parser??new OgameParser();
            parser.LoadHtml(source);
#if !NET45
            var node = parser.QuerySelector("#tutorial .tutorial_buttons a");
#else
            var node = parser.QuerySelector("//*[@id='tutorial']//*[@class='tutorial_buttons']//a");
#endif
            return null != node;
        }

        public static bool HasFleetSuccess(string source, OgameParser parser = null)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;
            parser = parser??new OgameParser();
            parser.LoadHtml(source);
#if !NET45
            var node = parser.QuerySelector(".success");
            if (null == node) return false;
            if (node.TextContent.Trim() == "派遣舰队") return true;
#else
            var node = parser.QuerySelector("//*[@class='success']");
            if (null == node) return false;
            if (node.InnerText.Trim() == "派遣舰队") return true;
#endif
            return false;
        }

        public static bool ParseFleetQueue(string source, out FleetQueue fleetQueue)
        {
            fleetQueue = null;

            if (string.IsNullOrWhiteSpace(source)) return false;
            var parser = new OgameParser();
            parser.LoadHtml(source);
#if !NET45
            var node = parser?.QuerySelector("#fleetdelaybox");
            if (null == node) return false;
            var trList = parser.QuerySelectorAll("center table tr").ToList();
            var idx = trList.FindIndex(e => e.Id == "fleetdelaybox");
            var tr = trList[idx + 1];
            var  TextContent = tr.TextContent;
#else
            var node = parser?.QuerySelector("//*[@id='fleetdelaybox']");
            if (null == node) return false;
            var trList = parser.QuerySelectorAll("//center//table//tr").ToList();
            var idx = trList.FindIndex(e => e.Id == "fleetdelaybox");
            var tr = trList[idx + 1];
            var TextContent = tr.InnerText.Trim();
#endif
            var mat = Regex.Match(TextContent, @"舰队 (?<jd>\d{1,2}) / (?<jdMax>\d{1,2})探险 (?<tx>\d) / (?<txMax>\d)");
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

            mat = Regex.Match(TextContent, @"舰队 (?<jd>\d{1,2}) / (?<jdMax>\d{1,2})探险");
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
            var parser = new OgameParser();
            parser.LoadHtml(source);
#if !NET45
            var node = parser?.QuerySelector("#fleetdelaybox");
            if (null == node) return false;
            var el = parser.QuerySelector($".l input[name={shipId}]");
            if (null == el) return false;
            var alt = el.GetAttribute("alt") ?? "";
#else 
            var node = parser?.QuerySelector("//*[@id='fleetdelaybox']");
            if (null == node) return false;
            var el = parser.QuerySelector($"//*[@class='l']//input[@name='{shipId}']");
            if (null == el) return false;
            var alt = el.GetAttributeValue("alt", "");
#endif
            alt.Trim();
            var arr = alt.Split(' ');
            if (arr.Length < 2) return false;
            total = int.Parse(arr[1]);
            return true;
        }

        public static bool ParseSuccess(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;
            var parser = new OgameParser();
            parser.LoadHtml(source);
#if !NET45
            var node = parser.QuerySelector(".success");
            if (null == node) return false;
            if (node.TextContent.Trim() == "派遣舰队") return true;
#else
            var node = parser.QuerySelector("//*[@class='success']");
            if (null == node) return false;
            if (node.InnerText.Trim() == "派遣舰队") return true;
#endif

            return false;
        }

        public static bool ParseGalaxyPage(string source, ref IDictionary<string, string> dict, OgameParser parser = null)
        {
#if !NET45
            if (string.IsNullOrWhiteSpace(source)) return false;
            parser = parser??new OgameParser();
            parser.LoadHtml(source);
            var list = parser?.QuerySelectorAll("#galaxypage tbody tr");
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

                var name = ParseText(childs[5].TextContent, "(");
                if (string.IsNullOrWhiteSpace(name)) continue;

                var rno = ParseText(childs[0].TextContent);
                var union = ParseText(childs[6].TextContent);
                var rank = "";
                var mat = Regex.Match(childs[5].InnerHtml, $@"玩家 (?<name>\S*) 排名 (?<rank>\d*)");
                if (mat.Success)
                {
                    name = mat.Groups["name"].Value;
                    rank = mat.Groups["rank"].Value;
                }

                dict[key] = $"{x}:{y}:{rno},{name},{union},{rank}";
            }
#else
            if (string.IsNullOrWhiteSpace(source)) return false;
            parser = parser ?? new OgameParser();
            parser.LoadHtml(source);
            var list = parser?.QuerySelectorAll("//*[@id='galaxypage']//tbody//tr");
            if (null == list) return false;

            var pagexy = list[0];
            var xy = pagexy?.InnerText.Replace("太阳系", "").Split(':');
            if (null == xy) return false;
            int x = int.Parse(xy[0]);
            int y = int.Parse(xy[1]);

            string key = $"{x}:{y:d3}";

            for (int z = 1; z < 16; z++)
            {
                int idx = z + 1;
                if (idx >= list.Count) break;

                var e = list[idx];
                var childs = e.SelectNodes("child::*");

                var name = ParseText(childs[5].InnerText, "(").Replace(HtmlSpace, ""); ;
                if (string.IsNullOrWhiteSpace(name)) continue;

                var rno = ParseText(childs[0].InnerText);
                var union = ParseText(childs[6].InnerText).Replace(HtmlSpace, "");
                var rank = "";
                var mat = Regex.Match(childs[5].InnerHtml, $@"玩家 (?<name>\S*) 排名 (?<rank>\d*)");
                if (mat.Success)
                {
                    name = mat.Groups["name"].Value;
                    rank = mat.Groups["rank"].Value;
                }

                dict[key] = $"{x}:{y}:{rno},{name},{union},{rank}";
            }
#endif
            return true;
        }

        public static bool ParseNpc(string source, out List<string> result, OgameParser parser = null)
        {
            result = null;
            if (string.IsNullOrEmpty(source)) return false;

            if (source.IndexOf("id=\"galaxy_form\"") < 0) return false;
            parser = parser??new OgameParser();
            parser.LoadHtml(source);
#if !NET45
            var list = parser.QuerySelectorAll("#galaxy_form table select[name='npczuobiao'] option");
            if (null == list) return false;

            result = list.Where(e => e.TextContent.Contains("海盗")).Select(e => e.TextContent.Trim()).ToList();
#else
            var list = parser.QuerySelectorAll("//*[@id='galaxy_form']//table//select[@name='npczuobiao']/option");
            if (null == list) return false;

            result = list.Where(e => e.InnerText.Contains("海盗")).Select(e => e.InnerText.Trim()).ToList();
#endif
            return true;
        }

        public static bool HasAttack(string source, Pos pos)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;
            var parser = new OgameParser();
            parser.LoadHtml(source);
#if !NET45
            var node = parser?.QuerySelector("#fleetdelaybox");
            if (null == node) return false;
            var thQuery = parser.QuerySelectorAll("center center table tr th");
            if (null == thQuery) return false;
            var target = $"[{pos.X}:{pos.Y}:{pos.Z}]";
            var ret = thQuery.Where(e => e.TextContent.Contains(target));
            if (null == ret) return false;
#else
            var node = parser?.QuerySelector("//*[@id='fleetdelaybox']");
            if (null == node) return false;

            var thQuery = parser.QuerySelectorAll("//center//center//table//tr//th");
            var target = $"[{pos.X}:{pos.Y}:{pos.Z}]";
            var ret = thQuery.Where(e => e.InnerText.Contains(target));
            if (null == ret) return false;
#endif
            return ret.Count() > 0;
        }
    }
}
