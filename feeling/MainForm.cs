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
using auto_update;
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
        bool mAutoExpedition1 = false;
        bool mAutoPirate = false;
        bool mAutoPirate1 = false;
        int mPirateInterval = 120; // 分
        int mPirateInterval1 = 120; // 分

        int mExpeditionInterval = 120; // 分
        int mExpeditionInterval1 = 120; // 分

        bool mAutoImperium = false;
        int mImperiumInterval = 240; // 分

        bool mIsBusy = false;

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

            if (!Directory.Exists(NativeConst.CfgDirectory))
            {
                Directory.CreateDirectory(NativeConst.CfgDirectory);
            }

            NativeController.Instance.StatusChangeEvent += OnStatusChange;
            NativeController.Instance.ScanGalaxyEvent += OnScanGalaxyEvent;
            NativeController.Instance.PlanetEvent += OnPlanetEvent;
            NativeController.Instance.OperTipsEvent += OnOperTipsChange;
            NativeController.Instance.NpcChangeEvent += OnNpcChange;

            InitData();

#if DEBUG
            btn_test.Visible = true;
#else
            btn_test.Visible = false;
#endif

            SystemSleep.PreventSleep();
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
                Invoke(new Action(async () =>
                {
                    await _LookThread();
                }));
                Thread.Sleep(1000 * 30);
            } while (true);
        }

        private async Task _LookThread()
        {
            try
            {
                if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

                var canAuto = CheckCanAuto();
                if (canAuto)
                {
                    await doAutoExpedtion();
                    await doAutoPirate();
                    await doAutoExpedtion(1);
                    await doAutoPirate(1);
                    await doAutoImperium();
                }

                SendData();
            }
            catch (Exception ex)
            {
                NativeLog.Error($"_LookThread {ex.Message}");
            }
        }

        private bool CheckCanAuto()
        {
            var dt = DateTime.Now;

            if (NativeController.User.MorningIdle)
            {
                if (dt.Hour >= 23 || dt.Hour < 5)
                {
                    NativeLog.Info("凌晨空闲模式");
                    return false;
                }
            }

            return true;
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

            cbox_auto_logout.Checked = NativeController.User.AutoLogout;
            cbox_morning_idle.Checked = NativeController.User.MorningIdle;

            InitExpedition();
            RedrawAccount();
            InitImperium();
            InitPirate();
        }

        protected void InitPirate()
        {
            // 默认值
            lb_hd_interval.Text = mPirateInterval.ToString();
            lb_hd_interval1.Text = mPirateInterval1.ToString();
            rbtn_cfg0.Checked = true;

            PirateUtil.Initialize();

            PirateCfg();

            hd_speed_cb.Items.Clear();
            ShipSpeed.SpeedLists.ForEach(e =>
            {
                hd_speed_cb.Items.Add(e);
            });

            hd_speed_cb.SelectedIndex = 0;
            NativeController.Instance.PirateSpeedIndex = 0;
        }

        protected void InitImperium()
        {
            // 读取配置
            ImperiumUtil.ReadCfg();

            var imperiumCfg = ImperiumUtil.MyImperium;
            if (null == imperiumCfg)
            {
                lb_tz_interval.Text = mImperiumInterval.ToString();
                return;
            }

            mImperiumInterval = imperiumCfg.Interval;
            mAutoImperium = imperiumCfg.Open;
            
            lb_tz_interval.Text = mImperiumInterval.ToString();
            cbox_tz_auto.Checked = mAutoImperium;
        }

        protected void InitExpedition()
        {
            lb_tx_interval.Text = mExpeditionInterval.ToString();
            lb_tx_interval1.Text = mExpeditionInterval1.ToString();

            Expedition.Initialize();

            var exShipOptions = Expedition.GetShipOptions();

            tx0_ship0_cb.Items.Clear();
            tx0_ship1_cb.Items.Clear();
            tx0_ship2_cb.Items.Clear();
            tx1_ship0_cb.Items.Clear();
            tx1_ship1_cb.Items.Clear();
            tx1_ship2_cb.Items.Clear();
            tx2_ship0_cb.Items.Clear();
            tx2_ship1_cb.Items.Clear();
            tx2_ship2_cb.Items.Clear();
            tx3_ship0_cb.Items.Clear();
            tx3_ship1_cb.Items.Clear();
            tx3_ship2_cb.Items.Clear();
            exShipOptions.ForEach(e =>
            {
                tx0_ship0_cb.Items.Add(e);
                tx0_ship1_cb.Items.Add(e);
                tx0_ship2_cb.Items.Add(e);
                tx1_ship0_cb.Items.Add(e);
                tx1_ship1_cb.Items.Add(e);
                tx1_ship2_cb.Items.Add(e);
                tx2_ship0_cb.Items.Add(e);
                tx2_ship1_cb.Items.Add(e);
                tx2_ship2_cb.Items.Add(e);
                tx3_ship0_cb.Items.Add(e);
                tx3_ship1_cb.Items.Add(e);
                tx3_ship2_cb.Items.Add(e);
            });

            tx0_ship0_cb.SelectedIndex = 0;
            tx0_ship1_cb.SelectedIndex = 0;
            tx0_ship2_cb.SelectedIndex = 0;
            tx1_ship0_cb.SelectedIndex = 0;
            tx1_ship1_cb.SelectedIndex = 0;
            tx1_ship2_cb.SelectedIndex = 0;
            tx2_ship0_cb.SelectedIndex = 0;
            tx2_ship1_cb.SelectedIndex = 0;
            tx2_ship2_cb.SelectedIndex = 0;
            tx3_ship0_cb.SelectedIndex = 0;
            tx3_ship1_cb.SelectedIndex = 0;
            tx3_ship2_cb.SelectedIndex = 0;

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

            NativeLog.Info($"读取海盗配置，当前配置：{idx + 1}");
            /*          
                // 读取配置
                if (!PirateUtil.ReadCfg(idx)) return false;
            */
            var pMissionCfg = PirateUtil.MyMission;
            var pMissionCfg1 = PirateUtil.MyMission1;

            // if (null == pMissionCfg) return false;

            var missionCfg = idx == 1 ? pMissionCfg1 : pMissionCfg;

            var planetList = NativeController.Instance.MyPlanet.List;
            var npcList = PirateUtil.NpcList;

            var list = missionCfg.List;

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

            cbox_hd_cfg_cross.Checked = missionCfg.IsCross;

            var interval = pMissionCfg.Interval;
            mPirateInterval = interval < 120 ? 120 : interval;
            lb_hd_interval.Text = mPirateInterval.ToString();

            interval = pMissionCfg1.Interval;
            mPirateInterval1 = interval < 120 ? 120 : interval;
            lb_hd_interval1.Text = mPirateInterval1.ToString();
            
            return true;
        }

        protected bool RevertCfg()
        {
            var cfgIdx = rbtn_ex_cfg1.Checked ? 1 : 0;
            // 读取配置
            // if (!Expedition.ReadCfg(cfgIdx)) return false;

            var exMissionCfg = Expedition.MyExMissionCfg;
            var exMissionCfg1 = Expedition.MyExMissionCfg1;

            var missionCfg = cfgIdx == 1 ? exMissionCfg1 : exMissionCfg;

            NativeLog.Info($"读取探险配置，当前配置：{cfgIdx + 1}");

            // if (null == exMissionCfg) return false;

            var planetList = NativeController.Instance.MyPlanet.List;
            var exOptions = Expedition.ShipOptions;

            for (var i = 0; i < 4; i++)
            {
                if (i >= missionCfg.List.Count)
                {
                    (Controls.Find($"tx{i}_ship0", true)[0] as TextBox).Text = "";
                    (Controls.Find($"tx{i}_ship1", true)[0] as TextBox).Text = "";
                    (Controls.Find($"tx{i}_ship2", true)[0] as TextBox).Text = "";
                    continue;
                }

                var mission = missionCfg.GetMission(i);
                var idx = planetList.FindIndex(e => e == mission.PlanetName);
                if (idx != -1)
                {
                    (Controls.Find($"tx{i}_planet", true)[0] as ComboBox).SelectedIndex = idx;
                }

                (Controls.Find($"tx{i}_ship0", true)[0] as TextBox).Text = "";
                (Controls.Find($"tx{i}_ship1", true)[0] as TextBox).Text = "";
                (Controls.Find($"tx{i}_ship2", true)[0] as TextBox).Text = "";

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

                if (mission.FleetList.Count > 2)
                {
                    fleet = mission.FleetList[2];
                    idx = exOptions.FindIndex(e => e == fleet.ShipType);
                    (Controls.Find($"tx{i}_ship2_cb", true)[0] as ComboBox).SelectedIndex = idx < 0 ? 0 : idx;
                    (Controls.Find($"tx{i}_ship2", true)[0] as TextBox).Text = fleet.Count.ToString();
                }
            }

            cbox_ex_cfg_cross.Checked = missionCfg.IsCross;

            var interval = exMissionCfg.Interval;
            mExpeditionInterval = interval < 60 ? 120 : interval;
            lb_tx_interval.Text = mExpeditionInterval.ToString();

            interval = exMissionCfg1.Interval;
            mExpeditionInterval1 = interval < 60 ? 120 : interval;
            lb_tx_interval1.Text = mExpeditionInterval1.ToString();

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
                    mLastContent = $"{DateTime.Now:G}|{sDesc}";
                }

                SendData();
            }));
        }

        protected void Redraw()
        {
            try
            {
                var operStatus = NativeController.Instance.MyOperStatus;
                var enabled = false;
                switch (operStatus)
                {
                    case OperStatus.System:
                        btn_galaxy_start.Enabled = false;
                        btn_galaxy_stop.Enabled = false;
                        SetExpeditionButton(false);
                        SetPirateButton(false);
                        btn_tz_start.Enabled = false;
                        btn_cross.Enabled = false;
                        btn_universe.Enabled = false;
                        cbox_auto_logout.Enabled = false;
                        btn_transfer.Enabled = false;
                        cbox_auto_transfer.Enabled = false;
                        SetQuickBtn(false);
                        break;
                    case OperStatus.Galaxy:
                        btn_galaxy_start.Enabled = false;
                        btn_galaxy_stop.Enabled = true;
                        SetExpeditionButton(false);
                        SetPirateButton(false);
                        btn_tz_start.Enabled = false;
                        btn_cross.Enabled = false;
                        btn_universe.Enabled = false;
                        cbox_auto_logout.Enabled = false;
                        btn_transfer.Enabled = false;
                        cbox_auto_transfer.Enabled = false;
                        SetQuickBtn(false);
                        break;
                    case OperStatus.Expedition:
                        btn_galaxy_start.Enabled = false;
                        btn_galaxy_stop.Enabled = false;
                        SetExpeditionButton(false, true);
                        SetPirateButton(false);
                        btn_tz_start.Enabled = false;
                        btn_cross.Enabled = false;
                        btn_universe.Enabled = false;
                        cbox_auto_logout.Enabled = false;
                        btn_transfer.Enabled = false;
                        cbox_auto_transfer.Enabled = false;
                        SetQuickBtn(false);
                        break;
                    case OperStatus.Pirate:
                        btn_galaxy_start.Enabled = false;
                        btn_galaxy_stop.Enabled = false;
                        SetExpeditionButton(false);
                        SetPirateButton(false, true);
                        btn_tz_start.Enabled = false;
                        btn_cross.Enabled = false;
                        btn_universe.Enabled = false;
                        cbox_auto_logout.Enabled = false;
                        btn_transfer.Enabled = false;
                        cbox_auto_transfer.Enabled = false;
                        SetQuickBtn(false);
                        break;
                    case OperStatus.None:
                    default:
                        btn_galaxy_start.Enabled = true;
                        btn_galaxy_stop.Enabled = false;
                        SetExpeditionButton(true);
                        SetPirateButton(true);
                        btn_tz_start.Enabled = true;
                        btn_cross.Enabled = true;
                        btn_universe.Enabled = true;
                        cbox_auto_logout.Enabled = true;
                        btn_transfer.Enabled = true;
                        cbox_auto_transfer.Enabled = true;
                        SetQuickBtn(true);
                        enabled = true;
                        break;
                }

                mWebBrowser.Enabled = enabled;
            }
            catch(Exception ex)
            {
                NativeLog.Error($"Redraw catch {ex.Message}");
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

            RevertCfg();
            PirateCfg();
            SendData();
        }

        private void RedrawOperTips(OperStatus operStatus, string tips)
        {
            var content = tips.Trim();
            if (content.Length > 0)
            {
                content = $"{DateTime.Now:G}|{content}";
            }

            switch (operStatus)
            {
                case OperStatus.Expedition:
                    tx_content.Text = content;
                    break;
                case OperStatus.Pirate:
                    w_hd_tips.Text = content;
                    break;
                default:
                    break;
            }

            if (content.Length > 0)
            {
                mLastContent = content;
            }

            SendData();
        }

        public void SetUserButton(bool enabled)
        {
            w_user_account.Enabled = enabled;
            btn_user_login.Enabled = enabled;
            btn_user_logout.Enabled = enabled;
            cbox_auto_logout.Enabled = enabled;
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
            hd_speed_cb.Enabled = enabled;
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
            NativeLog.Debug($"Web_OnFrameEnd {e.Url}");
            NativeController.Instance.HandleWebBrowserFrameEnd(e.Url);
            SendData();
        }

        private void Web_OnFrameStart(object sender, FrameLoadStartEventArgs e)
        {
            NativeLog.Debug("Web_OnFrameStart");
            SendData();

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
            mClient.HeartbeatHandler += onHeartbeat;
#endif

#if !DEBUG
            DoUpdate();
#endif
        }

        private void DoUpdate()
        {
            try
            {
                Text = $"情怀 v{CfgSettings.VersionDesc}";

                var updater = CfgSettings.ProcessDirectory + "auto_updater.exe";

                var confg = new UpdateConfig
                {
                    AppName = "feeling.exe",
                    VersionLocalPath = CfgSettings.SettingFile,
                    VersionUrl = "http://bkbibi.teammvp.beer/netcore/feeling/version.json",
                };

                if (File.Exists(updater) && AutoUpdate.NeedUpdate(confg))
                {
                    Process process = new Process();
                    var startInfo = new ProcessStartInfo(updater);
                    startInfo.UseShellExecute = true;
                    process.StartInfo = startInfo;
                    process.Start();
                    Environment.Exit(0);
                    return;
                }
            }
            catch (Exception ex)
            {
                NativeLog.Error($"DoUpdate {ex.Message}");
            }
        }

#if !NET45
        private void OnServerReceived(OgameData data)
        {
            Invoke(new Action(() => {
                DoServerReceived(data);
            }));
        }

        private void onHeartbeat()
        {
            Invoke(new Action(() => {
                SendData(CmdEnum.Hello.ToString());
            }));
        }

        private async void DoServerReceived(OgameData data)
        {
            try
            {
                if (OperStatus.None != NativeController.Instance.MyOperStatus)
                {
                    SendData();
                    return;
                }

                if (mIsBusy) return;

                switch (data.Cmd)
                {
                    case CmdEnum.Login:
                        NativeController.Instance.CanNotify = false;
                        await TryLogin();
                        break;
                    case CmdEnum.Logout:
                        NativeController.Instance.CanNotify = false;
                        await TryLogout();
                        break;
                    case CmdEnum.Pirate:
                        NativeController.Instance.CanNotify = false;
                        doPirate();
                        break;
                    case CmdEnum.Expedition:
                        NativeController.Instance.CanNotify = false;
                        doExpedtion();
                        break;
                    case CmdEnum.GetCode:
                        NativeController.Instance.CanNotify = false;
                        await TryGetAuth();
                        break;
                    case CmdEnum.AuthCode:
                        NativeController.Instance.CanNotify = false;
                        await TryAuthCode(data.Content);
                        break;
                    case CmdEnum.Imperium:
                        NativeController.Instance.CanNotify = false;
                        await DoImperium();
                        break;
                    case CmdEnum.Npc:
                        NativeController.Instance.CanNotify = false;
                        doRefreshNpc();
                        break;
                    case CmdEnum.AutoPirateOpen:
                        NativeController.Instance.CanNotify = false;
                        await SetAutoPirateOpen(data.AutoPirateOpen);
                        break;
                    case CmdEnum.AutoPirateOpen1:
                        NativeController.Instance.CanNotify = false;
                        await SetAutoPirateOpen1(data.AutoPirateOpen1);
                        break;
                    case CmdEnum.PirateCfg:
                        NativeController.Instance.CanNotify = false;
                        await SetPirateCfg(data.PirateCfgIndex);
                        break;
                    case CmdEnum.AutoExpeditionOpen:
                        NativeController.Instance.CanNotify = false;
                        await SetAutoExpeditionOpen(data.AutoExpeditionOpen);
                        break;
                    case CmdEnum.AutoExpeditionOpen1:
                        NativeController.Instance.CanNotify = false;
                        await SetAutoExpeditionOpen1(data.AutoExpeditionOpen1);
                        break;
                    case CmdEnum.GoCross:
                        NativeController.Instance.CanNotify = false;
                        await GoCross();
                        break;
                    case CmdEnum.BackUniverse:
                        NativeController.Instance.CanNotify = false;
                        await BackUniverse();
                        break;
                    case CmdEnum.ExpeditionCfg:
                        NativeController.Instance.CanNotify = false;
                        await SetExpeditionCfg(data.ExpeditionCfgIndex);
                        break;
                    case CmdEnum.AutoLogoutOpen:
                        NativeController.Instance.CanNotify = false;
                        await SetAutoLogoutOpen(data.AutoLogoutOpen);
                        break;
                    case CmdEnum.AutoImperiumOpen:
                        NativeController.Instance.CanNotify = false;
                        await SetAutoImperiumOpen(data.AutoImperiumOpen);
                        break;
                    case CmdEnum.QuickAutoCheck:
                        DoQuickAutoCheck();
                        break;
                    case CmdEnum.QuickAutoUncheck:
                        DoQuickAutoUncheck();
                        break;
                    case CmdEnum.QuickAutoStart:
                        DoQuickAutoStart();
                        break;
                    case CmdEnum.PirateSpeed:
                        await SetPirateSpeed(data.PirateSpeedIndex);
                        break;
                    case CmdEnum.MorningIdle:
                        NativeController.Instance.CanNotify = false;
                        await SetMorningIdle(data.MorningIdle);
                        break;
                    case CmdEnum.Transfer:
                        NativeController.Instance.CanNotify = false;
                        DoTransfer();
                        break;
                    case CmdEnum.AutoTransferOpen:
                        NativeController.Instance.CanNotify = false;
                        await SetAutoTransferOpen(data.AutoTransferOpen);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                NativeLog.Error($"feeling OnServerReceived catch {ex.Message}");
            }

            SendData();
        }
#endif

        private void SendData(string cmd = "Data")
        {
#if !NET45
            CmdEnum cmdEnum;

            try
            {
                cmdEnum = (CmdEnum)Enum.Parse(typeof(CmdEnum), cmd);
            }
            catch(Exception)
            {
                cmdEnum = CmdEnum.Data;
            }

            var operStatus = (int)NativeController.Instance.MyOperStatus;
            var status = (StatusEnum)operStatus;

            var fleetContent = "";
            var content = tx_content.Text.Trim();

            if (content.Contains("|"))
            {
                content = content.Substring(content.IndexOf("|") + 1);
            }

            fleetContent = content;

            content = w_hd_tips.Text.Trim();
            if (content.Length > 0)
            {
                if (content.Contains("|"))
                {
                    content = content.Substring(content.IndexOf("|") + 1);
                }
                
                fleetContent = fleetContent.Length > 0 ? $"{fleetContent}|{content}" : content;
            }

            var url = mWebBrowser?.Address ?? "";
            var mat = Regex.Match(url, $@"://(?<universe>\S*).cicihappy.com");
            var universe = "";
            if (mat.Success)
            {
                universe = mat.Groups["universe"].Value;
            }

            var gameData = new OgameData
            {
                Cmd = cmdEnum,
                Status = status,
                Content = mLastContent,
                Universe = universe,
                NpcUniverse = PirateUtil.Universe,
                PlanetUniverse = NativeController.Instance.MyPlanet.Universe,

                FleetContent = fleetContent,
                PirateCfgIndex = rbtn_cfg1.Checked ? 1 : 0,
                ExpeditionCfgIndex = rbtn_ex_cfg1.Checked ? 1 : 0,
                PirateSpeedIndex = NativeController.Instance.PirateSpeedIndex,

                AutoLogoutOpen = NativeController.User.AutoLogout,
                AutoPirateOpen = mAutoPirate,
                AutoPirateOpen1 = mAutoPirate1,
                AutoExpeditionOpen = mAutoExpedition,
                AutoExpeditionOpen1 = mAutoExpedition1,
                AutoImperiumOpen = mAutoImperium,
                AutoTransferOpen = NativeController.Instance.IsAutoTransfer,

                PirateAutoMsg = lb_hd_info.Text.Trim(),
                PirateAutoMsg1 = lb_hd_info1.Text.Trim(),
                ExpeditionAutoMsg = lb_tx_info.Text.Trim(),
                ExpeditionAutoMsg1 = lb_tx_info1.Text.Trim(),

                MorningIdle = NativeController.User.MorningIdle,
            };

            mClient?.SendData(gameData);
#endif
        }

#if !NET45
        private void OnServerConnected()
        {
            SendData(CmdEnum.Auth.ToString());
        }
#endif

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
            //只允许输入数字，粘贴数字
            if (!(char.IsNumber(e.KeyChar) || e.KeyChar == (char)8 || e.KeyChar == (char)3 || e.KeyChar == (char)22))
            {
                e.Handled = true;
            }
            try
            {
                if (e.KeyChar == (char)22)
                {
                    Convert.ToInt64(Clipboard.GetText());  //检查是否数字
                    Clipboard.SetText(Clipboard.GetText().Trim()); //去空格
                }
            }
            catch (Exception)
            {
                e.Handled = true;
                //throw;
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
                NativeLog.Error($"TryLogin catch {ex.Message}");
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
                NativeLog.Error($"TryGetAuth catch {ex.Message}");
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
                NativeLog.Error($"TryAuthCode catch {ex.Message}");
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
                NativeLog.Error($"TryLogout catch {ex.Message}");
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
                tx0_planet.Text.Trim(),
                tx0_ship0_cb.SelectedIndex,
                tx0_ship0.Text.Trim(),
                tx0_ship1_cb.SelectedIndex,
                tx0_ship1.Text.Trim(),
                tx0_ship2_cb.SelectedIndex,
                tx0_ship2.Text.Trim()
            );
            exMission.Add(
                tx1_planet.Text.Trim(),
                tx1_ship0_cb.SelectedIndex,
                tx1_ship0.Text.Trim(),
                tx1_ship1_cb.SelectedIndex,
                tx1_ship1.Text.Trim(),
                tx1_ship2_cb.SelectedIndex,
                tx1_ship2.Text.Trim()
            );
            exMission.Add(
                tx2_planet.Text.Trim(),
                tx2_ship0_cb.SelectedIndex,
                tx2_ship0.Text.Trim(),
                tx2_ship1_cb.SelectedIndex,
                tx2_ship1.Text.Trim(),
                tx2_ship2_cb.SelectedIndex,
                tx2_ship2.Text.Trim()
            );
            exMission.Add(
                tx3_planet.Text.Trim(),
                tx3_ship0_cb.SelectedIndex,
                tx3_ship0.Text.Trim(),
                tx3_ship1_cb.SelectedIndex,
                tx3_ship1.Text.Trim(),
                tx3_ship2_cb.SelectedIndex,
                tx3_ship2.Text.Trim()
            );

            exMission.Interval = rbtn_ex_cfg1.Checked ? mExpeditionInterval1 : mExpeditionInterval;
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
                return;
            }

            var idx = rbtn_ex_cfg1.Checked ? 1 : 0;
            var ret = MessageBox.Show($"确定保存配置 {idx + 1} 吗", "提示", MessageBoxButtons.YesNo);
            if (ret == DialogResult.Yes)
            {
                exMission.IsCross = NativeController.Instance.MyPlanet.Universe == "w1";
                exMission.Interval = idx == 1 ? mExpeditionInterval1 : mExpeditionInterval;
                Expedition.Save(exMission, idx);
                RevertCfg();
            }
        }

        private void btn_tx_revert_Click(object sender, EventArgs e)
        {
            var idx = rbtn_ex_cfg1.Checked ? 1 : 0;
            var ret = MessageBox.Show($"确定读取配置 {idx + 1} 吗", "提示", MessageBoxButtons.YesNo);
            if (DialogResult.Yes != ret) return;

            if (!RevertCfg())
            {
                MessageBox.Show($"读取探险配置 {idx + 1} 失败");
            }
        }

        private void rbtn_ex_cfg_CheckedChanged(object sender, EventArgs e)
        {
            var rbtn = sender as RadioButton;
            if (!rbtn.Checked) return;
            RevertCfg();
        }

        private void cbox_tx_auto_CheckedChanged(object sender, EventArgs e)
        {
            NativeController.Instance.IsAutoExpedition = cbox_tx_auto.Checked;
            mAutoExpedition = cbox_tx_auto.Checked;
            RedrawQuick();
        }

        private void cbox_tx_auto1_CheckedChanged(object sender, EventArgs e)
        {
            NativeController.Instance.IsAutoExpedition1 = cbox_tx_auto1.Checked;
            mAutoExpedition1 = cbox_tx_auto1.Checked;
            RedrawQuick();
        }

        private void doExpedtion(bool isAuto = false)
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

            var exMission = GetExMission();

            NativeLog.Info($"doExpedtion {JsonConvert.SerializeObject(exMission)}");
            NativeController.Instance.StartExpedition(exMission, rbtn_ex_cfg1.Checked ? 1 : 0, isAuto);
            Redraw();
        }

        private async Task doAutoExpedtion(int index = 0)
        {
            Label label;
            TimeSpan delta;
            double val;
            bool isClose;
            
            if (mIsBusy) return;

            var missionCfg = index == 1 ? Expedition.MyExMissionCfg1 : Expedition.MyExMissionCfg;

            if (index == 1)
            {
                delta = DateTime.Now - NativeController.Instance.LastExeditionTime1;
                isClose = !cbox_tx_auto1.Checked || !mAutoExpedition1;
                label = lb_tx_info1;
                val = mExpeditionInterval1 - delta.TotalMinutes;
            }
            else
            {
                delta = DateTime.Now - NativeController.Instance.LastExeditionTime;
                isClose = !cbox_tx_auto.Checked || !mAutoExpedition;
                label = lb_tx_info;
                val = mExpeditionInterval - delta.TotalMinutes;
            }

            val = val < 0 ? 0 : val;

            if (OperStatus.Expedition == NativeController.Instance.MyOperStatus)
            {
                label.Text = $"{DateTime.Now:G}|正在探险状态";
                return;
            }

            if (isClose)
            {
                label.Text = $"{DateTime.Now:G}|自动探险{index + 1}-关{(missionCfg.IsCross ? "-多维" : "")}，{Math.Ceiling(val)}分钟";
                return;
            }

            if (val > 0)
            {
                label.Text = $"{DateTime.Now:G}|自动探险{index + 1}-开{(missionCfg.IsCross ? "-多维" : "")}，{Math.Ceiling(val)}分钟";
                return;
            }

            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                label.Text = $"{DateTime.Now:G}|其他操作正忙";
                return;
            }

            if (!Network.IsConnected)
            {
                label.Text = $"{DateTime.Now:G}|网络异常";
                return;
            }

            label.Text = $"{DateTime.Now:G}|自动探险{index + 1}开始";

            mIsBusy = true;

            NativeController.Instance.CanNotify = false;
            await TryLogin();
            
            if (index == 1)
            {
                rbtn_ex_cfg1.Checked = true;
            }
            else
            {
                rbtn_ex_cfg0.Checked = true;
            }

            NativeLog.Info($"探险{index + 1}");

            await Task.Delay(500);

            if (missionCfg.IsCross)
            {
                await GoCross();
            }
            else
            {
                await BackUniverse();
                await SureLogin();
            }

            var isCross = PirateUtil.Universe == "w1";
            var isPlanetCross = NativeController.Instance.MyPlanet.Universe == "w1";
            if (missionCfg.IsCross != isCross ||
                missionCfg.IsCross != isPlanetCross ||
                !NativeController.Instance.MyPlanet.HasData)
            {
                await NativeController.Instance.DoRefreshNpc(true);
                if (!NativeController.Instance.MyPlanet.HasData)
                {
                    await NativeController.Instance.DoRefreshNpc(true);
                }
            }

            mIsBusy = false;
            doExpedtion(true);
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
            NativeController.Instance.StartPirate(pMission, rbtn_cfg1.Checked ? 1 : 0, isAuto);
            Redraw();
        }

        private void btn_hd_save_Click(object sender, EventArgs e)
        {
            var idx = rbtn_cfg1.Checked ? 1 : 0;
            var ret = MessageBox.Show($"确定保存配置 {idx + 1} 吗", "提示", MessageBoxButtons.YesNo);
            if (ret == DialogResult.Yes)
            {
                var pMission = GetPirateMission();
                pMission.Interval = idx == 1 ? mPirateInterval1 : mPirateInterval;
                PirateUtil.Save(pMission, idx);
                PirateCfg();
            }
        }

        private void btn_hd_revert_Click(object sender, EventArgs e)
        {
            var idx = rbtn_cfg1.Checked ? 1 : 0;
            var ret = MessageBox.Show($"确定读取配置 {idx + 1} 吗", "提示", MessageBoxButtons.YesNo);
            if (DialogResult.Yes != ret) return;

            if (!PirateCfg())
            {
                MessageBox.Show($"读取还原配置配置 {idx + 1} 失败");
            }
        }

        private void rbtn_cfg_CheckedChanged(object sender, EventArgs e)
        {
            var rbtn = sender as RadioButton;
            if (!rbtn.Checked) return;

            PirateCfg();
        }

        private void btn_hd_refresh_Click(object sender, EventArgs e)
        {
            doRefreshNpc();
        }

        private void doRefreshNpc()
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

            NativeController.Instance.RefreshNpc();
            Redraw();
        }

        private async Task SureLogin()
        {
            var source = await NativeController.Instance.GetFrameSourceAsync();

            NativeLog.Info($"SureLogin");
            if (string.IsNullOrWhiteSpace(source) || (source.Contains("退出成功") && source.Contains("重新登录")))
            {
                NativeLog.Info($"SureLogin TryLogin");
                await TryLogin();
            }
            NativeLog.Info($"SureLogin end");
        }

        private async Task doAutoPirate(int index = 0)
        {
            Label label;
            bool isClose;
            TimeSpan delta;
            double val;

            if (mIsBusy) return;

            var missionCfg = index == 1 ? PirateUtil.MyMission1 : PirateUtil.MyMission;

            if (index == 1)
            {
                delta = DateTime.Now - NativeController.Instance.LastPirateTime1;
                val = mPirateInterval1 - delta.TotalMinutes;
                val = val < 0 ? 0 : val;
                label = lb_hd_info1;
                isClose = !cbox_hd_auto1.Checked || !mAutoPirate1;
            }
            else
            {
                delta = DateTime.Now - NativeController.Instance.LastPirateTime;
                val = mPirateInterval - delta.TotalMinutes;
                val = val < 0 ? 0 : val;
                label = lb_hd_info;
                isClose = !cbox_hd_auto.Checked || !mAutoPirate;
            }

            if (OperStatus.Pirate == NativeController.Instance.MyOperStatus)
            {
                label.Text = $"{DateTime.Now:G}|正在海盗";
                return;
            }

            if (isClose)
            {
                label.Text = $"{DateTime.Now:G}|自动海盗{index + 1}-关{(missionCfg.IsCross ? "-多维": "")}，{Math.Ceiling(val)}分钟";
                return;
            }

            if (val > 0)
            {
                label.Text = $"{DateTime.Now:G}|自动海盗{index + 1}-开{(missionCfg.IsCross ? "-多维" : "")}，{Math.Ceiling(val)}分钟";

                if (val <= 50 && val >= 10)
                {
                    await DoAutoTransfer(index);
                }

                return;
            }

            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                label.Text = $"{DateTime.Now:G}|其他操作正忙";
                return;
            }

            if (!Network.IsConnected)
            {
                label.Text = $"{DateTime.Now:G}|网络异常";
                return;
            }

            label.Text = $"{DateTime.Now:G}|自动海盗{index + 1}开始";
            NativeLog.Info($"自动海盗{index + 1}开始");

            mIsBusy = true;
            
            NativeController.Instance.CanNotify = false;
            await TryLogin();

            NativeLog.Info($"切换配置{index + 1}");
            if (index == 1)
            {
                rbtn_cfg1.Checked = true;
            }
            else
            {
                rbtn_cfg0.Checked = true;
            }

            await Task.Delay(500);

            if (missionCfg.IsCross)
            {
                await GoCross();
            }
            else
            {
                await BackUniverse();
                await SureLogin();
            }

            NativeLog.Info($"检测是否刷球{index + 1}");
