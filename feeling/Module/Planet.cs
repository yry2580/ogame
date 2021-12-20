using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace feeling
{
    class Planet
    {
        OgameParser mHtmlParser = new OgameParser();
        public List<string> List = new List<string>();

        public bool HasData => List.Count > 0;
        public string Universe = "";

        public bool Parse(string source, string address = "")
        {
            NativeLog.Info("Planet Parse");
            if (!HtmlParser.ParseOwnerPlanets(source, out List<string> result, mHtmlParser)) return false;
            if (null == result) return false;

            var mat = Regex.Match(address, $@"://(?<universe>\S*).cicihappy.com");
            if (mat.Success)
            {
                Universe = mat.Groups["universe"].Value;
            }
            List = result;
            
            NativeLog.Info("Planet Parse end");
            return true;
        }

        public void Reset()
        {
            NativeLog.Info("Planet Reset");
            Universe = "";
            List.Clear();
        }

        public int GetPlanetIndex(string planetName)
        {
            return List.FindIndex(e => e.Contains(planetName));
        }

        public static int FindPlanet(string planetName, List<string> list)
        {
            if (string.IsNullOrWhiteSpace(planetName)) return -1;
            if (null == list || list.Count <= 0) return -1;
            var pos = planetName.LastIndexOf("[");
            if (pos <= 0) return -1;

            return list.FindIndex(e => e.Contains(planetName.Substring(0, pos - 1)) && e.Contains(planetName.Substring(pos)));
        }
    }
}
