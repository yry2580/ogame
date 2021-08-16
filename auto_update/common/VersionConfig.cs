using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auto_update
{
    public class VersionConfig
    {
        public int Major = 0;
        public int Minor = 0;
        public int Patch = 0;

        public uint Version
        {
            get
            {
                return uint.Parse(Desc, System.Globalization.NumberStyles.HexNumber);
            }
        }

        public string Desc
        {
            get
            {
                return $"{Major:d2}{Minor:d2}{Patch:d2}";
            }
        }

        public List<string> VersionList = new List<string>();
        public bool ParseVersion(string version)
        {
            if (version == null || version.Length != 6) return false;

            Major = int.Parse(version.Substring(0, 2));
            Minor = int.Parse(version.Substring(2, 2));
            Patch = int.Parse(version.Substring(4, 2));

            return true;
        }

        public string ProdDesc()
        {
            return $"{Major:d2}{Minor:d2}{0:d2}";
        }
    }
}