/*          
            var isCross = PirateUtil.Universe == "w1";
            var isPlanetCross = NativeController.Instance.MyPlanet.Universe == "w1";
            if (!PirateUtil.HasNpcData ||
                missionCfg.IsCross != isCross ||
                missionCfg.IsCross != isPlanetCross ||
                !NativeController.Instance.MyPlanet.HasData)
            {
                await NativeController.Instance.DoRefreshNpc(true);
                if (!PirateUtil.HasNpcData || !NativeController.Instance.MyPlanet.HasData)
                {
                    await NativeController.Instance.DoRefreshNpc(true);
                }
            }
*/

            await NativeController.Instance.DoRefreshNpc(true);
            if (!PirateUtil.HasNpcData || !NativeController.Instance.MyPlanet.HasData)
            {
                await NativeController.Instance.DoRefreshNpc(true);
            }

            mIsBusy = false;
            NativeLog.Info($"开始尝试处理海盗{index + 1}");
            doPirate(true);
        }

        private void cbox_hd_auto_CheckedChanged(object sender, EventArgs e)
        {
            NativeController.Instance.IsAutoPirate = cbox_hd_auto.Checked;
            mAutoPirate = cbox_hd_auto.Checked;
            RedrawQuick();
        }

        private void cbox_hd_auto1_CheckedChanged(object sender, EventArgs e)
        {
            NativeController.Instance.IsAutoPirate1 = cbox_hd_auto1.Checked;
            mAutoPirate1 = cbox_hd_auto1.Checked;
            RedrawQuick();
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
            PirateUtil.SetInterval(interval, 0);
        }

        private void btn_hd_interval1_Click(object sender, EventArgs e)
        {
            var txt = w_hd_inverval1.Text.Trim();
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

            mPirateInterval1 = interval;
            lb_hd_interval1.Text = mPirateInterval1.ToString();
            PirateUtil.SetInterval(interval, 1);
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

#region hotkey
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
#endregion

        private void btn_tz_interval_Click(object sender, EventArgs e)
        {
            var txt = w_tz_inverval.Text.Trim();
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

            mImperiumInterval = interval;
            lb_tz_interval.Text = mImperiumInterval.ToString();
        }

        private void btn_tz_save_Click(object sender, EventArgs e)
        {
            try
            {
                var cfg = new Imperium {
                    Interval = mImperiumInterval,
                    Open = mAutoImperium,
                };

                ImperiumUtil.Save(cfg);
                w_tz_tips.Text = $"{DateTime.Now:G} 统治保存成功";
            }
            catch(Exception ex)
            {
                NativeLog.Error($"tz_save catch {ex.Message}");
            }
        }

        private void cbox_tz_auto_CheckedChanged(object sender, EventArgs e)
        {
            mAutoImperium = cbox_tz_auto.Checked;
            NativeController.Instance.IsAutoImperium = mAutoImperium;
        }

        private async Task doAutoImperium()
        {
            var delta = DateTime.Now - NativeController.Instance.LastImperiumTime;
            var val = mImperiumInterval - delta.TotalMinutes;
            val = val < 0 ? 0 : val;

            if (mIsBusy) return;

            if (!cbox_tz_auto.Checked || !mAutoImperium)
            {
                lb_tz_info.Text = $"{DateTime.Now:G}|自动统治-关，{Math.Ceiling(val)}分钟";
                return;
            }

            if (mAutoPirate)
            {
                lb_tz_info.Text = $"{DateTime.Now:G}|已自动海盗，{Math.Ceiling(val)}分钟";
                return;
            }

            if (val > 0)
            {
                lb_tz_info.Text = $"{DateTime.Now:G}|自动统治-开，{Math.Ceiling(val)}分钟";
                return;
            }

            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                lb_tz_info.Text = $"{DateTime.Now:G}|其他操作正忙";
                return;
            }

            if (!Network.IsConnected)
            {
                lb_tz_info.Text = $"{DateTime.Now:G}|网络异常";
                return;
            }

            mIsBusy = true;

            NativeController.Instance.CanNotify = false;
            await TryLogin();
            await AdjustUniverse();
            
            mIsBusy = false;

            await DoImperium(true);
        }

        private async Task DoImperium(bool isAuto = false)
        {
            try
            {
                if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

                NativeController.Instance.SwitchStatus(OperStatus.System);

                await NativeController.Instance.StartImperium(isAuto);
            }
            catch (Exception ex)
            {
                NativeLog.Error($"doImperium catch {ex.Message}");
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private async void btn_tz_start_Click(object sender, EventArgs e)
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                MessageBox.Show("当前不是空闲状态，不能统治");
                return;
            }

            await DoImperium();
        }

        private async Task SetAutoPirateOpen(bool open)
        {
            try
            {
                var msg = open ? "开" : "关";
                mLastContent = $"{DateTime.Now:G}|设置自动海盗({msg})";
                if (OperStatus.None != NativeController.Instance.MyOperStatus)
                {
                    mLastContent = $"{DateTime.Now:G}|当前不是空闲状态，不能设置";
                    return;
                }

                NativeController.Instance.SwitchStatus(OperStatus.System);

                if (cbox_hd_auto.Checked != open)
                {
                    cbox_hd_auto.Checked = open;
                    await Task.Delay(200);
                }

                mLastContent = $"{DateTime.Now:G}|设置自动海盗({msg})完成";
            }
            catch (Exception ex)
            {
                NativeLog.Error($"SetAutoPirateOpen catch {ex.Message}");
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private async Task SetAutoPirateOpen1(bool open)
        {
            try
            {
                var msg = open ? "开" : "关";
                mLastContent = $"{DateTime.Now:G}|设置自动海盗2({msg})";
                if (OperStatus.None != NativeController.Instance.MyOperStatus)
                {
                    mLastContent = $"{DateTime.Now:G}|当前不是空闲状态，不能设置";
                    return;
                }

                NativeController.Instance.SwitchStatus(OperStatus.System);

                if (cbox_hd_auto1.Checked != open)
                {
                    cbox_hd_auto1.Checked = open;
                    await Task.Delay(200);
                }

                mLastContent = $"{DateTime.Now:G}|设置自动海盗2({msg})完成";
            }
            catch (Exception ex)
            {
                NativeLog.Error($"SetAutoPirateOpen1 catch {ex.Message}");
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private async Task SetPirateCfg(int idx)
        {
            try
            {
                mLastContent = $"{DateTime.Now:G}|读取海盗配置（{idx + 1}）";
                if (OperStatus.None != NativeController.Instance.MyOperStatus)
                {
                    mLastContent = $"{DateTime.Now:G}|当前不是空闲状态，不能读取";
                    return;
                }

                NativeController.Instance.SwitchStatus(OperStatus.System);

                //
                // var lastIdx = rbtn_cfg1.Checked ? 1 : 0;
                if (idx == 1)
                {
                    rbtn_cfg1.Checked = true;
                }
                else
                {
                    rbtn_cfg0.Checked = true;
                }
                await Task.Delay(200);

                var ret = PirateCfg();
                if (ret)
                {
                    mLastContent = $"{DateTime.Now:G}|读取海盗配置（{idx + 1}）完成"; ;
                }
                else
                {
                    mLastContent = $"{DateTime.Now:G}|读取海盗配置（{idx + 1}）失败"; ;
                }
            }
            catch (Exception ex)
            {
                NativeLog.Error($"SetPirateCfg catch {ex.Message}");
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private async Task SetAutoExpeditionOpen(bool open)
        {
            try
            {
                var msg = open ? "开" : "关";
                mLastContent = $"{DateTime.Now:G}|设置自动探险({msg})";
                if (OperStatus.None != NativeController.Instance.MyOperStatus)
                {
                    mLastContent = $"{DateTime.Now:G}|当前不是空闲状态，不能设置";
                    return;
                }

                NativeController.Instance.SwitchStatus(OperStatus.System);

                if (cbox_tx_auto.Checked != open)
                {
                    cbox_tx_auto.Checked = open;
                    await Task.Delay(200);
                }
                mLastContent = $"{DateTime.Now:G}|设置自动探险({msg})完成";
            }
            catch (Exception ex)
            {
                NativeLog.Error($"SetAutoPirateOpen catch {ex.Message}");
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private async Task SetAutoExpeditionOpen1(bool open)
        {
            try
            {
                var msg = open ? "开" : "关";
                mLastContent = $"{DateTime.Now:G}|设置自动探险2({msg})";
                if (OperStatus.None != NativeController.Instance.MyOperStatus)
                {
                    mLastContent = $"{DateTime.Now:G}|当前不是空闲状态，不能设置";
                    return;
                }

                NativeController.Instance.SwitchStatus(OperStatus.System);

                if (cbox_tx_auto1.Checked != open)
                {
                    cbox_tx_auto1.Checked = open;
                    await Task.Delay(200);
                }
                mLastContent = $"{DateTime.Now:G}|设置自动探险2({msg})完成";
            }
            catch (Exception ex)
            {
                NativeLog.Error($"SetAutoExpeditionOpen1 catch {ex.Message}");
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private async Task SetExpeditionCfg(int idx)
        {
            try
            {
                mLastContent = $"{DateTime.Now:G}|读取探险配置（{idx + 1}）";
                if (OperStatus.None != NativeController.Instance.MyOperStatus)
                {
                    mLastContent = $"{DateTime.Now:G}|当前不是空闲状态，不能读取";
                    return;
                }

                NativeController.Instance.SwitchStatus(OperStatus.System);

                //
                // var lastIdx = rbtn_cfg1.Checked ? 1 : 0;
                if (idx == 1)
                {
                    rbtn_ex_cfg1.Checked = true;
                }
                else
                {
                    rbtn_ex_cfg0.Checked = true;
                }
                await Task.Delay(200);

                var ret = RevertCfg();
                if (ret)
                {
                    mLastContent = $"{DateTime.Now:G}|读取探险配置（{idx + 1}）完成"; ;
                }
                else
                {
                    mLastContent = $"{DateTime.Now:G}|读取探险配置（{idx + 1}）失败"; ;
                }
            }
            catch (Exception ex)
            {
                NativeLog.Error($"SetExpeditionCfg catch {ex.Message}");
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private async void btn_cross_Click(object sender, EventArgs e)
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                MessageBox.Show("当前不是空闲状态，不能操作");
                return;
            }

            await GoCross();
        }

        private async Task GoCross()
        {
            try
            {
                if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

                NativeController.Instance.SwitchStatus(OperStatus.System);

                await NativeController.Instance.GoCross();
            }
            catch (Exception ex)
            {
                NativeLog.Error($"GoCross catch {ex.Message}");
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private async void btn_universe_Click(object sender, EventArgs e)
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                MessageBox.Show("当前不是空闲状态，不能操作");
                return;
            }

            await BackUniverse();
        }

        private async Task BackUniverse()
        {
            try
            {
                if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

                NativeController.Instance.SwitchStatus(OperStatus.System);

                await NativeController.Instance.BackUniverse();
            }
            catch (Exception ex)
            {
                NativeLog.Error($"BackUniverse catch {ex.Message}");
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private void cbox_auto_logout_CheckedChanged(object sender, EventArgs e)
        {
            var isChecked = cbox_auto_logout.Checked;
            NativeController.User.SetAutoLogout(isChecked);
        }

        private async Task AdjustUniverse()
        {
            try
            {
                if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

                var address = mWebBrowser.Address;
                var npcUniverse = PirateUtil.Universe;

                // 如果是多维
                if (npcUniverse == "w1")
                {
                    // 如果当前就是多维地址
                    if (address.Contains("w1.cicihappy.com/ogame/frames.php"))
                    {
                        return;
                    }

                    // 跳转多维
                    await GoCross();
                }
                else
                {
                    // 如果当前在多维
                    if (address.Contains("w1.cicihappy.com/ogame/frames.php"))
                    {
                        // 返回
                        await BackUniverse();
                    }
                }
            }
            catch (Exception ex)
            {
                NativeLog.Error($"AdjustUniverse catch {ex.Message}");
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private async Task SetAutoLogoutOpen(bool open)
        {
            try
            {
                var msg = open ? "开" : "关";
                mLastContent = $"{DateTime.Now:G}|设置自动退出({msg})";
                if (OperStatus.None != NativeController.Instance.MyOperStatus)
                {
                    mLastContent = $"{DateTime.Now:G}|当前不是空闲状态，不能设置";
                    return;
                }

                NativeController.Instance.SwitchStatus(OperStatus.System);

                if (cbox_auto_logout.Checked != open)
                {
                    cbox_auto_logout.Checked = open;
                    await Task.Delay(200);
                }
                mLastContent = $"{DateTime.Now:G}|设置自动退出({msg})完成";
            }
            catch (Exception ex)
            {
                NativeLog.Error($"SetAutoLogoutOpen catch {ex.Message}");
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private async Task SetAutoImperiumOpen(bool open)
        {
            try
            {
                var msg = open ? "开" : "关";
                mLastContent = $"{DateTime.Now:G}|设置自动统治({msg})";
                if (OperStatus.None != NativeController.Instance.MyOperStatus)
                {
                    mLastContent = $"{DateTime.Now:G}|当前不是空闲状态，不能设置";
                    return;
                }

                NativeController.Instance.SwitchStatus(OperStatus.System);

                if (cbox_tz_auto.Checked != open)
                {
                    cbox_tz_auto.Checked = open;
                    await Task.Delay(200);
                }
                mLastContent = $"{DateTime.Now:G}|设置自动统治({msg})完成";
            }
            catch (Exception ex)
            {
                NativeLog.Error($"SetAutoImperiumOpen catch {ex.Message}");
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private void RedrawQuick()
        {
            string content = "";

            if (mAutoExpedition)
            {
                content = "自动探险1";
            }

            if (mAutoPirate)
            {
                if (!string.IsNullOrWhiteSpace(content))
                {
                    content += "、";
                }
                content += "自动海盗1";
            }

            if (mAutoExpedition1)
            {
                if (!string.IsNullOrWhiteSpace(content))
                {
                    content += "、";
                }
                content += "自动探险2";
            }

            if (mAutoPirate1)
            {
                if (!string.IsNullOrWhiteSpace(content))
                {
                    content += "、";
                }
                content += "自动海盗2";
            }

            content = content.Length > 0 ? content : "无";
            lb_quick_auto.Text = content;
        }

        private void SetQuickBtn(bool enable)
        {
            btn_quick_auto_uncheck.Enabled = enable;
            btn_quick_auto_check.Enabled = enable;
            btn_quick_auto_start.Enabled = enable;
        }

        private void btn_quick_auto_check_Click(object sender, EventArgs e)
        {
            DoQuickAutoCheck();
            RedrawQuick();
        }

        private void btn_quick_auto_uncheck_Click(object sender, EventArgs e)
        {
            DoQuickAutoUncheck();
            RedrawQuick();
        }

        private void btn_quick_auto_start_Click(object sender, EventArgs e)
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                mLastContent = $"{DateTime.Now:G}|一键开始，其他操作正忙";
                NativeLog.Info("一键开始，其他操作正忙");
                return;
            }

            var ret = MessageBox.Show($"确定一键所选自动开始吗", "提示", MessageBoxButtons.YesNo);
            if (ret != DialogResult.Yes)
            {
                NativeLog.Info("一键开始确认框取消");
                return;
            }
            DoQuickAutoStart();
            RedrawQuick();
        }

        private void DoQuickAutoCheck()
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                mLastContent = $"{DateTime.Now:G}|一键打开自动，其他操作正忙";
                NativeLog.Info("一键打开自动，其他操作正忙");
                return;
            }

            NativeController.Instance.SwitchStatus(OperStatus.System);
            mLastContent = $"{DateTime.Now:G}|一键打开自动";
            NativeLog.Info("一键打开自动");
            cbox_tx_auto.Checked = true;
            cbox_hd_auto.Checked = true;
            cbox_tx_auto1.Checked = true;
            cbox_hd_auto1.Checked = true;
            Thread.Sleep(200);
            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private void DoQuickAutoUncheck()
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                mLastContent = $"{DateTime.Now:G}|一键关闭自动，其他操作正忙";
                NativeLog.Info("一键关闭自动，其他操作正忙");
                return;
            }

            NativeController.Instance.SwitchStatus(OperStatus.System);
            mLastContent = $"{DateTime.Now:G}|一键关闭自动";
            NativeLog.Info("一键关闭自动");
            cbox_tx_auto.Checked = false;
            cbox_hd_auto.Checked = false;
            cbox_tx_auto1.Checked = false;
            cbox_hd_auto1.Checked = false;
            Thread.Sleep(200);
            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private void DoQuickAutoStart()
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                mLastContent = $"{DateTime.Now:G}|一键开始，其他操作正忙";
                NativeLog.Info("一键开始，其他操作正忙");
                return;
            }

            NativeController.Instance.SwitchStatus(OperStatus.System);
            mLastContent = $"{DateTime.Now:G}|一键所选配置自动开始";
            NativeLog.Info("一键所选配置自动开始");
            NativeController.Instance.QuickClearLastTime();
            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private void btn_test_Click(object sender, EventArgs e)
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                NativeLog.Info("正忙，不能测试");
                return;
            }
            NativeController.Instance.CanNotify = false;
            NativeController.Instance.StartScanUser();
        }

        private async Task SetPirateSpeed(int index)
        {
            try
            {
                if (OperStatus.None != NativeController.Instance.MyOperStatus)
                {
                    mLastContent = $"{DateTime.Now:G}|当前不是空闲状态，不能设置";
                    return;
                }

                if (index < 0 || index >= ShipSpeed.SpeedLists.Count)
                {
                    mLastContent = $"{DateTime.Now:G}|设置海盗速度不符合{index}";
                    return;
                }

                var text = ShipSpeed.SpeedLists[index];
                mLastContent = $"{DateTime.Now:G}|设置海盗速度({text}%)";

                NativeController.Instance.SwitchStatus(OperStatus.System);
                hd_speed_cb.SelectedIndex = index;
                await Task.Delay(200);
                
                mLastContent = $"{DateTime.Now:G}|设置海盗速度({text}%)完成";
            }
            catch (Exception ex)
            {
                NativeLog.Error($"SetPirateSpeed catch {ex.Message}");
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private void hd_speed_cb_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idx = hd_speed_cb.SelectedIndex;
            if (idx < 0)
            {
                idx = 0;
                hd_speed_cb.SelectedIndex = idx;
            }
            NativeController.Instance.PirateSpeedIndex = idx;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SystemSleep.ResotreSleep();
        }

        private async Task SetMorningIdle(bool open)
        {
            try
            {
                var msg = open ? "开" : "关";
                mLastContent = $"{DateTime.Now:G}|设置凌晨空闲({msg})";
                if (OperStatus.None != NativeController.Instance.MyOperStatus)
                {
                    mLastContent = $"{DateTime.Now:G}|当前不是空闲状态，不能设置";
                    return;
                }

                NativeController.Instance.SwitchStatus(OperStatus.System);

                if (cbox_morning_idle.Checked != open)
                {
                    cbox_morning_idle.Checked = open;
                    await Task.Delay(200);
                }
                mLastContent = $"{DateTime.Now:G}|设置凌晨空闲({msg})完成";
            }
            catch (Exception ex)
            {
                NativeLog.Error($"SetMorningIdle catch {ex.Message}");
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private void cbox_morning_idle_CheckedChanged(object sender, EventArgs e)
        {
            var isChecked = cbox_morning_idle.Checked;
            NativeController.User.SetMorningIdle(isChecked);
        }

        private async Task SetAutoTransferOpen(bool open)
        {
            try
            {
                var msg = open ? "开" : "关";
                mLastContent = $"{DateTime.Now:G}|设置自动转运资源({msg})";
                if (OperStatus.None != NativeController.Instance.MyOperStatus)
                {
                    mLastContent = $"{DateTime.Now:G}|当前不是空闲状态，不能设置";
                    return;
                }

                NativeController.Instance.SwitchStatus(OperStatus.System);

                if (cbox_auto_transfer.Checked != open)
                {
                    cbox_auto_transfer.Checked = open;
                    await Task.Delay(200);
                }
                mLastContent = $"{DateTime.Now:G}|自动转运资源({msg})完成";
            }
            catch (Exception ex)
            {
                NativeLog.Error($"SetAutoTransferOpen catch {ex.Message}");
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private async Task DoAutoTransfer(int index = 0)
        {
            var now = DateTime.Now;

            if (!NativeController.Instance.IsAutoTransfer) return;
            if (now.Hour < 20) return;
            if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

            DateTime lastTime = NativeController.Instance.LastTransferTime;

            if (index != 0)
            {
                lastTime = NativeController.Instance.LastTransferTime1;
            }

            if (lastTime.Date.Equals(now.Date))
            {
                return;
            }

            mIsBusy = true;
            NativeLog.Info($"自动转运资源：{index}");

            NativeController.Instance.CanNotify = false;
            await TryLogin();

            var isPlanetCross = NativeController.Instance.MyPlanet.Universe.Contains("w1");
            var isCross = (index != 0);

            if (!NativeController.Instance.MyPlanet.HasData || isCross != isPlanetCross)
            {
                await NativeController.Instance.DoRefreshNpc(true);
                if (!NativeController.Instance.MyPlanet.HasData)
                {
                    await NativeController.Instance.DoRefreshNpc(true);
                }
            }

            mIsBusy = false;
            DoTransfer(true);
        }
        
        private async void DoTransfer(bool isAuto = false)
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus) return;
            NativeController.Instance.CanNotify = false;
            if (!isAuto)
            {
                await TryLogin();
            }
            NativeController.Instance.StartTransfer();
            Redraw();
        }

        private void btn_transfer_Click(object sender, EventArgs e)
        {
            DoTransfer();
        }

        private void cbox_auto_transfer_CheckedChanged(object sender, EventArgs e)
        {
            NativeController.Instance.IsAutoTransfer = cbox_auto_transfer.Checked;
        }

        private void btn_tx_interval_Click(object sender, EventArgs e)
        {
            var txt = txt_tx_interval.Text.Trim();
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

            mExpeditionInterval = interval;
            lb_tx_interval.Text = mExpeditionInterval.ToString();
            Expedition.SetInterval(interval, 0);
        }

        private void btn_tx_interval1_Click(object sender, EventArgs e)
        {
            var txt = txt_tx_interval1.Text.Trim();
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

            mExpeditionInterval1 = interval;
            lb_tx_interval1.Text = mExpeditionInterval1.ToString();
            Expedition.SetInterval(interval, 1);
        }
    }
}
