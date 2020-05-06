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

namespace youtubedlgui
{
    public partial class formMain : Form
    {
        public formHelpOptions formHelpOptionsInstance;
        private static ListView lv;
        private delegate void AddTextDelegate(Process p, String text);
        private static int CurrentDownloads = 0;
        private static int CurrentQueued = 0;

        public formMain()
        {
            InitializeComponent();            
        }

        public void SetDefaultOptions()
        {
            youtubedlgui.Properties.Settings.Default.Reset();
            textBoxOptions.Text = youtubedlgui.Properties.Settings.Default.Options;
        }

        private void formMain_Load(object sender, EventArgs e)
        {
            lv = listViewDownload;

            youtubedlgui.Properties.Settings.Default.Reload();
            if (youtubedlgui.Properties.Settings.Default.WorkDir == "") {
                youtubedlgui.Properties.Settings.Default.WorkDir = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }
            textBoxWorkDir.Text = youtubedlgui.Properties.Settings.Default.WorkDir;
            textBoxCommand.Text = youtubedlgui.Properties.Settings.Default.YoutubeDlExe;
            textBoxOptions.Text = youtubedlgui.Properties.Settings.Default.Options;
            checkBoxClipboardPaste.Checked = youtubedlgui.Properties.Settings.Default.ClipboardPaste;
            numericUpDownMaxDownloads.Value = youtubedlgui.Properties.Settings.Default.MaxDownloads;
            if (youtubedlgui.Properties.Settings.Default.FormWidth > 0) this.Width = youtubedlgui.Properties.Settings.Default.FormWidth;
            if (youtubedlgui.Properties.Settings.Default.FormHeight > 0) this.Height = youtubedlgui.Properties.Settings.Default.FormHeight;
            if (youtubedlgui.Properties.Settings.Default.ListColumn1Width > 0) listViewDownload.Columns[0].Width = youtubedlgui.Properties.Settings.Default.ListColumn1Width;
            if (youtubedlgui.Properties.Settings.Default.ListColumn2Width > 0) listViewDownload.Columns[1].Width = youtubedlgui.Properties.Settings.Default.ListColumn2Width;
            if (youtubedlgui.Properties.Settings.Default.ListColumn3Width > 0) listViewDownload.Columns[2].Width = youtubedlgui.Properties.Settings.Default.ListColumn3Width;
        }

