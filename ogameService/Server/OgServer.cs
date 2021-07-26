using Cowboy.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OgameService
{
    public class OgServer
    {
        TcpSocketServer mServer;
        volatile List<TcpSocketSession> mSessions = new List<TcpSocketSession>();
        volatile List<OgCell> mCellList = new List<OgCell>();

        public OgServer()
        {
            LogUtil.Info("OgServer");
            Start();
        }

        protected void Start()
        {
            var config = new TcpSocketServerConfiguration();
            mServer = new TcpSocketServer(17201, config);
            mServer.ClientConnected += OnClientConnected;
            mServer.ClientDisconnected += OnClientDisconnected;
            mServer.ClientDataReceived += OnClientDataReceived;

            try
            {
                LogUtil.Info("Listen");
                mServer.Listen();
                DoCheckHello();
            }
            catch (Exception ex)
            {
                LogUtil.Error($"Listen catch {ex.Message}");
            }
        }

        protected void OnClientConnected(object sender, TcpClientConnectedEventArgs e)
        {
            var sessionKey = e.Session.SessionKey;
            LogUtil.Info($"Connect {sessionKey}|{e.Session.RemoteEndPoint}");
            var session = mSessions.Find(item => item.SessionKey == sessionKey);
            if (session != null)
            {
                LogUtil.Warn($"连接已存在");
                return;
            }

            mSessions.Add(e.Session);
            LogUtil.Info($"添加 {sessionKey}");
        }

        protected void OnClientDisconnected(object sender, TcpClientDisconnectedEventArgs e)
        {
            try
            {
                var sessionKey = e.Session.SessionKey;
                LogUtil.Info($"Disconnected {sessionKey}");
                
                RemoveCell(sessionKey);
            }
            catch (Exception ex)
            {
                LogUtil.Error($"Disconnected catch {ex.Message}");
            }
        }

        protected void OnClientDataReceived(object sender, TcpClientDataReceivedEventArgs e)
        {
            try
            {
                var text = Encoding.UTF8.GetString(e.Data, e.DataOffset, e.DataLength);
                var data = OgameData.ParseData(text);
                if (data == null) return;

                var sessionKey = e.Session.SessionKey;

                LogUtil.Info($"Received {sessionKey}|{data.Cmd}|{data.Status}|{data.Content}|{data.PirateAutoMsg}");

                switch (data.Cmd)
                {
                    case CmdEnum.Auth:
                        DoAuth(sessionKey, e.Session, data);
                        break;
                    case CmdEnum.Hello:
                        DoHello(sessionKey, data);
                        break;
                    default:
                        DoRecv(sessionKey, data);
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        protected void DoAuth(string sessionKey, TcpSocketSession session, OgameData data)
        {
            LogUtil.Info($"DoAuth {sessionKey}|{data.Id}");
            if (string.IsNullOrWhiteSpace(sessionKey)) return;

            var valid = Guid.TryParse(data.Id, out _);
            if (!valid)
            {
                LogUtil.Info($"DoAuth failed! Id invalid");
                return;
            }

            var cell = mCellList.Find(c => c.SessionKey == sessionKey);

            if (null != cell)
            {
                cell.SetID(data.Id);
            } else
            {
                cell = new OgCell(data.Id);
                cell.SetSession(session);
                mCellList.Add(cell);
            }

            LogUtil.Info($"DoAuth SetData");
            cell.SetData(data);
        }

        protected void DoRecv(string sessionKey, OgameData data)
        {
            LogUtil.Info($"DoRecv {sessionKey}");
            
            var cell = mCellList.Find(c => c.SessionKey == sessionKey && c.Id == data.Id);

            if (null == cell)
            {
                LogUtil.Error($"DoRecv 没有取到cell");
                return;
            }

            LogUtil.Info($"DoRecv SetData");
            cell.SetData(data);
        }

        protected void DoHello(string sessionKey, OgameData data)
        {
            LogUtil.Info($"DoHello {sessionKey}");

            var cell = mCellList.Find(c => c.SessionKey == sessionKey && c.Id == data.Id);

            if (null == cell)
            {
                LogUtil.Error($"DoRecv 没有取到cell");
                return;
            }

            LogUtil.Info($"DoHello SetHello");
            cell.SetHello(data);
        }

        protected void DoCheckHello()
        {
            Task.Run(async() =>
            {
                await Task.Delay(1000 * 60);

                List<OgCell> list = new List<OgCell>();
                mCellList.ForEach(c =>
                {
                    if (c.IsOvertime()) {
                        LogUtil.Info($"超时 {c.SessionKey}");
                        list.Add(c);
                    }
                });

                if (list.Count > 0)
                {
                    list.ForEach(c => c.Close());
                }
            });
        }

        protected void RemoveCell(string sessionKey)
        {
            try
            {
                LogUtil.Info($"RemoveCell 111 {sessionKey}");
                LogUtil.Info($"RemoveCell mCellList 111{mCellList.Count}");
                var cell = mCellList.Find(i => i.SessionKey == sessionKey);
                if (null != cell)
                {
                    LogUtil.Info($"RemoveCell mCellList 222 {cell.Id}");
                    mCellList.Remove(cell);
                    cell = null;
                }

                LogUtil.Info($"RemoveCell mCellList 333 {mCellList.Count}");
                var session = mSessions.Find(e => e.SessionKey == sessionKey);
                if (null != session)
                {
                    mSessions.Remove(session);
                    session?.Close();
                    session = null;
                }

                LogUtil.Info($"RemoveCell 444 {sessionKey}");
            }
            catch (Exception ex)
            {
                LogUtil.Error($"RemoveCell catch {sessionKey} - {ex.Message}");
            }
        }

        #region API
        public List<OgameData> GetData(string id)
        {
            
            LogUtil.Info($"GetData {id}");
            if (string.IsNullOrWhiteSpace(id)) return null;

            var result = mCellList.FindAll(c => c.Id == id).Select(c => c.MyLastData).ToList();
            if (null == result)
            {
                LogUtil.Warn($"GetData 没取到对应cell");
                return null;
            }

            result.ForEach(d =>
            {
                LogUtil.Info($"GetData {d.Id}|{d.SessionKey}|{d.Status}|{d.Content}|{d.PirateAutoMsg}");
            });

            return result;
        }

        public bool OperLogin(string id, string key)
        {
            LogUtil.Info($"OperLogin {id}");
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

            var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
            if (null == result)
            {
                LogUtil.Warn($"OperLogin 没取到对应cell");
                return false;
            }

            OgameData data = new OgameData();
            data.Cmd = CmdEnum.Login;

            mServer.SendTo(result.SessionKey, OgameData.ToBytes(data));
            return true;
        }

        public bool OperLogout(string id, string key)
        {
            LogUtil.Info($"OperLogout {id}");
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

            var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
            if (null == result)
            {
                LogUtil.Warn($"OperLogout 没取到对应cell");
                return false;
            }

            OgameData data = new OgameData();
            data.Cmd = CmdEnum.Logout;

            mServer.SendTo(result.SessionKey, OgameData.ToBytes(data));
            return true;
        }

        public bool OperPirate(string id, string key)
        {
            LogUtil.Info($"OperPirate {id}");
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

            var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
            if (null == result)
            {
                LogUtil.Warn($"OperPirate 没取到对应cell");
                return false;
            }

            OgameData data = new OgameData();
            data.Cmd = CmdEnum.Pirate;

            mServer.SendTo(result.SessionKey, OgameData.ToBytes(data));
            return true;
        }

        public bool OperExpedition(string id, string key)
        {
            LogUtil.Info($"OperExpedition {id}");
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

            var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
            if (null == result)
            {
                LogUtil.Warn($"OperExpedition 没取到对应cell");
                return false;
            }

            OgameData data = new OgameData();
            data.Cmd = CmdEnum.Expedition;

            mServer.SendTo(result.SessionKey, OgameData.ToBytes(data));
            return true;
        }

        public bool OperGetCode(string id, string key)
        {
            LogUtil.Info($"OperGetCode {id}");
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

            var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
            if (null == result)
            {
                LogUtil.Warn($"OperGetCode 没取到对应cell");
                return false;
            }

            OgameData data = new OgameData();
            data.Cmd = CmdEnum.GetCode;

            mServer.SendTo(result.SessionKey, OgameData.ToBytes(data));
            return true;
        }

        public bool OperAuthCode(string id, string key, string code)
        {
            LogUtil.Info($"OperGetCode {id}-{code}");
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

            var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
            if (null == result)
            {
                LogUtil.Warn($"OperGetCode 没取到对应cell");
                return false;
            }

            OgameData data = new OgameData();
            data.Cmd = CmdEnum.AuthCode;
            data.Content = code;

            mServer.SendTo(result.SessionKey, OgameData.ToBytes(data));
            return true;
        }

        #endregion
    }
}
