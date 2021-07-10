using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeling
{
    class Planet
    {
        HtmlParser mHtmlParser = new HtmlParser();
        public List<string> List = new List<string>();

        public bool HasData => List.Count > 0;

        public void Parse(string source)
        {
            if (string.IsNullOrEmpty(source)) return;

            if (source.IndexOf("id=\"header_top\"") < 0)
            {
                return;
            }

            var doc = mHtmlParser.ParseDocument(source);
            var list = doc?.QuerySelectorAll("#header_top option");
            if (null == list || list.Length <= 0) return;
            List = list.Select(e => e.TextContent.Trim()).ToList();
        }

        public void Reset()
        {
            List.Clear();
        }

        public int GetPlanetIndex(string planetName)
        {
            return List.FindIndex(e => e.Contains(planetName));
        }
    }
}
