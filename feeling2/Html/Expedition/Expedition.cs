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
    class Expedition
    {
        // static string missionCfgFile = NativeConst.CfgDirectory + "ex_mission.cfg";
        public static ExMission MyExMissionCfg;
        public static ExMission MyExMissionCfg1;

        public static void Initialize()
        {
            // 读取配置
            ReadCfg(0);
            ReadCfg(1);

            if (null == MyExMissionCfg)
            {
                Save(new ExMission(), 0);
            }

            if (null == MyExMissionCfg1)
            {
                Save(new ExMission(), 1);
            }
        }

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

        protected static string GetFilePath(int idx = 0)
        {
            string flag = idx <= 0 ? "" : idx.ToString();
            return $"{NativeConst.CfgDirectory}ex_mission{flag}.cfg";
        }

        public static void Save(ExMission exMission, int idx = 0)
        {
            try
            {
                if (idx == 1)
                {
                    MyExMissionCfg1 = exMission;
                }
                else
                {
                    MyExMissionCfg = exMission;
                }

                string text = JsonConvert.SerializeObject(exMission, Formatting.Indented);
                File.WriteAllText(GetFilePath(idx), text);
            }
            catch (Exception ex)
            {
                NativeLog.Error($"expedition save catch {ex.Message}");
            }
        }

        public static bool ReadCfg(int idx = 0)
        {
            var filePath = GetFilePath(idx);
            if (!File.Exists(filePath)) return false;

            try
            {
                var text = File.ReadAllText(filePath);
                var exMission = JsonConvert.DeserializeObject<ExMission>(text);

                if (idx == 1)
                {
                    MyExMissionCfg1 = exMission;
                }
                else
                {
                    MyExMissionCfg = exMission;
                }

                return true;
            }
            catch (Exception ex)
            {
                NativeLog.Error($"expedition readCfg catch {ex.Message}");
                return false;
            }
        }

        public OgameParser Parser = new OgameParser();
    }
}
