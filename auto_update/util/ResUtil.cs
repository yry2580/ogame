using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;

namespace auto_update
{
    class ResUtil
    {
        public static bool DownloadFile(string url, string savePath)
        {
            bool result = false;
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("http")) return false;

            WebResponse response = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                request.CachePolicy = noCachePolicy;
                response = request.GetResponse();
                if (SaveBinaryFile(response, savePath))
                {
                    result = true;
                }
            }
            catch (SystemException ex)
            {
                Console.WriteLine($"DownloadFile exception: {ex.Message}");
            }
            finally
            {
                response?.Close();
            }

            return result;
        }

        protected static bool SaveBinaryFile(WebResponse response, string savePath)
        {
            bool result = false;

            try
            {
                var dir = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                using (var inStream = response.GetResponseStream())
                {
                    IoUtil.WriteToFile(savePath, inStream);
                }

                result = true;
            }
            catch (SystemException ex)
            {
               Console.WriteLine($"SaveBinaryFileexception: {ex.Message}");
            }

            return result;
        }

        public static bool ReadNetFile(string url, out string content)
        {
            bool result = false;
            content = "";
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("http")) return false;

            WebResponse response = null;
            try
            {
                HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.CachePolicy = noCachePolicy;
                request.Timeout = 60 * 1000;
                response = request.GetResponse();
                if (ReadBinaryFile(response, out content))
                {
                    result = true;
                }
                request = null;
            }
            catch (SystemException ex)
            {
                Console.WriteLine($"ReadNetFile exception: {ex.Message}");
            }
            finally
            {
                response?.Close();
            }

            return result;
        }

        protected static bool ReadBinaryFile(WebResponse response, out string content)
        {
            bool result = false;
            content = "";

            try
            {
                using (var inStream = response.GetResponseStream())
                {
                    if (IoUtil.ReadFrom(inStream, out content))
                    {
                        result = true;
                    }
                }
            }
            catch (SystemException ex)
            {
                Console.WriteLine($"ReadBinaryFile exception: {ex.Message}");
            }

            return result;
        }
    }
}
