using Cowboy.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OgameService
{
    class OgCell
    {
        private DateTime LastHello = DateTime.Now;

        public DateTime LastOperTime = DateTime.Now;
        public string SessionKey = "";
        public string Id = "";

        public TcpSocketSession MySession { get; private set; }
        public OgameData MyLastData = new OgameData();

        public OgCell(string id)
        {
            Id = id;
        }

        public void SetID(string id)
        {
            Id = id;
        }

        public void SetSession(TcpSocketSession session)
        {
            LogUtil.Info($"Cell SetSession {Id}");
            MySession = session;
            SessionKey = session.SessionKey;
            MyLastData.SessionKey = SessionKey;
        }

        public void SetData(OgameData data)
        {
            if (null == data) return;

            LogUtil.Info($"Cell SetData {Id}|{data.Cmd}|{data.Status}|{data.Content}");

            MyLastData = data;
            MyLastData.SessionKey = SessionKey;
        }

        public void SetHello(OgameData data)
        {
            LastHello = DateTime.Now;
        }

        public bool IsOvertime()
        {
            return (DateTime.Now - LastHello).TotalSeconds > (60 * 5);
        }

        internal void Close()
        {
            try
            {
                LastHello = DateTime.Now;
                MySession?.Close();
            }
            catch(Exception ex)
            {
                LogUtil.Error($"OgCell({SessionKey}) Close catch {ex.Message}");
            }
        }
    }
}
