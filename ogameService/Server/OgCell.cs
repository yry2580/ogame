using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cowboy.Sockets;

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

        public OgCell(TcpSocketSession session)
        {
            MySession = session;
            SessionKey = session.RemoteEndPoint.ToString();
            MyLastData.SessionKey = SessionKey;
            LogUtil.Info($"OgCell {SessionKey}");
        }

        public void SetID(string id)
        {
            Id = id;
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
    }
}
