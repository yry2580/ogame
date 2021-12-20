using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using CefSharp;

namespace feeling
{
    class JsDialogHandler : IJsDialogHandler
    {
        public bool OnBeforeUnloadDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string messageText, bool isReload, IJsDialogCallback callback)
        {
            return true;
        }

        public void OnDialogClosed(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        public bool OnJSDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, CefJsDialogType dialogType, string messageText, string defaultPromptText, IJsDialogCallback callback, ref bool suppressMessage)
        {
            /*switch (dialogType)
            {
                case CefJsDialogType.Alert:
                    NativeLog.Info("Alert");
                    suppressMessage = true;
                    return false;
                case CefJsDialogType.Confirm:
                    NativeLog.Info("Confirm");
                    if (originUrl == NativeConst.Homepage)
                    {
                        callback.Continue(true, "");
                        suppressMessage = false;
                        return true;
                    }
                    break;
                case CefJsDialogType.Prompt:
                    NativeLog.Info("Prompt");
                    break;
                default:
                    break;
            }
            return false;*/
            return true;
        }

        public void OnResetDialogState(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }
    }
}
