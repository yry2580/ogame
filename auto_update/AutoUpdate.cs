using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;

namespace auto_update
{
    public class AutoUpdate
    {
        public static AutoUpdateEvent UpdateEvent;

        static VersionConfig mNetVer;
        static VersionConfig mLocalVer;

        static readonly string mAutoUpdateFile = GetDir() + "autoupdate.log";
        static string mPatchDir = GetDir() + "patch/";
        static string mPatchName = "";
        static string mZipPath = "";

        public static string GetDir()
        {
            string dir;
            try
            {
                dir = AppDomain.CurrentDomain.BaseDirectory;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetDir {ex.Message}");
                dir = Environment.CurrentDirectory;
            }

            return dir;
        }

        public static VersionConfig GetLocalVersion(UpdateConfig config)
        {
            if (!File.Exists(config.VersionLocalPath)) return null;

            try
            {
                var text = File.ReadAllText(config.VersionLocalPath);
                mLocalVer = JsonConvert.DeserializeObject<VersionConfig>(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetLocalVersion catch {ex.Message}");
            }
            
            return mLocalVer;
        }

        public static VersionConfig GetNetVersion(UpdateConfig config)
        {
            try
            {
                if (ResUtil.ReadNetFile(config.VersionUrl, out string content))
                {
                    mNetVer = JsonConvert.DeserializeObject<VersionConfig>(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetNetVersion catch {ex.Message}");
            }

            return mNetVer;
        }

        public static bool NeedUpdate(UpdateConfig config)
        {
            if (null == config) return false;

            var localVerCfg = GetLocalVersion(config);
            if (null == localVerCfg) return false;

            var netVerCfg = GetNetVersion(config);
            if (null == netVerCfg) return false;

            if (!config.IsUpdater)
            {
                // 补丁名称
                mPatchName = $"patch_{localVerCfg.Desc}.zip";

                // 补丁保存路径
                mZipPath = mPatchDir + mPatchName;

                if (File.Exists(mZipPath))
                {
                    try
                    {
                        UnzipPatch();
                        File.Delete(mZipPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"UnzipPatch {ex.Message}");
                    }
                }
            }

            var result = false;

            if (config.IgnoreDebug)
            {
                if (localVerCfg.Major > netVerCfg.Major)
                {
                    result = false;
                }
                else if (localVerCfg.Major < netVerCfg.Major)
                {
                    result = true;
                }
                else if (localVerCfg.Minor < netVerCfg.Minor)
                {
                    result = true;
                }
            }
            else
            {
                result = localVerCfg.Version < netVerCfg.Version;
            }
            
            if (result)
            {
                var str = $"{DateTime.Now:yyyyMMdd}-{mNetVer.Desc}";
                var flag = GetAutoUpdateFlag();
                if (flag.Contains(str))
                {
                    return false;
                }
            }
            
            return result;
        }

        protected static void FireEvent(UpdateStatus status, string msg = "")
        {
            var args = new UpdateEventArgs
            {
                Status = status,
                Msg = msg,
            };

            UpdateEvent?.Invoke(args);
        }

        public static int HandleUpdate(UpdateConfig config)
        {
            config.IsUpdater = true;
            Console.WriteLine("自动更新");
            FireEvent(UpdateStatus.Init);
            if (!NeedUpdate(config))
            {
                Console.WriteLine("暂不需要自动更新");
                FireEvent(UpdateStatus.NoPatch);
                return -1;
            }

            Console.WriteLine("关闭进程");
            FireEvent(UpdateStatus.CloseProcess);
            if (!SureRunning(config))
            {
                Console.WriteLine("关闭进程失败");
                FireEvent(UpdateStatus.CloseProcessFailed);
                return -1;
            }

            CreateAutoUpdateFlag();
            Console.WriteLine("下载补丁");
            FireEvent(UpdateStatus.Download);
            if (!DownloadPatch(config))
            {
                Console.WriteLine("下载失败");
                FireEvent(UpdateStatus.DownloadFailed);
                return 1;
            }

            Console.WriteLine("解压补丁");
            FireEvent(UpdateStatus.Unzip);
            if (!UnzipPatch())
            {
                Console.WriteLine("解压补丁失败");
                FireEvent(UpdateStatus.UnzipFailed);
                return 2;
            }

            Console.WriteLine("更新完成");
            FireEvent(UpdateStatus.Finish);
            return 0;
        }

        public static bool HasAutoUpdateFlag()
        {
            return File.Exists(mAutoUpdateFile);
        }

        public static string GetAutoUpdateFlag()
        {
            string result = "";
            try
            {
                if (!File.Exists(mAutoUpdateFile)) return result;
                var rb = File.ReadAllBytes(mAutoUpdateFile);
                result = Encoding.UTF8.GetString(rb);
            }
            catch (Exception)
            {
            }

            return result;
        }

        public static void CreateAutoUpdateFlag()
        {
            try
            {
                var content = $"{DateTime.Now:yyyyMMdd}-{mNetVer.Desc}";
                byte[] bytes = Encoding.UTF8.GetBytes(content);
                File.WriteAllBytes(mAutoUpdateFile, bytes);
            }
            catch (Exception)
            {

            }
        }

        public static void DelAutoUpdateFlag()
        {
            try
            {
                if (HasAutoUpdateFlag())
                {
                    File.Delete(mAutoUpdateFile);
                }
            }
            catch (Exception)
            {
            }
        }

        protected static bool SureRunning(UpdateConfig config)
        {
            var processLists = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(config.AppName));

            if (processLists.Length > 0)
            {
                foreach (Process p in processLists)
                {

                    p.Kill();
                }
            }

            return true;
        }

        protected static bool DownloadPatch(UpdateConfig config)
        {
            var versionDesc = GetPatchVersionDesc(config);
            if (string.IsNullOrEmpty(versionDesc)) return false;

            // 补丁名称
            mPatchName = $"patch_{versionDesc}.zip";

            // 补丁保存路径
            mZipPath = mPatchDir + mPatchName;

            string dir = config.PatchNetDir;
            if (!dir.EndsWith("/"))
            {
                dir += "/";
            }

            var url = $"{dir}{mPatchName}";

            return ResUtil.DownloadFile(url, mZipPath);
        }

        protected static string GetPatchVersionDesc(UpdateConfig config)
        {
            string result = "";
            if (null == mNetVer) return result;

            var lists = mNetVer.VersionList;
            if (null == lists || lists.Count <= 0) return result;

            if (config.IgnoreDebug)
            {
                result = mNetVer.ProdDesc();
                if (lists.Contains(result)) return result;
            }
            result = mNetVer.Desc;
            return result;
        }

        protected static bool UnzipPatch()
        {
            bool result = false;
            try
            {
                if (!File.Exists(mZipPath)) return false;

                ZipArchive zipArchive = ZipFile.OpenRead(mZipPath);
                var curDir = GetDir();
                foreach (ZipArchiveEntry file in zipArchive.Entries)
                {
                    string fileFullName = Path.Combine(curDir, file.FullName);
                    string dir = Path.GetDirectoryName(fileFullName);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    if (file.Name == "")
                    {
                        continue;
                    }

                    try
                    {
                        file.ExtractToFile(fileFullName, true);
                    }
                    catch (Exception)
                    {
                        // nothing
                    }
                }
                zipArchive.Dispose();
                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UnzipPatch catch {ex.Message}");
            }

            return result;
        }
    }
}
