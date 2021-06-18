using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ogame
{
    public partial class Readme : Form
    {
        public Readme()
        {
            InitializeComponent();

            String strTip = "登录账号(建议使用刷图专用小号)后手动切换到要开始刷图的星系页面，例如1:1页面。";
            strTip += "按F1键开始或者结束自动刷图，刷图结果保存在工具目录下的ux.csv中,x是当前宇宙。";
            strTip += "星图过旧重新刷图时，需要手动删除旧的ux.csv，否则数据会追加在旧文件的后面。";
            strTip += "注意GM有安全验证机制，单个号建议刷3个系后换号，或者24小时后接着刷。更稳妥的是换号的同时重启路由器，可分配到新的动态ip(不确定GM是否会监控同一个ip的查图次数)";
            strTip += "1:1-3:499、4:1-6:499、7:1-9:499，为了防止账号被安全验证，工具一次只会自动刷1498页(3个系)，然后会自动停止。换号后按F1继续。";
            strTip += "工具每3.5秒自动翻页，使用工具期间建议不要使用电脑，避免网络或者电脑性能不足解析网页失败（一般是不会），最小化工具、关闭显示器等均可正常刷图";
            richTextBox1.Text = strTip;
        }
    }
}
