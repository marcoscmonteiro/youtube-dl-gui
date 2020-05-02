using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace yd
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            youtubedlgui.Properties.Settings.Default.Reload();
            if (youtubedlgui.Properties.Settings.Default.WorkDir == "") {
                youtubedlgui.Properties.Settings.Default.WorkDir = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }
            textBoxWorkDir.Text = youtubedlgui.Properties.Settings.Default.WorkDir;
            textBoxCommand.Text = youtubedlgui.Properties.Settings.Default.YoutubeDlExe;
            textBoxOptions.Text = youtubedlgui.Properties.Settings.Default.Options;
            checkBoxClipboardPaste.Checked = youtubedlgui.Properties.Settings.Default.ClipboardPaste;
        }

        private static ListView lv;

        private delegate void AddTextDelegate(Process p, String text);

        private static void AddText(Process p, String text)
        {
            if (!lv.InvokeRequired)
            {
                lv.Items.Find(p.Id.ToString(), false)[0].SubItems[1].Text = text;
                if (text.StartsWith("[download] Destination: ")) { lv.Items.Find(p.Id.ToString(), false)[0].SubItems[2].Text = text.Substring(24); }
                lv.Refresh();
                //tx.Text = text;
                //tx.Refresh();
            } else
            {
                //lstThreadResult.Invoke(new InformaTerminoIncluiGEDDelegate(InformaTerminoIncluiGED), TempoExecucao, TotalThreads, TempFileError, sArqErroFinal);
                lv.Invoke(new AddTextDelegate(AddText), p, text);
                return;
            }

        }

        private static void ProcessOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //(sendingProcess as Process).
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                AddText((Process)(sendingProcess), outLine.Data);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process ps = new Process();

            ps.StartInfo.UseShellExecute = false;
            ps.StartInfo.FileName = textBoxCommand.Text;
            ps.StartInfo.WorkingDirectory = textBoxWorkDir.Text;
            ps.StartInfo.Arguments = textBoxOptions.Text + " \"" + textBoxURL.Text + "\"";
            ps.StartInfo.RedirectStandardOutput = true;
            ps.StartInfo.RedirectStandardError = true;
            ps.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ps.StartInfo.CreateNoWindow = true;
            ps.OutputDataReceived += ProcessOutputHandler;

            ps.Start();
            
            ListViewItem lvi = new ListViewItem(textBoxURL.Text);
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.Tag = ps;
            lvi.Name = ps.Id.ToString();

            listView1.Items.Add(lvi);

            lv = listView1;
            ps.BeginOutputReadLine();

        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            if (checkBoxClipboardPaste.Checked)
            {
                textBoxURL.SelectAll();
                textBoxURL.Paste();
            }
          
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) {

                if (listView1.SelectedItems.Count > 0)
                {
                    String strVideo = textBoxWorkDir.Text + "\\" + listView1.SelectedItems[0].SubItems[2].Text;
                    toolStripMenuItemView.Enabled = System.IO.File.Exists(strVideo);
                    Process ps = (Process)(listView1.SelectedItems[0].Tag);
                    toolStripMenuItemStop.Enabled = (!(ps == null) && !ps.HasExited);
                }

                contextMenuStripListView.Show(listView1, e.Location); 
            }
        }

        private void toolStripMenuItemStop_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                Process ps = (Process)(listView1.SelectedItems[0].Tag);
                if (!(ps==null) && !ps.HasExited) { ps.Kill(); }
            }
        }

        private void toolStripMenuItemView_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                String strVideo = textBoxWorkDir.Text + "\\" + listView1.SelectedItems[0].SubItems[2].Text;
                if (System.IO.File.Exists(strVideo)) { Process.Start(strVideo); }
            }

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            youtubedlgui.Properties.Settings.Default.WorkDir = textBoxWorkDir.Text;
            youtubedlgui.Properties.Settings.Default.YoutubeDlExe = textBoxCommand.Text;
            youtubedlgui.Properties.Settings.Default.Options = textBoxOptions.Text;
            youtubedlgui.Properties.Settings.Default.ClipboardPaste = checkBoxClipboardPaste.Checked;
            youtubedlgui.Properties.Settings.Default.Save();
        }
    }
}
