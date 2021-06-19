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

namespace feeling
{
    public delegate void DelegateStatusChange();
    public delegate void DelegateScanGalaxy(string sDesc, string pDesc, string uDesc);

    class NativeController: Singleton<NativeController>
    {
        public OperStatus MyOperStatus = OperStatus.None;
        public volatile bool IsWorking = false;

        public string MyAddress = "";
        public DelegateStatusChange StatusChangeEvent;
        public DelegateScanGalaxy ScanGalaxyEvent;
        public ChromiumWebBrowser MyWebBrowser;

        Galaxy mGalaxy = new Galaxy();
        string mScanDesc = "";
        string mScanPage = "";
        string mScanUniverse = "";

        public static User User = new User();

        public void HandleWebBrowserFrameEnd(string url)
        {
            MyAddress = MyWebBrowser.Address;
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

        public void StartScanGalaxy()
        {
            if (IsWorking) return;
            IsWorking = true;

            SwitchStatues(OperStatus.Galaxy);

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
            SwitchStatues(OperStatus.None);
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

        protected void SwitchStatues(OperStatus operStatus)
        {
            if (MyOperStatus == operStatus) return;

            MyOperStatus = operStatus;

            StatusChangeEvent?.Invoke();
        }

        protected void FireScanGalaxy()
        {
            ScanGalaxyEvent?.Invoke(mScanDesc, mScanPage, mScanUniverse);
        }

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
                FrameRunJs(NativeScript.ToHome());
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
    }
}
