using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace feeling
{
    enum NetStatus
    {
        None = 0, // 未知
        NoConnected = 1, // 网络未连接
        ModemConnected = 2, // 采用调治解调器上网
        LanConnected = 3, // 采用网卡上网
        ModemNoConnected = 4, // 采用调治解调器上网,但是联不通指定网络
        LanNoConnected = 5,  // 采用网卡上网,但是联不通指定网络
    }

    class Network
    {
        private const int INTERNET_CONNECTION_MODEM = 1;
        private const int INTERNET_CONNECTION_LAN = 2;

        [DllImport("winInet.dll")]
        private static extern bool InternetGetConnectedState(ref int dwFlag, int dwReserved);

        /// <summary>
        /// 判断网络的连接状态
        /// </summary>
        /// <returns>
        /// 网络状态(1-->未联网;2-->采用调治解调器上网;3-->采用网卡上网)
        ///</returns>
        public static NetStatus GetNetStatus(string strNetAddress)
        {
            NetStatus iNetStatus = NetStatus.None;
            int dwFlag = 0;
            if (!InternetGetConnectedState(ref dwFlag, 0))
            {
                //没有能连上互联网
                iNetStatus = NetStatus.NoConnected;
            }
            else if ((dwFlag & INTERNET_CONNECTION_MODEM) != 0)
            {
                //采用调治解调器上网,需要进一步判断能否登录具体网站
                if (PingNetAddress(strNetAddress))
                {
                    //可以ping通给定的网址,网络OK
                    iNetStatus = NetStatus.ModemConnected;
                }
                else
                {
                    //不可以ping通给定的网址,网络不OK
                    iNetStatus = NetStatus.ModemNoConnected;
                }
            }
            else if ((dwFlag & INTERNET_CONNECTION_LAN) != 0)
            {
                //采用网卡上网,需要进一步判断能否登录具体网站
                if (PingNetAddress(strNetAddress))
                {
                    //可以ping通给定的网址,网络OK
                    iNetStatus = NetStatus.LanConnected;
                }
                else
                {
                    //不可以ping通给定的网址,网络不OK
                    iNetStatus = NetStatus.LanNoConnected;
                }
            }

            return iNetStatus;
        }

        /// <summary>
        /// ping 具体的网址看能否ping通
        /// </summary>
        /// <param name="strNetAdd"></param>
        /// <returns></returns>
        private static bool PingNetAddress(string strNetAdd)
        {
            bool flag;
            Ping ping = new Ping();
            try
            {
                PingReply pr = ping.Send(strNetAdd, 3000);
                /*              
                                if (pr.Status == IPStatus.TimedOut)
                                {
                                    flag = false;
                                }
                */

                flag = pr.Status == IPStatus.Success;
            }
            catch
            {
                flag = false;
            }
            return flag;
        }

        #region 业务
        static DateTime mLastTime = DateTime.Now;
        static bool mIsConnected = true;
        

        public static bool IsConnected
        {
            get
            {
                CheckNetwork();
                return mIsConnected;
            }
        }

        public static void CheckNetwork()
        {
            try
            {
                if (!mIsConnected)
                {
                    var sp = DateTime.Now - mLastTime;
                    if (sp.TotalMinutes < 3) return;
                }

                var ret = GetNetStatus("www.cicihappy.com");
                if (NetStatus.LanConnected == ret || NetStatus.ModemConnected == ret)
                {
                    mIsConnected = true;
                    return;
                }

                ret = GetNetStatus("baidu.com");
                if (NetStatus.LanConnected == ret || NetStatus.ModemConnected == ret)
                {
                    mIsConnected = true;
                    return;
                }

                mIsConnected = false;
                mLastTime = DateTime.Now;
            }
            catch(Exception ex)
            {
                NativeLog.Error($"CheckNetwork catch {ex.Message}");
                mIsConnected = false;
                mLastTime = DateTime.Now;
            }
        }

        #endregion
    }
}
