namespace UI
{
    partial class Form3
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.나의정보관리ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.예약관리ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.입퇴실ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.로그아웃ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.나의정보관리ToolStripMenuItem,
            this.예약관리ToolStripMenuItem,
            this.입퇴실ToolStripMenuItem,
            this.로그아웃ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(954, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 나의정보관리ToolStripMenuItem
            // 
            this.나의정보관리ToolStripMenuItem.Name = "나의정보관리ToolStripMenuItem";
            this.나의정보관리ToolStripMenuItem.Size = new System.Drawing.Size(99, 20);
            this.나의정보관리ToolStripMenuItem.Text = "나의 정보 관리";
            this.나의정보관리ToolStripMenuItem.Click += new System.EventHandler(this.나의정보관리ToolStripMenuItem_Click);
            // 
            // 예약관리ToolStripMenuItem
            // 
            this.예약관리ToolStripMenuItem.Name = "예약관리ToolStripMenuItem";
            this.예약관리ToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
            this.예약관리ToolStripMenuItem.Text = "예약 관리";
            this.예약관리ToolStripMenuItem.Click += new System.EventHandler(this.예약관리ToolStripMenuItem_Click);
            // 
            // 입퇴실ToolStripMenuItem
            // 
            this.입퇴실ToolStripMenuItem.Name = "입퇴실ToolStripMenuItem";
            this.입퇴실ToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.입퇴실ToolStripMenuItem.Text = "입퇴실";
            this.입퇴실ToolStripMenuItem.Click += new System.EventHandler(this.입퇴실ToolStripMenuItem_Click);
            // 
            // 로그아웃ToolStripMenuItem
            // 
            this.로그아웃ToolStripMenuItem.Name = "로그아웃ToolStripMenuItem";
            this.로그아웃ToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.로그아웃ToolStripMenuItem.Text = "로그아웃";
            this.로그아웃ToolStripMenuItem.Click += new System.EventHandler(this.로그아웃ToolStripMenuItem_Click);
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(954, 621);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form3";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "독서실 키오스크";
            this.Load += new System.EventHandler(this.Form3_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 나의정보관리ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 예약관리ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 입퇴실ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 로그아웃ToolStripMenuItem;
    }
}