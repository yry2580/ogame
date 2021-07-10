using System;
using System.Collections.Generic;
using System.Text;

namespace feeling
{
    class NativeScript
    {
        public static string ToGalaxy()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += "doc.querySelector(\"#menuTable .menubutton_table a[title='外部银河']\").click();";
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

        public static string ToFleet()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += "doc.querySelector(\"#menuTable .menubutton_table a[title*='舰 队']\").click();";
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

        public static string SelectPlanet(int index)
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += "var se = doc.querySelector('#header_top select');";
            script += $"se.selectedIndex={index};se.onchange();";
            return script;
        }

        public static string SetShip(string shipId, int count)
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\".l input[name='{shipId}']\").value={count};";
            return script;
        }

        public static string SetShipNext()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"input[name=send][value*='继续']\").click();";
            return script;
        }

        public static string SetTarget(int x, int y, int z, int pt)
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"input[name=galaxy]\").value={x};";
            script += $"doc.querySelector(\"input[name=system]\").value={y};";
            script += $"doc.querySelector(\"input[name=planet]\").value={z};";
            script += $"doc.querySelector(\"select[name=planettype] option[value='{pt}']\").selected=true;";
            return script;
        }

        public static string SetTargetNext()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"input[name=send][value*='继续']\").click();";
            return script;
        }

        public static string SetAttackConfirm()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"#tjsubmit input[type=submit]\").click()";
            return script;
        }

        public static string TutorialConfirm()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"#tutorial .tutorial_buttons a\").click()";
            return script;
        }
    }
}
