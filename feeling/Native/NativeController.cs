using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using Newtonsoft.Json;

namespace feeling
{
    public delegate void DelegateStatusChange();
    public delegate void DelegateScanGalaxy(string sDesc, string pDesc, string uDesc);
    public delegate void DelegatePlanet();
    public delegate void DelegateOperTips(OperStatus operStatus, string tips);
    public delegate void DelegateNpcChange();

    class NativeController: Singleton<NativeController>
    {
        public OperStatus MyOperStatus = OperStatus.None;
        public volatile bool IsWorking = false;
        public volatile bool IsPirateWorking = false;
        public volatile bool IsExpeditionWorking = false;

        public string MyAddress = "";
        public DelegateStatusChange StatusChangeEvent;
        public DelegateScanGalaxy ScanGalaxyEvent;
        public DelegatePlanet PlanetEvent;
        public DelegateOperTips OperTipsEvent;
        public DelegateNpcChange NpcChangeEvent;

        public ChromiumWebBrowser MyWebBrowser;

        Galaxy mGalaxy = new Galaxy();
        Planet mPlanet = new Planet();
        Expedition mExpedition = new Expedition();
        string mScanDesc = "";
        string mScanPage = "";
        string mScanUniverse = "";

        public Planet MyPlanet => mPlanet;
        public static User User = new User();

        OgameParser mHtmlParser = new OgameParser();

        // auto
        public bool IsAutoExpedition = false;
        public DateTime LastExeditionTime = DateTime.Now;

        public bool IsAutoPirate = false;
        public DateTime LastPirateTime = DateTime.Now;

        public bool CanNotify = true;

        public void HandleWebBrowserFrameEnd(string url)
        {
            MyAddress = MyWebBrowser.Address;

            if (url.Contains("ogame/frames.php"))
            {
                ScanPlanet();
            }

            if (url.Contains("ogame/galaxy.php"))
            {
                ScanNpc();
            }
        }

        public void ScanPlanet()
        {
            Task.Run(async() =>
            {
                try
                {
                    if (!mPlanet.HasData)
                    {
                        var source = await GetHauptframe()?.GetSourceAsync();

                        mPlanet.Parse(source);
                        PlanetEvent?.Invoke();
                    }
                }
                catch (Exception)
                { 
                }
            });
        }

