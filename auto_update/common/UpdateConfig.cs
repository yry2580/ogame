using System;
using System.Collections.Generic;
using System.Text;

namespace auto_update
{
    public class UpdateConfig
    {
        public string PatchNetDir = "";
        public string VersionUrl = "";
        public string AppName = "";
        public string VersionLocalPath = "";
        public bool IsUpdater = false;
        public bool IgnoreDebug = false;
    }
}
