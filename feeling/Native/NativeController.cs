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

    class NativeController : Singleton<NativeController>
    {
        public OperStatus MyOperStatus = OperStatus.None;
        public volatile bool IsWorking = false;
        public volatile bool IsPirateWorking = false;
        public volatile bool IsExpeditionWorking = false;
        public volatile bool IsUserWorking = false;
        public volatile bool IsTransferWorking = false;
        public volatile bool IsDetectWorking = false;

        public volatile string MyAddress = "";
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
        public bool IsAutoExpedition1 = false;
        public DateTime LastExpeditionTime = DateTime.Now;
        public DateTime LastExpeditionTime1 = DateTime.Now;

        public bool IsAutoPirate = false;
        public bool IsAutoPirate1 = false;
        public DateTime LastPirateTime = DateTime.Now;
        public DateTime LastPirateTime1 = DateTime.Now;

        public bool CanNotify = true;

        public bool IsAutoImperium = false;
        public DateTime LastImperiumTime = DateTime.Now;

        public int PirateSpeedIndex = 0;

        public DateTime LastGetBonusTime = DateTime.Now.AddDays(-1);

        public bool IsAutoTransfer = false;
        public DateTime LastTransferTime = DateTime.Now.AddDays(-1);
        public DateTime LastTransferTime1 = DateTime.Now.AddDays(-1);

        public DateTime LastGatherTime = DateTime.Now.AddDays(-1);
        public DateTime LastOneClickExpeditionTime = DateTime.Now;

        public void HandleWebBrowserFrameEnd(string url)
        {
            MyAddress = MyWebBrowser.Address;

            if (url.Contains("ogame/frames.php"))
            {
                ScanPlanet();
                ScanNpc();
            }
        }

        public void ScanPlanet()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (!mPlanet.HasData)
                    {
                        var source = await GetFrameSourceAsync();

                        mPlanet.Parse(source, MyAddress);
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
                        var source = await GetFrameSourceAsync();
                        if (source.Contains("id=\"galaxy_form\""))
                        {
                            PirateUtil.ParseNpc(source);
                            NpcChangeEvent?.Invoke();
                        }
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
            return MyWebBrowser.GetBrowser()?.GetFrame("Hauptframe");
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

            var universeName = "";

            var mat = Regex.Match(MyAddress, $@"://(?<universe>\S*).cicihappy.com");
            if (mat.Success)
            {
                universeName = mat.Groups["universe"].Value;
            }

            if (string.IsNullOrWhiteSpace(universeName))
            {
                StopScanGalaxy();
                if (CanNotify)
                {
                    Toast("请先登录");
                }
                return;
            }

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
                            Toast("刷图完成");
                        }
                        break;
                    }

                    mScanDesc = $"刷图次数：{count}";
                    if (count > 1498)
                    {
                        if (CanNotify)
                        {
                            Toast("刷图次数过多，建议换号");
                        }
                        break;
                    }

                    count++;
                    FrameRunJs(NativeScript.RefreshGalaxy(mGalaxy.NextX, mGalaxy.NextY));
                    await Task.Delay(200);
                    FrameRunJs(NativeScript.RefreshGalaxySubmit());
                    mScanPage = $"{mGalaxy.NextX}:{mGalaxy.NextY}";
                    FireScanGalaxy();
                    await Task.Delay(1500);
                    var source = await GetFrameSourceAsync();
                    if (!mGalaxy.AddPage(source))
                    {
                        if (lastError)
                        {
                            mScanDesc = $"刷图异常";
                            if (CanNotify)
                            {
                                Toast("刷图异常，检测下再开始");
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
                NativeLog.Error($"Scan galaxy catch {ex.Message}");
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
                Toast("暂无可扫描的星图可以保存");
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
            var source = "";
            if (account.Length <= 0 || psw.Length <= 0 || universe <= 0 || universe > 24)
            {
                if (CanNotify)
                {
                    // Toast("请输入正确的账号、密码或宇宙");
                }
                OperTipsEvent.Invoke(OperStatus.System, $"请输入正确账号、密码或宇宙");
                return;
            }

            OperTipsEvent.Invoke(OperStatus.System, $"开始登录");
            NativeLog.Info("开始登录");

            // 
            Reload();
            source = await GetFrameSourceAsync();
            bool needLogin = false;

            // 当前还在游戏页面
            if (HtmlParser.IsGameUrl(MyAddress))
            {
                if (source.Contains("退出成功") && source.Contains("重新登录"))
                {
                    needLogin = true;
                }
                else if (HtmlParser.HasTutorial(source))
                {
                    FrameRunJs(NativeScript.TutorialConfirm());
                    await Task.Delay(1500);
                    source = await GetFrameSourceAsync();

                    if (HtmlParser.IsGameUrl(MyAddress) && HtmlParser.IsInGame(source))
                    {
                        if (CanNotify)
                        {
                            Toast("已经是登录状态");
                        }
                        OperTipsEvent.Invoke(OperStatus.System, $"已经是登录状态");
                        return;
                    }

                    needLogin = true;
                }
                else
                {
                    if (HtmlParser.IsGameUrl(MyAddress) && HtmlParser.IsInGame(source))
                    {
                        if (CanNotify)
                        {
                            Toast("已经是登录状态");
                        }
                        OperTipsEvent.Invoke(OperStatus.System, $"已经是登录状态");
                        return;
                    }
                    needLogin = true;
                }
            }
            else
            {
                needLogin = true;
            }

            if (!needLogin)
            {
                OperTipsEvent.Invoke(OperStatus.System, $"不需要重新登录");
                NativeLog.Info("不需要重新登录");
                return;
            }

            await DoLoginAsync(account, psw, universe);
            OperTipsEvent.Invoke(OperStatus.System, $"登录输入完成");
            NativeLog.Info("登录输入完成");
            try
            {
                await Task.Delay(2000);
                source = await GetFrameSourceAsync();
                await GoHome(1000);
                if (HtmlParser.IsGameUrl(MyAddress))
                {
                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasTutorial(source))
                    {
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                    }
                }
            }
            catch (Exception)
            {
            }

            OperTipsEvent.Invoke(OperStatus.System, $"登录操作结束");
            NativeLog.Info("登录操作结束");
        }

        protected async Task DoLoginAsync(string account, string psw, int universe)
        {
            if (!HtmlParser.IsHomeUrl(MyAddress))
            {
                MyWebBrowser.Load(NativeConst.Homepage);
                await Task.Delay(3000);
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
            try
            {
                OperTipsEvent.Invoke(OperStatus.System, $"退出登录");
                NativeLog.Info("开始退出登录");

                Reload();
                var source = await GetFrameSourceAsync();

                if (!HtmlParser.IsGameUrl(MyAddress))
                {
                    if (CanNotify)
                    {
                        Toast("退出失败，可能不在游戏页");
                    }
                    OperTipsEvent.Invoke(OperStatus.System, $"退出失败，可能不在游戏页");
                    NativeLog.Info("退出失败，可能不在游戏页");
                    return;
                }

                if (OperStatus.System != MyOperStatus)
                {
                    if (CanNotify)
                    {
                        Toast("当前正在忙，不建议退出");
                    }
                    OperTipsEvent.Invoke(OperStatus.System, $"当前正在忙，不建议退出");
                    NativeLog.Info("当前正在忙，不建议退出");
                    return;
                }

                source = await GetFrameSourceAsync();
                if (!HtmlParser.HasLogoutBtn(source))
                {
                    await GoHome();
                    await Task.Delay(1500);
                    source = await GetFrameSourceAsync();
                }

                if (!HtmlParser.IsInGame(source))
                {
                    if (CanNotify)
                    {
                        // Toast("退出失败，无退出按钮");
                    }
                    OperTipsEvent.Invoke(OperStatus.System, $"退出失败，无退出按钮");
                    NativeLog.Info("退出失败，无退出按钮");
                    return;
                }

                NativeLog.Info("退出登录");
                FrameRunJs(NativeScript.ToLogout());
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                NativeLog.Error($"LogoutAsync catch {ex.Message}");
            }

            NativeLog.Info("退出操作结束");
            OperTipsEvent.Invoke(OperStatus.System, $"退出操作结束");
        }
        #endregion login

        #region Expedition
        void GoFleetPage(int delay = 1500)
        {
            FrameRunJs(NativeScript.ToFleet());
            Thread.Sleep(delay);
        }

        internal void StartExpedition(ExMission exMission, int index = 0, bool isAuto = false)
        {
            SwitchStatus(OperStatus.Expedition);
            IsExpeditionWorking = true;
            OperTipsEvent.Invoke(OperStatus.Expedition, $"开始探险");
            Task.Run(() =>
            {
                DoExpedition(exMission, index, isAuto);
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
                    OperTipsEvent.Invoke(OperStatus.Expedition, $"刷NPC");
                    mPlanet.Reset();
                    Reload();
                    await GoHome(1500);

                    if (HtmlParser.IsGameUrl(MyAddress))
                    {
                        var source = await GetFrameSourceAsync();
                        await Task.Delay(500);
                    }
                }
                catch (Exception ex)
                {
                    NativeLog.Error($"RefreshPlanet catch {ex.Message}");
                }

                OperTipsEvent.Invoke(OperStatus.Expedition, $"刷NPC结束");
                SwitchStatus(OperStatus.None);
            });
        }

        internal void StopExpedition()
        {
            if (!IsExpeditionWorking) return;

            IsExpeditionWorking = false;
        }

        protected async void DoExpedition(ExMission exMission, int cfgIndex = 0, bool isAuto = false)
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

            try
            {

                OperTipsEvent.Invoke(OperStatus.Expedition, $"开始处理探险");

                if (exMission.List.Count <= 0)
                {
                    OperTipsEvent.Invoke(OperStatus.Expedition, $"没有探险任务");

                    SetLastExpeditionTime(cfgIndex);
                    IsExpeditionWorking = false;
                    SwitchStatus(OperStatus.None);
                    return;
                }

                Reload();
                if (HtmlParser.IsGameUrl(MyAddress))
                {
                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasTutorial(source))
                    {
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                        source = await GetFrameSourceAsync();
                        if (!HtmlParser.IsInGame(source))
                        {
                            OperTipsEvent.Invoke(OperStatus.Expedition, $"探险结束，没有登录");
                            SetLastExpeditionTime(cfgIndex);
                            IsExpeditionWorking = false;
                            SwitchStatus(OperStatus.None);
                            return;
                        }
                    }
                }
                else
                {
                    OperTipsEvent.Invoke(OperStatus.Expedition, $"探险结束，没有登录");
                    SetLastExpeditionTime(cfgIndex);
                    IsExpeditionWorking = false;
                    SwitchStatus(OperStatus.None);
                    return;
                }

                await GoHome(1500);

                do
                {
                    NativeLog.Info($"ex-mession index{index}");
                    OperTipsEvent.Invoke(OperStatus.Expedition, $"探险{index + 1}");

                    if (index >= exMission.List.Count)
                    {
                        if (!IsAutoExpedition && CanNotify)
                        {
                            // Toast($"探险派出结束，请检测是否成功{_count}/{exMission.List.Count}");
                        }
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"探险派出结束{_count}/{exMission.List.Count}");
                        success = true;
                        break;
                    }

                    if (!IsExpeditionWorking)
                    {
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"探险结束被停止");
                        success = true;
                        break;
                    }

                    if (lastErr)
                    {
                        Reload();
                    }
                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasTutorial(source, mExpedition.Parser))
                    {
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"存在错误");
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                    }

                    // 切换舰队页面
                    GoFleetPage();

                    source = await GetFrameSourceAsync();
                    if (HtmlParser.ParseFleetQueue(source, out FleetQueue fq))
                    {
                        if (index < fq.ExCount)
                        {
                            NativeLog.Info($"修正探险序号：{index}({fq.ExCount})");
                            index = fq.ExCount;
                        }
                    }

                    var mission = exMission.GetMission(index);
                    NativeLog.Info($"mission {JsonConvert.SerializeObject(mission)}");

                    // 切换触发球
                    int idx = mPlanet.GetPlanetIndex(mission.PlanetName);
                    if (idx < 0)
                    {
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"没找到探险出发球");
                        _nextFunc(false);
                        continue;
                    }
                    FrameRunJs(NativeScript.SelectPlanet(idx));
                    await Task.Delay(1500);

                    // 查看舰队队列
                    source = await GetFrameSourceAsync();
                    if (!HtmlParser.ParseFleetQueue(source, out fq))
                    {
                        _nextFunc(false);
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"解析探险队列有误");
                        continue;
                    }

                    if (fq.ExCount >= fq.ExMaxCount)
                    {
                        if (!IsAutoExpedition && CanNotify)
                        {
                            // Toast("探险队列已满");
                        }
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"探险队列已满");
                        success = true;
                        break;
                    }

                    if (fq.Count >= fq.MaxCount)
                    {
                        if (!IsAutoExpedition && CanNotify)
                        {
                            // Toast("航道已满");
                        }
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"航道已满");
                        break;
                    }

                    // 派遣探险
                    bool flag = true;
                    for (var i = 0; i < mission.FleetList.Count; i++)
                    {
                        var fleet = mission.FleetList[i];
                        var shipId = Ship.GetShipId(fleet.ShipType);
                        var count = fleet.Count;
                        if (HtmlParser.ParseShip(source, shipId, out int total))
                        {
                            NativeLog.Info($"shipId{shipId} count{count}");
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
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"没有足够舰队数");
                        _nextFunc(true);
                        continue;
                    }

                    // 继续
                    FrameRunJs(NativeScript.SetShipNext());
                    await Task.Delay(1500);

                    source = await GetFrameSourceAsync();

                    // 设置目标点
                    FrameRunJs(NativeScript.SetTarget(mission.X, mission.Y, mission.Z, (int)PlanetType.Star));
                    await Task.Delay(200);

                    source = await GetFrameSourceAsync();
                    // 设置目标继续
                    FrameRunJs(NativeScript.SetTargetNext());
                    await Task.Delay(1500);

                    source = await GetFrameSourceAsync();

                    var confirmType = HtmlParser.AttackConfirmType(source);
                    // 攻击确认
                    FrameRunJs(NativeScript.SetAttackConfirm(confirmType));
                    await Task.Delay(1500);

                    // 查看结果
                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasFleetSuccess(source, mExpedition.Parser))
                    {
                        // Console.WriteLine($"HasFleetSuccess");
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"探险派出成功");
                        _nextFunc(true);
                        _count++;
                        continue;
                    }

                    if (HtmlParser.HasTutorial(source, mExpedition.Parser))
                    {
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"派遣错误");
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                    }

                    _nextFunc(false);
                } while (true);
            }
            catch (Exception ex)
            {
                NativeLog.Error($"DoExpedition catch {ex.Message}");
            }

            OperTipsEvent.Invoke(OperStatus.Expedition, $"探险派遣结束{_count}/{exMission.List.Count}");

            // autoLogin
            var autoLogout = false;

            // 如果存在派遣成功的
            if (_count >= 0 || success)
            {
                SetLastExpeditionTime(cfgIndex);

                if (IsExpeditionWorking && isAuto)
                {
                    autoLogout = CanAutoLogout();
                }
            }

            IsExpeditionWorking = false;
            if (autoLogout)
            {
                SwitchStatus(OperStatus.System);
                await LogoutAsync();
                SwitchStatus(OperStatus.None);
            }
            else
            {
                SwitchStatus(OperStatus.None);
            }
        }

        private void SetLastExpeditionTime(int index)
        {
            if (index == 1)
            {
                LastExpeditionTime1 = DateTime.Now;
            }
            else
            {
                LastExpeditionTime = DateTime.Now;
            }
        }

        #endregion

        #region npc 海盗

        /// <summary>
        /// 刷新NPC
        /// </summary>
        internal void RefreshNpc(bool isAuto = false)
        {
            SwitchStatus(OperStatus.System);
            Task.Run(async () =>
            {
                await DoRefreshNpc(isAuto);
            });
        }

        internal async Task DoRefreshNpc(bool isAuto = false)
        {
            try
            {
                SwitchStatus(OperStatus.System);
                OperTipsEvent.Invoke(OperStatus.System, $"刷球");
                NativeLog.Info("刷球");
                mPlanet.Reset();
                PirateUtil.ResetNpc();
                Reload();
                var source = await GetFrameSourceAsync();
                await GoHome(1500);

                if (HtmlParser.IsGameUrl(MyAddress))
                {
                    await GetFrameSourceAsync();
                    FrameRunJs(NativeScript.ToGalaxy());
                    await Task.Delay(1500);
                    source = await GetFrameSourceAsync();
                    PirateUtil.ParseNpc(source, MyAddress);
                    NpcChangeEvent?.Invoke();
                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                NativeLog.Error($"DoRefreshNpc catch {ex.Message}");
            }

            OperTipsEvent.Invoke(OperStatus.System, $"刷球完成");
            NativeLog.Info("刷球完成");
            SwitchStatus(OperStatus.None);
        }

        internal void StartPirate(PirateMission pMission, int cfgIndex = 0, bool isAuto = false)
        {
            SwitchStatus(OperStatus.Pirate);
            IsPirateWorking = true;
            Task.Run(() =>
            {
                DoPirate(pMission, cfgIndex, isAuto);
            });
        }

        internal void StopPirate()
        {
            if (!IsPirateWorking) return;
            IsPirateWorking = false;
        }

        protected async void DoPirate(PirateMission pMission, int cfgIndex = 0, bool isAuto = false)
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

            try
            {
                OperTipsEvent.Invoke(OperStatus.Pirate, $"开始海盗");
                NativeLog.Info("开始海盗");

                if (pMission.MissionCount <= 0)
                {
                    OperTipsEvent.Invoke(OperStatus.Pirate, $"没有海盗任务");
                    NativeLog.Info("没有海盗任务");
                    SetLastPirateTime(cfgIndex);
                    IsPirateWorking = false;
                    SwitchStatus(OperStatus.None);
                    return;
                }

                Reload();

                if (HtmlParser.IsGameUrl(MyAddress))
                {
                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasTutorial(source))
                    {
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                        source = await GetFrameSourceAsync();
                        if (!HtmlParser.IsInGame(source))
                        {
                            OperTipsEvent.Invoke(OperStatus.Pirate, $"海盗结束，没有登录");
                            NativeLog.Info("海盗任务结束，没有登录");
                            SetLastPirateTime(cfgIndex);
                            IsPirateWorking = false;
                            SwitchStatus(OperStatus.None);
                            return;
                        }
                    }
                }
                else
                {
                    OperTipsEvent.Invoke(OperStatus.Pirate, $"海盗结束，没有登录");
                    NativeLog.Info("海盗任务结束，没有登录");
                    SetLastPirateTime(cfgIndex);
                    IsPirateWorking = false;
                    SwitchStatus(OperStatus.None);
                    return;
                }

                await GoHome(1500);

                do
                {
                    NativeLog.Info($"PirateMission index{index}");
                    if (index >= pMission.MissionCount)
                    {
                        if (!isAuto && CanNotify)
                        {
                            // Toast($"海盗任务派出结束，请检测是否成功{_count}/{pMission.MissionCount}");
                        }
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"海盗任务结束{_count}/{pMission.MissionCount}");
                        success = true;
                        break;
                    }

                    OperTipsEvent.Invoke(OperStatus.Pirate, $"海盗任务{index + 1}/{pMission.MissionCount}");

                    if (!IsPirateWorking)
                    {
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"海盗任务停止");
                        success = true;
                        break;
                    }

                    var mission = pMission.GetMission(index);

                    NativeLog.Info($"mission {JsonConvert.SerializeObject(mission)}");

                    if (lastErr)
                    {
                        Reload();
                        NativeLog.Info($"重载");
                    }

                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasTutorial(source, mHtmlParser))
                    {
                        NativeLog.Info($"存在错误");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"存在错误");
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                    }

                    // 切换舰队页面
                    GoFleetPage();
                    NativeLog.Info($"切换舰队");
                    source = await GetFrameSourceAsync();
                    if (index <= 1 && HtmlParser.IsWechatCodePage(source))
                    {
                        NativeLog.Info($"在微信验证页");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"在微信验证页");
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
                    source = await GetFrameSourceAsync();
                    if (!source.Contains("id=\"fleetdelaybox\""))
                    {
                        GoFleetPage();
                        source = await GetFrameSourceAsync();
                    }

                    if (!HtmlParser.ParseFleetQueue(source, out FleetQueue fq))
                    {
                        _nextFunc(false);
                        NativeLog.Info($"解析航道队列有误");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"解析航道队列有误");
                        continue;
                    }

                    if (fq.Count >= fq.MaxCount)
                    {
                        if (!isAuto && CanNotify)
                        {
                            // Toast("航道已满");
                        }
                        NativeLog.Info($"航道已满");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"航道已满");
                        success = true;
                        break;
                    }

                    if (HtmlParser.HasAttack(source, mission.TargetPos))
                    {
                        NativeLog.Info($"已经存在攻击任务");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"已存在攻击目标任务");
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
                        if (HtmlParser.ParseShip(source, shipId, out int total))
                        {
                            NativeLog.Info($"shipId{shipId} count{count}");
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
                        NativeLog.Info($"没有足够舰队数");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"没有足够舰队数");
                        _nextFunc(true);
                        continue;
                    }
                    await Task.Delay(500);

                    // 继续
                    FrameRunJs(NativeScript.SetShipNext());
                    await Task.Delay(1500);

                    source = await GetFrameSourceAsync();

                    // 设置目标点
                    FrameRunJs(NativeScript.SetTarget(mission.X, mission.Y, mission.Z, (int)PlanetType.Star));
                    await Task.Delay(500);
                    source = await GetFrameSourceAsync();
                    if (PirateSpeedIndex != 0)
                    {
                        FrameRunJs(NativeScript.SetSpeed(PirateSpeedIndex));
                        await Task.Delay(500);
                        source = await GetFrameSourceAsync();
                    }

                    // 设置目标继续
                    FrameRunJs(NativeScript.SetTargetNext());
                    await Task.Delay(1500);

                    source = await GetFrameSourceAsync();
                    // 攻击
                    FrameRunJs(NativeScript.SetAttack());
                    await Task.Delay(300);

                    var confirmType = HtmlParser.AttackConfirmType(source);
                    // 攻击确认
                    FrameRunJs(NativeScript.SetAttackConfirm(confirmType));
                    await Task.Delay(1500);

                    // 查看结果
                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasFleetSuccess(source, mExpedition.Parser))
                    {
                        NativeLog.Info($"派遣成功");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"派遣成功");
                        _nextFunc(true);
                        _count++;
                        continue;
                    }

                    if (HtmlParser.HasTutorial(source, mExpedition.Parser))
                    {
                        NativeLog.Info($"派遣错误");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"派遣错误");
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                    }

                    _nextFunc(false);
                } while (true);
            }
            catch (Exception ex)
            {
                NativeLog.Error($"DoPirate catch {ex.Message}");
            }

            OperTipsEvent.Invoke(OperStatus.Pirate, $"海盗任务结束{_count}/{pMission.MissionCount}");
            NativeLog.Info($"海盗任务结束{_count}-{success}");

            var checkNpc = false;
            var autoLogout = false;
            // 如果存在派遣成功的
            if (_count >= 0 || success)
            {
                SetLastPirateTime(cfgIndex);

                if (IsPirateWorking && isAuto)
                {
                    // checkNpc = CheckRefreshNpc(pMission);
                    autoLogout = CanAutoLogout();
                }

                await DoGetBonus();
            }

            IsPirateWorking = false;
            SwitchStatus(OperStatus.None);

            if (IsAutoImperium)
            {
                await StartImperium();
            }

            if (checkNpc)
            {
                // 刷新一下NPC
                RefreshNpc(true);
            }
            else if (autoLogout)
            {
                SwitchStatus(OperStatus.System);
                await LogoutAsync();
                SwitchStatus(OperStatus.None);
            }
        }

        private void SetLastPirateTime(int index = 0)
        {
            if (index == 1)
            {
                LastPirateTime1 = DateTime.Now;
            }
            else
            {
                LastPirateTime = DateTime.Now;
            }
        }

        private async Task DoGetBonus()
        {
            try
            {
                var now = DateTime.Now;
                if (LastGetBonusTime.Date.Equals(now.Date))
                {
                    NativeLog.Info("领取海盗资源-今天处理过了");
                    return;
                }

                NativeLog.Info("领取海盗资源");
                Reload();
                var source = await GetFrameSourceAsync();
                await GoHome(1500);

                if (HtmlParser.IsGameUrl(MyAddress))
                {
                    NativeLog.Info("领取海盗资源-任务礼包");
                    await GetFrameSourceAsync();
                    FrameRunJs(NativeScript.ToTaskList());
                    await Task.Delay(1500);
                    source = await GetFrameSourceAsync();
                    NativeLog.Info("领取海盗资源-攻击海盗分页");
                    FrameRunJs(NativeScript.TaskListAttackHaidaoTab());
                    await Task.Delay(1500);
                    source = await GetFrameSourceAsync();
                    NativeLog.Info("领取海盗资源-领取资源");
                    FrameRunJs(NativeScript.TaskListGetAttackHaidaoBonus());
                    await Task.Delay(1500);

                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasTutorial(source))
                    {
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                        source = await GetFrameSourceAsync();
                    }
                    NativeLog.Info("领取远征资源-探险远征分页");
                    FrameRunJs(NativeScript.TaskListYuanzhengTab());
                    await Task.Delay(1500);
                    source = await GetFrameSourceAsync();
                    NativeLog.Info("领取远征资源-领取资源");
                    FrameRunJs(NativeScript.TaskListGetYuanzhengBonus());
                    await Task.Delay(1500);
                    Reload();
                    await GetFrameSourceAsync();

                    // 记录
                    LastGetBonusTime = now;
                }
            }
            catch (Exception ex)
            {
                NativeLog.Error($"DoGetHaidaoZiyuan catch {ex.Message}");
            }

            NativeLog.Info("领取海盗资源结束");
        }

        private bool CheckRefreshNpc(PirateMission pMission)
        {
            if (null == pMission) return false;
            if (pMission.MissionCount <= 0) return false;

            var pMissionCfg = PirateUtil.MyMission;
            if (null == pMissionCfg || pMissionCfg.MissionCount <= 0) return false;

            return pMissionCfg.MissionCount - pMission.MissionCount >= 8;
        }

        #endregion

        #region 验证码
        internal async Task GetCode()
        {
            SwitchStatus(OperStatus.System);
            OperTipsEvent.Invoke(OperStatus.System, $"开始获取验证码");

            try
            {
                Reload();
                var source = await GetFrameSourceAsync();
                await GoHome(1500);
                source = await GetFrameSourceAsync();

                FrameRunJs(NativeScript.ToCode());
                await Task.Delay(1500);
                source = await GetFrameSourceAsync();
                if (HtmlParser.IsWechatCodePage(source))
                {
                    FrameRunJs(NativeScript.GetCode());
                    OperTipsEvent.Invoke(OperStatus.System, $"点击获取验证码");
                    NativeLog.Info("点击获取验证码");
                }
                else
                {
                    NativeLog.Warn("不在验证码页");
                    OperTipsEvent.Invoke(OperStatus.System, $"不在验证码页");
                }
            }
            catch (Exception ex)
            {
                OperTipsEvent.Invoke(OperStatus.System, $"获取验证码异常");
                NativeLog.Error($"不在验证码页 {ex.Message}");
            }
        }

        internal async Task AuthCode(string code)
        {
            SwitchStatus(OperStatus.System);
            OperTipsEvent.Invoke(OperStatus.System, $"输入验证码");

            try
            {
                // Reload();
                // await GoHome(1500);
                var source = await GetFrameSourceAsync();
                if (HtmlParser.IsWechatCodePage(source))
                {
                    FrameRunJs(NativeScript.AuthCode(code));
                    await Task.Delay(200);
                    FrameRunJs(NativeScript.SubmitCode());
                    OperTipsEvent.Invoke(OperStatus.System, $"输入验证码");
                }
                else
                {
                    OperTipsEvent.Invoke(OperStatus.System, $"不在验证码页");
                }
            }
            catch (SystemException)
            {
                OperTipsEvent.Invoke(OperStatus.System, $"输入验证码异常");
            }
        }

        #endregion

        #region 统治
        public async Task StartImperium(bool isAuto = false)
        {
            try
            {
                OperTipsEvent.Invoke(OperStatus.System, $"统治");

                Reload();

                if (!HtmlParser.IsGameUrl(MyAddress))
                {
                    OperTipsEvent.Invoke(OperStatus.System, $"统治失败，可能不在游戏页");
                    return;
                }

                var source = await GetFrameSourceAsync();
                if (!HtmlParser.HasImperium(source))
                {
                    await GoHome();
                    await Task.Delay(1500);
                }

                source = await GetFrameSourceAsync();
                if (!HtmlParser.HasImperium(source))
                {
                    OperTipsEvent.Invoke(OperStatus.System, $"统治失败，没有统治按钮");
                    return;
                }

                FrameRunJs(NativeScript.ToImperium());
                await Task.Delay(1500);
                source = await GetFrameSourceAsync();
                if (!HtmlParser.HasImperiumDetail(source))
                {
                    OperTipsEvent.Invoke(OperStatus.System, $"统治失败，没有统治详情按钮");
                    return;
                }

                FrameRunJs(NativeScript.ToImperiumDetail());
                await Task.Delay(1500);
                source = await GetFrameSourceAsync();

                Reload();

                var autoLogut = false;
                if (isAuto)
                {
                    autoLogut = CanAutoLogout();
                }

                if (autoLogut)
                {
                    await LogoutAsync();
                }
            }
            catch (Exception ex)
            {
                NativeLog.Error($"StartImperium catch {ex.Message}");
            }

            OperTipsEvent.Invoke(OperStatus.System, $"统治结束");
        }
        #endregion 统治

        #region 多维
        public async Task GoCross()
        {
            try
            {
                OperTipsEvent.Invoke(OperStatus.System, "切换多维宇宙");

                Reload();

                var source = await GetFrameSourceAsync();

                var address = MyWebBrowser.Address;
                if (address.Contains("w1.cicihappy.com/ogame/frames.php"))
                {
                    OperTipsEvent.Invoke(OperStatus.System, "当前处于多维宇宙");
                    return;
                }

                if (HtmlParser.IsGameUrl(MyAddress))
                {
                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasTutorial(source))
                    {
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                        source = await GetFrameSourceAsync();
                    }
                }

                await GoHome(1500);
                await GetFrameSourceAsync();
                // 切换舰队页面
                GoFleetPage();

                // 查看舰队队列
                source = await GetFrameSourceAsync();
                if (source.Contains("id=\"fleetdelaybox\""))
                {
                    mPlanet.Reset();
                    FrameRunJs(NativeScript.ToCross());
                    await Task.Delay(2500);
                    source = await GetFrameSourceAsync();
                }
            }
            catch (Exception ex)
            {
                NativeLog.Error($"GoCross catch {ex.Message}");
            }
            OperTipsEvent.Invoke(OperStatus.System, "切换多维宇宙结束");
        }
        #endregion

        #region 返回本宇宙
        public async Task BackUniverse()
        {
            try
            {
                OperTipsEvent.Invoke(OperStatus.System, $"返回本宇宙");
                NativeLog.Info("返回本宇宙");
                Reload();

                var source = await GetFrameSourceAsync();
                var address = MyWebBrowser.Address;

                if (!address.Contains("w1.cicihappy.com/ogame/frames.php"))
                {
                    OperTipsEvent.Invoke(OperStatus.System, $"当前不是多维宇宙");
                    NativeLog.Info("当前不是多维宇宙");
                    return;
                }

                if (HtmlParser.IsGameUrl(MyAddress))
                {
                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasTutorial(source))
                    {
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                        source = await GetFrameSourceAsync();
                    }
                }

                NativeLog.Info("返回概况");
                await GoHome(1500);
                await GetFrameSourceAsync();
                // 切换舰队页面
                GoFleetPage();

                NativeLog.Info("切换舰队页");

                // 查看舰队队列
                source = await GetFrameSourceAsync();
                if (source.Contains("id=\"fleetdelaybox\""))
                {
                    mPlanet.Reset();
                    FrameRunJs(NativeScript.BackUniverse());
                    await Task.Delay(2500);
                    source = await GetFrameSourceAsync();
                }
            }
            catch (Exception ex)
            {
                NativeLog.Error($"BackUniverse catch {ex.Message}");
            }
            OperTipsEvent.Invoke(OperStatus.System, $"返回本宇宙结束");
            NativeLog.Info("返回本宇宙结束");
        }
        #endregion

        #region 转移资源到月球
        internal void StartTransfer()
        {
            SwitchStatus(OperStatus.System);
            IsTransferWorking = true;

            Task.Run(() =>
            {
                DoTransfer();
            });
        }

        internal void StopTransfer()
        {
            if (!IsTransferWorking) return;
            IsTransferWorking = false;
        }

        protected async void DoTransfer()
        {
            int index = 0;
            int _count = 0;
            bool lastErr = false;
            string source = "";

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

            var planetLists = MyPlanet.List;
            try
            {
                OperTipsEvent.Invoke(OperStatus.System, $"开始转移资源");
                NativeLog.Info($"开始转移资源");

                Reload();

                if (HtmlParser.IsGameUrl(MyAddress))
                {
                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasTutorial(source))
                    {
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                        source = await GetFrameSourceAsync();
                        if (!HtmlParser.IsInGame(source))
                        {
                            OperTipsEvent.Invoke(OperStatus.System, $"转移资源结束，没有登录");
                            NativeLog.Info("转移资源-没有登录");
                            IsTransferWorking = false;
                            SwitchStatus(OperStatus.None);
                            return;
                        }
                    }
                }
                else
                {
                    OperTipsEvent.Invoke(OperStatus.System, $"转移资源结束，没有登录");
                    NativeLog.Info("转移资源-没有登录");
                    IsTransferWorking = false;
                    SwitchStatus(OperStatus.None);
                    return;
                }

                await GoHome(1500);

                do
                {
                    NativeLog.Info($"资源转移 index{index}");
                    if (index >= planetLists.Count)
                    {
                        OperTipsEvent.Invoke(OperStatus.System, $"资源转移结束{_count}/{planetLists.Count}");
                        break;
                    }

                    OperTipsEvent.Invoke(OperStatus.System, $"资源转移{index + 1}/{planetLists.Count}");

                    if (!IsTransferWorking)
                    {
                        OperTipsEvent.Invoke(OperStatus.System, $"资源转移停止");
                        break;
                    }

                    var planetDesc = planetLists[index];

                    if (lastErr)
                    {
                        Reload();
                        NativeLog.Info($"重载");
                    }

                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasTutorial(source, mHtmlParser))
                    {
                        NativeLog.Info($"存在错误");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"存在错误");
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                    }

                    // 切换舰队页面
                    GoFleetPage();
                    NativeLog.Info($"切换舰队");
                    source = await GetFrameSourceAsync();
                    if (index <= 1 && HtmlParser.IsWechatCodePage(source))
                    {
                        NativeLog.Info($"在微信验证页");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"在微信验证页");
                        break;
                    }

                    FrameRunJs(NativeScript.SelectPlanet(index));
                    await Task.Delay(1500);

                    // 查看舰队队列
                    source = await GetFrameSourceAsync();
                    if (!source.Contains("id=\"fleetdelaybox\""))
                    {
                        GoFleetPage();
                        source = await GetFrameSourceAsync();
                    }

                    if (HtmlParser.IsMoon(source))
                    {
                        NativeLog.Info($"没有能量（{planetDesc}）不需要转移");
                        _nextFunc(true);
                        continue;
                    }

                    if (!HtmlParser.ParseFleetQueue(source, out FleetQueue fq))
                    {
                        _nextFunc(false);
                        NativeLog.Info($"解析航道队列有误");
                        OperTipsEvent.Invoke(OperStatus.System, $"解析航道队列有误");
                        continue;
                    }

                    if (fq.Count >= fq.MaxCount)
                    {
                        NativeLog.Info($"航道已满");
                        OperTipsEvent.Invoke(OperStatus.System, $"航道已满");
                        break;
                    }

                    bool flag = false;
                    var shipId = Ship.GetShipId(ShipType.LC);
                    if (HtmlParser.ParseShip(source, shipId, out int total))
                    {
                        if (total > 0)
                        {
                            FrameRunJs(NativeScript.SetShip(shipId, total));
                            await Task.Delay(100);
                            flag = true;
                        }
                    }

                    if (!flag)
                    {
                        NativeLog.Info($"没有大型运输机");
                        OperTipsEvent.Invoke(OperStatus.System, $"没有大型运输机");
                        _nextFunc(true);
                        continue;
                    }
                    await Task.Delay(500);

                    // 继续
                    FrameRunJs(NativeScript.SetShipNext());
                    await Task.Delay(1500);

                    source = await GetFrameSourceAsync();

                    // 设置目标点
                    FrameRunJs(NativeScript.SetTargetType((int)PlanetType.Moon));
                    await Task.Delay(500);
                    source = await GetFrameSourceAsync();

                    // 设置目标继续
                    FrameRunJs(NativeScript.SetTargetNext());
                    await Task.Delay(1500);

                    source = await GetFrameSourceAsync();
                    // 运输
                    FrameRunJs(NativeScript.SetTransfer());
                    await Task.Delay(300);

                    FrameRunJs(NativeScript.SetMaxResource(1));
                    await Task.Delay(100);
                    FrameRunJs(NativeScript.SetMaxResource(2));
                    await Task.Delay(100);
                    FrameRunJs(NativeScript.SetMaxResource(3));
                    await Task.Delay(100);

                    var confirmType = HtmlParser.AttackConfirmType(source);
                    // 攻击确认
                    FrameRunJs(NativeScript.SetAttackConfirm(confirmType));
                    await Task.Delay(1500);

                    // 查看结果
                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasFleetSuccess(source, mExpedition.Parser))
                    {
                        NativeLog.Info($"派遣成功");
                        OperTipsEvent.Invoke(OperStatus.System, $"派遣成功");
                        _nextFunc(true);
                        _count++;
                        continue;
                    }

                    if (HtmlParser.HasTutorial(source, mExpedition.Parser))
                    {
                        NativeLog.Info($"派遣错误");
                        OperTipsEvent.Invoke(OperStatus.System, $"派遣错误");
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                    }

                    _nextFunc(false);
                } while (true);
            }
            catch (Exception ex)
            {
                NativeLog.Error($"DoTransfer catch {ex.Message}");
            }

            OperTipsEvent.Invoke(OperStatus.System, $"资源转运结束{_count}/{planetLists.Count}");

            IsTransferWorking = false;

            if (MyPlanet.Universe.Contains("w1"))
            {
                LastTransferTime1 = DateTime.Now;
            }
            else
            {
                LastTransferTime = DateTime.Now;
            }

            SwitchStatus(OperStatus.None);
        }

        #endregion 转移资源到月球
        public void SwitchStatus(OperStatus operStatus)
        {
            if (MyOperStatus == operStatus) return;

            MyOperStatus = operStatus;

            StatusChangeEvent?.Invoke();
        }

        public void Reload()
        {
            MyWebBrowser.Reload();
            Thread.Sleep(2500);
        }

        public async Task GoHome(int delay = 500)
        {
            FrameRunJs(NativeScript.ToHome());
            await Task.Delay(delay);
        }

        public async Task<string> GetFrameSourceAsync(int retry = 3)
        {
            int n = 0;
            string source;

            do
            {
                try
                {
                    var _task = GetHauptframe()?.GetSourceAsync();
                    if (_task == await Task.WhenAny(_task, Task.Delay(30000)))
                    {
                        source = await _task;
                    }
                    else
                    {
                        NativeLog.Error($"GetFrameSourceAsync 超时");
                        source = "";
                    }
                    // source = await GetHauptframe()?.GetSourceAsync();
                }
                catch (Exception ex)
                {
                    NativeLog.Error($"GetFrameSourceAsync catch {ex.Message}");
                    source = "";
                }
                if (!string.IsNullOrWhiteSpace(source))
                {
                    break;
                }

                await Task.Delay(1000);
                n++;
            } while (n < retry);
            return source;
        }

        public bool CanAutoLogout()
        {
            var now = DateTime.Now;
            TimeSpan delta;
            double val = 0;
            double min = 100;
            bool hasAuto = false;

            if (!User.AutoLogout) return false;

            var xMissionCfg = Expedition.MyExMissionCfg;
            if (IsAutoExpedition && null != xMissionCfg)
            {
                hasAuto = true;
                delta = now - LastExpeditionTime;
                val = xMissionCfg.Interval - delta.TotalMinutes;
                val = val < 0 ? 0 : val;
                min = Math.Min(val, min);
            }

            var xMissionCfg1 = Expedition.MyExMissionCfg1;
            if (IsAutoExpedition1 && null != xMissionCfg1)
            {
                hasAuto = true;
                delta = now - LastExpeditionTime1;
                val = xMissionCfg1.Interval - delta.TotalMinutes;
                val = val < 0 ? 0 : val;
                min = Math.Min(val, min);
            }

            var pMissionCfg = PirateUtil.MyMission;
            if (IsAutoPirate && null != pMissionCfg)
            {
                hasAuto = true;
                delta = now - LastPirateTime;
                val = pMissionCfg.Interval - delta.TotalMinutes;
                val = val < 0 ? 0 : val;
                min = Math.Min(val, min);
            }

            var pMissionCfg1 = PirateUtil.MyMission1;
            if (IsAutoPirate1 && null != pMissionCfg1)
            {
                hasAuto = true;
                delta = now - LastPirateTime1;
                val = pMissionCfg1.Interval - delta.TotalMinutes;
                val = val < 0 ? 0 : val;
                min = Math.Min(val, min);
            }

            var imperium = ImperiumUtil.MyImperium;
            if (IsAutoImperium && null != imperium)
            {
                hasAuto = true;
                delta = now - LastImperiumTime;
                val = imperium.Interval - delta.TotalMinutes;
                val = val < 0 ? 0 : val;
                min = Math.Min(val, min);
            }

            if (!hasAuto) return false;

            return min >= 10;
        }

        public void QuickClearLastTime()
        {
            var dt = DateTime.Now;
            dt = dt.AddDays(-1);

            if (IsAutoExpedition)
            {
                LastExpeditionTime = dt;
            }

            if (IsAutoPirate)
            {
                LastPirateTime = dt;
            }

            if (IsAutoExpedition1)
            {
                LastExpeditionTime1 = dt;
            }

            if (IsAutoPirate1)
            {
                LastPirateTime1 = dt;
            }
        }

        #region 收集用户

        internal void StartScanUser()
        {
            SwitchStatus(OperStatus.System);
            IsUserWorking = true;

            Task.Run(() =>
            {
                DoScanUser();
            });
        }

        protected async void DoScanUser()
        {
            var lists = new List<RankUser>();

            var account = "null404";
            var psw = "yaya520184";
            var universe = 1;
            string source = "";
            string name = "";
            string crossName = "";
            var ret = false;
            bool lastError = false;

            try
            {
                OperTipsEvent.Invoke(OperStatus.System, $"收集用户开始");
                NativeLog.Info($"收集用户开始");
                NativeLog.Info($"扫描排行榜");

                do
                {
                    var arr = await DoUniverseUser(account, psw, universe);
                    NativeLog.Info($"收集 u{universe} 多维账号");
                    if (null != arr && arr.Count > 0)
                    {
                        for (int i = 0; i < arr.Count; i++)
                        {
                            name = arr[i].Name;

                            FrameRunJs(NativeScript.ToSearch());
                            await Task.Delay(1500);
                            source = await GetFrameSourceAsync();

                            FrameRunJs(NativeScript.SearchUser(name));
                            await Task.Delay(200);

                            FrameRunJs(NativeScript.SearchUserSubmit());
                            await Task.Delay(1500);
                            source = await GetFrameSourceAsync();

                            ret = HtmlParser.ParseSearchCrossName(source, name, out crossName);
                            if (ret)
                            {
                                arr[i].CrossName = crossName;
                            }

                        }

                        lists.AddRange(arr);
                    }
                    else
                    {
                        if (!lastError)
                        {
                            lastError = true;
                            NativeLog.Info($"重试");
                            continue;
                        }
                    }

                    lastError = false;
                    NativeLog.Info($"收集 u{universe} 多维账号 结束");

                    await LogoutAsync();
                    await Task.Delay(2000);
                    source = await GetFrameSourceAsync();

                    universe++;
                } while (universe <= NativeConst.MaxUniverseCount);

                NativeLog.Info($"搜索用户结束");
            }
            catch (Exception ex)
            {
                NativeLog.Error($"DoScanUser catch {ex.Message}");
            }

            RankUser.Save(lists);

            OperTipsEvent.Invoke(OperStatus.System, $"收集用户结束");
            NativeLog.Info($"收集用户结束");
            IsUserWorking = false;
            SwitchStatus(OperStatus.None);
        }

        protected async Task<List<RankUser>> DoUniverseUser(string account, string psw, int universe)
        {
            var lists = new List<RankUser>();
            string source;

            OperTipsEvent.Invoke(OperStatus.System, $"开始收集 u{universe}");
            NativeLog.Info($"开始收集 u{universe}");
            await LoginAsync(account, psw, universe);

            try
            {
                Reload();

                if (HtmlParser.IsGameUrl(MyAddress))
                {
                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasTutorial(source))
                    {
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                        source = await GetFrameSourceAsync();
                    }
                }

                await GoHome(1500);

                FrameRunJs(NativeScript.ToRank());
                await Task.Delay(1500);
                source = await GetFrameSourceAsync();

                FrameRunJs(NativeScript.SelectRank(0));
                await Task.Delay(2000);
                source = await GetFrameSourceAsync();

                var ret = HtmlParser.ParseRank(source, universe, ref lists);
            }
            catch (Exception ex)
            {
                NativeLog.Error($"DoUniverseUser catch {ex.Message}");
            }

            OperTipsEvent.Invoke(OperStatus.System, $"收集 u{universe} 结束");
            NativeLog.Info($"收集 u{universe} 结束");

            return lists;
        }

        internal void StopScanUser()
        {
            if (!IsUserWorking) return;
            IsUserWorking = false;
        }
        #endregion

        public void ResetStatus()
        {
            StopScanGalaxy();
            StopExpedition();
            StopPirate();
            StopScanUser();
            StopTransfer();
            StopGather();
            StopDetect();
            SwitchStatus(OperStatus.None);
        }

        public void Toast(string content)
        {
            MessageBox.Show(content);
        }

        internal void StartGather(GatherMission gMission)
        {
            SwitchStatus(OperStatus.System);
            IsWorking = true;
            Task.Run(() =>
            {
                DoGather(gMission);
            });
        }

        internal void StopGather()
        {
            if (!IsWorking) return;
            IsWorking = false;
        }

        protected async void DoGather(GatherMission gMission)
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

            try
            {
                OperTipsEvent.Invoke(OperStatus.System, $"开始集中资源");
                NativeLog.Info("开始集中资源");

                if (gMission.MissionCount <= 0)
                {
                    OperTipsEvent.Invoke(OperStatus.System, $"没有集中任务");
                    NativeLog.Info("没有集中任务");
                    StopGather();
                    SwitchStatus(OperStatus.None);
                    return;
                }

                Reload();

                if (HtmlParser.IsGameUrl(MyAddress))
                {
                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasTutorial(source))
                    {
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                        source = await GetFrameSourceAsync();
                        if (!HtmlParser.IsInGame(source))
                        {
                            OperTipsEvent.Invoke(OperStatus.System, $"集中任务结束，没有登录");
                            NativeLog.Info("集中任务结束，没有登录");
                            StopGather();
                            SwitchStatus(OperStatus.None);
                            return;
                        }
                    }
                }
                else
                {
                    OperTipsEvent.Invoke(OperStatus.System, $"集中任务结束，没有登录");
                    NativeLog.Info("集中任务结束，没有登录");
                    StopGather();
                    SwitchStatus(OperStatus.None);
                    return;
                }

                await GoHome(1500);

                do
                {
                    NativeLog.Info($"GatherMission index{index}");
                    if (index >= gMission.MissionCount)
                    {
                        OperTipsEvent.Invoke(OperStatus.System, $"集中任务结束{_count}/{gMission.MissionCount}");
                        success = true;
                        break;
                    }

                    OperTipsEvent.Invoke(OperStatus.System, $"集中任务{index + 1}/{gMission.MissionCount}");

                    if (!IsWorking)
                    {
                        OperTipsEvent.Invoke(OperStatus.System, $"集中任务停止");
                        success = true;
                        break;
                    }

                    var mission = gMission.GetMission(index);

                    NativeLog.Info($"mission {JsonConvert.SerializeObject(mission)}");

                    if (lastErr)
                    {
                        Reload();
                        NativeLog.Info($"重载");
                    }

                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasTutorial(source, mHtmlParser))
                    {
                        NativeLog.Info($"存在错误");
                        OperTipsEvent.Invoke(OperStatus.System, $"存在错误");
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                    }

                    // 切换舰队页面
                    GoFleetPage();
                    NativeLog.Info($"切换舰队");
                    source = await GetFrameSourceAsync();
                    if (index <= 1 && HtmlParser.IsWechatCodePage(source))
                    {
                        NativeLog.Info($"在微信验证页");
                        OperTipsEvent.Invoke(OperStatus.System, $"在微信验证页");
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
                    source = await GetFrameSourceAsync();
                    if (!source.Contains("id=\"fleetdelaybox\""))
                    {
                        GoFleetPage();
                        source = await GetFrameSourceAsync();
                    }

                    if (!HtmlParser.ParseFleetQueue(source, out FleetQueue fq))
                    {
                        _nextFunc(false);
                        NativeLog.Info($"解析航道队列有误");
                        OperTipsEvent.Invoke(OperStatus.System, $"解析航道队列有误");
                        continue;
                    }

                    if (fq.Count >= fq.MaxCount)
                    {
                        NativeLog.Info($"航道已满");
                        OperTipsEvent.Invoke(OperStatus.System, $"航道已满");
                        success = true;
                        break;
                    }

                    // 派遣任务
                    bool flag = true;
                    for (var i = 0; i < mission.FleetList.Count; i++)
                    {
                        var fleet = mission.FleetList[i];
                        var shipId = Ship.GetShipId(fleet.ShipType);
                        var count = fleet.Count;
                        if (HtmlParser.ParseShip(source, shipId, out int total))
                        {
                            NativeLog.Info($"shipId{shipId} count{count}");
                            count = count <= 0 ? total : count;
                            if (total >= count && count > 0)
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
                        NativeLog.Info($"没有大型运输机");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"没有大型运输机");
                        _nextFunc(true);
                        continue;
                    }
                    await Task.Delay(500);

                    // 继续
                    FrameRunJs(NativeScript.SetShipNext());
                    await Task.Delay(1500);

                    source = await GetFrameSourceAsync();

                    // 设置目标点
                    FrameRunJs(NativeScript.SetTarget(mission.X, mission.Y, mission.Z, (int)PlanetType.Moon));
                    await Task.Delay(500);
                    source = await GetFrameSourceAsync();

                    // 设置目标继续
                    FrameRunJs(NativeScript.SetTargetNext());
                    await Task.Delay(1500);

                    source = await GetFrameSourceAsync();

                    // 运输
                    FrameRunJs(NativeScript.SetTransfer());
                    await Task.Delay(300);

                    FrameRunJs(NativeScript.SetMaxResource(1));
                    await Task.Delay(100);
                    FrameRunJs(NativeScript.SetMaxResource(2));
                    await Task.Delay(100);
                    FrameRunJs(NativeScript.SetMaxResource(3));
                    await Task.Delay(100);

                    var confirmType = HtmlParser.AttackConfirmType(source);
                    // 确认
                    FrameRunJs(NativeScript.SetAttackConfirm(confirmType));
                    await Task.Delay(1500);

                    // 查看结果
                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasFleetSuccess(source, mExpedition.Parser))
                    {
                        NativeLog.Info($"派遣成功");
                        OperTipsEvent.Invoke(OperStatus.Pirate, $"派遣成功");
                        _nextFunc(true);
                        _count++;
                        continue;
                    }

                    if (HtmlParser.HasTutorial(source, mExpedition.Parser))
                    {
                        NativeLog.Info($"派遣错误");
                        OperTipsEvent.Invoke(OperStatus.System, $"派遣错误");
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                    }

                    _nextFunc(false);
                } while (true);
            }
            catch (Exception ex)
            {
                NativeLog.Error($"DoGather catch {ex.Message}");
            }

            OperTipsEvent.Invoke(OperStatus.System, $"集中任务结束{_count}/{gMission.MissionCount}");
            NativeLog.Info($"集中任务结束{_count}-{success}");

            LastGatherTime = DateTime.Now;
            StopGather();
            SwitchStatus(OperStatus.None);
        }

        internal void StartDetect(int x, int y, int count)
        {
            SwitchStatus(OperStatus.System);
            IsDetectWorking = true;
            IsWorking = true;
            Task.Run(() =>
            {
                DoDetect(x, y, count);
            });
        }

        internal void StopDetect()
        {
            IsDetectWorking = false;
            if (!IsWorking) return;
            IsWorking = false;
        }

        protected async void DoDetect(int x, int y, int count)
        {
            string source = "";

            try
            {
                OperTipsEvent.Invoke(OperStatus.System, $"开始侦查任务{x}:{y}-{count}");
                NativeLog.Info($"开始侦查{x}:{y}-{count}");

                if (x <= 0 || x > 9 || y <= 0 || y > 499 || count <= 0)
                {
                    OperTipsEvent.Invoke(OperStatus.System, $"侦查任务不符合格式");
                    NativeLog.Info("侦查任务不符合格式");
                    StopDetect();
                    SwitchStatus(OperStatus.None);
                    return;
                }

                Reload();

                if (HtmlParser.IsGameUrl(MyAddress))
                {
                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasTutorial(source))
                    {
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                        source = await GetFrameSourceAsync();
                        if (!HtmlParser.IsInGame(source))
                        {
                            OperTipsEvent.Invoke(OperStatus.System, $"侦查任务，没有登录");
                            NativeLog.Info("侦查任务，没有登录");
                            StopDetect();
                            SwitchStatus(OperStatus.None);
                            return;
                        }
                    }
                }
                else
                {
                    OperTipsEvent.Invoke(OperStatus.System, $"集中任务结束，没有登录");
                    NativeLog.Info("集中任务结束，没有登录");
                    StopDetect();
                    SwitchStatus(OperStatus.None);
                    return;
                }

                FrameRunJs(NativeScript.ToGalaxy());
                Thread.Sleep(1500);

                int _x = x;
                int _y = y - 1;

                for (int i = 0; i < count; i++)
                {
                    _y++;
                    if (_y > 499)
                    {
                        _y = 1;
                        _x++;
                    }

                    if (_x <= 9)
                    {
                        await DoDetectTask(_x, _y);
                    }
                }
            }
            catch (Exception ex)
            {
                NativeLog.Error($"DoDetect catch {ex.Message}");
            }

            StopDetect();
            SwitchStatus(OperStatus.None);
        }

        private async Task DoDetectTask(int x, int y, bool isRetry = false)
        {
            try
            {
                await Task.Delay(1000);
                if (!IsDetectWorking) return;

                NativeLog.Info($"DoDetectTask {x}:{y}-{isRetry}");

                string source = await GetFrameSourceAsync();
                if (HtmlParser.HasTutorial(source))
                {
                    FrameRunJs(NativeScript.TutorialConfirm());
                    await Task.Delay(1500);
                    source = await GetFrameSourceAsync();
                }

                // if not in galaxy page, go to galaxy page
                if (!HtmlParser.IsGalaxyPage(source))
                {
                    FrameRunJs(NativeScript.ToGalaxy());
                    Thread.Sleep(1500);
                }

                FrameRunJs(NativeScript.RefreshGalaxy(x, y));
                await Task.Delay(200);
                FrameRunJs(NativeScript.RefreshGalaxySubmit());
                await Task.Delay(1500);
                source = await GetFrameSourceAsync();
                var mat = Regex.Match(source, $@"太阳系 {x}:{y}");
                if (!mat.Success)
                {
                    if (!isRetry)
                    {
                        await DoDetectTask(x, y, true);
                    }

                    return;
                }

                var ret = HtmlParser.ParseGalaxyDelectCount(source, out int count);
                if (!ret)
                {
                    if (!isRetry)
                    {
                        await DoDetectTask(x, y, true);
                    }

                    return;
                }

                NativeLog.Info($"DoDetectTask count:{count}");
                for (int i = 0; i < count; i++)
                {
                    if (!IsDetectWorking) return;

                    NativeLog.Info($"DoDetectTask count:{count} i: {i}");
                    FrameRunJs(NativeScript.SpyGalaxyDetect(i));
                    await Task.Delay(1000);
                }
            }
            catch(Exception ex)
            {
                NativeLog.Error($"DoDetectTask catch {ex.Message}");
            }
        }

        internal void StartOneClickExpedition()
        {
            IsWorking = true;
            SwitchStatus(OperStatus.System);
            Task.Run(() =>
            {
                DoOneClickExpedition();
            });
        }

        protected async void DoOneClickExpedition()
        {
            string source = "";

            try
            {
                OperTipsEvent.Invoke(OperStatus.System, $"开始一键远征");
                NativeLog.Info("开始一键远征");

                Reload();

                if (HtmlParser.IsGameUrl(MyAddress))
                {
                    source = await GetFrameSourceAsync();
                    if (HtmlParser.HasTutorial(source))
                    {
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                        source = await GetFrameSourceAsync();
                        if (!HtmlParser.IsInGame(source))
                        {
                            OperTipsEvent.Invoke(OperStatus.System, $"一键远征结束，没有登录");
                            NativeLog.Info("一键远征结束，没有登录");
                            return;
                        }
                    }
                }
                else
                {
                    OperTipsEvent.Invoke(OperStatus.System, $"一键远征结束，没有登录");
                    NativeLog.Info("一键远征结束，没有登录");
                    return;
                }

                await GoHome(1500);

                /*Reload();
                NativeLog.Info($"重载");*/

                source = await GetFrameSourceAsync();
                if (HtmlParser.HasTutorial(source, mHtmlParser))
                {
                    NativeLog.Info($"存在错误");
                    OperTipsEvent.Invoke(OperStatus.System, $"存在错误");
                    FrameRunJs(NativeScript.TutorialConfirm());
                    await Task.Delay(1500);
                }

                // 切换舰队页面
                GoFleetPage();
                NativeLog.Info($"切换舰队");
                source = await GetFrameSourceAsync();
                if (HtmlParser.IsWechatCodePage(source))
                {
                    NativeLog.Info($"在微信验证页");
                    OperTipsEvent.Invoke(OperStatus.System, $"在微信验证页");
                    return;
                }

                // 一键远征
                FrameRunJs(NativeScript.OnClickExpedition());
                await Task.Delay(1500);

                source = await GetFrameSourceAsync();

                // 一键多维拉资源
                FrameRunJs(NativeScript.OnClickDWExpedition());
                await Task.Delay(1500);

                // 查看结果
                source = await GetFrameSourceAsync();
                if (HtmlParser.HasTutorial(source, mHtmlParser))
                {
                    NativeLog.Info($"一键远征结束错误");
                    OperTipsEvent.Invoke(OperStatus.System, $"一键远征结束");
                    FrameRunJs(NativeScript.TutorialConfirm());
                    await Task.Delay(1500);
                }
            }
            catch (Exception ex)
            {
                NativeLog.Error($"DoOneClickExpedition catch {ex.Message}");
            }
            finally
            {
                OperTipsEvent.Invoke(OperStatus.System, $"一键远征结束");
                NativeLog.Info($"一键远征结束");
                LastOneClickExpeditionTime = DateTime.Now;
                SwitchStatus(OperStatus.None);
            }
        }
    }
}
