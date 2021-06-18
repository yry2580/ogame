using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ogame
{
    public partial class Form1 : Form
    {
        const int SM_LEFT = 1028;
        const int SM_TOP = 363;
        const int SM_RIGHT = 1114;
        const int SM_BOTTOM = 421;
        const string SM_BEGIN_TEXT = "<TH height=30 width=30><A";           // 玩家坐标有效数据的起始标记
        const string SM_STEP_TEXT = "</TR>";
        const string SM_POS_START_TEXT = ">星球 ";
        const string SM_POS_END_TEXT = "</td>";
        const string SM_NAME_START_TEXT = ">玩家 ";
        const string SM_SUN_BEGIN_FLAG_TEXT = "<TD class=c colSpan=8>太阳系 ";   // 太阳系起始标记
        const string SM_SUN_END_FLAG_TEXT = "</TD></TR>";                        // 太阳系结束标记

        //const int SM_STATUS_1 = 1;  // 正常遍历，小坐标从前往后，例如1:1到1:499
        //const int SM_STATUS_2 = 2;  // 小坐标到了499，大坐标加1，例如到了1:499后，跳到2:499
        //const int SM_STATUS_3 = 3;  // 大坐标加1后，从后往前遍历小坐标，例如到了2:499后，从2:499到2:1
        //const int SM_STATUS_4 = 4;  // 小坐标反向遍历到1后，大坐标加1。例如到了2:1后，跳到3:1。此后状态回到SM_STATUS_1继续循环。

        //const int SM_SAME_SUN_MAX_TIMES = 5;    // 同一个太阳系重复刷新5次说明已经刷图到末尾不能翻页了

        private Hashtable m_tStarMap = new Hashtable();
        private int m_nTime1Tick = 0;
        //private int m_nStatus = SM_STATUS_1;
        private string m_strLastPos = "";           // 上次的太阳系
        //private int m_nSamePosTimes = 0;            // 刷同一个太阳系的次数，3次刷到同一个太阳系认为已经扫描完所有星图
        private string m_strUName;                  // 宇宙名词

        public Form1()
        {
            InitializeComponent();
            webBrowser1.Navigate("http://u18.cicihappy.com/ogame");
        }

        // 开始/停止刷图
        private void btnStartViewMap_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                // 当前已开始则停止
                StopViewMap();
            }
            else
            {
                // 开始刷图
                StartViewMap();
            }
        }

        // 排序并保存
        private void btnSave_Click(object sender, EventArgs e)
        {
            SortFile();
        }

        // 使用说明
        private void btnReadme_Click(object sender, EventArgs e)
        {
            Readme rdForm = new Readme();
            rdForm.Show();
        }

        // 打开星图文件
        private void btnText_Click(object sender, EventArgs e)
        {
            // 打开到星图文件目录
            OpenStarMapFile();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            m_nTime1Tick += 1;
            labelStatus.Text = string.Format("已开始，第{0}次查询", m_nTime1Tick);

            if (m_nTime1Tick >= 1498)
            {
                // 1498次后建议换号，否则可能会被GM弹安全验证
                Complete();
                labelStatus.Text = "请换号继续";
                return;
            }

            if ("9:499" == m_strLastPos)
            {
                // 刷到最后一页了
                Complete();
                labelStatus.Text = "已完成";
                return;
            }

            // 取星图子页面
            HtmlWindowCollection frames = webBrowser1.Document.Window.Frames;
            if (frames.Count == 0)
                return;

            HtmlWindow frameWindow = null;
            try
            {
                frameWindow = frames["Hauptframe"];
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return;
            }

            HtmlDocument htmlDoc = frameWindow.Document;
            if (null == htmlDoc)
                return;

            // 银河系
            HtmlElement eGalaxy = htmlDoc.GetElementById("galaxy");
            if (null == eGalaxy)
                return;

            // 太阳系
            HtmlElement eSun = htmlDoc.GetElementById("system");
            if (null == eSun)
                return;

            // 翻页按钮
            HtmlElement eRightBtn = htmlDoc.GetElementById("systemRight");
            if (null == eRightBtn)
                return;

            string[] arr = m_strLastPos.Split(':');
            if (2 != arr.Length)
                return;

            if ("499" == arr[1])
            {
                // 某个银河系到了最后一页，自动修正到下一个银河系的第一个太阳系，例如到了1:499后自动修正到2:1

                // 银河系+1
                eGalaxy.SetAttribute("value", (Convert.ToInt32(arr[0]) + 1).ToString());

                // 太阳系设置到1
                eSun.SetAttribute("value", "1");

                // 显示当前页的按钮，这个按钮没有id，没有name，需要用遍历的方式获取
                HtmlElementCollection eInputs = htmlDoc.GetElementsByTagName("input");
                for (int i = 0; i < eInputs.Count; i++)
                {
                    if (eInputs[i].GetAttribute("value") == "显示")
                    {
                        // 模拟点击，显示下个银河系的第一个太阳系
                        eInputs[i].InvokeMember("Click");
                        return;
                    }

                }
            }

            // 自动点击翻页按钮，显示下一个太阳系
            eRightBtn.InvokeMember("Click");
        }

        // 解析页面，游戏页面用了iframe子页面来实现，因此不能直接解析需要先获取到对应的子页面
        private void GetWebContent()
        {
            HtmlWindowCollection frames = webBrowser1.Document.Window.Frames;
            if (frames.Count == 0)
                return;

            HtmlWindow frameWindow = null;
            try
            {
                // 通过研究网页代码，发现我们需要的子页面名称是Hauptframe
                frameWindow = frames["Hauptframe"];
            }
            catch(Exception e)
            {
                // 取不到Hauptframe子页面时返回
                Console.WriteLine(e.Message);
                return;
            }

            HtmlDocument htmlDoc = frameWindow.Document;
            if (null == htmlDoc.Body)
                return;

            string strText = htmlDoc.Body.InnerHtml;
            if (String.IsNullOrEmpty(strText))
                return;

            // 打印网页内容，方便调试
            //Console.Write(strText);


            // 获取当前解析的太阳系，例如1:100:x中的1:100
            int nSunPos = strText.IndexOf(SM_SUN_BEGIN_FLAG_TEXT);
            if (nSunPos < 0)
                return;
            int nSunEnd = strText.IndexOf(SM_SUN_END_FLAG_TEXT, nSunPos + SM_SUN_BEGIN_FLAG_TEXT.Length);
            if (nSunEnd < 0)
                return;
            string strSun = strText.Substring(nSunPos + SM_SUN_BEGIN_FLAG_TEXT.Length, nSunEnd - (nSunPos + SM_SUN_BEGIN_FLAG_TEXT.Length));

            // 根据当前太阳系设置下一自动状态
            do
            {
                if (string.IsNullOrEmpty(strSun))
                    break;

                labelPage.Text = strSun;
                m_strLastPos = strSun;

                string[] arrPos = strSun.Split(':');
                if (2 != arrPos.Length)
                    break;
/*旧代码，移动鼠标点击按钮的旧方案时用来处理银河系、太阳系到边界后的翻页
                if (SM_STATUS_1 == m_nStatus)
                {
                    // 正常遍历，小坐标从前往后，例如1: 1到1: 499
                    if ("499" != arrPos[1])
                        // 没到当前星系最后一页
                        break;

                    // 到了最后一页，变更状态
                    m_nStatus = SM_STATUS_2;
                    break;
                }

                if (SM_STATUS_2 == m_nStatus)
                {
                    // 小坐标到了499，大坐标加1，例如到了1:499后，跳到2:499
                    // 该状态只持续1页，获取2:499的数据后马上变更到下一状态
                    m_nStatus = SM_STATUS_3;
                    break;
                }

                if (SM_STATUS_3 == m_nStatus)
                {
                    // 大坐标加1后，从后往前遍历小坐标，例如到了2:499后，从2:499到2:1
                    if ("1" != arrPos[1])
                        // 还没到星系的第一页
                        break;

                    // 到了第一页，变更状态
                    m_nStatus = SM_STATUS_4;
                    break;
                }

                if (SM_STATUS_4 == m_nStatus)
                {
                    // 小坐标反向遍历到1后，大坐标加1。例如到了2:1后，跳到3:1。此后状态回到SM_STATUS_1继续循环。
                    m_nStatus = SM_STATUS_1;
                    break;
                }
*/
            } while (false);

            // 获取玩家坐标起始位置，从这里开始解析每个太阳系最多15个玩家坐标
            int nPos = strText.IndexOf(SM_BEGIN_TEXT);
            if (nPos < 0)
                return;
            strText = strText.Substring(nPos);

            List<string> lStar = new List<string>();
            nPos = strText.IndexOf(SM_STEP_TEXT);
            while (nPos > 0)
            {
                string strStar = strText.Substring(0, nPos + SM_STEP_TEXT.Length);
                lStar.Add(strStar);
                if (lStar.Count == 15)
                    // 每页最多15个坐标
                    break;

                strText = strText.Substring(nPos + SM_STEP_TEXT.Length);
                nPos = strText.IndexOf(SM_STEP_TEXT);
            }

            string strPos = "";
            string strName = "";
            foreach (string str in lStar)
            {
                GetSingleInfo(str, ref strPos, ref strName);

                if (string.IsNullOrEmpty(strPos) || string.IsNullOrEmpty(strName))
                    continue;

                m_tStarMap[strPos] = strName;
            }
        }

        private void GetSingleInfo(string strText ,ref string strPos, ref string strName)
        {
            strPos = "";
            strName = "";

            if (String.IsNullOrEmpty(strText))
                return;

            // 找坐标
            int nPos = strText.IndexOf(SM_POS_START_TEXT);
            if (nPos <= 0)
                return;

            int nPosEnd = strText.IndexOf(SM_POS_END_TEXT, nPos);
            if (nPosEnd <= 0)
                return;

            // >星球 殖民地 [1:298:1]
            string strTemp = strText.Substring(nPos, nPosEnd - nPos);

            nPos = strTemp.IndexOf('[');
            nPosEnd = strTemp.IndexOf(']');
            if (nPos < 0 || nPosEnd < 0)
                return;

            strPos = strTemp.Substring(nPos + 1, nPosEnd - nPos - 1);

            // 找玩家名字
            nPos = strText.IndexOf(SM_NAME_START_TEXT);
            if (nPos <= 0)
                return;

            nPosEnd = strText.IndexOf(SM_POS_END_TEXT, nPos);
            if (nPosEnd <= 0)
                return;

            // mm778899 排名 235
            strTemp = strText.Substring(nPos + SM_NAME_START_TEXT.Length, nPosEnd - (nPos + SM_NAME_START_TEXT.Length) - 1);

            nPos = strTemp.IndexOf(" 排名 ");
            if (nPos < 0)
                return;

            strName = strTemp.Substring(0, nPos);
        }

        // 处理快捷键
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                // 按下F1
                if (timer1.Enabled)
                {
                    // 当前已开启刷图，按F1是停止
                    StopViewMap();
                }
                else
                {
                    // 开始刷图
                    StartViewMap();
                }

                return true;
            }
            else if (keyData == Keys.F2)
            {
                SortFile();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void WriteLocal()
        {
            if (m_tStarMap.Count == 0)
                return;

            // 文件名是宇宙.csv，例如u18.csv
            FileStream fs = new FileStream(GetFileName(), FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            int i = 0;
            String strText = "";
            foreach (string key in m_tStarMap.Keys)
            {
                strText += string.Format("{0},{1}\r", key, m_tStarMap[key]);
                if (++i == 100)
                {
                    // 每100条写一次本地文件
                    sw.Write(strText);
                    i = 0;
                    strText = "";
                }
            }

            sw.Write(strText);
            sw.Flush();
            sw.Close();
            fs.Close();

            m_tStarMap.Clear();
        }

        public void SortFile()
        {
            // 将本地文件全部取出来加工整理一下
            // 1是需要去掉重复的(网络卡顿时可能重复刷到同一页的星图)，2是做一下排序
            string strFile = GetFileName();
            if (string.IsNullOrEmpty(strFile))
                return;

            // 写本地文件
            WriteLocal();

            try
            {
                Hashtable map = new Hashtable();
                using (StreamReader sr = new StreamReader(GetFileName()))
                {
                    string line, point, name;

                    while ((line = sr.ReadLine()) != null)
                    {
                        int nPos = line.IndexOf(',');
                        if (nPos <= 0)
                            continue;

                        point = line.Substring(0, nPos);
                        name = line.Substring(nPos + 1);
                        map[point] = name;
                    }
                }

                if (map.Count == 0)
                    return;

                using (StreamWriter sw = new StreamWriter(GetFileName()))
                {
                    string[] keyArray = new string[map.Count];
                    map.Keys.CopyTo(keyArray, 0);
                    Array.Sort(keyArray);
                    int i = 0;
                    String strText = "";
                    foreach(string key in keyArray)
                    {
                        strText += string.Format("{0},{1}\r", key, map[key].ToString());
                        if (++i == 100)
                        {
                            // 每100条写一次本地文件
                            sw.Write(strText);
                            i = 0;
                            strText = "";
                        }
                    }

                    if (!string.IsNullOrEmpty(strText))
                        sw.Write(strText);

                    sw.Flush();
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }

        public void OpenStarMapFile()
        {
            // 打开到当前目录
            System.Diagnostics.Process.Start(System.IO.Directory.GetCurrentDirectory());
        }

        // 开始刷图
        public void StartViewMap()
        {
            timer1.Enabled = true;
            timer1.Start();
            m_nTime1Tick = 0;

            // 按钮变更为停止
            btnStartViewMap.Text = "停止";

            // 获取当前页的数据
            GetWebContent();
        }

        // 停止刷图
        public void StopViewMap()
        {
            timer1.Stop();
            timer1.Enabled = false;

            // 按钮变更为开始
            labelStatus.Text = "停止";

            btnStartViewMap.Text = "开始";

            // 停止扫描时将已经扫描的数据写本地
            SortFile();
        }
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            String url = webBrowser1.Url.ToString();
            int nPos = url.IndexOf(".cicihappy.com/ogame/frames.php");
            if (nPos > 0)
            {
                string strUName = url.Substring(0, nPos);
                if (strUName.IndexOf("http://") == 0)
                {
                    m_strUName = strUName.Substring("http://".Length);
                    labelU.Text = m_strUName;
                }
            }

            GetWebContent();
        }

        // 星图扫描完成
        private void Complete()
        {
            timer1.Stop();
            timer1.Enabled = false;
            labelStatus.Text = "已完成";

            SortFile();
        }

        private string GetFileName()
        {
            string strFile = string.Format("{0}.csv", m_strUName);
            return strFile;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 关闭界面时主动保存一次
            SortFile();
        }
    }
}
