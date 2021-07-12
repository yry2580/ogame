using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Html.Parser;
using CefSharp;
using CefSharp.WinForms;
using Newtonsoft.Json;

namespace feeling
{
    public delegate void DelegateStatusChange();
    public delegate void DelegateScanGalaxy(string sDesc, string pDesc, string uDesc);
    public delegate void DelegatePlanet();
    public delegate void DelegateOperTips(OperStatus operStatus, string tips);

    class NativeController: Singleton<NativeController>
    {
        public OperStatus MyOperStatus = OperStatus.None;
        public volatile bool IsWorking = false;

        public string MyAddress = "";
        public DelegateStatusChange StatusChangeEvent;
        public DelegateScanGalaxy ScanGalaxyEvent;
        public DelegatePlanet PlanetEvent;
        public DelegateOperTips OperTipsEvent;

        public ChromiumWebBrowser MyWebBrowser;

        Galaxy mGalaxy = new Galaxy();
        Planet mPlanet = new Planet();
        Expedition mExpedition = new Expedition();
        string mScanDesc = "";
        string mScanPage = "";
        string mScanUniverse = "";

        public Planet MyPlanet => mPlanet;
        public static User User = new User();


        // auto
        public bool IsAutoExpedition = false;
        public DateTime LastExeditionTime = DateTime.Now;

        public void HandleWebBrowserFrameEnd(string url)
        {
            MyAddress = MyWebBrowser.Address;

            if (url.Contains("ogame/frames.php"))
            {
                ScanPlanet();
            }
        }

        public void ScanPlanet()
        {
            Task.Run(async() =>
            {
                try
                {
                    var source = await GetHauptframe()?.GetSourceAsync();
                    if (!mPlanet.HasData)
                    {
                        mPlanet.Parse(source);
                        PlanetEvent?.Invoke();
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
                MessageBox.Show("请先登录");
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
                        MessageBox.Show("刷图完成");
                        break;
                    }

                    mScanDesc = $"刷图次数：{count}";
                    if (count > 1498)
                    {
                        MessageBox.Show("刷图次数过多，建议换号");
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
                            MessageBox.Show("刷图异常，检测下再开始");
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
            if (account.Length <= 0 || psw.Length <= 0 || universe <= 0 || universe > 24)
            {
                MessageBox.Show("请输入正确的账号、密码或宇宙");
                return;
            }

            if (HtmlUtil.IsGameUrl(MyAddress))
            {
                var source = await GetHauptframe().GetSourceAsync();
                if (HtmlUtil.IsInGame(source))
                {
                    MessageBox.Show("已经是登录状态");
                    return;
                }

                await DoLoginAsync(account, psw, universe);
            } else
            {
                await DoLoginAsync(account, psw, universe);
            }

            await Task.Delay(1000);
            await GoHome();
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
            if (!HtmlUtil.IsGameUrl(MyAddress))
            {
                MessageBox.Show("退出失败，可能不在游戏页");
                return;
            }

            if (OperStatus.Galaxy == MyOperStatus)
            {
                MessageBox.Show("当前正在刷图，不建议退出");
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
                MessageBox.Show("退出失败，无退出按钮");
                return;
            }

            FrameRunJs(NativeScript.ToLogout());
            await Task.Delay(500);
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

            OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:G}|开始探险");
            Task.Run(() =>
            {
                DoExpedition(exMission);
            });
        }

        protected async void DoExpedition(ExMission exMission)
        {
            int index = 0;
            int _count = 0;
            bool lastErr = false;
            string source = "";

            var errFunc = new Action(() =>
            {
                if (lastErr)
                {
                    lastErr = false;
                    index++;
                }
                else
                {
                    lastErr = true;
                }
            });

            await GoHome(2000);

            try
            {
                do
                {

                    Console.WriteLine($"ex-mession index{index}");
                    OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:G}|探险{index+1}");

                    if (index >= exMission.List.Count)
                    {
                        if (!IsAutoExpedition)
                        {
                            MessageBox.Show($"探险派出结束，请检测是否成功{_count}/{exMission.List.Count}");
                        }
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:G}|探险派出结束{_count}/{exMission.List.Count}");
                        break;
                    }

                    var mission = exMission.GetMission(index);

                    Console.WriteLine($"mission {JsonConvert.SerializeObject(mission)}");

                    source = await GetHauptframe().GetSourceAsync();
                    if (HtmlUtil.HasTutorial(source, mExpedition.Parser))
                    {
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:G}|存在错误");
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
                        errFunc();
                        continue;
                    }
                    FrameRunJs(NativeScript.SelectPlanet(idx));
                    await Task.Delay(1500);

                    // 查看舰队队列
                    source = await GetHauptframe().GetSourceAsync();
                    if (!mExpedition.ParseFleetQueue(source, out FleetQueue fq))
                    {
                        errFunc();
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:G}|解析探险队列有误");
                        continue;
                    }

                    if (fq.ExCount >= fq.ExMaxCount)
                    {
                        if (!IsAutoExpedition)
                        {
                            MessageBox.Show("探险队列已满");
                        }
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:G}|探险队列已满");
                        break;
                    }

                    if (fq.Count >= fq.MaxCount)
                    {
                        if (!IsAutoExpedition)
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
                        if (mExpedition.ParseShip(source, shipId, out int total))
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
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:G}|没有足够舰队数");
                        errFunc();
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
                        index++;
                        _count++;
                        continue;
                    }

                    if (HtmlUtil.HasTutorial(source, mExpedition.Parser))
                    {
                        OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:G}|派遣错误");
                        FrameRunJs(NativeScript.TutorialConfirm());
                        await Task.Delay(1500);
                    }

                    errFunc();
                } while (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DoExpedition catch {ex.Message}");
            }

            OperTipsEvent.Invoke(OperStatus.Expedition, $"{DateTime.Now:G}|探险派遣结束{_count}/{exMission.List.Count}");

            // 如果存在派遣成功的
            if (_count >= 0)
            {
                LastExeditionTime = DateTime.Now;
            }

            SwitchStatus(OperStatus.None);
        }
        #endregion

        protected void SwitchStatus(OperStatus operStatus)
        {
            if (MyOperStatus == operStatus) return;

            MyOperStatus = operStatus;

            StatusChangeEvent?.Invoke();
        }

        public async Task GoHome(int delay = 500)
        {
            FrameRunJs(NativeScript.ToHome());
            await Task.Delay(delay);
        }
    }
}
