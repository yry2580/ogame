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
        int mPirateInterval = 120; // 分

        bool mAutoImperium = false;
        int mImperiumInterval = 240; // 分

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
                Thread.Sleep(1000 * 60);
            } while (true);
        }

        private async Task _LookThread()
        {
            try
            {
                if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

                await doAutoExpedtion();
                await doAutoPirate();
                await doAutoImperium();
                SendData();
            }
            catch (Exception ex)
            {
                NativeLog.Error($"_LookThread {ex.Message}");
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
            cbox_auto_login.Checked = NativeController.User.AutoLogin;

            InitExpedition();
            RedrawAccount();
            InitImperium();
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

            var pMissionCfg = PirateUtil.MyMission;

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
            mPirateInterval = interval < 120 ? 120 : interval;
            lb_hd_interval.Text = mPirateInterval.ToString();

            return true;
        }

        protected bool RevertCfg()
        {
            var cfgIdx = rbtn_ex_cfg1.Checked ? 1 : 0;
            // 读取配置
            if (!Expedition.ReadCfg(cfgIdx)) return false;


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
                        cbox_auto_login.Enabled = false;
                        break;
                    case OperStatus.Galaxy:
                        btn_galaxy_start.Enabled = false;
                        btn_galaxy_stop.Enabled = true;
                        SetExpeditionButton(false);
                        SetPirateButton(false);
                        btn_tz_start.Enabled = false;
                        btn_cross.Enabled = false;
                        btn_universe.Enabled = false;
                        cbox_auto_login.Enabled = false;
                        break;
                    case OperStatus.Expedition:
                        btn_galaxy_start.Enabled = false;
                        btn_galaxy_stop.Enabled = false;
                        SetExpeditionButton(false, true);
                        SetPirateButton(false);
                        btn_tz_start.Enabled = false;
                        btn_cross.Enabled = false;
                        btn_universe.Enabled = false;
                        cbox_auto_login.Enabled = false;
                        break;
                    case OperStatus.Pirate:
                        btn_galaxy_start.Enabled = false;
                        btn_galaxy_stop.Enabled = false;
                        SetExpeditionButton(false);
                        SetPirateButton(false, true);
                        btn_tz_start.Enabled = false;
                        btn_cross.Enabled = false;
                        btn_universe.Enabled = false;
                        cbox_auto_login.Enabled = false;
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
                        cbox_auto_login.Enabled = true;
                        break;
                }

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
            cbox_auto_login.Enabled = enabled;
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
                SendData(CmdEnum.Hello);
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
                    case CmdEnum.PirateCfg:
                        NativeController.Instance.CanNotify = false;
                        await SetPirateCfg(data.PirateCfgIndex);
                        break;
                    case CmdEnum.AutoExpeditionOpen:
                        NativeController.Instance.CanNotify = false;
                        await SetAutoExpeditionOpen(data.AutoExpeditionOpen);
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
                    case CmdEnum.AutoLoginOpen:
                        NativeController.Instance.CanNotify = false;
                        await SetAutoLoginOpen(data.AutoLoginOpen);
                        break;
                    case CmdEnum.AutoImperiumOpen:
                        NativeController.Instance.CanNotify = false;
                        await SetAutoImperiumOpen(data.AutoImperiumOpen);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogUtil.Error($"feeling OnServerReceived catch {ex.Message}");
            }

            SendData();
        }
#endif


        private void SendData(CmdEnum cmd = CmdEnum.Data)
        {
#if !NET45
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
                Cmd = cmd,
                Status = status,
                Content = mLastContent,
                ExpeditionAutoMsg = lb_tx_info.Text.Trim(),
                PirateAutoMsg = lb_hd_info.Text.Trim(),
                FleetContent = fleetContent,
                AutoPirateOpen = mAutoPirate,
                AutoExpeditionOpen = mAutoExpedition,
                PirateCfgIndex = rbtn_cfg1.Checked ? 1 : 0,
                Universe = universe,
                ExpeditionCfgIndex = rbtn_ex_cfg1.Checked ? 1 : 0,
                NpcUniverse = PirateUtil.Universe,
                PlanetUniverse = NativeController.Instance.MyPlanet.Universe,
                AutoLoginOpen = NativeController.User.AutoLogin,
                AutoImperiumOpen = mAutoImperium,
            };

            mClient?.SendData(gameData);
