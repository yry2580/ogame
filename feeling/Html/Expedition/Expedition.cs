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
        static string missionCfgFile = NativeConst.CfgDirectory + "ex_mission.cfg";

        public static ExMission MyExMissionCfg;
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

        public static void Save(ExMission exMission)
        {
            try
            {
                MyExMissionCfg = exMission;
                string text = JsonConvert.SerializeObject(exMission, Formatting.Indented);
                File.WriteAllText(missionCfgFile, text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"expedition save catch {ex.Message}");
            }
        }

        public static void ReadCfg()
        {
            if (!File.Exists(missionCfgFile)) return;

            try
            {
                var text = File.ReadAllText(missionCfgFile);
                MyExMissionCfg = JsonConvert.DeserializeObject<ExMission>(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"expedition readCfg catch {ex.Message}");
            }
        }

        public OgameParser Parser = new OgameParser();
    }
}
