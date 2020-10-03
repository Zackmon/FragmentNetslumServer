using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using FragmentServerWV;

namespace FragmentServerWV_WinForm
{
    public partial class Form1 : Form
    {
        private static Form1 instance = null;

        public static Form1 getInstance()
        {
            if (instance == null)
            {
                instance = new Form1();
            }

            return instance;
        }

        public Form1()
        {
            InitializeComponent();
            LogEventDelegate logEventDelegate = new LogEventDelegate();
            logEventDelegate.Logging += WriteToRtb1;
            Config.Load();
            //Log.InitLogs(rtb1);
            Log.InitLogs(logEventDelegate);
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startToolStripMenuItem.Enabled = 
            startProxyModeToolStripMenuItem.Enabled = false;
            Server.Start();
        }

       
        private void level1AllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.LogTreshold = 0;
            level3MediumToolStripMenuItem.Checked =
            level5FewToolStripMenuItem.Checked = false;
            level1AllToolStripMenuItem.Checked = true;
        }

        private void level3MediumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.LogTreshold = 2;
            level1AllToolStripMenuItem.Checked =
            level5FewToolStripMenuItem.Checked = false;
            level3MediumToolStripMenuItem.Checked = true;
        }

        private void level5FewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.LogTreshold = 4;
            level1AllToolStripMenuItem.Checked =
            level3MediumToolStripMenuItem.Checked = false;
            level5FewToolStripMenuItem.Checked = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Server.Stop();
        }

        private void dumpDecoderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DumpDecoder d = new DumpDecoder();
            d.ShowDialog();
        }

        private void startProxyModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*string input = Microsoft.VisualBasic.Interaction.InputBox("Please enter target IP", "Proxy Mode", "192.3.139.22");
            if (input != "")
            {
                startToolStripMenuItem.Enabled =
                startProxyModeToolStripMenuItem.Enabled = false;
                Server.StartProxy(input);
            }*/
        }

        public void WriteToRtb1(String text, int LogSize)
        {
            rtb1.Invoke((MethodInvoker) delegate()
            {
                rtb1.AppendText(text);
                if (rtb1.Text.Length > LogSize)
                    rtb1.Text = rtb1.Text.Substring(rtb1.Text.Length - LogSize);
                rtb1.SelectionStart = rtb1.Text.Length;
                rtb1.ScrollToCaret();
            });
        }
        
    }
}
