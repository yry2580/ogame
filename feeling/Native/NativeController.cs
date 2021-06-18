using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        public string MyUrl = "";
        public DelegateStatusChange StatusChangeEvent;
        public DelegateScanGalaxy ScanGalaxyEvent;
        public ChromiumWebBrowser MyWebBrowser;

        Galaxy mGalaxy = new Galaxy();
        string mScanDesc = "";
        string mScanPage = "";
        string mScanUniverse = "";

        public void HandleWebBrowserFrameEnd(string url)
        {
            MyUrl = url;
        }

        public void RunJs(string jsCode)
        {
            MyWebBrowser?.ExecuteScriptAsync(jsCode);
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
            var mat = Regex.Match(MyUrl, $@"u(?<universe>\S*).cicihappy.com");
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
    }
}
