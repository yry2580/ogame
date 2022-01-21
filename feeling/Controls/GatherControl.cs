using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace feeling
{
    public partial class GatherControl : UserControl
    {
        protected List<string> mPlanets = new List<string>();
        public List<string> AllOptions { get; private set; } = new List<string>();

        public GatherControl()
        {
            InitializeComponent();
        }

        [Browsable(true)]
        [Description("设置标题的值")]
        public string MyTitle
        {
            get
            {
                return u_box.Text;
            }
            set
            {
                u_box.Text = value;
            }
        }

        [Browsable(true)]
        [Description("设置数量")]
        public int MyCount
        {
            get
            {
                var txt = u_count.Text.Trim();
                if (txt.Length <= 0)
                {
                    return 0;
                }

                return int.Parse(txt);
            }
            set
            {
                u_count.Text = value.ToString();
            }
        }

        public string MyPlanet
        {
            get
            {
                return u_combo_box.Text.Trim();
            }
            set
            {
                var idx = Planet.FindPlanet(value, mPlanets);
                if (idx != -1)
                {
                    u_combo_box.SelectedIndex = idx;
                }
                else
                {
                    u_combo_box.SelectedIndex = mPlanets.Count > 0 ? 0 : -1;
                }
            }
        }


        public List<string> MyOptions
        {
            get
            {
                var arr = new List<string>();
                foreach (string outstr in u_cbox_list.CheckedItems)
                {
                    arr.Add(outstr);
                }
                return arr;
            }

            set
            {
                for (int i = 0; i < AllOptions.Count; i++)
                {
                    if (value.Contains(AllOptions[i]))
                    {
                        u_cbox_list.SetItemChecked(i, true);
                    }
                    else
                    {
                        u_cbox_list.SetItemChecked(i, false);
                    }
                }
            }
        }

        private void u_count_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        public void SetPlanets(List<string> planets)
        {
            var oldVal = MyPlanet;
            mPlanets = planets;
            u_combo_box.Items.Clear();
            planets.ForEach(e =>
            {
                u_combo_box.Items.Add(e);
            });

            MyPlanet = oldVal;
        }

        public void SetAllOptions(List<string> options)
        {
            var oldList = MyOptions;
            AllOptions = options;
            u_cbox_list.Items.Clear();

            options.ForEach(e =>
            {
                u_cbox_list.Items.Add(e);
            });
            MyOptions = oldList;
        }
    }
}
