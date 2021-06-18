using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace feeling
{
    public partial class MainForm : Form
    {
        ChromiumWebBrowser mWebBrowser;

        public MainForm()
        {
            InitBrowser();
            InitializeComponent();

            if (!Directory.Exists(NativeConst.FileDirectory))
            {
                Directory.CreateDirectory(NativeConst.FileDirectory);
            }

            NativeController.Instance.StatusChangeEvent += OnStatusChange;
            NativeController.Instance.ScanGalaxyEvent += OnScanGalaxyEvent;
        }

        protected void InitWebHandler()
        {
            mWebBrowser.JsDialogHandler = new JsDialogHandler();
            mWebBrowser.LifeSpanHandler = new OpenPageSelf();
        }

        protected void InitBrowser()
        {
            var settings = new CefSettings();
            settings.Locale = "zh-CN";
            Cef.Initialize(settings);
            mWebBrowser = new ChromiumWebBrowser(NativeConst.Homepage);

            mWebBrowser.Dock = DockStyle.Fill;
            mWebBrowser.FrameLoadStart += Web_OnFrameStart;
            mWebBrowser.FrameLoadEnd += Web_OnFrameEnd;

            NativeController.Instance.MyWebBrowser = mWebBrowser;
            InitWebHandler();
        }

        private void OnStatusChange()
        {
            Invoke(new Action(() => {
                Redraw();
            }));
        }

        private void OnScanGalaxyEvent(string sDesc, string pDesc, string uDesc)
        {
            Invoke(new Action(() => {
                w_galaxy_status.Text = sDesc;
                w_galaxy_page.Text = pDesc;
                w_galaxy_universe.Text = uDesc;
            }));
        }

        protected void Redraw()
        {
            try
            {
                var operStatus = NativeController.Instance.MyOperStatus;

                switch (operStatus)
                {
                    case OperStatus.Galaxy:
                        btn_galaxy_start.Enabled = false;
                        btn_galaxy_stop.Enabled = true;
                        break;
                    case OperStatus.None:
                    default:
                        btn_galaxy_start.Enabled = true;
                        btn_galaxy_stop.Enabled = false;
                        break;
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine($"Redraw catch {ex.Message}");
            }

        }

        private void Web_OnFrameEnd(object sender, FrameLoadEndEventArgs e)
        {
            Console.WriteLine($"Web_OnFrameEnd", e.Url);
            NativeController.Instance.HandleWebBrowserFrameEnd(e.Url);
        }

        private void Web_OnFrameStart(object sender, FrameLoadStartEventArgs e)
        {
            Console.WriteLine("Web_OnFrameStart");
#if Debug
            mWebBrowser.GetBrowser().ShowDevTools();
#endif
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            w_split_container.Panel1.Controls.Add(mWebBrowser);

            Redraw();
        }

        private void btn_galaxy_open_Click(object sender, EventArgs e)
        {
            Process.Start(NativeConst.FileDirectory);
        }

        private void btn_galaxy_start_Click(object sender, EventArgs e)
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                MessageBox.Show("当前不是空闲状态，不能刷图");
                return;
            }

            NativeController.Instance.StartScanGalaxy();

            Redraw();
        }

        private void btn_galaxy_stop_Click(object sender, EventArgs e)
        {
            if (OperStatus.Galaxy != NativeController.Instance.MyOperStatus)
            {
                MessageBox.Show("当前不是刷图状态");
                return;
            }

            NativeController.Instance.StopScanGalaxy();
            Redraw();
        }

        private void btn_galaxy_save_Click(object sender, EventArgs e)
        {
            if (OperStatus.Galaxy == NativeController.Instance.MyOperStatus)
            {
                MessageBox.Show("当前正在刷图，暂不能保存");
                return;
            }

            NativeController.Instance.SaveGalaxy();
            Redraw();
        }
    }
}
