using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using CefSharp;

namespace feeling
{
    class DownloadHandler : IDownloadHandler
    {
        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            WebBrowser ie = new WebBrowser();
            ie.Navigate(downloadItem.Url);
            browser.CloseBrowser(false);
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            downloadItem.IsCancelled = true;
        }
    }
}
