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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using Newtonsoft.Json;
#if !NET45
using OgameService;
#endif

namespace feeling
{
    public partial class MainForm : Form
    {
        ChromiumWebBrowser mWebBrowser;

        Thread mThread;
        bool mAutoExpedition = false;
        bool mAutoPirate = false;
        bool mAutoPirateLogin = false;
        int mPirateInterval = 120; // 分
#if !NET45
        OgClient mClient;
#endif
        string mLastContent = "";

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
            NativeController.Instance.OperTipsEvent += OnOperTipsChange;
            NativeController.Instance.NpcChangeEvent += OnNpcChange;

            InitData();
        }

        private void OnNpcChange()
        {
            Invoke(new Action(() => {
                RedrawNpc();
            }));
        }

        private void LookBackThread()
        {
            do
            {
                Invoke(new Action(() =>
                {
                    _LookThread();
                }));
                Thread.Sleep(1000 * 60);
            } while (true);
        }

        private void _LookThread()
        {
            try
            {
                if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

                doAutoExpedtion();
                doAutoPirate();
                SendData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"_LookThread {ex.Message}");
            }
        }

        private void OnOperTipsChange(OperStatus operStatus, string tips)
        {
            Invoke(new Action(() => {
                RedrawOperTips(operStatus, tips);
            }));
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

            // 默认值
            lb_hd_interval.Text = mPirateInterval.ToString();
            rbtn_cfg0.Checked = true;

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

        private void RedrawNpc()
        {
            var npcList = PirateUtil.NpcList;
            if (npcList.Count <= 0) return;

            for (int i = 0; i < 5; i++)
            {
                PirateControl control = Controls.Find($"w_pirate{i}", true)[0] as PirateControl;
                if (null == control) continue;

                var list = npcList.Where(e => e.StartsWith($"[{i+1}")).ToList();
                control.SetAllOptions(list);
            }
        }

        protected bool PirateCfg()
        {
            var idx = rbtn_cfg1.Checked ? 1 : 0;

            // 读取配置
            if (!PirateUtil.ReadCfg(idx)) return false;

            var pMissionCfg = PirateUtil.MyPirateMission;

            if (null == pMissionCfg) return false;

            var planetList = NativeController.Instance.MyPlanet.List;
            var npcList = PirateUtil.NpcList;

            var list = pMissionCfg.List;

            for (int i = 0; i < 5; i++)
            {
                PirateControl control = Controls.Find($"w_pirate{i}", true)[0] as PirateControl;
                if (null == control) continue;

                var pirate = i < list.Count ? list[i] : new Pirate();

                var _list = npcList.Where(e => e.StartsWith($"[{i + 1}")).ToList();
                control.SetAllOptions(_list);
                control.SetPlanets(planetList);

                control.MyCount = pirate.Count;
                control.MyMode = pirate.Mode;
                control.MyPlanet = pirate.PlanetName;
                control.MyOptions = pirate.Options;
            }

            var interval = pMissionCfg.Interval;
            mAutoPirateLogin = pMissionCfg.AutoLogin;
            mPirateInterval = interval < 120 ? 120 : interval;
            lb_hd_interval.Text = mPirateInterval.ToString();

            return true;
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
            settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.190 Safari/537.36";
            settings.CefCommandLineArgs.Add("disable-gpu", "1");
            settings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
            settings.CefCommandLineArgs.Add("enable-begin-frame-scheduling", "1");
            settings.CefCommandLineArgs.Add("disable-gpu-vsync", "1");

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

                if (sDesc.Length > 0)
                {
                    mLastContent = sDesc;
                }

                SendData();
            }));
        }

        protected void Redraw()
        {
            try
            {
                var operStatus = NativeController.Instance.MyOperStatus;

                switch (operStatus)
                {
                    case OperStatus.System:
                        btn_galaxy_start.Enabled = false;
                        btn_galaxy_stop.Enabled = false;
                        SetExpeditionButton(false);
                        SetPirateButton(false);
                        break;
                    case OperStatus.Galaxy:
                        btn_galaxy_start.Enabled = false;
                        btn_galaxy_stop.Enabled = true;
                        SetExpeditionButton(false);
                        SetPirateButton(false);
                        break;
                    case OperStatus.Expedition:
                        btn_galaxy_start.Enabled = false;
                        btn_galaxy_stop.Enabled = false;
                        SetExpeditionButton(false, true);
                        SetPirateButton(false);
                        break;
                    case OperStatus.Pirate:
                        btn_galaxy_start.Enabled = false;
                        btn_galaxy_stop.Enabled = false;
                        SetExpeditionButton(false);
                        SetPirateButton(false, true);
                        break;
                    case OperStatus.None:
                    default:
                        btn_galaxy_start.Enabled = true;
                        btn_galaxy_stop.Enabled = false;
                        SetExpeditionButton(true);
                        SetPirateButton(true);
                        break;
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine($"Redraw catch {ex.Message}");
            }

            SendData();
        }

        protected void RedrawPlanet()
        {
            var lists = NativeController.Instance.MyPlanet.List;

            if (lists.Count <= 0)
            {
                NativeController.Instance.ScanPlanet();
                return;
            }

            var txt0 = tx0_planet.Text;
            var txt1 = tx1_planet.Text;
            var txt2 = tx2_planet.Text;
            var txt3 = tx3_planet.Text;

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
            });

            var idx = Planet.FindPlanet(txt0, lists);
            tx0_planet.SelectedIndex = idx != -1 ? idx : 0;
            
            idx = Planet.FindPlanet(txt1, lists);
            tx1_planet.SelectedIndex = idx != -1 ? idx : 0;
            
            idx = Planet.FindPlanet(txt2, lists);
            tx2_planet.SelectedIndex = idx != -1 ? idx : 0;

            idx = Planet.FindPlanet(txt3, lists);
            tx3_planet.SelectedIndex = idx != -1 ? idx : 0;

            w_pirate0.SetPlanets(lists);
            w_pirate1.SetPlanets(lists);
            w_pirate2.SetPlanets(lists);
            w_pirate3.SetPlanets(lists);
            w_pirate4.SetPlanets(lists);
        }

        private void RedrawOperTips(OperStatus operStatus, string tips)
        {
            switch (operStatus)
            {
                case OperStatus.Expedition:
                    tx_content.Text = tips.Trim();
                    break;
                case OperStatus.Pirate:
                    w_hd_tips.Text = tips.Trim();
                    break;
                default:
                    break;
            }

            if (tips.Trim().Length > 0)
            {
                mLastContent = tips;
            }
            SendData();
        }

        public void SetUserButton(bool enabled)
        {
            w_user_account.Enabled = enabled;
            btn_user_login.Enabled = enabled;
            btn_user_logout.Enabled = enabled;
        }

        public void SetPirateButton(bool enabled, bool canStop = false)
        {
            btn_hd_start.Enabled = enabled;
            btn_hd_save.Enabled = enabled;
            btn_hd_revert.Enabled = enabled;
            btn_hd_refresh.Enabled = enabled;
            cbox_hd_auto.Enabled = enabled;
            w_hd_inverval.Enabled = enabled;
            btn_hd_interval.Enabled = enabled;
            btn_hd_stop.Enabled = !enabled && canStop;
            rbtn_cfg0.Enabled = enabled;
            rbtn_cfg1.Enabled = enabled;
        }

        public void SetExpeditionButton(bool enabled, bool canStop = false)
        {
            btn_tx_start.Enabled = enabled;
            btn_tx_save.Enabled = enabled;
            btn_tx_revert.Enabled = enabled;
            btn_tx_planet.Enabled = enabled;
            cbox_tx_auto.Enabled = enabled;
            btn_tx_stop.Enabled = !enabled && canStop;
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

            mThread = new Thread(LookBackThread);
            mThread.IsBackground = true;
            mThread.Start();
#if !NET45
            mClient = new OgClient();
            mClient.Connected += OnServerConnected;
            mClient.DataReceived += OnServerReceived;
#endif
        }

