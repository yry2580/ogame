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
            script += "doc.querySelector(\"#menuTable .menubutton_table a[title='概 况']\").click();";
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
        /// 设置速度
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string SetSpeed(int index)
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += "var se = doc.querySelector(\"select[name='speed']\");";
            script += $"se.selectedIndex={index};se.onchange();";
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
        /// 攻击
        /// </summary>
        /// <returns></returns>
        public static string SetAttack()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"input[name='mission'][value='1']\").click()";
            return script;
        }

        /// <summary>
        /// 攻击确认
        /// </summary>
        /// <returns></returns>
        public static string SetAttackConfirm(int type = 0)
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            if (type == 1)
            {
                script += $"doc.querySelector(\"#tjsubmit input[value='继续']\").click()";
            }
            else if(type == 2)
            {
                script += $"doc.querySelector(\"#tjsubmit input[type='submit']\").click()";
            }
            else if(type == 3)
            {
                script += $"doc.querySelector(\"#tjsubmit input[name='submit1']\").click()";
            }
            else
            {
                script += $"doc.querySelector(\"#tjsubmit input\").click()";
            }
            
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

        public static string GetCode()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"form input[value = '点击发送微信验证码']\").click()";
            return script;
        }

        public static string AuthCode(string code)
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"form input[name='overviewcode']\").value='{code}'";
            return script;
        }

        public static string SubmitCode()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"form input[value='提交']\").click()";
            return script;
        }

        public static string ToImperium()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"#header_top a[href='imperium.php']\").click()";
            return script;
        }

        public static string ToImperiumDetail()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"form[action='imperium.php'] input[type='submit'][value='查看详情']\").click()";
            return script;
        }

        public static string ToCross()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"a[href='dwenter.php']\").click()";
            return script;
        }

        public static string BackUniverse()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"a[href*='.cicihappy.com/ogame/frames.php'][href*='http://u']\").click()";
            return script;
        }

        public static string ToRank()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += "doc.querySelector(\"#header_top a[href*='stat.php']\").click()";
            return script;
        }

        public static string SelectRank(int index)
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += "var se = doc.querySelector(\"#statform select[name='range']\");";
            script += $"se.selectedIndex={index};se.onchange();";
            return script;
        }

        public static string ToSearch()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += "doc.querySelector(\"#header_top a[href*='search.php']\").click()";
            return script;
        }

        public static string SearchUser(string name)
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"form[action='search.php'] input[name='searchtext']\").value='{name}'";
            return script;
        }

        public static string SearchUserSubmit()
        {
            var script = "doc = window.parent.frames['Hauptframe'].document;";
            script += $"doc.querySelector(\"form[action='search.php'] input[type='submit']\").click()";
            return script;
        }
    }
}
