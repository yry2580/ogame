using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if !NET45
    using OgameService;
#endif

namespace feeling
{
    class NativeLog
    {
#if !NET45
        public static void Info(string msg)
        {
            LogUtil.Info(msg);
        }

        public static void Debug(string msg)
        {
            LogUtil.Debug(msg);
        }

        public static void Error(string msg)
        {
            LogUtil.Error(msg);
        }
        public static void Warn(string msg)
        {
            LogUtil.Warn(msg);
        }
#else
        public static void Info(string msg)
        {
            Console.WriteLine(msg);
        }

        public static void Debug(string msg)
        {
            Console.WriteLine(msg);
        }

        public static void Error(string msg)
        {
            Console.WriteLine(msg);
        }

        public static void Warn(string msg)
        {
            Console.WriteLine(msg);
        }
#endif
    }
}