#if !NET45
        private void OnServerReceived(OgameData data)
        {
            Invoke(new Action(() => {
                DoServerReceived(data);
            }));
        }

        private async void DoServerReceived(OgameData data)
        {
            try
            {
                var operStatus = NativeController.Instance.MyOperStatus;
                var lastContent = mLastContent;
                var hdContent = lb_hd_info.Text.Trim();
                if (OperStatus.None != operStatus)
                {
                    mClient.SendData((StatusEnum)((int)operStatus), lastContent, hdContent);
                    return;
                }
                switch (data.Cmd)
                {
                    case CmdEnum.Login:
                        NativeController.Instance.CanNotify = false;
                        await TryLogin();
                        mClient.SendData((StatusEnum)((int)operStatus), lastContent, hdContent);
                        break;
                    case CmdEnum.Logout:
                        NativeController.Instance.CanNotify = false;
                        await TryLogout();
                        mClient.SendData((StatusEnum)((int)operStatus), lastContent, hdContent);
                        break;
                    case CmdEnum.Pirate:
                        NativeController.Instance.CanNotify = false;
                        doPirate();
                        mClient.SendData((StatusEnum)((int)operStatus), lastContent, hdContent);
                        break;
                    case CmdEnum.Expedition:
                        NativeController.Instance.CanNotify = false;
                        doExpedtion();
                        mClient.SendData((StatusEnum)((int)operStatus), lastContent, hdContent);
                        break;
                    case CmdEnum.GetCode:
                        NativeController.Instance.CanNotify = false;
                        await TryGetAuth();
                        mClient.SendData((StatusEnum)((int)operStatus), lastContent, hdContent);
                        break;
                    case CmdEnum.AuthCode:
                        NativeController.Instance.CanNotify = false;
                        await TryAuthCode(data.Content);
                        mClient.SendData((StatusEnum)((int)operStatus), lastContent, hdContent);
                        break;
                    default:
                        mClient.SendData((StatusEnum)((int)operStatus), lastContent, hdContent);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogUtil.Error($"feeling OnServerReceived catch {ex.Message}");
            }
        }

#endif
        private void SendData()
        {
#if !NET45
            var operStatus = (int)NativeController.Instance.MyOperStatus;
            var status = (StatusEnum)operStatus;
            mClient?.SendData(status, mLastContent, lb_hd_info.Text.Trim(), lb_tx_info.Text.Trim());
#endif
        }

        private void OnServerConnected()
        {
#if !NET45
            var operStatus = (int)NativeController.Instance.MyOperStatus;
            var status = (StatusEnum)operStatus;
            mClient?.SendAuth(status, mLastContent, lb_hd_info.Text.Trim(), lb_tx_info.Text.Trim());
#endif
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
            NativeController.Instance.CanNotify = true;
            await TryLogin();
        }

        private async Task TryLogin()
        {
            try
            {
                if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

                var account = w_user_account.Text.Trim();
                var psw = w_user_password.Text.Trim();
                var str = w_user_universe.Text.Trim();
                int universe = str.Length <= 0 ? 0 : int.Parse(str);

                NativeController.Instance.SwitchStatus(OperStatus.System);

                SetUserButton(false);
                await NativeController.Instance.LoginAsync(account, psw, universe);
            }
            catch(Exception ex)
            {
                LogUtil.Error($"TryLogin catch {ex.Message}");
            }

            SetUserButton(true);
            RedrawAccount();

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private async Task TryGetAuth()
        {
            try
            {
                if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

                var account = w_user_account.Text.Trim();
                var psw = w_user_password.Text.Trim();
                var str = w_user_universe.Text.Trim();
                int universe = str.Length <= 0 ? 0 : int.Parse(str);

                NativeController.Instance.SwitchStatus(OperStatus.System);

                SetUserButton(false);
                await NativeController.Instance.LoginAsync(account, psw, universe);
                await NativeController.Instance.GetCode();
            }
            catch (Exception ex)
            {
                LogUtil.Error($"TryGetAuth catch {ex.Message}");
            }

            SetUserButton(true);

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private async Task TryAuthCode(string code)
        {
            try
            {
                if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

                NativeController.Instance.SwitchStatus(OperStatus.System);

                SetUserButton(false);
                await NativeController.Instance.AuthCode(code);
            }
            catch (Exception ex)
            {
                LogUtil.Error($"TryAuthCode catch {ex.Message}");
            }

            SetUserButton(true);

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private async void btn_user_logout_Click(object sender, EventArgs e)
        {
            NativeController.Instance.CanNotify = true;
            await TryLogout();
        }

        private async Task TryLogout()
        {
            try
            {
                if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

                NativeController.Instance.SwitchStatus(OperStatus.System);
                SetUserButton(false);
                await NativeController.Instance.LogoutAsync();
            }
            catch(Exception ex)
            {
                LogUtil.Error($"TryLogout catch {ex.Message}");
            }
            SetUserButton(true);
            NativeController.Instance.SwitchStatus(OperStatus.None);
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
            
            doExpedtion();
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

        private void xbox_auto_CheckedChanged(object sender, EventArgs e)
        {
            NativeController.Instance.IsAutoExpedition = cbox_tx_auto.Checked;
            mAutoExpedition = cbox_tx_auto.Checked;
        }

        private void doExpedtion()
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

            var exMission = GetExMission();
            
            NativeController.Instance.StartExpedition(exMission);
            Redraw();
        }

        private void doAutoExpedtion()
        {
            if (OperStatus.Expedition == NativeController.Instance.MyOperStatus)
            {
                lb_tx_info.Text = $"{DateTime.Now:G}|正在探险状态";
                return;
            }

            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                lb_tx_info.Text = $"{DateTime.Now:G}|其他操作正忙";
                return;
            }

            var delta = DateTime.Now - NativeController.Instance.LastExeditionTime;
            var val = mPirateInterval - delta.TotalMinutes;
            val = val < 0 ? 0 : val;

            if (!cbox_tx_auto.Checked || !mAutoExpedition)
            {
                lb_tx_info.Text = $"{DateTime.Now:G}|没有设置自动探险；时间差{Math.Ceiling(val)}分钟";
                return;
            }

            if (delta.TotalMinutes < 120)
            {
                lb_tx_info.Text = $"{DateTime.Now:G}|大概还差{Math.Ceiling(val)}分钟可自动探险";
                return;
            }

            doExpedtion();
        }

        private PirateMission GetPirateMission()
        {
            PirateMission pMission = new PirateMission();

            for(int i = 0; i < 5; i++)
            {
                PirateControl control = Controls.Find($"w_pirate{i}", true)[0] as PirateControl;
                if (null == control) continue;

                pMission.Add(new Pirate
                {
                    Index = i,
                    Count = control.MyCount,
                    Mode = control.MyMode,
                    Options = control.MyOptions,
                    PlanetName = control.MyPlanet,
                    AllOptions = control.AllOptions
                });
            }

            return pMission;
        }

        private void btn_hd_start_Click(object sender, EventArgs e)
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                MessageBox.Show("当前正在忙，暂不能操作");
                return;
            }

            var pMission = GetPirateMission();
            if (pMission.MissionCount <= 0)
            {
                MessageBox.Show("海盗任务配置无效，请检测后再开始");
                return;
            }

            NativeController.Instance.CanNotify = true;

            doPirate();
        }

        private void doPirate(bool isAuto = false)
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

            var pMission = GetPirateMission();
            NativeController.Instance.StartPirate(pMission, isAuto);
            Redraw();
        }

        private void btn_hd_save_Click(object sender, EventArgs e)
        {
            var idx = rbtn_cfg1.Checked ? 1 : 0;
            var ret = MessageBox.Show($"确定保存配置{idx + 1}吗", "提示", MessageBoxButtons.YesNo);
            if (ret == DialogResult.Yes)
            {
                var pMission = GetPirateMission();
                pMission.Interval = mPirateInterval;
                pMission.AutoLogin = mAutoPirateLogin;
                PirateUtil.Save(pMission, idx);
            }
        }

        private void btn_hd_revert_Click(object sender, EventArgs e)
        {
            var idx = rbtn_cfg1.Checked ? 1 : 0;
            var ret = MessageBox.Show($"确定读取配置{idx + 1}吗", "提示", MessageBoxButtons.YesNo);
            if (DialogResult.Yes != ret) return;

            if (!PirateCfg())
            {
                MessageBox.Show("读取配置还原失败");
            }
        }

        private void btn_hd_refresh_Click(object sender, EventArgs e)
        {
            NativeController.Instance.RefreshNpc();
            Redraw();
        }

        private async void doAutoPirate()
        {
            if (OperStatus.Pirate == NativeController.Instance.MyOperStatus)
            {
                lb_hd_info.Text = $"{DateTime.Now:G}|正在海盗状态";
                return;
            }
            
            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                lb_hd_info.Text = $"{DateTime.Now:G}|其他操作正忙";
                return;
            }

            var delta = DateTime.Now - NativeController.Instance.LastPirateTime;
            var val = mPirateInterval - delta.TotalMinutes;
            val = val < 0 ? 0 : val;

            if (!cbox_hd_auto.Checked || !mAutoPirate)
            {
                lb_hd_info.Text = $"{DateTime.Now:G}|没有设置自动海盗；自动时间差{Math.Ceiling(val)}分钟";
                return;
            }

            if (delta.TotalMinutes < mPirateInterval)
            {
                lb_hd_info.Text = $"{DateTime.Now:G}|大概还差{Math.Ceiling(val)}分钟可自动海盗";
                return;
            }

            if (mAutoPirateLogin)
            {
                NativeController.Instance.CanNotify = false;
                await TryLogin();
            }

            doPirate(true);
        }

        private void cbox_hd_auto_CheckedChanged(object sender, EventArgs e)
        {
            NativeController.Instance.IsAutoPirate = cbox_hd_auto.Checked;
            mAutoPirate = cbox_hd_auto.Checked;
        }

        private void btn_hd_interval_Click(object sender, EventArgs e)
        {
            var txt = w_hd_inverval.Text.Trim();
            if (txt.Length <= 0)
            {
                MessageBox.Show("请输入数值");
                return;
            }

            var interval = int.Parse(txt);
            if (interval < 60)
            {
                MessageBox.Show("间隔不能小于60分钟");
                return;
            }

            mPirateInterval = interval;
            lb_hd_interval.Text = mPirateInterval.ToString();
        }

        private void btn_hd_stop_Click(object sender, EventArgs e)
        {
            NativeController.Instance.StopPirate();
        }

        private void btn_tx_stop_Click(object sender, EventArgs e)
        {
            NativeController.Instance.StopExpedition();
        }

        private void btn_tx_planet_Click(object sender, EventArgs e)
        {
            NativeController.Instance.RefreshPlanet();
            Redraw();
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            HotKey.RegisterHotKey(Handle, 107, HotKey.KeyModifiers.Alt, Keys.D8);
        }

        private void MainForm_Leave(object sender, EventArgs e)
        {
            HotKey.UnregisterHotKey(Handle, 107);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    switch (m.WParam.ToInt32())
                    {
                        case 107: // 按下的是Alt+8
                            BossKey();
                            break;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        //老板键：显示|隐藏 窗体
        private void BossKey()
        {
            if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Minimized;
                Hide();//隐藏窗体
            }
            else
            {
                Visible = true;
                WindowState = FormWindowState.Normal;
            }
        }
    }
}
