namespace ogame
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.btnSave = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label4 = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.btnText = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.labelPage = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.labelU = new System.Windows.Forms.Label();
            this.btnReadme = new System.Windows.Forms.Button();
            this.gbStarMap = new System.Windows.Forms.GroupBox();
            this.btnStartViewMap = new System.Windows.Forms.Button();
            this.gbHaiDao = new System.Windows.Forms.GroupBox();
            this.btnStartHaiDao = new System.Windows.Forms.Button();
            this.textHaiDao = new System.Windows.Forms.RichTextBox();
            this.gbStarMap.SuspendLayout();
            this.gbHaiDao.SuspendLayout();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(1153, 685);
            this.webBrowser1.TabIndex = 0;
            this.webBrowser1.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser1_DocumentCompleted);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(75, 135);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(52, 23);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 3500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "工作状态：";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(77, 22);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(29, 12);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "停止";
            // 
            // btnText
            // 
            this.btnText.Location = new System.Drawing.Point(139, 135);
            this.btnText.Name = "btnText";
            this.btnText.Size = new System.Drawing.Size(52, 23);
            this.btnText.TabIndex = 4;
            this.btnText.Text = "打开";
            this.btnText.UseVisualStyleBackColor = true;
            this.btnText.Click += new System.EventHandler(this.btnText_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 3;
            this.label5.Text = "当前页面：";
            // 
            // labelPage
            // 
            this.labelPage.AutoSize = true;
            this.labelPage.Location = new System.Drawing.Point(77, 43);
            this.labelPage.Name = "labelPage";
            this.labelPage.Size = new System.Drawing.Size(17, 12);
            this.labelPage.TabIndex = 3;
            this.labelPage.Text = "无";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(592, 688);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(0, 12);
            this.label7.TabIndex = 3;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 64);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 12);
            this.label8.TabIndex = 3;
            this.label8.Text = "当前宇宙：";
            // 
            // labelU
            // 
            this.labelU.AutoSize = true;
            this.labelU.Location = new System.Drawing.Point(77, 64);
            this.labelU.Name = "labelU";
            this.labelU.Size = new System.Drawing.Size(17, 12);
            this.labelU.TabIndex = 3;
            this.labelU.Text = "无";
            // 
            // btnReadme
            // 
            this.btnReadme.Location = new System.Drawing.Point(457, 834);
            this.btnReadme.Name = "btnReadme";
            this.btnReadme.Size = new System.Drawing.Size(75, 23);
            this.btnReadme.TabIndex = 1;
            this.btnReadme.Text = "使用说明";
            this.btnReadme.UseVisualStyleBackColor = true;
            this.btnReadme.Click += new System.EventHandler(this.btnReadme_Click);
            // 
            // gbStarMap
            // 
            this.gbStarMap.Controls.Add(this.btnStartViewMap);
            this.gbStarMap.Controls.Add(this.label5);
            this.gbStarMap.Controls.Add(this.btnText);
            this.gbStarMap.Controls.Add(this.label4);
            this.gbStarMap.Controls.Add(this.labelU);
            this.gbStarMap.Controls.Add(this.label8);
            this.gbStarMap.Controls.Add(this.btnSave);
            this.gbStarMap.Controls.Add(this.labelPage);
            this.gbStarMap.Controls.Add(this.labelStatus);
            this.gbStarMap.Location = new System.Drawing.Point(17, 698);
            this.gbStarMap.Name = "gbStarMap";
            this.gbStarMap.Size = new System.Drawing.Size(197, 159);
            this.gbStarMap.TabIndex = 5;
            this.gbStarMap.TabStop = false;
            this.gbStarMap.Text = "刷图";
            // 
            // btnStartViewMap
            // 
            this.btnStartViewMap.Location = new System.Drawing.Point(11, 135);
            this.btnStartViewMap.Name = "btnStartViewMap";
            this.btnStartViewMap.Size = new System.Drawing.Size(52, 23);
            this.btnStartViewMap.TabIndex = 5;
            this.btnStartViewMap.Text = "开始";
            this.btnStartViewMap.UseVisualStyleBackColor = true;
            this.btnStartViewMap.Click += new System.EventHandler(this.btnStartViewMap_Click);
            // 
            // gbHaiDao
            // 
            this.gbHaiDao.Controls.Add(this.textHaiDao);
            this.gbHaiDao.Controls.Add(this.btnStartHaiDao);
            this.gbHaiDao.Location = new System.Drawing.Point(241, 698);
            this.gbHaiDao.Name = "gbHaiDao";
            this.gbHaiDao.Size = new System.Drawing.Size(200, 161);
            this.gbHaiDao.TabIndex = 6;
            this.gbHaiDao.TabStop = false;
            this.gbHaiDao.Text = "海盗";
            // 
            // btnStartHaiDao
            // 
            this.btnStartHaiDao.Location = new System.Drawing.Point(54, 136);
            this.btnStartHaiDao.Name = "btnStartHaiDao";
            this.btnStartHaiDao.Size = new System.Drawing.Size(75, 23);
            this.btnStartHaiDao.TabIndex = 0;
            this.btnStartHaiDao.Text = "一键海盗";
            this.btnStartHaiDao.UseVisualStyleBackColor = true;
            // 
            // textHaiDao
            // 
            this.textHaiDao.Dock = System.Windows.Forms.DockStyle.Top;
            this.textHaiDao.Location = new System.Drawing.Point(3, 17);
            this.textHaiDao.Name = "textHaiDao";
            this.textHaiDao.ReadOnly = true;
            this.textHaiDao.Size = new System.Drawing.Size(194, 113);
            this.textHaiDao.TabIndex = 1;
            this.textHaiDao.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1153, 862);
            this.Controls.Add(this.gbHaiDao);
            this.Controls.Add(this.gbStarMap);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnReadme);
            this.Controls.Add(this.webBrowser1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "情怀";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.gbStarMap.ResumeLayout(false);
            this.gbStarMap.PerformLayout();
            this.gbHaiDao.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button btnText;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label labelPage;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label labelU;
        private System.Windows.Forms.Button btnReadme;
        private System.Windows.Forms.GroupBox gbStarMap;
        private System.Windows.Forms.Button btnStartViewMap;
        private System.Windows.Forms.GroupBox gbHaiDao;
        private System.Windows.Forms.RichTextBox textHaiDao;
        private System.Windows.Forms.Button btnStartHaiDao;
    }
}

