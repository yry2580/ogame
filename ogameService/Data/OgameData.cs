using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OgameService
{
    public class OgameData
    {
        public CmdEnum Cmd = CmdEnum.None;
        public ErrEnum Error = ErrEnum.Ok;
        public StatusEnum Status = StatusEnum.None;
        public string Id = "";
        public string Content = "";
        public string SessionKey = "";

        public string Universe = "";
        public string NpcUniverse = "";
        public string PlanetUniverse = "";

        public string FleetContent = "";
        public int PirateCfgIndex = 0;
        public int PirateSpeedIndex = 0;
        public int ExpeditionCfgIndex = 0;

        public bool AutoLogoutOpen = false;
        public bool AutoPirateOpen = false;
        public bool AutoPirateOpen1 = false;
        public bool AutoExpeditionOpen = false;
        public bool AutoExpeditionOpen1 = false;
        public bool AutoImperiumOpen = false;
        public bool AutoTransferOpen = false;

        public string PirateAutoMsg = "";
        public string PirateAutoMsg1 = "";
        public string ExpeditionAutoMsg = "";
        public string ExpeditionAutoMsg1 = "";

        public bool MorningIdle = false;

        public static byte[] ToBytes(OgameData data)
        {
            return Encoding.UTF8.GetBytes(ToJson(data));
        }

        public static string ToJson(OgameData data)
        {
            string content = "";

            if (null == data) return content;

            try
            {
                content = JsonConvert.SerializeObject(data);
            }
            catch (Exception ex)
            {
                LogUtil.Error($"ToJson catch {ex.Message}");
            }

            return content;
        }

        public static OgameData ParseData(string content)
        {
            OgameData result = null;
            try
            {
                if (string.IsNullOrWhiteSpace(content)) return result;
                result = JsonConvert.DeserializeObject<OgameData>(content);
            }
            catch (Exception ex)
            {
                LogUtil.Error($"ParseData catch {ex.Message}");
            }

            return result;
        }
    }
}