        public void ScanNpc()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (!PirateUtil.HasNpcData)
                    {
                        var source = await GetHauptframe()?.GetSourceAsync();
                        PirateUtil.ParseNpc(source);
                        NpcChangeEvent?.Invoke();
                    }
                }
                catch (Exception)
                {
                }
            });
        }

        public void RunJs(string jsCode)
        {
            MyWebBrowser?.GetBrowser()?.MainFrame.ExecuteJavaScriptAsync(jsCode);
        }

        public void FrameRunJs(string jsCode)
        {
            GetHauptframe()?.ExecuteJavaScriptAsync(jsCode);
        }

        public IFrame GetHauptframe()
        {
            return MyWebBrowser.GetBrowser().GetFrame("Hauptframe");
        }

        #region galaxy

        public void StartScanGalaxy()
        {
            if (IsWorking) return;
            IsWorking = true;

            SwitchStatus(OperStatus.Galaxy);

            Reload();
            FrameRunJs(NativeScript.ToGalaxy());
            Thread.Sleep(1500);

            int universe = 0;
            var mat = Regex.Match(MyAddress, $@"u(?<universe>\S*).cicihappy.com");
            if (mat.Success)
            {
                universe = int.Parse(mat.Groups["universe"].Value);
            }

            if (universe == 0)
            {
                StopScanGalaxy();
                if (CanNotify)
                {
                    MessageBox.Show("请先登录");
                }
                return;
            }

            var universeName = $"u{universe}";
            mScanUniverse = universeName;

            if (mGalaxy.UniverseName != universeName)
            {
                mGalaxy.UniverseName = universeName;
                mGalaxy.Clear();
            }

            Task.Run(() =>
            {
                ScanGalaxy();
            });
        }

        protected async void ScanGalaxy()
        {
            int count = 0;
            bool lastError = false;

            mScanDesc = "开始";
            FireScanGalaxy();

            try
            {
                do
                {
                    if (!IsWorking) break;

                    if (!mGalaxy.TryMove())
                    {
                        mScanDesc = "刷图完成";
                        if (CanNotify)
                        {
                            MessageBox.Show("刷图完成");
                        }
                        break;
                    }

                    mScanDesc = $"刷图次数：{count}";
                    if (count > 1498)
                    {
                        if (CanNotify)
                        {
                            MessageBox.Show("刷图次数过多，建议换号");
                        }
                        break;
                    }

                    count++;
                    FrameRunJs(NativeScript.RefreshGalaxy(mGalaxy.NextX, mGalaxy.NextY));
                    mScanPage = $"{mGalaxy.NextX}:{mGalaxy.NextY}";
                    FireScanGalaxy();
                    await Task.Delay(1500);
                    var source = await GetHauptframe().GetSourceAsync();
                    if (!mGalaxy.AddPage(source))
                    {
                        if (lastError)
                        {
                            mScanDesc = $"刷图异常";
                            if (CanNotify)
                            {
                                MessageBox.Show("刷图异常，检测下再开始");
                            }
                            break;
                        }

                        lastError = true;
                        FrameRunJs(NativeScript.ToGalaxy());
                        await Task.Delay(1500);
                    }
                    else
                    {
                        lastError = false;
                    }

                } while (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Scan galaxy catch {ex.Message}");
            }

            _StopScanGalaxy();
            FireScanGalaxy();
        }

        public void StopScanGalaxy()
        {
            if (OperStatus.Galaxy != MyOperStatus) return;
            mScanDesc = $"停止刷图";
            _StopScanGalaxy();
        }

        protected void _StopScanGalaxy()
        {
            if (OperStatus.Galaxy != MyOperStatus) return;
            IsWorking = false;
            SwitchStatus(OperStatus.None);
            FireScanGalaxy();
        }

        public void SaveGalaxy()
        {
            if (mGalaxy.Count <= 0)
            {
                MessageBox.Show("暂无可扫描的星图可以保存");
                return;
            }

            mGalaxy.Save();
        }

        protected void FireScanGalaxy()
        {
            ScanGalaxyEvent?.Invoke(mScanDesc, mScanPage, mScanUniverse);
        }

        #endregion

        #region login

        public async Task LoginAsync(string account, string psw, int universe)
        {
            // Reload();

            var source = "";
            if (account.Length <= 0 || psw.Length <= 0 || universe <= 0 || universe > 24)
            {
                if (CanNotify)
                {
                    MessageBox.Show("请输入正确的账号、密码或宇宙");
                }
                OperTipsEvent.Invoke(OperStatus.System, $"{DateTime.Now:G}|请输入正确账号、密码或宇宙");
                return;
            }

            OperTipsEvent.Invoke(OperStatus.System, $"{DateTime.Now:G}开始登录");

            MyWebBrowser.Load(NativeConst.Homepage);
            Thread.Sleep(2000);
            source = await MyWebBrowser.GetSourceAsync();

            if (HtmlUtil.IsGameUrl(MyAddress))
            {
                try
                {
                    source = await GetHauptframe().GetSourceAsync();
                    if (HtmlUtil.IsInGame(source))
                    {
                        if (CanNotify)
                        {
                            MessageBox.Show("已经是登录状态");
                        }
                        OperTipsEvent.Invoke(OperStatus.System, $"{DateTime.Now:G}|已经是登录状态");
                        return;
                    }
                }
                catch(Exception)
                {
                }

                await DoLoginAsync(account, psw, universe);
            } else
            {
                await DoLoginAsync(account, psw, universe);
            }

            OperTipsEvent.Invoke(OperStatus.System, $"{DateTime.Now:G}登录输入完成");
            try
            {

                await Task.Delay(1000);
                await GoHome(1000);

                if (HtmlUtil.IsGameUrl(MyAddress))
                {
                    source = await GetHauptframe()?.GetSourceAsync();
                    if (HtmlUtil.HasTutorial(source))
                    {
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                    }
                }
            }
            catch(Exception)
            {
            }

            OperTipsEvent.Invoke(OperStatus.System, $"{DateTime.Now:G}登录操作结束");
        }

        protected async Task DoLoginAsync(string account, string psw, int universe)
        {
            if (!HtmlUtil.IsHomeUrl(MyAddress))
            {
                MyWebBrowser.Load(NativeConst.Homepage);
                await Task.Delay(2000);
            }

            await MyWebBrowser.GetSourceAsync();
            await Task.Delay(100);

            var jsAccount = $"document.getElementsByName('username')[0].value ='{account}'";
            var jsPassword = $"document.getElementsByName('password')[0].value = '{psw}'";
            var jsUniverse = $"document.getElementsByName('universe')[0][{universe}].selected=true";
            var jsSubmit = "document.getElementsByClassName('loginanniu')[0].click()";

            // input account
            RunJs(jsAccount);

            // input password
            RunJs(jsPassword);

            RunJs(jsUniverse);

            Thread.Sleep(100);

            // submit
            RunJs(jsSubmit);

            User.SetUserData(account, psw, universe);

            mPlanet.Reset();
        }

        public async Task LogoutAsync()
        {
            OperTipsEvent.Invoke(OperStatus.System, $"{DateTime.Now:G}退出登录");

            if (!HtmlUtil.IsGameUrl(MyAddress))
            {
                if (CanNotify)
                {
                    MessageBox.Show("退出失败，可能不在游戏页");
                }
                OperTipsEvent.Invoke(OperStatus.System, $"{DateTime.Now:G}退出失败，可能不在游戏页");
                return;
            }

            if (OperStatus.System != MyOperStatus)
            {
                if (CanNotify)
                {
                    MessageBox.Show("当前正在忙，不建议退出");
                }
                OperTipsEvent.Invoke(OperStatus.System, $"{DateTime.Now:G}当前正在忙，不建议退出");
                return;
            }

            var source = await GetHauptframe().GetSourceAsync();
            if (!HtmlUtil.HasLogoutBtn(source))
            {
                await GoHome();
                await Task.Delay(1500);
            }

            if (!HtmlUtil.IsInGame(source))
            {
                if (CanNotify)
                {
                    MessageBox.Show("退出失败，无退出按钮");
                }
                OperTipsEvent.Invoke(OperStatus.System, $"{DateTime.Now:G}退出失败，无退出按钮");
                return;
            }

            FrameRunJs(NativeScript.ToLogout());
            await Task.Delay(500);

            OperTipsEvent.Invoke(OperStatus.System, $"{DateTime.Now:G}退出操作结束");
        }
        #endregion login

        #region Expedition
        void GoFleetPage(int delay = 1500)
        {
            FrameRunJs(NativeScript.ToFleet());
            Thread.Sleep(delay);
        }

        internal void StartExpedition(ExMission exMission)
        {
            SwitchStatus(OperStatus.Expedition);
            IsExpeditionWorking = true;
            OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:G}|开始探险");
            Task.Run(() =>
            {
                DoExpedition(exMission);
            });
        }

        /// <summary>
        /// 刷新NPC
        /// </summary>
        internal void RefreshPlanet()
        {
            SwitchStatus(OperStatus.Expedition);
            Task.Run(async () =>
            {
                try
                {
                    OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:G}|刷NPC");
                    mPlanet.Reset();
                    Reload();
                    await GoHome(1500);

                    if (HtmlUtil.IsGameUrl(MyAddress))
                    {
                        var source = await GetHauptframe().GetSourceAsync();
                        await Task.Delay(500);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"RefreshPlanet catch {ex.Message}");
                }

                OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:G}|刷NPC结束");
                SwitchStatus(OperStatus.None);
            });
        }

        internal void StopExpedition()
        {
            if (!IsExpeditionWorking) return;

            IsExpeditionWorking = false;
        }

        protected async void DoExpedition(ExMission exMission)
        {
            int index = 0;
            int _count = 0;
            bool lastErr = false;
            string source = "";
            bool success = false;

            var _nextFunc = new Action<bool>(isSkip =>
            {
                if (lastErr || isSkip)
                {
                    lastErr = false;
                    index++;
                }
                else
                {
                    lastErr = true;
                }
            });

            OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:G}|开始处理探险");

            if (exMission.List.Count <= 0)
            {
                OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:MM:dd-hh:mm}|没有探险任务");
                
                LastExeditionTime = DateTime.Now;
                IsExpeditionWorking = false;
                SwitchStatus(OperStatus.None);
                return;
            }

            Reload();
            if (HtmlUtil.IsGameUrl(MyAddress))
            {
                source = await GetHauptframe().GetSourceAsync();
                if (HtmlUtil.HasTutorial(source))
                {
                    FrameRunJs(NativeScript.TutorialConfirm());
                    await Task.Delay(1500);
                    source = await GetHauptframe().GetSourceAsync();
                    if (!HtmlUtil.IsInGame(source))
                    {
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:MM:dd-hh:mm}|探险结束，没有登录");
                        LastExeditionTime = DateTime.Now;
                        IsExpeditionWorking = false;
                        SwitchStatus(OperStatus.None);
                        return;
                    }
                }
            }
            else
            {
                OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:MM:dd-hh:mm}|探险结束，没有登录");
                LastExeditionTime = DateTime.Now;
                IsExpeditionWorking = false;
                SwitchStatus(OperStatus.None);
                return;
            }

            await GoHome(1500);

            try
            {
                do
                {
                    Console.WriteLine($"ex-mession index{index}");
                    OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:MM:dd-hh:mm}|探险{index+1}");

                    if (index >= exMission.List.Count)
                    {
                        if (!IsAutoExpedition && CanNotify)
                        {
                            MessageBox.Show($"探险派出结束，请检测是否成功{_count}/{exMission.List.Count}");
                        }
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:MM:dd-hh:mm}|探险派出结束{_count}/{exMission.List.Count}");
                        success = true;
                        break;
                    }

                    if (!IsExpeditionWorking)
                    {
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:MM:dd-hh:mm}|探险结束被停止");
                        success = true;
                        break;
                    }

                    var mission = exMission.GetMission(index);

                    Console.WriteLine($"mission {JsonConvert.SerializeObject(mission)}");

                    source = await GetHauptframe().GetSourceAsync();
                    if (HtmlUtil.HasTutorial(source, mExpedition.Parser))
                    {
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:MM:dd-hh:mm}|存在错误");
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                    }

                    // 切换舰队页面
                    GoFleetPage();

                    source = await GetHauptframe().GetSourceAsync();

                    // 切换触发球
                    int idx = mPlanet.GetPlanetIndex(mission.PlanetName);
                    if (idx < 0)
                    {
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:G}|没找到探险出发球");
                        _nextFunc(false);
                        continue;
                    }
                    FrameRunJs(NativeScript.SelectPlanet(idx));
                    await Task.Delay(1500);

                    // 查看舰队队列
                    source = await GetHauptframe().GetSourceAsync();
                    if (!HtmlUtil.ParseFleetQueue(source, out FleetQueue fq))
                    {
                        _nextFunc(false);
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:MM:dd-hh:mm}|解析探险队列有误");
                        continue;
                    }

                    if (fq.ExCount >= fq.ExMaxCount)
                    {
                        if (!IsAutoExpedition && CanNotify)
                        {
                            MessageBox.Show("探险队列已满");
                        }
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:MM:dd-hh:mm}|探险队列已满");
                        success = true;
                        break;
                    }

                    if (fq.Count >= fq.MaxCount)
                    {
                        if (!IsAutoExpedition && CanNotify)
                        {
                            MessageBox.Show("航道已满");
                        }
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:G}|航道已满");
                        break;
                    }

                    // 派遣探险
                    bool flag = true;
                    for (var i = 0; i < mission.FleetList.Count; i++)
                    {
                        var fleet = mission.FleetList[i];
                        var shipId = Ship.GetShipId(fleet.ShipType);
                        var count = fleet.Count;
                        if (HtmlUtil.ParseShip(source, shipId, out int total))
                        {
                            Console.WriteLine($"shipId{shipId} count{count}");
                            if (total >= count)
                            {
                                FrameRunJs(NativeScript.SetShip(shipId, count));
                                await Task.Delay(100);
                                continue;
                            }
                        }

                        flag = false;
                        break;
                    }

                    if (!flag)
                    {
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:MM:dd-hh:mm}|没有足够舰队数");
                        _nextFunc(true);
                        continue;
                    }

                    // 继续
                    FrameRunJs(NativeScript.SetShipNext());
                    await Task.Delay(1500);

                    source = await GetHauptframe().GetSourceAsync();

                    // 设置目标点
                    FrameRunJs(NativeScript.SetTarget(mission.X, mission.Y, mission.Z, (int)PlanetType.Star));
                    await Task.Delay(200);

                    source = await GetHauptframe().GetSourceAsync();
                    // 设置目标继续
                    FrameRunJs(NativeScript.SetTargetNext());
                    await Task.Delay(1500);

                    source = await GetHauptframe().GetSourceAsync();
                    // 攻击确认
                    FrameRunJs(NativeScript.SetAttackConfirm());
                    await Task.Delay(1500);

                    // 查看结果
                    source = await GetHauptframe().GetSourceAsync();
                    if (HtmlUtil.HasFleetSuccess(source, mExpedition.Parser))
                    {
                        // Console.WriteLine($"HasFleetSuccess");
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:G}|探险派出成功");
                        _nextFunc(true);
                        _count++;
                        continue;
                    }

                    if (HtmlUtil.HasTutorial(source, mExpedition.Parser))
                    {
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:MM:dd-hh:mm}|派遣错误");
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                    }

                    _nextFunc(false);
                } while (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DoExpedition catch {ex.Message}");
            }

            OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:MM:dd-hh:mm}|探险派遣结束{_count}/{exMission.List.Count}");

            // 如果存在派遣成功的
            if (_count >= 0 || success)
            {
                LastExeditionTime = DateTime.Now;
            }

            IsExpeditionWorking = false;
            SwitchStatus(OperStatus.None);
        }
        #endregion

        #region npc 海盗

        /// <summary>
        /// 刷新NPC
        /// </summary>
        internal void RefreshNpc(bool isAuto = false)
        {
            SwitchStatus(OperStatus.Pirate);
            Task.Run(async () =>
            {
                try
                {
                    OperTipsEvent.Invoke(OperStatus.Pirate, $"{DateTime.Now:MM:dd-HH:mm:ss}|刷球");
                    if (!isAuto)
                    {
                        mPlanet.Reset();
                    }
                    PirateUtil.ResetNpc();
                    Reload();
                    await GoHome(1500);

                    if (HtmlUtil.IsGameUrl(MyAddress))
                    {
                        var source = await GetHauptframe()?.GetSourceAsync();
                        FrameRunJs(NativeScript.ToGalaxy());
                        await Task.Delay(1500);
                        source = await GetHauptframe().GetSourceAsync();
                        await Task.Delay(500);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"RefreshNpc catch {ex.Message}");
                }

                OperTipsEvent.Invoke(OperStatus.Pirate, $"{DateTime.Now:MM:dd-HH:mm:ss}|刷球完成");
                SwitchStatus(OperStatus.None);
            });
        }

        internal void StartPirate(PirateMission pMission)
        {
            SwitchStatus(OperStatus.Pirate);
            IsPirateWorking = true;

            Task.Run(() =>
            {
                DoPirate(pMission);
            });
        }

        internal void StopPirate()
        {
            if (!IsPirateWorking) return;
            IsPirateWorking = false;
        }

        protected async void DoPirate(PirateMission pMission)
        {
            int index = 0;
            int _count = 0;
            bool lastErr = false;
            string source = "";
            bool success = false;

            OperTipsEvent.Invoke(OperStatus.Pirate, $"{DateTime.Now:MM:dd-HH:mm:ss}|开始海盗");

            var _nextFunc = new Action<bool>(isSkip =>
            {
                if (lastErr || isSkip)
                {
                    lastErr = false;
                    index++;
                }
                else
                {
                    lastErr = true;
                }
            });

            if (pMission.MissionCount <= 0)
            {
                OperTipsEvent.Invoke(OperStatus.Pirate, $"{DateTime.Now:MM:dd-HH:mm:ss}|没有海盗任务");
                LastPirateTime = DateTime.Now;
                IsPirateWorking = false;
                SwitchStatus(OperStatus.None);
                return;
            }

            Reload();

            if (HtmlUtil.IsGameUrl(MyAddress))
            {
                source = await GetHauptframe().GetSourceAsync();
                if (HtmlUtil.HasTutorial(source))
                {
                    FrameRunJs(NativeScript.TutorialConfirm());
                    await Task.Delay(1500);
                    source = await GetHauptframe().GetSourceAsync();
                    if (!HtmlUtil.IsInGame(source))
                    {
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:MM:dd-hh:mm}|海盗结束，没有登录");
                        LastExeditionTime = DateTime.Now;
                        IsExpeditionWorking = false;
                        SwitchStatus(OperStatus.None);
                        return;
                    }
                }
            }  else
            {
                OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:MM:dd-hh:mm}|海盗结束，没有登录");
                LastExeditionTime = DateTime.Now;
                IsExpeditionWorking = false;
                SwitchStatus(OperStatus.None);
                return;
            }

            await GoHome(1500);

            try
            {
                do
                {
                    Console.WriteLine($"{DateTime.Now:MM:dd-HH:mm:ss}|PirateMission index{index}");
                    if (index >= pMission.MissionCount)
                    {
                        if (!IsAutoPirate && CanNotify)
                        {
                            MessageBox.Show($"海盗任务派出结束，请检测是否成功{_count}/{pMission.MissionCount}");
                        }
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"{DateTime.Now:MM:dd-HH:mm:ss}|海盗任务结束{_count}/{pMission.MissionCount}");
                        success = true;
                        break;
                    }

                    OperTipsEvent.Invoke(OperStatus.Pirate, $"{DateTime.Now:MM:dd-HH:mm:ss}|海盗任务{index + 1}/{pMission.MissionCount}");

                    if (!IsPirateWorking)
                    {
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"{DateTime.Now:MM:dd-HH:mm:ss}|海盗任务停止");
                        success = true;
                        break;
                    }

                    var mission = pMission.GetMission(index);

                    Console.WriteLine($"{DateTime.Now:G}|mission {JsonConvert.SerializeObject(mission)}");

                    source = await GetHauptframe().GetSourceAsync();
                    if (HtmlUtil.HasTutorial(source, mHtmlParser))
                    {
                        Console.WriteLine($"{DateTime.Now:G}|存在错误");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"{DateTime.Now:MM:dd-HH:mm:ss}|存在错误");
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                    }

                    // 切换舰队页面
                    GoFleetPage();
                    source = await GetHauptframe().GetSourceAsync();

                    if (index <= 1 && HtmlUtil.IsWechatCodePage(source))
                    {
                        Console.WriteLine($"{DateTime.Now:G}|在微信验证页");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"{DateTime.Now:MM:dd-HH:mm:ss}|需微信验证页");
                        success = true;
                        break;
                    }
                    
                    // 切换触发球
                    int idx = mPlanet.GetPlanetIndex(mission.PlanetName);
                    if (idx < 0)
                    {
                        _nextFunc(false);
                        continue;
                    }

                    FrameRunJs(NativeScript.SelectPlanet(idx));
                    await Task.Delay(1500);

                    // 查看舰队队列
                    source = await GetHauptframe().GetSourceAsync();
                    if (!HtmlUtil.ParseFleetQueue(source, out FleetQueue fq))
                    {
                        _nextFunc(false);
                        Console.WriteLine($"{DateTime.Now:G}|解析航道队列有误");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"{DateTime.Now:MM:dd-HH:mm:ss}|解析航道队列有误");
                        continue;
                    }

                    if (fq.Count >= fq.MaxCount)
                    {
                        if (!IsAutoPirate && CanNotify)
                        {
                            MessageBox.Show("航道已满");
                        }
                        Console.WriteLine($"{DateTime.Now:G}|航道已满");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"{DateTime.Now:MM:dd-HH:mm:ss}|航道已满");
                        success = true;
                        break;
                    }

                    if (HtmlUtil.HasAttack(source, mission.TargetPos))
                    {
                        Console.WriteLine($"{DateTime.Now:G}|已经存在攻击任务");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"{DateTime.Now:MM:dd-HH:mm:ss}|已存在攻击目标任务");
                        _nextFunc(true);
                        continue;
                    }

                    // 派遣任务
                    bool flag = true;
                    for (var i = 0; i < mission.FleetList.Count; i++)
                    {
                        var fleet = mission.FleetList[i];
                        var shipId = Ship.GetShipId(fleet.ShipType);
                        var count = fleet.Count;
                        if (HtmlUtil.ParseShip(source, shipId, out int total))
                        {
                            Console.WriteLine($"shipId{shipId} count{count}");
                            if (total >= count)
                            {
                                FrameRunJs(NativeScript.SetShip(shipId, count));
                                await Task.Delay(100);
                                continue;
                            }
                        }

                        flag = false;
                        break;
                    }

                    if (!flag)
                    {
                        Console.WriteLine($"{DateTime.Now:G}|没有足够舰队数");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"{DateTime.Now:MM:dd-HH:mm:ss}|没有足够舰队数");
                        _nextFunc(true);
                        continue;
                    }
                    await Task.Delay(500);

                    // 继续
                    FrameRunJs(NativeScript.SetShipNext());
                    await Task.Delay(1500);

                    source = await GetHauptframe().GetSourceAsync();

                    // 设置目标点
                    FrameRunJs(NativeScript.SetTarget(mission.X, mission.Y, mission.Z, (int)PlanetType.Star));
                    await Task.Delay(500);

                    source = await GetHauptframe().GetSourceAsync();
                    // 设置目标继续
                    FrameRunJs(NativeScript.SetTargetNext());
                    await Task.Delay(1500);

                    source = await GetHauptframe().GetSourceAsync();
                    // 攻击
                    FrameRunJs(NativeScript.SetAttack());
                    await Task.Delay(300);

                    // 攻击确认
                    FrameRunJs(NativeScript.SetAttackConfirm());
                    await Task.Delay(1500);

                    // 查看结果
                    source = await GetHauptframe().GetSourceAsync();
                    if (HtmlUtil.HasFleetSuccess(source, mExpedition.Parser))
                    {
                        Console.WriteLine($"{DateTime.Now:G}|派遣成功");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"{DateTime.Now:MM:dd-HH:mm:ss}|派遣成功");
                        _nextFunc(true);
                        _count++;
                        continue;
                    }

                    if (HtmlUtil.HasTutorial(source, mExpedition.Parser))
                    {
                        Console.WriteLine($"{DateTime.Now:G}|派遣错误");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"{DateTime.Now:MM:dd-HH:mm:ss}|派遣错误");
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                    }

                    _nextFunc(false);
                } while (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DoPirate catch {ex.Message}");
            }

            OperTipsEvent.Invoke(OperStatus.Pirate, $"{DateTime.Now:MM:dd-HH:mm:ss}|海盗任务结束{_count}/{pMission.MissionCount}");

            var checkNpc = false;
            // 如果存在派遣成功的
            if (_count >= 0 || success)
            {
                LastPirateTime = DateTime.Now;

                if (IsPirateWorking && IsAutoPirate)
                {
                    checkNpc = CheckRefreshNpc(pMission);
                }
            }

            IsPirateWorking = false;
            SwitchStatus(OperStatus.None);
            if (checkNpc)
            {
                // 刷新一下NPC
                RefreshNpc(true);
            }
        }

        private bool CheckRefreshNpc(PirateMission pMission)
        {
            if (null == pMission) return false;
            if (pMission.MissionCount <= 0) return false;

            var pMissionCfg = PirateUtil.MyPirateMission;
            if (null == pMissionCfg || pMissionCfg.MissionCount <= 0) return false;

            return pMissionCfg.MissionCount - pMission.MissionCount >= 8;
        }

        #endregion

        public void SwitchStatus(OperStatus operStatus)
        {
            if (MyOperStatus == operStatus) return;

            MyOperStatus = operStatus;

            StatusChangeEvent?.Invoke();
        }

        public void Reload()
        {
            MyWebBrowser.Reload();
            Thread.Sleep(2000);
        }

        public async Task GoHome(int delay = 500)
        {
            FrameRunJs(NativeScript.ToHome());
            await Task.Delay(delay);
        }
    }
}
