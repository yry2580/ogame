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
    }
}
