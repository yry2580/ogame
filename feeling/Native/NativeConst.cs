﻿using System;
using System.Collections.Generic;
using System.Text;

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