#endif
        }

        private void OnServerConnected()
        {
            SendData(CmdEnum.Auth);
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
#if !NET45
                LogUtil.Error($"TryLogin catch {ex.Message}");
#endif
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
#if !NET45
                LogUtil.Error($"TryGetAuth catch {ex.Message}");
#endif
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
#if !NET45
                LogUtil.Error($"TryAuthCode catch {ex.Message}");
#endif
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
#if !NET45
                LogUtil.Error($"TryLogout catch {ex.Message}");
#endif
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
                return;
            }

            var idx = rbtn_ex_cfg1.Checked ? 1 : 0;
            var ret = MessageBox.Show($"确定保存配置 {idx + 1} 吗", "提示", MessageBoxButtons.YesNo);
            if (ret == DialogResult.Yes)
            {
                Expedition.Save(exMission, idx);
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

        private void xbox_auto_CheckedChanged(object sender, EventArgs e)
        {
            NativeController.Instance.IsAutoExpedition = cbox_tx_auto.Checked;
            mAutoExpedition = cbox_tx_auto.Checked;
        }

        private void doExpedtion(bool autoLogin = false)
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

            var exMission = GetExMission();
            
            NativeController.Instance.StartExpedition(exMission, autoLogin);
            Redraw();
        }

        private async Task doAutoExpedtion()
        {
            var delta = DateTime.Now - NativeController.Instance.LastExeditionTime;
            var val = 120 - delta.TotalMinutes;
            val = val < 0 ? 0 : val;

            if (OperStatus.Expedition == NativeController.Instance.MyOperStatus)
            {
                lb_tx_info.Text = $"{DateTime.Now:G}|正在探险状态";
                return;
            }

            if (!cbox_tx_auto.Checked || !mAutoExpedition)
            {
                lb_tx_info.Text = $"{DateTime.Now:G}|自动探险-关，{Math.Ceiling(val)}分钟";
                return;
            }

            if (delta.TotalMinutes < 120)
            {
                lb_tx_info.Text = $"{DateTime.Now:G}|自动探险-开，{Math.Ceiling(val)}分钟";
                return;
            }

            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                lb_tx_info.Text = $"{DateTime.Now:G}|其他操作正忙";
                return;
            }

            var autoLogin = NativeController.User.AutoLogin;
            if (autoLogin)
            {
                NativeController.Instance.CanNotify = false;
                await TryLogin();
                await AdjustUniverse();

                var exMission = GetExMission();
                if (exMission.List.Count <= 0 || NativeController.Instance.MyPlanet.List.Count <= 0)
                {
                    await NativeController.Instance.DoRefreshNpc(true);
                }
            }

            doExpedtion(autoLogin);
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

        private void doPirate(bool isAuto = false, bool autoLogin = false)
        {
            if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

            var pMission = GetPirateMission();
            NativeController.Instance.StartPirate(pMission, isAuto, autoLogin);
            Redraw();
        }

        private void btn_hd_save_Click(object sender, EventArgs e)
        {
            var idx = rbtn_cfg1.Checked ? 1 : 0;
            var ret = MessageBox.Show($"确定保存配置 {idx + 1} 吗", "提示", MessageBoxButtons.YesNo);
            if (ret == DialogResult.Yes)
            {
                var pMission = GetPirateMission();
                pMission.Interval = mPirateInterval;
                PirateUtil.Save(pMission, idx);
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

        private async Task doAutoPirate()
        {
            if (OperStatus.Pirate == NativeController.Instance.MyOperStatus)
            {
                lb_hd_info.Text = $"{DateTime.Now:G}|正在海盗状态";
                return;
            }

            var delta = DateTime.Now - NativeController.Instance.LastPirateTime;
            var val = mPirateInterval - delta.TotalMinutes;
            val = val < 0 ? 0 : val;

            if (!cbox_hd_auto.Checked || !mAutoPirate)
            {
                lb_hd_info.Text = $"{DateTime.Now:G}|自动海盗-关，{Math.Ceiling(val)}分钟";
                return;
            }

            if (delta.TotalMinutes < mPirateInterval)
            {
                lb_hd_info.Text = $"{DateTime.Now:G}|自动海盗-开，{Math.Ceiling(val)}分钟";
                return;
            }

            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                lb_hd_info.Text = $"{DateTime.Now:G}|其他操作正忙";
                return;
            }

            var autoLogin = NativeController.User.AutoLogin;
            if (autoLogin)
            {
                NativeController.Instance.CanNotify = false;
                await TryLogin();
                await AdjustUniverse();

                var pMission = GetPirateMission();
                if (pMission.List.Count <= 0 || NativeController.Instance.MyPlanet.List.Count <= 0)
                {
                    await NativeController.Instance.DoRefreshNpc(true);
                }
            }

            doPirate(true, autoLogin);
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

            if (delta.TotalMinutes < mImperiumInterval)
            {
                lb_tz_info.Text = $"{DateTime.Now:G}|自动统治-开，{Math.Ceiling(val)}分钟";
                return;
            }

            if (OperStatus.None != NativeController.Instance.MyOperStatus)
            {
                lb_tz_info.Text = $"{DateTime.Now:G}|其他操作正忙";
                return;
            }

            var autoLogin = NativeController.User.AutoLogin;
            if (autoLogin)
            {
                NativeController.Instance.CanNotify = false;
                await TryLogin();
                await AdjustUniverse();
            }

            await DoImperium(autoLogin);
        }

        private async Task DoImperium(bool autoLogin = false)
        {
            try
            {
                if (OperStatus.None != NativeController.Instance.MyOperStatus) return;

                NativeController.Instance.SwitchStatus(OperStatus.System);

                await NativeController.Instance.StartImperium(autoLogin);
            }
            catch (Exception ex)
            {
#if !NET45
                LogUtil.Error($"doImperium catch {ex.Message}");
#endif
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
#if !NET45
                LogUtil.Error($"SetAutoPirateOpen catch {ex.Message}");
#endif
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
#if !NET45
                LogUtil.Error($"SetPirateCfg catch {ex.Message}");
#endif
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
#if !NET45
                LogUtil.Error($"SetAutoPirateOpen catch {ex.Message}");
#endif
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
#if !NET45
                LogUtil.Error($"SetExpeditionCfg catch {ex.Message}");
#endif
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
#if !NET45
                LogUtil.Error($"GoCross catch {ex.Message}");
#endif
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
#if !NET45
                LogUtil.Error($"BackUniverse catch {ex.Message}");
#endif
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private void cbox_auto_login_CheckedChanged(object sender, EventArgs e)
        {
            var autoLogin = cbox_auto_login.Checked;
            NativeController.User.SetAutoLogin(autoLogin);
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
#if !NET45
                LogUtil.Error($"AdjustUniverse catch {ex.Message}");
#endif
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }

        private async Task SetAutoLoginOpen(bool open)
        {
            try
            {
                var msg = open ? "开" : "关";
                mLastContent = $"{DateTime.Now:G}|设置自动登录({msg})";
                if (OperStatus.None != NativeController.Instance.MyOperStatus)
                {
                    mLastContent = $"{DateTime.Now:G}|当前不是空闲状态，不能设置";
                    return;
                }

                NativeController.Instance.SwitchStatus(OperStatus.System);

                if (cbox_auto_login.Checked != open)
                {
                    cbox_auto_login.Checked = open;
                    await Task.Delay(200);
                }
                mLastContent = $"{DateTime.Now:G}|设置自动登录({msg})完成";
            }
            catch (Exception ex)
            {
#if !NET45
                LogUtil.Error($"SetAutoLoginOpen catch {ex.Message}");
#endif
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
#if !NET45
                LogUtil.Error($"SetAutoImperiumOpen catch {ex.Message}");
#endif
            }

            NativeController.Instance.SwitchStatus(OperStatus.None);
        }
    }
}
