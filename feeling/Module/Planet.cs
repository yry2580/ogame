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

            List<string> list = new List<string>();

            for(int i = 0; i < result.Count; i++)
            {
                string name = result[i];
                var _mat = NativeConst.PlanetRegex.Match(name);
                if (_mat.Success)
                {
                    list.Add(_mat.Value);
                }
                else
                {
                    list.Add(name);
                }
            }

            List = list;
            
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
            return FindPlanet(planetName, List);
        }

        public static int FindPlanet(string planetName, List<string> list)
        {
            if (string.IsNullOrWhiteSpace(planetName)) return -1;
            if (null == list || list.Count <= 0) return -1;

            var mat = NativeConst.PlanetRegex.Match(planetName);
            if (!mat.Success)
            {
                mat = NativeConst.PlanetRegex1.Match(planetName);
            }
            if (!mat.Success) return -1;
            var val0 = mat.Result("$1");
            var val1 = mat.Result("$2");
            return list.FindIndex(e => e.Contains(val0) && e.Contains(val1));
        }
    }
}
