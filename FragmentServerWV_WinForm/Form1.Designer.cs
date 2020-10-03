namespace FragmentServerWV_WinForm
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.serverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startProxyModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.level1AllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.level3MediumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.level5FewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dumpDecoderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rtb1 = new System.Windows.Forms.RichTextBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.serverToolStripMenuItem,
            this.logToolStripMenuItem,
            this.toolsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(488, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // serverToolStripMenuItem
            // 
            this.serverToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.startProxyModeToolStripMenuItem});
            this.serverToolStripMenuItem.Name = "serverToolStripMenuItem";
            this.serverToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.serverToolStripMenuItem.Text = "Server";
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.startToolStripMenuItem.Text = "Start";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.startToolStripMenuItem_Click);
            // 
            // startProxyModeToolStripMenuItem
            // 
            this.startProxyModeToolStripMenuItem.Name = "startProxyModeToolStripMenuItem";
            this.startProxyModeToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.startProxyModeToolStripMenuItem.Text = "Start Proxy Mode";
            this.startProxyModeToolStripMenuItem.Click += new System.EventHandler(this.startProxyModeToolStripMenuItem_Click);
            // 
            // logToolStripMenuItem
            // 
            this.logToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.level1AllToolStripMenuItem,
            this.level3MediumToolStripMenuItem,
            this.level5FewToolStripMenuItem});
            this.logToolStripMenuItem.Name = "logToolStripMenuItem";
            this.logToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.logToolStripMenuItem.Text = "Log";
            // 
            // level1AllToolStripMenuItem
            // 
            this.level1AllToolStripMenuItem.Checked = true;
            this.level1AllToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.level1AllToolStripMenuItem.Name = "level1AllToolStripMenuItem";
            this.level1AllToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.level1AllToolStripMenuItem.Text = "Level 1 - all";
            this.level1AllToolStripMenuItem.Click += new System.EventHandler(this.level1AllToolStripMenuItem_Click);
            // 
            // level3MediumToolStripMenuItem
            // 
            this.level3MediumToolStripMenuItem.Name = "level3MediumToolStripMenuItem";
            this.level3MediumToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.level3MediumToolStripMenuItem.Text = "Level 3 - medium";
            this.level3MediumToolStripMenuItem.Click += new System.EventHandler(this.level3MediumToolStripMenuItem_Click);
            // 
            // level5FewToolStripMenuItem
            // 
            this.level5FewToolStripMenuItem.Name = "level5FewToolStripMenuItem";
            this.level5FewToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.level5FewToolStripMenuItem.Text = "Level 5 - few";
            this.level5FewToolStripMenuItem.Click += new System.EventHandler(this.level5FewToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dumpDecoderToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // dumpDecoderToolStripMenuItem
            // 
            this.dumpDecoderToolStripMenuItem.Name = "dumpDecoderToolStripMenuItem";
            this.dumpDecoderToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.dumpDecoderToolStripMenuItem.Text = "Dump Decoder";
            this.dumpDecoderToolStripMenuItem.Click += new System.EventHandler(this.dumpDecoderToolStripMenuItem_Click);
            // 
            // rtb1
            // 
            this.rtb1.DetectUrls = false;
            this.rtb1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb1.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.rtb1.Location = new System.Drawing.Point(0, 24);
            this.rtb1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rtb1.Name = "rtb1";
            this.rtb1.ReadOnly = true;
            this.rtb1.Size = new System.Drawing.Size(488, 352);
            this.rtb1.TabIndex = 1;
            this.rtb1.Text = "";
            this.rtb1.WordWrap = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(488, 376);
            this.Controls.Add(this.rtb1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "Form1";
            this.Text = ".hack//frägment server by warranty voider";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.RichTextBox rtb1;
        private System.Windows.Forms.ToolStripMenuItem serverToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem level1AllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem level3MediumToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem level5FewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dumpDecoderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startProxyModeToolStripMenuItem;
    }
}

