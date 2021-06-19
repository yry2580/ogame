﻿
namespace feeling
{
    partial class MainForm
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.w_split_container = new System.Windows.Forms.SplitContainer();
            this.w_galaxy_box = new System.Windows.Forms.GroupBox();
            this.btn_galaxy_open = new System.Windows.Forms.Button();
            this.btn_galaxy_save = new System.Windows.Forms.Button();
            this.btn_galaxy_stop = new System.Windows.Forms.Button();
            this.btn_galaxy_start = new System.Windows.Forms.Button();
            this.w_galaxy_universe = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.w_galaxy_page = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.w_galaxy_status = new System.Windows.Forms.Label();
            this.w_galaxy_status_lb = new System.Windows.Forms.Label();
            this.w_user_box = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.w_user_password = new System.Windows.Forms.TextBox();
            this.w_user_universe = new System.Windows.Forms.TextBox();
            this.btn_user_login = new System.Windows.Forms.Button();
            this.btn_user_logout = new System.Windows.Forms.Button();
            this.w_user_account = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.w_split_container)).BeginInit();
            this.w_split_container.Panel2.SuspendLayout();
            this.w_split_container.SuspendLayout();
            this.w_galaxy_box.SuspendLayout();
            this.w_user_box.SuspendLayout();
            this.SuspendLayout();
            // 
            // w_split_container
            // 
            this.w_split_container.Dock = System.Windows.Forms.DockStyle.Fill;
            this.w_split_container.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.w_split_container.Location = new System.Drawing.Point(0, 0);
            this.w_split_container.Name = "w_split_container";
            this.w_split_container.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // w_split_container.Panel2
            // 
            this.w_split_container.Panel2.Controls.Add(this.w_user_box);
            this.w_split_container.Panel2.Controls.Add(this.w_galaxy_box);
            this.w_split_container.Size = new System.Drawing.Size(1153, 861);
            this.w_split_container.SplitterDistance = 666;
            this.w_split_container.TabIndex = 0;
            // 
            // w_galaxy_box
            // 
            this.w_galaxy_box.Controls.Add(this.btn_galaxy_open);
            this.w_galaxy_box.Controls.Add(this.btn_galaxy_save);
            this.w_galaxy_box.Controls.Add(this.btn_galaxy_stop);
            this.w_galaxy_box.Controls.Add(this.btn_galaxy_start);
            this.w_galaxy_box.Controls.Add(this.w_galaxy_universe);
            this.w_galaxy_box.Controls.Add(this.label4);
            this.w_galaxy_box.Controls.Add(this.w_galaxy_page);
            this.w_galaxy_box.Controls.Add(this.label2);
            this.w_galaxy_box.Controls.Add(this.w_galaxy_status);
            this.w_galaxy_box.Controls.Add(this.w_galaxy_status_lb);
            this.w_galaxy_box.Location = new System.Drawing.Point(226, 7);
            this.w_galaxy_box.Name = "w_galaxy_box";
            this.w_galaxy_box.Size = new System.Drawing.Size(221, 172);
            this.w_galaxy_box.TabIndex = 0;
            this.w_galaxy_box.TabStop = false;
            this.w_galaxy_box.Text = "星图";
            // 
            // btn_galaxy_open
            // 
            this.btn_galaxy_open.Location = new System.Drawing.Point(167, 143);
            this.btn_galaxy_open.Name = "btn_galaxy_open";
            this.btn_galaxy_open.Size = new System.Drawing.Size(50, 22);
            this.btn_galaxy_open.TabIndex = 9;
            this.btn_galaxy_open.Text = "打开";
            this.btn_galaxy_open.UseVisualStyleBackColor = true;
            this.btn_galaxy_open.Click += new System.EventHandler(this.btn_galaxy_open_Click);
            // 
            // btn_galaxy_save
            // 
            this.btn_galaxy_save.Location = new System.Drawing.Point(113, 143);
            this.btn_galaxy_save.Name = "btn_galaxy_save";
            this.btn_galaxy_save.Size = new System.Drawing.Size(50, 22);
            this.btn_galaxy_save.TabIndex = 8;
            this.btn_galaxy_save.Text = "保存";
            this.btn_galaxy_save.UseVisualStyleBackColor = true;
            this.btn_galaxy_save.Click += new System.EventHandler(this.btn_galaxy_save_Click);
            // 
            // btn_galaxy_stop
            // 
            this.btn_galaxy_stop.Location = new System.Drawing.Point(59, 143);
            this.btn_galaxy_stop.Name = "btn_galaxy_stop";
            this.btn_galaxy_stop.Size = new System.Drawing.Size(50, 22);
            this.btn_galaxy_stop.TabIndex = 7;
            this.btn_galaxy_stop.Text = "停止";
            this.btn_galaxy_stop.UseVisualStyleBackColor = true;
            this.btn_galaxy_stop.Click += new System.EventHandler(this.btn_galaxy_stop_Click);
            // 
            // btn_galaxy_start
            // 
            this.btn_galaxy_start.Location = new System.Drawing.Point(4, 143);
            this.btn_galaxy_start.Name = "btn_galaxy_start";
            this.btn_galaxy_start.Size = new System.Drawing.Size(50, 22);
            this.btn_galaxy_start.TabIndex = 6;
            this.btn_galaxy_start.Text = "开始";
            this.btn_galaxy_start.UseVisualStyleBackColor = true;
            this.btn_galaxy_start.Click += new System.EventHandler(this.btn_galaxy_start_Click);
            // 
            // w_galaxy_universe
            // 
            this.w_galaxy_universe.AutoSize = true;
            this.w_galaxy_universe.Location = new System.Drawing.Point(77, 88);
            this.w_galaxy_universe.Name = "w_galaxy_universe";
            this.w_galaxy_universe.Size = new System.Drawing.Size(17, 12);
            this.w_galaxy_universe.TabIndex = 5;
            this.w_galaxy_universe.Text = "无";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "当前宇宙：";
            // 
            // w_galaxy_page
            // 
            this.w_galaxy_page.AutoSize = true;
            this.w_galaxy_page.Location = new System.Drawing.Point(77, 57);
            this.w_galaxy_page.Name = "w_galaxy_page";
            this.w_galaxy_page.Size = new System.Drawing.Size(17, 12);
            this.w_galaxy_page.TabIndex = 3;
            this.w_galaxy_page.Text = "无";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "当前页面：";
            // 
            // w_galaxy_status
            // 
            this.w_galaxy_status.AutoSize = true;
            this.w_galaxy_status.Location = new System.Drawing.Point(77, 28);
            this.w_galaxy_status.Name = "w_galaxy_status";
            this.w_galaxy_status.Size = new System.Drawing.Size(17, 12);
            this.w_galaxy_status.TabIndex = 1;
            this.w_galaxy_status.Text = "无";
            // 
            // w_galaxy_status_lb
            // 
            this.w_galaxy_status_lb.AutoSize = true;
            this.w_galaxy_status_lb.Location = new System.Drawing.Point(6, 28);
            this.w_galaxy_status_lb.Name = "w_galaxy_status_lb";
            this.w_galaxy_status_lb.Size = new System.Drawing.Size(65, 12);
            this.w_galaxy_status_lb.TabIndex = 0;
            this.w_galaxy_status_lb.Text = "当前状态：";
            // 
            // w_user_box
            // 
            this.w_user_box.Controls.Add(this.w_user_account);
            this.w_user_box.Controls.Add(this.btn_user_logout);
            this.w_user_box.Controls.Add(this.btn_user_login);
            this.w_user_box.Controls.Add(this.w_user_universe);
            this.w_user_box.Controls.Add(this.w_user_password);
            this.w_user_box.Controls.Add(this.label5);
            this.w_user_box.Controls.Add(this.label3);
            this.w_user_box.Controls.Add(this.label1);
            this.w_user_box.Location = new System.Drawing.Point(9, 7);
            this.w_user_box.Name = "w_user_box";
            this.w_user_box.Size = new System.Drawing.Size(211, 172);
            this.w_user_box.TabIndex = 1;
            this.w_user_box.TabStop = false;
            this.w_user_box.Text = "用户";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 10;
            this.label1.Text = "账 号:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "宇 宙：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 57);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 12);
            this.label5.TabIndex = 12;
            this.label5.Text = "密 码：";
            // 
            // w_user_password
            // 
            this.w_user_password.Location = new System.Drawing.Point(53, 54);
            this.w_user_password.MaxLength = 20;
            this.w_user_password.Name = "w_user_password";
            this.w_user_password.Size = new System.Drawing.Size(152, 21);
            this.w_user_password.TabIndex = 14;
            this.w_user_password.UseSystemPasswordChar = true;
            this.w_user_password.WordWrap = false;
            // 
            // w_user_universe
            // 
            this.w_user_universe.Location = new System.Drawing.Point(53, 85);
            this.w_user_universe.MaxLength = 2;
            this.w_user_universe.Name = "w_user_universe";
            this.w_user_universe.Size = new System.Drawing.Size(26, 21);
            this.w_user_universe.TabIndex = 15;
            this.w_user_universe.WordWrap = false;
            this.w_user_universe.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.w_user_universe_KeyPress);
            // 
            // btn_user_login
            // 
            this.btn_user_login.Location = new System.Drawing.Point(8, 142);
            this.btn_user_login.Name = "btn_user_login";
            this.btn_user_login.Size = new System.Drawing.Size(75, 23);
            this.btn_user_login.TabIndex = 16;
            this.btn_user_login.Text = "登 录";
            this.btn_user_login.UseVisualStyleBackColor = true;
            this.btn_user_login.Click += new System.EventHandler(this.btn_user_login_Click);
            // 
            // btn_user_logout
            // 
            this.btn_user_logout.Location = new System.Drawing.Point(130, 142);
            this.btn_user_logout.Name = "btn_user_logout";
            this.btn_user_logout.Size = new System.Drawing.Size(75, 23);
            this.btn_user_logout.TabIndex = 17;
            this.btn_user_logout.Text = "退 出";
            this.btn_user_logout.UseVisualStyleBackColor = true;
            this.btn_user_logout.Click += new System.EventHandler(this.btn_user_logout_Click);
            // 
            // w_user_account
            // 
            this.w_user_account.FormattingEnabled = true;
            this.w_user_account.Location = new System.Drawing.Point(53, 25);
            this.w_user_account.Name = "w_user_account";
            this.w_user_account.Size = new System.Drawing.Size(152, 20);
            this.w_user_account.TabIndex = 18;
            this.w_user_account.SelectedIndexChanged += new System.EventHandler(this.w_user_account_SelectedIndexChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1153, 861);
            this.Controls.Add(this.w_split_container);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "情怀";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.w_split_container.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.w_split_container)).EndInit();
            this.w_split_container.ResumeLayout(false);
            this.w_galaxy_box.ResumeLayout(false);
            this.w_galaxy_box.PerformLayout();
            this.w_user_box.ResumeLayout(false);
            this.w_user_box.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer w_split_container;
        private System.Windows.Forms.GroupBox w_galaxy_box;
        private System.Windows.Forms.Label w_galaxy_status_lb;
        private System.Windows.Forms.Label w_galaxy_status;
        private System.Windows.Forms.Label w_galaxy_universe;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label w_galaxy_page;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_galaxy_start;
        private System.Windows.Forms.Button btn_galaxy_stop;
        private System.Windows.Forms.Button btn_galaxy_save;
        private System.Windows.Forms.Button btn_galaxy_open;
        private System.Windows.Forms.GroupBox w_user_box;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox w_user_universe;
        private System.Windows.Forms.TextBox w_user_password;
        private System.Windows.Forms.Button btn_user_logout;
        private System.Windows.Forms.Button btn_user_login;
        private System.Windows.Forms.ComboBox w_user_account;
    }
}

