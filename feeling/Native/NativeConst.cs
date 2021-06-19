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

    enum OperStatus
    {
        None = 0,
        Galaxy,
    }
}
