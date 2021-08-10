using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace feeling
{
    class PirateUtil
    {
        // static string mCfgFile = NativeConst.CurrentDirectory + "pirate_mission.cfg";
        public static PirateMission MyMission;
        public static PirateMission MyCrossMission;

        static OgameParser mParser = new OgameParser();

        public static List<string> NpcList = new List<string>();
        public static bool HasNpcData => NpcList.Count > 0;
        public static string Universe = "";

        protected static string GetFilePath(int idx = 0)
        {
            string flag = idx <= 0 ? "" : idx.ToString();
            return $"{NativeConst.CfgDirectory}pirate_mission{flag}.cfg";
        }

        public static void Save(PirateMission pMission, int idx = 0)
        {
            try
            {
                MyMission = pMission;
                string text = JsonConvert.SerializeObject(pMission, Formatting.Indented);
                File.WriteAllText(GetFilePath(idx), text);
            }
            catch (Exception ex)
            {
                NativeLog.Error($"PirateUtil save catch {ex.Message}");
            }
        }

        public static bool ReadCfg(int idx = 0)
        {
            var filePath = GetFilePath(idx);
            if (!File.Exists(filePath)) return false;

            try
            {
                var text = File.ReadAllText(filePath);
                MyMission = JsonConvert.DeserializeObject<PirateMission>(text);
                return true;
            }
            catch (Exception ex)
            {
                NativeLog.Error($"PirateUtil readCfg catch {ex.Message}");
                return false;
            }
        }

        public static void ParseNpc(string source, string address = "")
        {
            NativeLog.Info($"ParseNpc");
            if (!HtmlUtil.ParseNpc(source, out List<string> result, mParser)) return;
            if (null == result) return;

            var mat = Regex.Match(address, $@"://(?<universe>\S*).cicihappy.com");
            if (mat.Success)
            {
                Universe = mat.Groups["universe"].Value;
            }

            NpcList = result;
        }

        public static void ResetNpc()
        {
            Universe = "";
            NpcList.Clear();
        }
    }
}
