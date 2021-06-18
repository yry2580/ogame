
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
            this.w_split_container = new System.Windows.Forms.SplitContainer();
            this.w_galaxy_box = new System.Windows.Forms.GroupBox();
            this.w_galaxy_status_lb = new System.Windows.Forms.Label();
            this.w_galaxy_status = new System.Windows.Forms.Label();
            this.w_galaxy_page = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.w_galaxy_universe = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btn_galaxy_start = new System.Windows.Forms.Button();
            this.btn_galaxy_stop = new System.Windows.Forms.Button();
            this.btn_galaxy_save = new System.Windows.Forms.Button();
            this.btn_galaxy_open = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.w_split_container)).BeginInit();
            this.w_split_container.Panel2.SuspendLayout();
            this.w_split_container.SuspendLayout();
            this.w_galaxy_box.SuspendLayout();
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
            this.w_galaxy_box.Location = new System.Drawing.Point(12, 10);
            this.w_galaxy_box.Name = "w_galaxy_box";
            this.w_galaxy_box.Size = new System.Drawing.Size(221, 172);
            this.w_galaxy_box.TabIndex = 0;
            this.w_galaxy_box.TabStop = false;
            this.w_galaxy_box.Text = "星图";
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
            // w_galaxy_status
            // 
            this.w_galaxy_status.AutoSize = true;
            this.w_galaxy_status.Location = new System.Drawing.Point(77, 28);
            this.w_galaxy_status.Name = "w_galaxy_status";
            this.w_galaxy_status.Size = new System.Drawing.Size(17, 12);
            this.w_galaxy_status.TabIndex = 1;
            this.w_galaxy_status.Text = "无";
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
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1153, 861);
            this.Controls.Add(this.w_split_container);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "情怀";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.w_split_container.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.w_split_container)).EndInit();
            this.w_split_container.ResumeLayout(false);
            this.w_galaxy_box.ResumeLayout(false);
            this.w_galaxy_box.PerformLayout();
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
    }
}

