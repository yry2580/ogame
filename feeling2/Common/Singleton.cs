using System;
using System.Collections.Generic;
using System.Text;

namespace feeling
{
    public class Singleton<T> where T : class
    {
        protected static T _instance;
        private static readonly object _syncObject = new object();

        public static T GetInstance()
        {
            if (_instance == null)
            {
                lock (_syncObject)
                {
                    if (_instance == null)
                    {
                        _instance = (T)Activator.CreateInstance(typeof(T), true);
                    }
                }
            }
            return _instance;
        }

        public static T Instance { get { return GetInstance(); } }
    }
}
