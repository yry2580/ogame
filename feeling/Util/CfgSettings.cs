using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace feeling
{
    class CfgSettings
    {
        public static string SettingFile = ProcessDirectory + "appsettings.json";

        static Settings mSettings;
        static CfgSettings()
        {
            mSettings = GetSettings();
        }

        public static Settings GetSettings()
        {
            if (!File.Exists(SettingFile)) return null;

            try
            {
                var text = File.ReadAllText(SettingFile);
                mSettings = JsonConvert.DeserializeObject<Settings>(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetSettings catch {ex.Message}");
            }

            return mSettings;
        }

        public static string ProcessDirectory
        {
            get
            {
                // return AppContext.BaseDirectory;
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public static int Major => mSettings.Major;
        public static int Minor => mSettings.Minor;
        public static int Patch => mSettings.Patch;
    }

    class Settings
    {
        public int Major = 0;
        public int Minor = 0;
        public int Patch = 0;
    }
}
