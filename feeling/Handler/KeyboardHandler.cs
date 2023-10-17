using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;

namespace feeling
{
    class KeyboardHandler : IKeyboardHandler
    {
        public bool OnKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
        {
            if (type == KeyType.KeyUp && Enum.IsDefined(typeof(Keys), windowsKeyCode))
            {
                var key = (Keys)windowsKeyCode;
                switch (key)
                {
                    case Keys.F12:
                        browser.ShowDevTools();
                        break;

                    case Keys.F5:

                        if (modifiers == CefEventFlags.ControlDown)
                        {
                            //MessageBox.Show("ctrl+f5");
                            browser.Reload(true); //强制忽略缓存

                        }
                        else
                        {
                            //MessageBox.Show("f5");
                            browser.Reload();
                        }
                        break;


                }
            }
            return false;
        }

        public bool OnPreKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
        {
            /*            switch(windowsKeyCode)
                        {
                            case 0x74:
                                if (KeyType.RawKeyDown == type)
                                {
                                    browser.Reload();
                                }
                                break;
                            default:
                                break;
                        }
            */
            return false;
        }
    }
}
