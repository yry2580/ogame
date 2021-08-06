using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeling
{
    class ImperiumUtil
    {
        static string mCfgFile = NativeConst.CfgDirectory + "Imperium.cfg";
        public static Imperium MyImperium;

        public static void Save(Imperium imperium)
        {
            try
            {
                MyImperium = imperium;
                string text = JsonConvert.SerializeObject(imperium, Formatting.Indented);
                File.WriteAllText(mCfgFile, text);
            }
            catch (Exception ex)
            {
                NativeLog.Error($"ImperiumUtil save catch {ex.Message}");
            }
        }

        public static bool ReadCfg()
        {
            if (!File.Exists(mCfgFile)) return false;

            try
            {
                var text = File.ReadAllText(mCfgFile);
                MyImperium = JsonConvert.DeserializeObject<Imperium>(text);
                return true;
            }
            catch (Exception ex)
            {
                NativeLog.Error($"ImperiumUtil readCfg catch {ex.Message}");
                return false;
            }
        }
    }
}
