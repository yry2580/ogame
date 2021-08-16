using auto_update;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace auto_updater
{
    public partial class auto_updater : Form
    {
        public auto_updater()
        {
            InitializeComponent();
            // BibiChatUpdate.UpdateEvent += onUpdateEvent;
            AutoUpdate.UpdateEvent += OnUpdateEvent;
        }

        private void OnUpdateEvent(UpdateEventArgs args)
        {
            try
            {
                switch (args.Status)
                {
                    case auto_update.UpdateStatus.Init:
                        ShowContent("开始更新");
                        break;
                    case auto_update.UpdateStatus.NoPatch:
                        ShowContent("暂不需要更新");
                        break;
                    case auto_update.UpdateStatus.CloseProcess:
                        ShowContent("关闭相关进程");
                        break;
                    case auto_update.UpdateStatus.CloseProcessFailed:
                        ShowContent("关闭相关进程失败");
                        break;
                    case auto_update.UpdateStatus.Download:
                        ShowContent("下载补丁中，请稍候");
                        break;
                    case auto_update.UpdateStatus.DownloadFailed:
                        ShowContent("下载补丁失败");
                        break;
                    case auto_update.UpdateStatus.Unzip:
                        ShowContent("解压安装补丁");
                        break;
                    case auto_update.UpdateStatus.UnzipFailed:
                        ShowContent("补丁解压安装失败");
                        break;
                    case auto_update.UpdateStatus.Finish:
                        ShowContent("安装成功");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        protected void ShowContent(string content)
        {
            Invoke(new Action(delegate
            {
                lb_content.Text = content;
            }));
        }

        protected void UpdatePatch()
        {
            Thread.Sleep(500);

            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var confg = new UpdateConfig
            {
                AppName = "feeling.exe",
                VersionLocalPath = dir + "appsettings.json",
                VersionUrl = "http://bkbibi.teammvp.beer/netcore/feeling/version.json",
                PatchNetDir = "http://bkbibi.teammvp.beer/netcore/feeling/patch/",
                IsUpdater = true,
            };

            AutoUpdate.HandleUpdate(confg);
            Thread.Sleep(100);

            try
            {
                var exeFile = AppDomain.CurrentDomain.BaseDirectory + confg.AppName;
                if (File.Exists(exeFile))
                {
                    Process process = new Process();
                    var startInfo = new ProcessStartInfo(exeFile);
                    startInfo.UseShellExecute = true;
                    process.StartInfo = startInfo;
                    process.Start();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        private void auto_updater_Load(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                UpdatePatch();
            });
        }
    }
}
