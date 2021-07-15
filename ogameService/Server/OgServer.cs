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
        List<TcpSocketSession> mSessions = new List<TcpSocketSession>();
        List<OgCell> mCellList = new List<OgCell>();

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
                mCellList.RemoveAll(i => i.SessionKey == sessionKey);
                mSessions.Remove(e.Session);

                LogUtil.Info($"移除 {sessionKey}");
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
                LogUtil.Info($"GetData {d.SessionKey}|{d.Id}|{d.Status}|{d.Content}|{d.PirateAutoMsg}");
            });

            return result;
        }

        #endregion
    }
}
