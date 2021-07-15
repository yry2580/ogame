using Cowboy.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace OgameService
{
    public delegate void OgEventHandler();
    public delegate void OgEventHandler<T>(T data);

    public class OgClient
    {
        string mClientFile = AppDomain.CurrentDomain.BaseDirectory + "OgClient.cfg";

        public event OgEventHandler Connected;
        public event OgEventHandler<OgameData> DataReceived;

        TcpSocketClient mClient;
        Timer mTimer;
        string MyId = "";

        OgClientCfg MyCfg;

        public OgClient()
        {
            LogUtil.Warn("OgClient");
            InitConfig();
            ConnectServer();
            InitTimer();
        }

        protected void InitConfig()
        {
            if (!File.Exists(mClientFile))
            {
                SaveConfig(true);
                return;
            }

            try
            {
                var text = File.ReadAllText(mClientFile);
                MyCfg = JsonConvert.DeserializeObject<OgClientCfg>(text);
                MyId = MyCfg.Id;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"InitConfig catch {ex.Message}");
                SaveConfig(true);
            }
        }

        public void SaveConfig(bool isNew = false)
        {
            LogUtil.Info($"SaveConfig {isNew}");

            try
            {
                if (isNew)
                {
                    MyCfg = new OgClientCfg();
                    MyId = Guid.NewGuid().ToString();
                    MyCfg.Id = MyId;
                }
                string text = JsonConvert.SerializeObject(MyCfg, Formatting.Indented);
                File.WriteAllText(mClientFile, text);
            }
            catch (Exception ex)
            {
                LogUtil.Error($"SaveConfig catch {ex.Message}");
            }
        }

        protected void InitTimer()
        {
            if (null == mTimer)
            {
                mTimer = new Timer(1000 * 60);
                mTimer.AutoReset = true;
                mTimer.Elapsed += new ElapsedEventHandler(LoopTimerCheck);
                mTimer.Enabled = true;
            }
            else
            {
                mTimer.Enabled = true;
            }
        }

        void LoopTimerCheck(object sender, ElapsedEventArgs e)
        {
            LogUtil.Info($"LoopTimerUpdate");

            if (null != mClient)
            {
                if (TcpSocketConnectionState.Connected == mClient.State)
                {
                    return;
                }

                if (TcpSocketConnectionState.Connecting == mClient.State)
                {
                    return;
                }
            }

            ConnectServer();
        }

        public void ConnectServer()
        {
            try
            {
                if (null != mClient && TcpSocketConnectionState.Connected == mClient.State)
                {
                    return;
                }

                var config = new TcpSocketClientConfiguration();

                if (null != mClient)
                {
                    mClient.Shutdown();
                    mClient = null;
                }
#if DEBUG
                // IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 17201);
                IPEndPoint ip = new IPEndPoint(IPAddress.Parse("112.74.170.178"), 17201);
#else
                IPEndPoint ip = new IPEndPoint(IPAddress.Parse("112.74.170.178"), 17201);
#endif
                mClient = new TcpSocketClient(ip, config);
                mClient.ServerConnected += OnServerConnected;
                mClient.ServerDisconnected += OnServerDisconnected;
                mClient.ServerDataReceived += OnServerDataReceived;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"ConnectServer catch {ex.Message}");
            }

            TryConnect();
        }

        protected void TryConnect()
        {
            if (null == mClient) return;
            if (mClient.State == TcpSocketConnectionState.Connected) return;

            Task.Run(() =>
            {
                try
                {
                    if (null == mClient)
                    {
                        return;
                    }
                    mClient.Connect();
                }
                catch (Exception ex)
                {
                    LogUtil.Error($"TryConnet catch {ex.Message}");
                    mClient.Close();
                }
            });
        }

        protected void OnServerConnected(object sender, TcpServerConnectedEventArgs e)
        {
            LogUtil.Warn("OnServerConnected");
            Connected.Invoke();
        }

        protected void OnServerDisconnected(object sender, TcpServerDisconnectedEventArgs e)
        {
            LogUtil.Warn("OnServerDisconnected");
        }

        protected void OnServerDataReceived(object sender, TcpServerDataReceivedEventArgs e)
        {
            try
            {
                var text = Encoding.UTF8.GetString(e.Data, e.DataOffset, e.DataLength);
                var data = OgameData.ParseData(text);
                if (data == null) return;

                LogUtil.Info($"Received {data.Cmd}");
                DataReceived.Invoke(data);
            }
            catch (Exception)
            {

            }
        }

        protected byte[] MakeGameData(CmdEnum cmd, StatusEnum status, string content = "", string pirateAutoMsg = "")
        {
            var data = new OgameData
            {
                Cmd = cmd,
                Id = MyId,
                Content = content,
                Status = status,
                PirateAutoMsg = pirateAutoMsg,
            };

            return OgameData.ToBytes(data);
        }

        protected void Post(CmdEnum cmd, StatusEnum status, string content = "", string pirateAutoMsg = "")
        {
            try
            {
                mClient?.Send(MakeGameData(cmd, status, content, pirateAutoMsg));
            }
            catch (Exception ex)
            {
                LogUtil.Error($"Post catch {ex.Message}");
            }
        }

        #region API
        public void SendAuth(StatusEnum status, string content = "", string pirateAutoMsg = "")
        {
            Post(CmdEnum.Auth, status, content, pirateAutoMsg);
        }

        public void SendData(StatusEnum status, string content = "", string pirateAutoMsg = "")
        {
            if (MyId.Length <= 0) return;
            
            Post(CmdEnum.Hello, status, content, pirateAutoMsg);
        }

        #endregion
    }
}
