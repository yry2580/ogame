using System;
using System.Collections.Generic;
using System.Text;

namespace feeling
{
    class NativeScript
    {
        /// <summary>
        /// 外部银河
        /// </summary>
        /// <returns></returns>
        public static string ToGalaxy()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += "doc.querySelector(\"#menuTable .menubutton_table a[title='外部银河']\").click();";
            return script;
        }

        /// <summary>
        /// 概况
        /// </summary>
        /// <returns></returns>
        public static string ToHome()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += "doc.querySelectorAll('#menuTable .menubutton_table a')[0].click();";
            return script;
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public static string ToLogout()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += "doc.querySelector('#header_top a[accesskey=s]').click()";
            return script;
        }

        /// <summary>
        /// 舰队页
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 刷星图
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static string RefreshGalaxy(int x, int y)
        {
            var script = $"document.querySelectorAll('#galaxy_form table input[type=text]')[0].value = {x};";
            script += $"document.querySelectorAll('#galaxy_form table input[type=text]')[1].value= {y};";
            script += $"document.querySelectorAll('#galaxy_form table input[type=submit]')[0].click();";
            return script;
        }

        /// <summary>
        /// 选择星球
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string SelectPlanet(int index)
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += "var se = doc.querySelector('#header_top select');";
            script += $"se.selectedIndex={index};se.onchange();";
            return script;
        }

        /// <summary>
        /// 设置船只
        /// </summary>
        /// <param name="shipId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string SetShip(string shipId, int count)
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\".l input[name='{shipId}']\").value={count};";
            return script;
        }

        /// <summary>
        /// 设置船只=》继续
        /// </summary>
        /// <returns></returns>
        public static string SetShipNext()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"input[name=send][value*='继续']\").click();";
            return script;
        }

        /// <summary>
        /// 设置目标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static string SetTarget(int x, int y, int z, int pt)
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"input[name=galaxy]\").value={x};";
            script += $"doc.querySelector(\"input[name=system]\").value={y};";
            script += $"doc.querySelector(\"input[name=planet]\").value={z};";
            script += $"doc.querySelector(\"select[name=planettype] option[value='{pt}']\").selected=true;";
            return script;
        }

        /// <summary>
        /// 设置目标=》继续
        /// </summary>
        /// <returns></returns>
        public static string SetTargetNext()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"input[name=send][value*='继续']\").click();";
            return script;
        }

        /// <summary>
        /// 攻击确认
        /// </summary>
        /// <returns></returns>
        public static string SetAttackConfirm()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"#tjsubmit input[type=submit]\").click()";
            return script;
        }

        /// <summary>
        /// 提示确认
        /// </summary>
        /// <returns></returns>
        public static string TutorialConfirm()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"#tutorial .tutorial_buttons a\").click()";
            return script;
        }
    }
}
