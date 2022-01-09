using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace feeling
{
    class NativeConst
    {
        public static string Homepage = "http://www.cicihappy.com/";

        public static string CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static string FileDirectory = CurrentDirectory + "Out/";
        public static string CfgDirectory = CurrentDirectory + "UserCfg/";
        public static int MaxUniverseCount = 24;
        public static int RankPageSize = 30;
        public static Regex PlanetRegex = new Regex(@"(\[\d{1}:\d{1,3}:\d{1,2}\]).{1}([0-9A-Za-z\u4e00-\u9fa5-:\[\]]+)");
        public static Regex PlanetRegex1 = new Regex(@"([0-9A-Za-z\u4e00-\u9fa5]+).{1}(\[\d{1}:\d{1,3}:\d{1,2}\])");
    }

    public enum OperStatus
    {
        None = 0,
        System = 1,
        Galaxy,
        Expedition,
        Pirate,
    }
}
