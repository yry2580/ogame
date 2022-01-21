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
    class GatherUtil
    {
        public static GatherMission MyMission;
        public static GatherMission MyMission1;

        public static string Universe = "";

        public static void Initialize()
        {
            // 读取配置
            ReadCfg(0);
            ReadCfg(1);

            if (null == MyMission)
            {
                Save(new GatherMission(), 0);
            }

            if (null == MyMission1)
            {
                Save(new GatherMission(), 1);
            }
        }

        protected static string GetFilePath(int idx = 0)
        {
            string flag = idx <= 0 ? "" : idx.ToString();
            return $"{NativeConst.CfgDirectory}gather_mission{flag}.cfg";
        }

        public static void Save(GatherMission pMission, int idx = 0)
        {
            try
            {
                pMission.IsCross = Universe == "w1";

                NativeLog.Info($"GatherUtil save idx: {idx}、isCross: {pMission.IsCross}");
                if (idx == 1)
                {
                    MyMission1 = pMission;
                }
                else
                {
                    MyMission = pMission;
                }

                string text = JsonConvert.SerializeObject(pMission, Formatting.Indented);
                File.WriteAllText(GetFilePath(idx), text);
            }
            catch (Exception ex)
            {
                NativeLog.Error($"GatherUtil save catch {ex.Message}");
            }
        }

        public static bool ReadCfg(int idx = 0)
        {
            var filePath = GetFilePath(idx);
            if (!File.Exists(filePath)) return false;

            try
            {
                var text = File.ReadAllText(filePath);
                var mission = JsonConvert.DeserializeObject<GatherMission>(text);
                if (idx == 1)
                {
                    MyMission1 = mission;
                }
                else
                {
                    MyMission = mission;
                }
                return true;
            }
            catch (Exception ex)
            {
                NativeLog.Error($"GatherUtil readCfg catch {ex.Message}");
                return false;
            }
        }
    }
}
