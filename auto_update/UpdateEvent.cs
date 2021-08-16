using System;
using System.Collections.Generic;
using System.Text;

namespace auto_update
{
    public delegate void AutoUpdateEvent(UpdateEventArgs args);
    public class UpdateEventArgs
    {
        public UpdateStatus Status = UpdateStatus.Init;
        public string Msg = "";
    }

    public enum UpdateStatus
    {
        Init = 0,
        NoPatch,
        CloseProcess,
        CloseProcessFailed,
        Download,
        DownloadFailed,
        Unzip,
        UnzipFailed,
        Finish,
    }
}
