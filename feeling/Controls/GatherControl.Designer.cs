
namespace feeling
{
    partial class GatherControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.u_count = new System.Windows.Forms.TextBox();
            this.u_cbox_list = new System.Windows.Forms.CheckedListBox();
            this.u_box = new System.Windows.Forms.GroupBox();
            this.u_combo_box = new System.Windows.Forms.ComboBox();
            this.u_box.SuspendLayout();
            this.SuspendLayout();
            // 
            // u_count
            // 
            this.u_count.Location = new System.Drawing.Point(147, 17);
            this.u_count.Name = "u_count";
            this.u_count.Size = new System.Drawing.Size(41, 21);
            this.u_count.TabIndex = 0;
            this.u_count.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.u_count_KeyPress);
            // 
            // u_cbox_list
            // 
            this.u_cbox_list.FormattingEnabled = true;
            this.u_cbox_list.Location = new System.Drawing.Point(0, 43);
            this.u_cbox_list.Name = "u_cbox_list";
            this.u_cbox_list.Size = new System.Drawing.Size(194, 100);
            this.u_cbox_list.TabIndex = 5;
            // 
            // u_box
            // 
            this.u_box.Controls.Add(this.u_combo_box);
            this.u_box.Controls.Add(this.u_cbox_list);
            this.u_box.Controls.Add(this.u_count);
            this.u_box.Location = new System.Drawing.Point(3, 3);
            this.u_box.Name = "u_box";
            this.u_box.Size = new System.Drawing.Size(194, 144);
            this.u_box.TabIndex = 6;
            this.u_box.TabStop = false;
            this.u_box.Text = "集中";
            // 
            // u_combo_box
            // 
            this.u_combo_box.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.u_combo_box.FormattingEnabled = true;
            this.u_combo_box.Location = new System.Drawing.Point(0, 17);
            this.u_combo_box.Name = "u_combo_box";
            this.u_combo_box.Size = new System.Drawing.Size(141, 20);
            this.u_combo_box.TabIndex = 9;
            // 
            // GatherControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.Controls.Add(this.u_box);
            this.Name = "GatherControl";
            this.Size = new System.Drawing.Size(200, 150);
            this.u_box.ResumeLayout(false);
            this.u_box.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox u_count;
        private System.Windows.Forms.CheckedListBox u_cbox_list;
        private System.Windows.Forms.GroupBox u_box;
        private System.Windows.Forms.ComboBox u_combo_box;
    }
}
