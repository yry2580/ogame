﻿using System;
using System.Collections.Generic;
using System.Text;

namespace feeling
{
    class NativeScript
    {
        public static string ToGalaxy()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += "doc.querySelector('#menuTable .menubutton_table a[title=外部银河]').click();";
            return script;
        }

        public static string ToHome()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += "doc.querySelectorAll('#menuTable .menubutton_table a')[0].click();";
            return script;
        }

        public static string ToLogout()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += "doc.querySelector('#header_top a[accesskey=s]').click()";
            return script;
        }

        public static string TestMenuTable()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += "doc.querySelectorAll('#menuTable .menubutton_table a');";
            return script;
        }

        public static string ReturnScript(string script)
        {
            return $"(function() {{ return {script}; }})();";
        }

        public static string RefreshGalaxy(int x, int y)
        {
            var script = $"document.querySelectorAll('#galaxy_form table input[type=text]')[0].value = {x};";
            script += $"document.querySelectorAll('#galaxy_form table input[type=text]')[1].value= {y};";
            script += $"document.querySelectorAll('#galaxy_form table input[type=submit]')[0].click();";
            return script;
        }
    }
}
