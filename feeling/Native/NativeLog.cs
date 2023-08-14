using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeling
{
    class NativeLog
    {
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
    }
}
