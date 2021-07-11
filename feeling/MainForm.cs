using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using Newtonsoft.Json;

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
            NativeController.Instance.PlanetEvent += OnPlanetEvent;

            InitData();
        }

        private void OnPlanetEvent()
        {
            Invoke(new Action(() => {
                RedrawPlanet();
            }));
        }

        protected void InitData()
        {
            w_user_account.Text = NativeController.User.Account;
            w_user_password.Text = NativeController.User.Password;
            w_user_universe.Text = NativeController.User.Universe.ToString();

            InitExpedition();
            RedrawAccount();
        }

        protected void InitExpedition()
        {
            var exShipOptions = Expedition.GetShipOptions();

            tx0_ship0_cb.Items.Clear();
            tx0_ship1_cb.Items.Clear();
            tx1_ship0_cb.Items.Clear();
            tx1_ship1_cb.Items.Clear();
            tx2_ship0_cb.Items.Clear();
            tx2_ship1_cb.Items.Clear();
            tx3_ship0_cb.Items.Clear();
            tx3_ship1_cb.Items.Clear();
            exShipOptions.ForEach(e =>
            {
                tx0_ship0_cb.Items.Add(e);
                tx0_ship1_cb.Items.Add(e);
                tx1_ship0_cb.Items.Add(e);
                tx1_ship1_cb.Items.Add(e);
                tx2_ship0_cb.Items.Add(e);
                tx2_ship1_cb.Items.Add(e);
                tx3_ship0_cb.Items.Add(e);
                tx3_ship1_cb.Items.Add(e);
            });

            tx0_ship0_cb.SelectedIndex = 0;
            tx0_ship1_cb.SelectedIndex = 0;
            tx1_ship0_cb.SelectedIndex = 0;
            tx1_ship1_cb.SelectedIndex = 0;
            tx2_ship0_cb.SelectedIndex = 0;
            tx2_ship1_cb.SelectedIndex = 0;
            tx3_ship0_cb.SelectedIndex = 0;
            tx3_ship1_cb.SelectedIndex = 0;

            RevertCfg();
        }

        protected bool RevertCfg()
        {
            // 读取配置
            Expedition.ReadCfg();

            var exMissionCfg = Expedition.MyExMissionCfg;

            if (null == exMissionCfg) return false;

            var planetList = NativeController.Instance.MyPlanet.List;
            var exOptions = Expedition.ShipOptions;

            for (var i = 0; i < 4; i++)
            {
                if (i >= exMissionCfg.List.Count)
                {
                    (Controls.Find($"tx{i}_ship0", true)[0] as TextBox).Text = "";
                    (Controls.Find($"tx{i}_ship1", true)[0] as TextBox).Text = "";
                    continue;
                }

                var mission = exMissionCfg.GetMission(i);
                var idx = planetList.FindIndex(e => e == mission.PlanetName);
                if (idx != -1)
                {
                    (Controls.Find($"tx{i}_planet", true)[0] as ComboBox).SelectedIndex = idx;
                }

                (Controls.Find($"tx{i}_ship0", true)[0] as TextBox).Text = "";
                (Controls.Find($"tx{i}_ship1", true)[0] as TextBox).Text = "";

                var count = mission.FleetList.Count;
                var fleet = mission.FleetList[0];
                idx = exOptions.FindIndex(e => e == fleet.ShipType);

                (Controls.Find($"tx{i}_ship0_cb", true)[0] as ComboBox).SelectedIndex = idx < 0 ? 0 : idx;
                (Controls.Find($"tx{i}_ship0", true)[0] as TextBox).Text = fleet.Count.ToString();

                if (mission.FleetList.Count > 1)
                {
                    fleet = mission.FleetList[1];
                    idx = exOptions.FindIndex(e => e == fleet.ShipType);
                    (Controls.Find($"tx{i}_ship1_cb", true)[0] as ComboBox).SelectedIndex = idx < 0 ? 0 : idx;
                    (Controls.Find($"tx{i}_ship1", true)[0] as TextBox).Text = fleet.Count.ToString();
                }
            }

            return true;
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
            
            /*settings.CefCommandLineArgs.Add("disable-gpu", "1");
            settings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
            settings.CefCommandLineArgs.Add("enable-begin-frame-scheduling", "1");
            settings.CefCommandLineArgs.Add("disable-gpu-vsync", "1"); //Disable Vsync*/

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
                        btn_tx_start.Enabled = false;
                        btn_tx_revert.Enabled = false;
                        break;
                    case OperStatus.Expedition:
                        btn_galaxy_start.Enabled = false;
                        btn_galaxy_stop.Enabled = false;
                        btn_tx_start.Enabled = false;
                        btn_tx_revert.Enabled = false;
                        break;
                    case OperStatus.None:
                    default:
                        btn_galaxy_start.Enabled = true;
                        btn_galaxy_stop.Enabled = false;
                        btn_tx_start.Enabled = true;
                        btn_tx_revert.Enabled = true;
                        break;
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine($"Redraw catch {ex.Message}");
            }

        }

        protected void RedrawPlanet()
        {
            var lists = NativeController.Instance.MyPlanet.List;

            if (lists.Count <= 0)
            {
                NativeController.Instance.ScanPlanet();
                return;
            }

            tx0_planet.Items.Clear();
            tx1_planet.Items.Clear();
            tx2_planet.Items.Clear();
            tx3_planet.Items.Clear();

            lists.ForEach(e =>
            {
                if (string.IsNullOrWhiteSpace(e)) return;
                tx0_planet.Items.Add(e);
                tx1_planet.Items.Add(e);
                tx2_planet.Items.Add(e);
                tx3_planet.Items.Add(e);

                tx0_planet.SelectedIndex = 0;
                tx1_planet.SelectedIndex = 0;
                tx2_planet.SelectedIndex = 0;
                tx3_planet.SelectedIndex = 0;
            });
        }


        public void SetUserButton(bool enabled)
        {
            w_user_account.Enabled = enabled;
            btn_user_login.Enabled = enabled;
            btn_user_logout.Enabled = enabled;
        }

        protected void RedrawAccount()
        {
            var lists = NativeController.User.GetAccountLists();
            lists.Sort();

            w_user_account.Items.Clear();

            lists.ForEach(e =>
            {
                if (string.IsNullOrWhiteSpace(e)) return;
                w_user_account.Items.Add(e);
            });
        }

        private void Web_OnFrameEnd(object sender, FrameLoadEndEventArgs e)
        {
            Console.WriteLine($"Web_OnFrameEnd {e.Url}");
            NativeController.Instance.HandleWebBrowserFrameEnd(e.Url);
        }

        private void Web_OnFrameStart(object sender, FrameLoadStartEventArgs e)
        {
            Console.WriteLine("Web_OnFrameStart");
#if DEBUG
            // mWebBrowser.GetBrowser().ShowDevTools();
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

        private void w_count_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private async void btn_user_login_Click(object sender, EventArgs e)
        {
            var account = w_user_account.Text.Trim();
            var psw = w_user_password.Text.Trim();
            var str = w_user_universe.Text.Trim();
            int universe = str.Length <= 0 ? 0 : int.Parse(str);

            SetUserButton(false);
            await NativeController.Instance.LoginAsync(account, psw, universe);
            SetUserButton(true);
            RedrawAccount();
        }

        private async void btn_user_logout_Click(object sender, EventArgs e)
        {
            SetUserButton(false);
            await NativeController.Instance.LogoutAsync();
            SetUserButton(true);
        }

        private void w_user_account_SelectedIndexChanged(object sender, EventArgs e)
        {
            var psw = NativeController.User.GetPassword(w_user_account.Text.Trim());
            if (string.IsNullOrEmpty(psw)) return;
            w_user_password.Text = psw;
        }
        
        private ExMission GetExMission()
        {
            ExMission exMission = new ExMission();
            exMission.Add(
                tx0_ship0_cb.SelectedIndex,
                tx0_ship0.Text.Trim(),
                tx0_ship1_cb.SelectedIndex,
                tx0_ship1.Text.Trim(),
                tx0_planet.Text.Trim()
            );
            exMission.Add(
                tx1_ship0_cb.SelectedIndex,
                tx1_ship0.Text.Trim(),
                tx1_ship1_cb.SelectedIndex,
                tx1_ship1.Text.Trim(),
                tx1_planet.Text.Trim()
            );
            exMission.Add(
                tx2_ship0_cb.SelectedIndex,
                tx2_ship0.Text.Trim(),
                tx2_ship1_cb.SelectedIndex,
                tx2_ship1.Text.Trim(),
                tx2_planet.Text.Trim()
            );
            exMission.Add(
                tx3_ship0_cb.SelectedIndex,
                tx3_ship0.Text.Trim(),
                tx3_ship1_cb.SelectedIndex,
                tx3_ship1.Text.Trim(),
                tx3_planet.Text.Trim()
            );
            return exMission;
        }
        private void btn_tx_start_Click(object sender, EventArgs e)
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                MessageBox.Show("当前正在忙，暂不能操作");
                return;
            }

            var exMission = GetExMission();

            if (exMission.List.Count <= 0)
            {
                MessageBox.Show("探险任务配置无效，请检测后再开始");
                return;
            }

            NativeController.Instance.StartExpedition(exMission);
            Redraw();
        }

        private void btn_tx_save_Click(object sender, EventArgs e)
        {
            var exMission = GetExMission();

            if (exMission.List.Count <= 0)
            {
                MessageBox.Show("探险任务配置无效，请检测后再保存");
            }

            Expedition.Save(exMission);
        }

        private void btn_tx_revert_Click(object sender, EventArgs e)
        {
            if (!RevertCfg())
            {
                MessageBox.Show("读取配置失败");
            }
        }
    }
}
