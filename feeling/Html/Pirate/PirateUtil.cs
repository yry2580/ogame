using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeling
{
    class PirateUtil
    {
        static string mCfgFile = NativeConst.CurrentDirectory + "pirate_mission.cfg";

        public static PirateMission MyPirateMission;

        static HtmlParser mParser = new HtmlParser();

        public static List<string> NpcList = new List<string>();
        public static bool HasNpcData => NpcList.Count > 0;

        public static void Save(PirateMission pMission)
        {
            try
            {
                MyPirateMission = pMission;
                string text = JsonConvert.SerializeObject(pMission, Formatting.Indented);
                File.WriteAllText(mCfgFile, text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PirateUtil save catch {ex.Message}");
            }
        }

        public static void ReadCfg()
        {
            if (!File.Exists(mCfgFile)) return;

            try
            {
                var text = File.ReadAllText(mCfgFile);
                MyPirateMission = JsonConvert.DeserializeObject<PirateMission>(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PirateUtil readCfg catch {ex.Message}");
            }
        }

        public static void ParseNpc(string source)
        {
            Console.WriteLine($"ParseNpc");

            if (string.IsNullOrEmpty(source)) return;

            if (source.IndexOf("id=\"galaxy_form\"") < 0)
            {
                return;
            }

            var doc = mParser.ParseDocument(source);
            var list = doc?.QuerySelectorAll("#galaxy_form table select[name='npczuobiao'] option");
            if (null == list) return;

            NpcList = list.Where(e => e.TextContent.Contains("海盗")).Select(e => e.TextContent.Trim()).ToList();
        }

        public static void ResetNpc()
        {
            NpcList.Clear();
        }

        public static bool HasAttack(string source, Pos pos)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;
            var parser = new HtmlParser();
            var doc = parser.ParseDocument(source);
            var node = doc?.QuerySelector("#fleetdelaybox");
            if (null == node) return false;

            var thQuery = doc.QuerySelectorAll("center center table tr th");
            var target = $"[{pos.X}:{pos.Y}:{pos.Z}]";
            var ret = thQuery.Where(e => e.TextContent.Contains(target));
            if (null == ret) return false;
            return ret.Count() > 0;
        }
    }
}