        private static void AddText(Process p, String text)
        {
            if (!lv.InvokeRequired)
            {
                ListViewItem lvi = lv.Items.Find(p.Id.ToString(), false)[0];
                if (!(lvi == null))
                {
                    lvi.SubItems[1].Text = text;
                    if (lvi.ImageKey!="GreenArrow") lvi.ImageKey = "GreenArrow";
                    if (lvi.SubItems[1].Tag == null) lvi.SubItems[1].Tag = new StringBuilder(text); else ((StringBuilder)(lvi.SubItems[1].Tag)).Append(Environment.NewLine + text);

                    if (text.StartsWith("[download] Destination: ")) lvi.SubItems[2].Text = text.Substring(24);
                    if (text.StartsWith("[ffmpeg] Merging formats into ")) lvi.SubItems[2].Text = text.Substring(31).Trim('"');
                    //lv.Refresh();
                }
            } else
            {
                lv.Invoke(new AddTextDelegate(AddText), p, text);
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

        private void VideoDownload(String URL)
        {
            Process ps = new Process();

            ps.StartInfo.UseShellExecute = false;
            ps.StartInfo.FileName = AppContext.BaseDirectory + "\\" + textBoxCommand.Text;
            ps.StartInfo.WorkingDirectory = textBoxWorkDir.Text;
            ps.StartInfo.Arguments = textBoxOptions.Text + " \"" + URL + "\"";
            ps.StartInfo.RedirectStandardOutput = true;
            ps.StartInfo.RedirectStandardError = true;
            ps.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            ps.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            ps.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ps.StartInfo.CreateNoWindow = true;
            ps.OutputDataReceived += ProcessOutputHandler;
            ps.ErrorDataReceived += ProcessOutputHandler;
            ps.EnableRaisingEvents = true;
            ps.Exited += ProcessExitedHandler;

            ListViewItem lvi = new ListViewItem(textBoxURL.Text);
            lvi.SubItems.Add("Queued");
            lvi.SubItems.Add("");
            lvi.Tag = ps;
            lvi.Name = "q"; // Queued by default
            lvi.ImageKey = "GreyBall";
            listViewDownload.Items.Add(lvi);
            CurrentQueued += 1;

            StartQueuedProcess();

            timerMonitor.Start();
        }

        private void StartQueuedProcess()
        {
            if (CurrentDownloads >= numericUpDownMaxDownloads.Value) return;

            foreach (ListViewItem lvi in listViewDownload.Items.Find("q", false)) 
            {
                if (CurrentDownloads < numericUpDownMaxDownloads.Value)
                {
                    Process ps = (Process)lvi.Tag;
                    ps.Start();
                    lvi.Name = ps.Id.ToString();                    
                    lvi.SubItems[1].Text = "Preparing...";
                    ps.BeginOutputReadLine();
                    ps.BeginErrorReadLine();
                    CurrentDownloads += 1;
                    CurrentQueued -= 1;
                }
                else break;
            }

        }


        private void ProcessExitedHandler(object sender, EventArgs e)
        {
            CurrentDownloads -= 1;
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            VideoDownload(textBoxURL.Text);
        }

        private void formMain_Activated(object sender, EventArgs e)
        {
            if (checkBoxClipboardPaste.Checked)
            {
                if (Clipboard.ContainsText() && (Clipboard.GetText().StartsWith("https://") || Clipboard.GetText().StartsWith("http://")))
                {
                    textBoxURL.SelectAll();
                    textBoxURL.Paste();
                }
            }
        }

        private void listViewDownload_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) {

                if (listViewDownload.SelectedItems.Count > 0)
                {
                    String strVideo = ((Process)(listViewDownload.SelectedItems[0].Tag)).StartInfo.WorkingDirectory + "\\" + listViewDownload.SelectedItems[0].SubItems[2].Text;
                    String strVideoPart = ((Process)(listViewDownload.SelectedItems[0].Tag)).StartInfo.WorkingDirectory + "\\" + listViewDownload.SelectedItems[0].SubItems[2].Text + ".part";
                    toolStripMenuItemView.Enabled = System.IO.File.Exists(strVideo);
                    Process ps = (Process)(listViewDownload.SelectedItems[0].Tag);
                    toolStripMenuItemStop.Enabled = (!(ps == null) && !ps.HasExited);
                    toolStripMenuItemRetry.Enabled = ((ps == null) || ps.HasExited);
                    ToolStripMenuItemDeletePartial.Enabled = (((ps == null) || ps.HasExited) && System.IO.File.Exists(strVideoPart));
                }

                contextMenuStripListView.Show(listViewDownload, e.Location); 
            }
        }

        private void toolStripMenuItemStop_Click(object sender, EventArgs e)
        {
            if (listViewDownload.SelectedItems.Count > 0)
            {
                Process ps = (Process)(listViewDownload.SelectedItems[0].Tag);
                if (!(ps==null) && !ps.HasExited) { ps.Kill(); }
            }
        }

        private void toolStripMenuItemView_Click(object sender, EventArgs e)
        {
            if (!(listViewDownload.SelectedItems[0]==null))
            {
                String strVideo = ((Process)(listViewDownload.SelectedItems[0].Tag)).StartInfo.WorkingDirectory + "\\" + listViewDownload.SelectedItems[0].SubItems[2].Text;
                if (System.IO.File.Exists(strVideo)) { Process.Start(strVideo); }
            }

        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            youtubedlgui.Properties.Settings.Default.WorkDir = textBoxWorkDir.Text;
            youtubedlgui.Properties.Settings.Default.YoutubeDlExe = textBoxCommand.Text;
            youtubedlgui.Properties.Settings.Default.Options = textBoxOptions.Text;
            youtubedlgui.Properties.Settings.Default.ClipboardPaste = checkBoxClipboardPaste.Checked;
            youtubedlgui.Properties.Settings.Default.FormWidth = this.Width;
            youtubedlgui.Properties.Settings.Default.FormHeight = this.Height;
            youtubedlgui.Properties.Settings.Default.ListColumn1Width = listViewDownload.Columns[0].Width;
            youtubedlgui.Properties.Settings.Default.ListColumn2Width = listViewDownload.Columns[1].Width;
            youtubedlgui.Properties.Settings.Default.ListColumn3Width = listViewDownload.Columns[2].Width;
            youtubedlgui.Properties.Settings.Default.MaxDownloads = Decimal.ToInt32(numericUpDownMaxDownloads.Value);
            youtubedlgui.Properties.Settings.Default.Save();
        }

