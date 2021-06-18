using System;
using System.Collections.Generic;
using System.Text;

namespace feeling
{
    class HtmlUtil
    {
        public static string HtmlSpace = "&nbsp;";

        public static string ParseText(string source, string separator = "")
        {
            if (string.IsNullOrWhiteSpace(source)) return "";

            if (HtmlSpace == source) return "";

            if (!string.IsNullOrEmpty(separator))
            {
                int idx = source.IndexOf(separator);
                if (idx >= 0)
                {
                    source = source.Substring(0, idx);
                }
            }
            return source.Trim();
        }
    }
}
