using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cowboy.Sockets;

namespace OgameService
{
    public class OgServer
    {
        TcpSocketServer mServer;
        volatile List<string> mSessions = new List<string>();
        TaskFactory mTaskFactory;

        IDictionary<string, OgCell> mCellDict = new ConcurrentDictionary<string, OgCell>();

        public OgServer()
        {
            LogUtil.Info("OgServer");
            Start();
        }

        protected void Start()
        {
            var config = new TcpSocketServerConfiguration();
            config.KeepAlive = true;
            // "112.74.170.178"-"172.20.170.52"
#if DEBUG
            mServer = new TcpSocketServer(17201, config);
#else
            mServer = new TcpSocketServer(17201, config);
#endif
            mServer.ClientConnected += OnClientConnected;
            mServer.ClientDisconnected += OnClientDisconnected;
            mServer.ClientDataReceived += OnClientDataReceived;
            try
            {
                var scheduler = new LimitedConcurrencyLevelTaskScheduler(1);
                mTaskFactory = new TaskFactory(scheduler);

                LogUtil.Info("Listen: Connect、Disconnected、Auth、Received");
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
            mTaskFactory.StartNew(() =>
            {
                DoConnected(e.Session);
            });
        }

        protected void DoConnected(TcpSocketSession session)
        {
            try
            {
                string sessionKey = session.RemoteEndPoint.ToString();
                LogUtil.Warn($"Connect {sessionKey}");
                mSessions.Add(sessionKey);
                LogUtil.Info($"添加 {sessionKey}");
                mCellDict[sessionKey] = new OgCell(session);
            }
            catch (Exception ex)
            {
                LogUtil.Error($"Connect catch {ex.Message}");
            }
        }

        protected void OnClientDisconnected(object sender, TcpClientDisconnectedEventArgs e)
        {
            mTaskFactory.StartNew(() =>
            {
                DoDisconnected(e.Session);
            });
        }

        protected void DoDisconnected(TcpSocketSession session)
        {
            try
            {
                var sessionKey = session.RemoteEndPoint.ToString();
                LogUtil.Error($"Disconnected {sessionKey}");
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
                var sessionKey = e.Session.RemoteEndPoint.ToString();
                var text = Encoding.UTF8.GetString(e.Data, e.DataOffset, e.DataLength);
                LogUtil.Warn($"Received {sessionKey}");
                var data = OgameData.ParseData(text);
                if (data == null) return;

                LogUtil.Info($"Received {data.Id}|{data.Cmd}|{data.Status}|{data.Content}|{data.FleetContent}");

                switch (data.Cmd)
                {
                    case CmdEnum.Auth:
                        // DoAuth(sessionKey, data);
                        mTaskFactory.StartNew(() =>
                        {
                            DoAuth(sessionKey, data);
                        });
                        break;
                    default:
                        DoRecv(sessionKey, data);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OnClientDataReceived catch {ex.Message}");
            }

            LogUtil.Warn($"Received end");
        }

        protected void DoAuth(string sessionKey, OgameData data)
        {
            LogUtil.Info($"DoAuth {sessionKey}|{data.Id}");
            if (string.IsNullOrWhiteSpace(sessionKey)) return;

            var valid = Guid.TryParse(data.Id, out _);
            if (!valid)
            {
                LogUtil.Info($"DoAuth failed! Id invalid");
                return;
            }

            mCellDict.TryGetValue(sessionKey, out OgCell cell);
            // var cell = mCellList.Find(c => c.SessionKey == sessionKey);

            if (null != cell)
            {
                cell.SetID(data.Id);
                cell.SetData(data);
            }

            LogUtil.Info($"DoAuth end");
        }

        protected void DoRecv(string sessionKey, OgameData data)
        {
            LogUtil.Info($"DoRecv {sessionKey}|{data.Id}");

            // var cell = mCellList.Find(c => c.SessionKey == sessionKey && c.Id == data.Id);
            mCellDict.TryGetValue(sessionKey, out OgCell cell);
            if (null == cell)
            {
                LogUtil.Error($"DoRecv 没有取到cell");
                return;
            }

            cell.SetData(data);
            LogUtil.Info($"DoRecv end");
        }

        protected void DoCheckHello()
        {
            Task.Run(async() =>
            {
                do
                {
                    try
                    {
                        await Task.Delay(1000 * 60);

                        var arr = mCellDict.Values.ToList();
                        arr.ForEach(c =>
                        {
                            if (null != c && !mSessions.Contains(c.SessionKey))
                            {
                                LogUtil.Error($"失效 {c.Id}");
                                RemoveCell(c.SessionKey);
                            }
                        });
                    }
                    catch(Exception ex)
                    {
                        LogUtil.Error($"DoCheckHello catch {ex.Message}");
                    }
                } while (true);
            });
        }

        protected void RemoveCell(string sessionKey)
        {
            try
            {
                LogUtil.Info($"RemoveCell 111 {sessionKey}");
                mCellDict.TryGetValue(sessionKey, out OgCell cell);
                if (null != cell)
                {
                    LogUtil.Info($"RemoveCell 222 {cell.Id}");
                    // cell.Close();
                    cell = null;
                }
                var ret = mCellDict.Remove(sessionKey);
                LogUtil.Info($"RemoveCell 333 TryRemove: {ret}");
                ret = mSessions.Remove(sessionKey);
                LogUtil.Info($"RemoveCell 444 Sessions Remove -{ret}");
            }
            catch (Exception ex)
            {
                LogUtil.Error($"RemoveCell catch {sessionKey} - {ex.Message}");
            }
        }

    #region API
        public List<OgameData> GetData(string id)
        {
            List<OgameData> result;
            try
            {
                LogUtil.Info($"GetData {id}");
                if (string.IsNullOrWhiteSpace(id)) return null;

                result = mCellDict.Values.Where(c => c.Id == id && mSessions.Contains(c.SessionKey)).Select(c => c.MyLastData).ToList();
                // var result = mCellList.FindAll(c => c.Id == id).Select(c => c.MyLastData).ToList();
                if (null == result)
                {
                    LogUtil.Warn($"GetData 没取到对应cell");
                    return null;
                }

                result.ForEach(d =>
                {
                    LogUtil.Info($"GetData => {d.Id}|{d.SessionKey}|{d.Status}|{d.Content}|{d.FleetContent}");
                });
            }
            catch(Exception ex)
            {
                LogUtil.Error($"GetData {id} catch {ex.Message}");
                result = null;
            }

            return result;
        }

        public List<OgameData> GetAllData()
        {
            LogUtil.Warn($"GetAllData");
            List<OgameData> result;
            try
            {
                result = mCellDict.Values.Select(c => c.MyLastData).ToList();
                // var result = mCellList.FindAll(c => c.Id == id).Select(c => c.MyLastData).ToList();
                if (null == result)
                {
                    LogUtil.Warn($"GetAllData 没取到对应cell");
                    return null;
                }

                result.ForEach(d =>
                {
                    LogUtil.Info($"GetAllData => {d.Id}|{d.SessionKey}|{d.Status}|{d.Content}|{d.FleetContent}");
                });
            }
            catch(Exception ex)
            {
                LogUtil.Error($"GetAllData catch {ex.Message}");
                result = null;
            }

            return result;
        }

        public bool OperLogin(string id, string key)
        {
            LogUtil.Info($"OperLogin {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperLogin 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.Login;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch(Exception ex)
            {
                LogUtil.Error($"OperLogin {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperLogout(string id, string key)
        {
            LogUtil.Info($"OperLogout {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperLogout 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.Logout;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch(Exception ex)
            {
                LogUtil.Error($"OperLogout {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperPirate(string id, string key)
        {
            LogUtil.Info($"OperPirate {id}");
            try
            {

                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperPirate 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.Pirate;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch(Exception ex)
            {
                LogUtil.Error($"OperPirate {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperExpedition(string id, string key)
        {
            LogUtil.Info($"OperExpedition {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperExpedition 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.Expedition;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperExpedition {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperGetCode(string id, string key)
        {
            LogUtil.Info($"OperGetCode {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperGetCode 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.GetCode;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperGetCode {key} catch {ex.Message}");
            }
            
            return false;
        }

        public bool OperAuthCode(string id, string key, string code)
        {
            LogUtil.Warn($"OperAuthCode {id}|{code}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperGetCode 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.AuthCode;
                data.Content = code;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperAuthCode {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperImperium(string id, string key)
        {
            LogUtil.Warn($"OperImperium {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperImperium 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.Imperium;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperImperium {key} catch {ex.Message}");
            }

            return false;
        }

        public bool OperRefreshNpc(string id, string key)
        {
            LogUtil.Warn($"OperRefreshNpc {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperRefreshNpc 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.Npc;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperRefreshNpc {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperScreenshot(string id, string key)
        {
            LogUtil.Warn($"OperScreenshot {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperScreenshot 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.Screenshot;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperScreenshot {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperFs(string id, string key)
        {
            LogUtil.Warn($"OperFs {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperFs 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.Fs;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperFs {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperAutoPirateOpen(string id, string key, bool open, int index)
        {
            LogUtil.Warn($"OperAutoPirateOpen {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperAutoPirateOpen 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();

                if (index == 1)
                {
                    data.Cmd = CmdEnum.AutoPirateOpen1;
                    data.AutoPirateOpen1 = open;
                } else
                {
                    data.Cmd = CmdEnum.AutoPirateOpen;
                    data.AutoPirateOpen = open;
                }
                
                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperAutoPirateOpen {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperPirateCfg(string id, string key, int index)
        {
            LogUtil.Warn($"OperPirateCfg {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperPirateCfg 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.PirateCfg;
                data.PirateCfgIndex = index;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperPirateCfg {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperAutoExpeditionOpen(string id, string key, bool open, int index)
        {
            LogUtil.Warn($"OperAutoExpeditionOpen {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperAutoExpeditionOpen 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                if (index == 1)
                {
                    data.Cmd = CmdEnum.AutoExpeditionOpen1;
                    data.AutoExpeditionOpen1 = open;
                }
                else
                {
                    data.Cmd = CmdEnum.AutoExpeditionOpen;
                    data.AutoExpeditionOpen = open;
                }

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperAutoExpeditionOpen {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperGoCross(string id, string key)
        {
            LogUtil.Warn($"OperGoCross {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperGoCross 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.GoCross;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperGoCross {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperBackUniverse(string id, string key)
        {
            LogUtil.Warn($"OperBackUniverse {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperBackUniverse 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.BackUniverse;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperBackUniverse {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperExpeditionCfg(string id, string key, int index)
        {
            LogUtil.Warn($"OperExpeditionCfg {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperExpeditionCfg 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.ExpeditionCfg;
                data.ExpeditionCfgIndex = index;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperExpeditionCfg {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperAutoLoginOpen(string id, string key, bool open)
        {
            LogUtil.Warn($"OperAutoLoginOpen {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperAutoLoginOpen 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.AutoLoginOpen;
                data.AutoLoginOpen = open;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperAutoLoginOpen {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperAutoImperiumOpen(string id, string key, bool open)
        {
            LogUtil.Warn($"OperAutoImperiumOpen {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperAutoImperiumOpen 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.AutoImperiumOpen;
                data.AutoImperiumOpen = open;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperAutoImperiumOpen {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperQuickAutoCheck(string id, string key)
        {
            LogUtil.Warn($"OperQuickAutoCheck {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperQuickAutoCheck 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.QuickAutoCheck;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperQuickAutoCheck {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperQuickAutoUncheck(string id, string key)
        {
            LogUtil.Warn($"OperQuickAutoUncheck {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperQuickAutoUncheck 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.QuickAutoUncheck;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperQuickAutoUncheck {key} catch {ex.Message}");
            }
            return false;
        }

        public bool OperQuickAutoStart(string id, string key)
        {
            LogUtil.Warn($"OperQuickAutoStart {id}");
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(key)) return false;

                mCellDict.TryGetValue(key, out OgCell result);
                // var result = mCellList.Find(c => c.Id == id && c.SessionKey == key);
                if (null == result || null == result.MySession)
                {
                    LogUtil.Warn($"OperQuickAutoStart 没取到对应cell");
                    return false;
                }

                OgameData data = new OgameData();
                data.Cmd = CmdEnum.QuickAutoStart;

                mServer.SendTo(result.MySession, OgameData.ToBytes(data));
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"OperQuickAutoStart {key} catch {ex.Message}");
            }
            return false;
        }

        public void ShowAllCell()
        {
            LogUtil.Warn("=============== ShowAllCell =====================");
            mCellDict.Values.ToList().ForEach(e =>
            {
                LogUtil.Info($"{e.Id}-{e.SessionKey}-{e.MyLastData.Status}-{e.MyLastData.Content}");
            });
            LogUtil.Warn("=============== ShowAllCell end =================");
        }

    #endregion
    }
}