        private void listViewDownload_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            toolStripMenuItemView_Click(sender, e);
        }

        private void toolStripMenuItemRetry_Click(object sender, EventArgs e)
        {
            if (listViewDownload.SelectedItems.Count > 0)
            {
                Process ps = (Process)(listViewDownload.SelectedItems[0].Tag);
                if ((ps == null) || ps.HasExited)
                {
                    if (listViewDownload.SelectedItems.Count > 0)
                    {
                        String URL = listViewDownload.SelectedItems[0].SubItems[0].Text;
                        VideoDownload(URL);
                    }
                }
            }
        }

        private void buttonHelpOptions_Click(object sender, EventArgs e)
        {
            Process ps = new Process();

            ps.StartInfo.UseShellExecute = false;
            ps.StartInfo.FileName = AppContext.BaseDirectory + "\\" + textBoxCommand.Text;
            ps.StartInfo.WorkingDirectory = textBoxWorkDir.Text;
            ps.StartInfo.Arguments = "--help";
            ps.StartInfo.RedirectStandardOutput = true;
            ps.StartInfo.RedirectStandardError = true;
            ps.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ps.StartInfo.CreateNoWindow = true;

            ps.Start();

            String HelpText = ps.StandardOutput.ReadToEnd();

            ps.WaitForExit();

            if (formHelpOptionsInstance == null)
            {
                formHelpOptionsInstance = new formHelpOptions();
                formHelpOptionsInstance.ParentMainForm = this;
            }
            formHelpOptionsInstance.Show();
            formHelpOptionsInstance.setText(HelpText);
            formHelpOptionsInstance.BringToFront();

        }

        private void buttonWorkDir_Click(object sender, EventArgs e)
        {
            folderBrowserDialogWorkDir.SelectedPath = textBoxWorkDir.Text;
            if (folderBrowserDialogWorkDir.ShowDialog() == DialogResult.OK) textBoxWorkDir.Text = folderBrowserDialogWorkDir.SelectedPath;
        }

        private void ToolStripMenuItemDeletePartial_Click(object sender, EventArgs e)
        {
            String strVideoPart = ((Process)(listViewDownload.SelectedItems[0].Tag)).StartInfo.WorkingDirectory + "\\" + listViewDownload.SelectedItems[0].SubItems[2].Text + ".part";
            if (System.IO.File.Exists(strVideoPart)) { System.IO.File.Delete(strVideoPart); }
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            formUpdate fu = new formUpdate();
            fu.command = AppContext.BaseDirectory + "\\" + textBoxCommand.Text;
            fu.ShowDialog();
        }

        private void IconRefresh()
        {
            int Errors = 0;
            int Succeed = 0;

            foreach(ListViewItem lvi in listViewDownload.Items)
            {
                if (lvi.Name!="q" && ((Process)(lvi.Tag)).HasExited) 
                {
                    String strVideo = ((Process)(lvi.Tag)).StartInfo.WorkingDirectory + "\\" + lvi.SubItems[2].Text;
                    if (System.IO.File.Exists(strVideo)) lvi.ImageKey = "GreenBall"; else lvi.ImageKey = "RedX";
                }
                if (lvi.ImageKey == "RedX") Errors += 1;
                if (lvi.ImageKey == "GreenBall") Succeed += 1;
            }
            toolStripStatusLabelError.Text = "Errors: " + Errors.ToString();
            toolStripStatusLabelSucceed.Text = "Succeed: " + Succeed.ToString();
        }

        private void timerMonitor_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabelDownloads.Text = "Downloads: " + CurrentDownloads.ToString();
            toolStripStatusLabelQueued.Text = "Queued: " + CurrentQueued.ToString();
            if (CurrentQueued>0) StartQueuedProcess();
            if (CurrentQueued == 0 && CurrentDownloads == 0) timerMonitor.Stop();
            IconRefresh();
        }

        private void toolStripMenuItemViewLog_Click(object sender, EventArgs e)
        {
            if (!(listViewDownload.SelectedItems[0]==null) && !(listViewDownload.SelectedItems[0].SubItems[1].Tag==null))
            {
                formViewLog fvl = new formViewLog((StringBuilder)(listViewDownload.SelectedItems[0].SubItems[1].Tag));
            }
            
        }

        private void textBoxURL_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r') buttonDownload_Click(sender, null);
        }
    }
}
