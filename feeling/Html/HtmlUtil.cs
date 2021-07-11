using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
