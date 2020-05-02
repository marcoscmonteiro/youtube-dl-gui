using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;


namespace youtubedlgui
{
    public partial class formUpdate : Form
    {
        public formUpdate()
        {
            InitializeComponent();
        }

        public String command;

        private static TextBox tx;
        private static Boolean EndUpdate;
        private delegate void AddTextDelegate(Process p, String text);
        private Boolean AlreadyActivated = false;

        private static void AddText(Process p, String text)
        {
            if (!tx.InvokeRequired)
            {
                tx.Text += text;
                tx.Refresh();
            }
            else
            {
                tx.Invoke(new AddTextDelegate(AddText), p, text);
                return;
            }

        }

        private static void ProcessOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                AddText((Process)(sendingProcess), outLine.Data);
            }
        }


        private void Update(String command)
        {
            Process ps = new Process();

            ps.StartInfo.UseShellExecute = false;
            ps.StartInfo.FileName = command;
            ps.StartInfo.Arguments = "-U";
            ps.StartInfo.RedirectStandardOutput = true;
            ps.StartInfo.RedirectStandardError = true;
            ps.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ps.StartInfo.CreateNoWindow = true;
            ps.OutputDataReceived += ProcessOutputHandler;
            ps.ErrorDataReceived += ProcessOutputHandler;
            ps.EnableRaisingEvents = true;

            ps.Exited += Ps_Exited;
            tx = textBoxUpdate;
            EndUpdate = false;

            ps.Start();

            ps.BeginOutputReadLine();
            ps.BeginErrorReadLine();
            timerVerifyUpdate.Start();
        }

        private void Ps_Exited(object sender, EventArgs e)
        {
            if (!buttonClose.InvokeRequired)
            {
                buttonClose.Enabled = true;
            }
            else
            {
                EndUpdate = true;
            }
        }

        private void formUpdate_Load(object sender, EventArgs e)
        {

        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void formUpdate_Activated(object sender, EventArgs e)
        {
            if (!AlreadyActivated)
            {
                buttonClose.Enabled = false;
                Update(command);
                AlreadyActivated = true;
            }

        }

        private void timerVerifyUpdate_Tick(object sender, EventArgs e)
        {
            if (EndUpdate)
            {
                buttonClose.Enabled = true;
                timerVerifyUpdate.Stop();
            }
        }

        private void formUpdate_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!EndUpdate) {
                MessageBox.Show("Youtube-dl is updating... Wait please.");
                e.Cancel = true;
            }
        }
    }
}
