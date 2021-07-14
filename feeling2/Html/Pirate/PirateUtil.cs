﻿using Newtonsoft.Json;
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
        // static string mCfgFile = NativeConst.CurrentDirectory + "pirate_mission.cfg";
        public static PirateMission MyPirateMission;

        static OgameParser mParser = new OgameParser();

        public static List<string> NpcList = new List<string>();
        public static bool HasNpcData => NpcList.Count > 0;

        protected static string GetFilePath(int idx = 0)
        {
            string flag = idx <= 0 ? "" : idx.ToString();
            return $"{NativeConst.CurrentDirectory}pirate_mission{flag}.cfg";
        }

        public static void Save(PirateMission pMission, int idx = 0)
        {
            try
            {
                MyPirateMission = pMission;
                string text = JsonConvert.SerializeObject(pMission, Formatting.Indented);
                File.WriteAllText(GetFilePath(idx), text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PirateUtil save catch {ex.Message}");
            }
        }

        public static bool ReadCfg(int idx = 0)
        {
            var filePath = GetFilePath(idx);
            if (!File.Exists(filePath)) return false;

            try
            {
                var text = File.ReadAllText(filePath);
                MyPirateMission = JsonConvert.DeserializeObject<PirateMission>(text);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PirateUtil readCfg catch {ex.Message}");
                return false;
            }
        }

        public static void ParseNpc(string source)
        {
            Console.WriteLine($"ParseNpc");
            if (!HtmlUtil.ParseNpc(source, out List<string> result, mParser)) return;
            if (null == result) return;

            NpcList = result;
        }

        public static void ResetNpc()
        {
            NpcList.Clear();
        }
    }
}