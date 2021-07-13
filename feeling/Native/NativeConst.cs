using System;
using System.Collections.Generic;
using System.Text;

namespace feeling
{
    class NativeConst
    {
        public static string Homepage = "http://www.cicihappy.com/";

        public static string CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static string FileDirectory = CurrentDirectory + "Out/";
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
