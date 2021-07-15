using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace OgameService
{
    public class LogUtil
    {
        protected static readonly ILog MyLogger = LogManager.GetLogger("ogservice");

        public static void Info(string msg)
        {
            MyLogger?.Info(msg);
        }

        public static void Debug(string msg)
        {
            MyLogger?.Debug(msg);
        }

        public static void Error(string msg)
        {
            MyLogger?.Error(msg);
        }

        public static void Warn(string msg)
        {
            MyLogger?.Warn(msg);
        }
    }
}
