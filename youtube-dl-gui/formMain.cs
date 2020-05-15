using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

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
            comboBoxMaxQuality.SelectedIndex = youtubedlgui.Properties.Settings.Default.MaxQuality;
            comboBoxAudioOnly.SelectedIndex = youtubedlgui.Properties.Settings.Default.AudioOnly;
            checkBoxPlayList.Checked = youtubedlgui.Properties.Settings.Default.playlist;
            checkBoxNoCacheDir.Checked = youtubedlgui.Properties.Settings.Default.nocachedir;
            if (youtubedlgui.Properties.Settings.Default.FormWidth > 0) this.Width = youtubedlgui.Properties.Settings.Default.FormWidth;
            if (youtubedlgui.Properties.Settings.Default.FormHeight > 0) this.Height = youtubedlgui.Properties.Settings.Default.FormHeight;
            if (youtubedlgui.Properties.Settings.Default.ListColumn1Width > 0) listViewDownload.Columns[0].Width = youtubedlgui.Properties.Settings.Default.ListColumn1Width;
            if (youtubedlgui.Properties.Settings.Default.ListColumn2Width > 0) listViewDownload.Columns[1].Width = youtubedlgui.Properties.Settings.Default.ListColumn2Width;
            if (youtubedlgui.Properties.Settings.Default.ListColumn3Width > 0) listViewDownload.Columns[2].Width = youtubedlgui.Properties.Settings.Default.ListColumn3Width;
        }

        private static void ExtractFileName(String TextToProcess, String lTrimText, ListViewItem lvi)
        {
            if (TextToProcess.StartsWith(lTrimText))
            {
                String Filename = TextToProcess.Substring(lTrimText.Length);
                Filename = Filename.TrimStart('\"');
                Filename = Filename.TrimEnd('\"');
                lvi.SubItems[2].Text = Filename;

            }
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
                    ListViewAddTextToLog(lvi, text);

                    ExtractFileName(text, "[download] Destination: ", lvi);
                    ExtractFileName(text, "[ffmpeg] Merging formats into ", lvi);
                    ExtractFileName(text, "[ffmpeg] Destination: ", lvi);
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

        private static void ListViewAddTextToLog(ListViewItem lvi, String text)
        {
        if (lvi.SubItems[1].Tag == null) lvi.SubItems[1].Tag = new StringBuilder(text); else ((StringBuilder)(lvi.SubItems[1].Tag)).Append(Environment.NewLine + text);
        }

        private void VideoDownload(String CommandLineArguments, String Workdir, String Command)
        {

            if (!System.IO.Directory.Exists(Workdir))
            {
                MessageBox.Show(this, "Work directory does not exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxWorkDir.Focus();
                return;
            }

            Process ps = new Process();

            ps.StartInfo.UseShellExecute = false;
            ps.StartInfo.FileName = AppContext.BaseDirectory + Command;
            ps.StartInfo.WorkingDirectory = Workdir.TrimEnd('\\');
            ps.StartInfo.Arguments = CommandLineArguments;
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
            ListViewAddTextToLog(lvi, "Executable: " + ps.StartInfo.FileName);
            ListViewAddTextToLog(lvi, "Arguments : " + ps.StartInfo.Arguments);
            ListViewAddTextToLog(lvi, "Work Dir. : " + ps.StartInfo.WorkingDirectory);
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

        private String ProcessCommandLineOptions()
        {
            String URL = textBoxURL.Text.Trim();
            String CommandLine = "--encoding UTF8 ";

            if (!(URL.StartsWith("http://")) && !(URL.StartsWith("https://")))
            {
                MessageBox.Show(this, "Invalid URL. URL must begin with http:// or https://.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxURL.Focus();
                return "";
            }

            String AudioOnly = "";
            if (comboBoxAudioOnly.SelectedIndex > 0)
            {
                AudioOnly = "-x ";
                if (comboBoxAudioOnly.SelectedIndex>1) 
                {
                    AudioOnly += "--audio-format " + comboBoxAudioOnly.SelectedItem.ToString() + " ";
                }
            }

            String Quality = "";
            if (AudioOnly == "")
            {
                if (comboBoxMaxQuality.SelectedIndex > 0)
                {
                    Quality = "-f ";
                    if (comboBoxMaxQuality.SelectedIndex == comboBoxMaxQuality.Items.Count - 1)
                        Quality += "\"worstvideo+worstaudio/worst\" ";
                    else
                    {
                        String QualityH = comboBoxMaxQuality.SelectedItem.ToString().Split('p')[0];
                        Quality += String.Format("\"bestvideo[height<=?{0}]+bestaudio/best[height<=?{0}]\"", QualityH) + " ";
                    }

                }
            }
            else Quality = "-f \"worstvideo+bestaudio\" ";

            CommandLine += textBoxOptions.Text.Trim() + " ";

            if (checkBoxNoCacheDir.Checked) CommandLine += "--no-cache-dir ";
            if (!checkBoxPlayList.Checked) CommandLine += "--no-playlist ";
          
            CommandLine += Quality;
            CommandLine += AudioOnly;
            CommandLine += "\"" + URL + "\"";

            return CommandLine;
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            String CommandLineArguments = ProcessCommandLineOptions();
            if (CommandLineArguments != "") VideoDownload(CommandLineArguments, textBoxWorkDir.Text, textBoxCommand.Text);
        }

        private void formMain_Activated(object sender, EventArgs e)
        {
            if (checkBoxClipboardPaste.Checked)
            {
                if (Clipboard.ContainsText() && (Clipboard.GetText().StartsWith("https://") || Clipboard.GetText().StartsWith("http://")))
                {
                    textBoxURL.SelectAll();
                    textBoxURL.Paste();
                    textBoxURL.Focus();
                    textBoxURL.SelectAll();                    
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
                        VideoDownload(URL, textBoxURL.Text, textBoxCommand.Text);
                    }
                }
            }
        }

        public String ExecuteCommandReturnOutput(String Command, String Arguments)
        {
            Process ps = new Process();

            ps.StartInfo.UseShellExecute = false;
            ps.StartInfo.FileName = Command;
            ps.StartInfo.Arguments = Arguments;
            ps.StartInfo.RedirectStandardOutput = true;
            ps.StartInfo.RedirectStandardError = true;
            ps.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ps.StartInfo.CreateNoWindow = true;

            ps.Start();

            String Output = ps.StandardOutput.ReadToEnd();

            ps.WaitForExit();
            ps.Close();

            return Output;
        }

        private void buttonHelpOptions_Click(object sender, EventArgs e)
        {
            String HelpText = ExecuteCommandReturnOutput(AppContext.BaseDirectory + "\\" + textBoxCommand.Text, "--help");

            formHelpOptionsInstance = new formHelpOptions();
            formHelpOptionsInstance.HelpOptions = HelpText;
            formHelpOptionsInstance.Options = textBoxOptions.Text;
            formHelpOptionsInstance.ShowDialog();
            textBoxOptions.Text = formHelpOptionsInstance.Options;
            formHelpOptionsInstance = null;
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
            toolStripStatusLabelErrorN.Text = Errors.ToString();
            toolStripStatusLabelSucceedN.Text = Succeed.ToString();
        }

        private void timerMonitor_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabelDownloadsN.Text = CurrentDownloads.ToString();
            toolStripStatusLabelQueuedN.Text = CurrentQueued.ToString();
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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CurrentDownloads>0 || CurrentQueued>0)
            {
                e.Cancel = (MessageBox.Show(this, 
                    "There are Downloads running and/or Videos queued. Form closing will stop and cancel all pending operations. Really want to close?", 
                    "Confirm form closing", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question, 
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes);
            }
        }

        private void formMain_FormClosed(object sender, FormClosedEventArgs e)
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
            youtubedlgui.Properties.Settings.Default.MaxQuality = comboBoxMaxQuality.SelectedIndex;
            youtubedlgui.Properties.Settings.Default.AudioOnly = comboBoxAudioOnly.SelectedIndex;
            youtubedlgui.Properties.Settings.Default.playlist = checkBoxPlayList.Checked;
            youtubedlgui.Properties.Settings.Default.nocachedir = checkBoxNoCacheDir.Checked;

            youtubedlgui.Properties.Settings.Default.Save();
        }

        private void comboBoxAudioOnly_SelectedIndexChanged(object sender, EventArgs e)
        {
            labelMaxVideoQuality.Visible = (comboBoxAudioOnly.SelectedIndex == 0);
            comboBoxMaxQuality.Visible = (comboBoxAudioOnly.SelectedIndex == 0);
        }
    }
}
