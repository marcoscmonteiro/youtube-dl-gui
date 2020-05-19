using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
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

        private Process psSelected = null;
        private String FileNameSelected = "";
        private String strVideoSelected = "";
        private String strVideoPartSelected = "";
        private static ListViewItem ListViewItemSelected = null;
        private static String YoutubeDLexe = "youtube-dl.exe";

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
            textBoxOptions.Text = youtubedlgui.Properties.Settings.Default.Options;
            checkBoxClipboardPaste.Checked = youtubedlgui.Properties.Settings.Default.ClipboardPaste;
            numericUpDownMaxDownloads.Value = youtubedlgui.Properties.Settings.Default.MaxDownloads;
            comboBoxMaxQuality.SelectedIndex = youtubedlgui.Properties.Settings.Default.MaxQuality;
            comboBoxAudioOnly.SelectedIndex = youtubedlgui.Properties.Settings.Default.AudioOnly;
            checkBoxPlayList.Checked = youtubedlgui.Properties.Settings.Default.playlist;
            checkBoxNoCacheDir.Checked = youtubedlgui.Properties.Settings.Default.nocachedir;
            checkBoxNoPart.Checked = youtubedlgui.Properties.Settings.Default.NoPart;
            if (youtubedlgui.Properties.Settings.Default.FormWidth > 0) this.Width = youtubedlgui.Properties.Settings.Default.FormWidth;
            if (youtubedlgui.Properties.Settings.Default.FormHeight > 0) this.Height = youtubedlgui.Properties.Settings.Default.FormHeight;
            if (youtubedlgui.Properties.Settings.Default.FormTop > 0) this.Top = youtubedlgui.Properties.Settings.Default.FormTop;
            if (youtubedlgui.Properties.Settings.Default.FormLeft > 0) this.Left = youtubedlgui.Properties.Settings.Default.FormLeft;
            if (youtubedlgui.Properties.Settings.Default.ListColumn1Width > 0) listViewDownload.Columns[0].Width = youtubedlgui.Properties.Settings.Default.ListColumn1Width;
            if (youtubedlgui.Properties.Settings.Default.ListColumn2Width > 0) listViewDownload.Columns[1].Width = youtubedlgui.Properties.Settings.Default.ListColumn2Width;
            if (youtubedlgui.Properties.Settings.Default.ListColumn3Width > 0) listViewDownload.Columns[2].Width = youtubedlgui.Properties.Settings.Default.ListColumn3Width;
            if (youtubedlgui.Properties.Settings.Default.ListColumn4Width > 0) listViewDownload.Columns[3].Width = youtubedlgui.Properties.Settings.Default.ListColumn4Width;
            if (youtubedlgui.Properties.Settings.Default.ListColumn5Width > 0) listViewDownload.Columns[4].Width = youtubedlgui.Properties.Settings.Default.ListColumn5Width;
            checkBoxFFPlay.Checked = youtubedlgui.Properties.Settings.Default.ffPlay;
            if (youtubedlgui.Properties.Settings.Default.DownloadListItems != "") DeserializeDownloadList(youtubedlgui.Properties.Settings.Default.DownloadListItems);
            IconRefresh();
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

        private void VideoDownload(String URL, String CommandLineArguments, String Workdir, String Command, ListViewItem Readylvi = null)
        {

            if (!System.IO.Directory.Exists(Workdir.Trim()))
            {
                DialogResult dr = MessageBox.Show(this, "Work directory does not exists and download can't continue. Try to create?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr==DialogResult.Yes)
                {
                    try 
                    {
                        System.IO.Directory.CreateDirectory(Workdir.Trim());
                    } 
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, String.Format("Error on create directory:\r{0}\r\rDownload can't continue.", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBoxWorkDir.Focus();
                        return;
                    }                    
                } else
                {
                    textBoxWorkDir.Focus();
                    return;
                }              
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

            ListViewItem lvi = Readylvi;

            if (lvi==null) lvi = new ListViewItem(URL); else 
            { 
                lvi.SubItems.Clear();
                lvi.Text = URL;
            }

            lvi.SubItems.Add("Queued");
            lvi.SubItems.Add("");
            lvi.SubItems.Add(ps.StartInfo.WorkingDirectory);
            lvi.SubItems.Add(ps.StartInfo.Arguments);
            lvi.Tag = ps;
            lvi.Name = "q"; // Queued by default
            lvi.ImageKey = "GreyBall";
            ListViewAddTextToLog(lvi, "Executable: " + ps.StartInfo.FileName);
            ListViewAddTextToLog(lvi, "Arguments : " + ps.StartInfo.Arguments);
            ListViewAddTextToLog(lvi, "Work Dir. : " + ps.StartInfo.WorkingDirectory);
            if (Readylvi==null) listViewDownload.Items.Add(lvi);
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

        private String ProcessCommandLineOptions(String URL)
        {
            URL = URL.Trim();

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

            String CommandLine = "--encoding UTF8 --ignore-config " + textBoxOptions.Text.Trim() + " ";

            if (checkBoxNoCacheDir.Checked) CommandLine += "--no-cache-dir ";
            if (!checkBoxPlayList.Checked) CommandLine += "--no-playlist ";
            if (checkBoxNoPart.Checked) CommandLine += "--no-part ";

            CommandLine += Quality;
            CommandLine += AudioOnly;
            CommandLine += "\"" + URL + "\"";

            return CommandLine;
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            String CommandLineArguments = ProcessCommandLineOptions(textBoxURL.Text.Trim());
            if (CommandLineArguments != "") VideoDownload(textBoxURL.Text.Trim(), CommandLineArguments, textBoxWorkDir.Text, YoutubeDLexe);
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

        private void EnableDisableContextMenuAndButtons(ListViewItem lv = null)
        {
            psSelected = null;
            FileNameSelected = "";
            strVideoSelected = "";
            strVideoPartSelected = "";
            ListViewItemSelected = lv;

            if (ListViewItemSelected == null && listViewDownload.SelectedItems.Count > 0)
            {
                ListViewItemSelected = listViewDownload.SelectedItems[0];
            }
            if (ListViewItemSelected != null)
            {
                psSelected = (Process)(ListViewItemSelected.Tag);
                FileNameSelected = ListViewItemSelected.SubItems[2].Text;
                strVideoSelected = ListViewItemSelected.SubItems[3].Text.TrimEnd('\\') + "\\" + FileNameSelected;
                strVideoPartSelected = strVideoSelected + ".part";
                if (!System.IO.File.Exists(strVideoSelected)) strVideoSelected = "";                
                if (!System.IO.File.Exists(strVideoPartSelected)) strVideoPartSelected = "";
            }
            toolStripMenuItemPlayVideo.Enabled = (strVideoSelected!="" || strVideoPartSelected!="");
            buttonPlayVideo.Enabled = toolStripMenuItemPlayVideo.Enabled;
            toolStripMenuItemStop.Enabled = (!(psSelected == null) && !psSelected.HasExited);
            buttonStopDownload.Enabled = toolStripMenuItemStop.Enabled;
            toolStripMenuItemRetry.Enabled = ListViewItemSelected!=null && ((psSelected == null) || psSelected.HasExited);
            buttonRetryDownload.Enabled = toolStripMenuItemRetry.Enabled;
            toolStripMenuItemDelete.Enabled = strVideoPartSelected!="" || strVideoSelected != "";
            buttonDeleteVideo.Enabled = toolStripMenuItemDelete.Enabled;
            toolStripMenuItemViewLog.Enabled = (ListViewItemSelected != null && ListViewItemSelected.SubItems[1].Tag != null);
            buttonViewLog.Enabled = toolStripMenuItemViewLog.Enabled;
            toolStripMenuItemOpenFolder.Enabled = strVideoPartSelected != "" || strVideoSelected != "";
            buttonOpenFolder.Enabled = toolStripMenuItemOpenFolder.Enabled;
            toolStripMenuItemRemoveDownload.Enabled = (ListViewItemSelected != null);
            buttonRemoveDownload.Enabled = toolStripMenuItemRemoveDownload.Enabled;

        }

        private void listViewDownload_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) {

                if (listViewDownload.SelectedItems.Count > 0)
                {
                    EnableDisableContextMenuAndButtons();
                }

                contextMenuStripListView.Show(listViewDownload, e.Location); 
            }
        }

        private void toolStripMenuItemStop_Click(object sender, EventArgs e)
        {
            if (psSelected!=null && !psSelected.HasExited) { 
                psSelected.Kill();
                EnableDisableContextMenuAndButtons();
            }
        }

        private void toolStripMenuItemView_Click(object sender, EventArgs e)
        {
            String FileToPlay = strVideoPartSelected;
            if (strVideoSelected != "") FileToPlay = strVideoSelected;

            if (FileToPlay == "") return;

            if (!checkBoxFFPlay.Checked && strVideoSelected!="") Process.Start(FileToPlay);
            else
            {
                // start ffplay 
                Process ffplay = new Process
                {
                    StartInfo =
                    {
                    FileName = AppContext.BaseDirectory + "ffplay.exe",
                    Arguments = "\"" + FileToPlay + "\"",
                    // hides the command window
                    CreateNoWindow = true, 
                    RedirectStandardError = false,
                    RedirectStandardOutput = false,
                    UseShellExecute = false
                    }
                };

                ffplay.EnableRaisingEvents = true;
                ffplay.Start();
            }

        }

        private void listViewDownload_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            toolStripMenuItemView_Click(sender, e);
        }

        private void toolStripMenuItemRetry_Click(object sender, EventArgs e)
        {
            //if (strVideoSelected != "") { MessageBox.Show(this, "Video already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (ListViewItemSelected!=null && ((psSelected == null) || psSelected.HasExited))
                VideoDownload(ListViewItemSelected.SubItems[0].Text, ListViewItemSelected.SubItems[4].Text, ListViewItemSelected.SubItems[3].Text, YoutubeDLexe, ListViewItemSelected);
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
            String HelpText = ExecuteCommandReturnOutput(AppContext.BaseDirectory + "\\" + YoutubeDLexe, "--help");

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

        private void toolStripMenuItemDelete_Click(object sender, EventArgs e)
        {
            if (strVideoPartSelected!="") 
            {
                try
                {
                    System.IO.File.Delete(strVideoPartSelected);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, String.Format("Error on delete \"{0}\":\r{1}", strVideoPartSelected, ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (strVideoSelected!="") 
            {
                try
                {
                    System.IO.File.Delete(strVideoSelected);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, String.Format("Error on delete \"{0}\":\r{1}", strVideoSelected, ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            EnableDisableContextMenuAndButtons();
            IconRefresh();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            formUpdate fu = new formUpdate();
            fu.command = AppContext.BaseDirectory + "\\" + YoutubeDLexe;
            fu.ShowDialog();
        }

        private void IconRefresh()
        {
            int Errors = 0;
            int Succeed = 0;

            foreach(ListViewItem lvi in listViewDownload.Items)
            {
                Process ps = ((Process)(lvi.Tag));
                if (lvi.Name!="q" && ps!=null && ps.HasExited) 
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
            EnableDisableContextMenuAndButtons();
        }

        private void toolStripMenuItemViewLog_Click(object sender, EventArgs e)
        {
            if (ListViewItemSelected!=null && ListViewItemSelected.SubItems[1].Tag!=null)
            {
                formViewLog fvl = new formViewLog((StringBuilder)(ListViewItemSelected.SubItems[1].Tag));
                fvl.Dispose();
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
                
                if (!e.Cancel) 
                {
                    timerMonitor.Enabled = false;
                    foreach (ListViewItem lvi in listViewDownload.Items)
                    {
                        Process ps = ((Process)(lvi.Tag));
                        if (lvi.Name!="q" && ps!=null && !ps.HasExited)
                        {
                            ps.Kill();                            
                        };
                        lvi.ImageKey = "RedX";
                    }

                }
            }
        }

        private String SerializeDownloadList()
        {
            List<ListItemDownload> llid = new List<ListItemDownload>();
            foreach (ListViewItem lvi in listViewDownload.Items)
            {
                ListItemDownload lvd = new ListItemDownload
                {
                    URL = lvi.SubItems[0].Text,
                    Status = lvi.SubItems[1].Text,
                    FileName = lvi.SubItems[2].Text,
                    WorkDir = lvi.SubItems[3].Text,
                    YDArguments = lvi.SubItems[4].Text,   
                    Name = "L" + lvi.Name,
                    ImageKey = lvi.ImageKey,
                    Log = ((StringBuilder)(lvi.SubItems[1].Tag)).ToString()
                };                
                llid.Add(lvd);
            }
            return (JsonConvert.SerializeObject(llid));
        }

        private void DeserializeDownloadList(String dl)
        {
            List<ListItemDownload> llid = JsonConvert.DeserializeObject<List<ListItemDownload>>(dl);
            foreach (ListItemDownload lid in llid)
            {
                String ImageKey = "RedX";
                if (System.IO.File.Exists(lid.WorkDir + "\\" + lid.FileName)) ImageKey = "GreenBall";
                ListViewItem lvi = listViewDownload.Items.Add(Name, lid.URL, ImageKey);
                lvi.SubItems.Add(lid.Status);
                lvi.SubItems.Add(lid.FileName);
                lvi.SubItems.Add(lid.WorkDir);
                lvi.SubItems.Add(lid.YDArguments);
                lvi.SubItems[1].Tag = new StringBuilder(lid.Log);
            }
        }

        private void formMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            youtubedlgui.Properties.Settings.Default.WorkDir = textBoxWorkDir.Text;            
            youtubedlgui.Properties.Settings.Default.Options = textBoxOptions.Text;
            youtubedlgui.Properties.Settings.Default.ClipboardPaste = checkBoxClipboardPaste.Checked;
            youtubedlgui.Properties.Settings.Default.FormWidth = this.Width;
            youtubedlgui.Properties.Settings.Default.FormHeight = this.Height;
            youtubedlgui.Properties.Settings.Default.FormTop = this.Top;
            youtubedlgui.Properties.Settings.Default.FormLeft = this.Left;
            youtubedlgui.Properties.Settings.Default.ListColumn1Width = listViewDownload.Columns[0].Width;
            youtubedlgui.Properties.Settings.Default.ListColumn2Width = listViewDownload.Columns[1].Width;
            youtubedlgui.Properties.Settings.Default.ListColumn3Width = listViewDownload.Columns[2].Width;
            youtubedlgui.Properties.Settings.Default.ListColumn4Width = listViewDownload.Columns[3].Width;
            youtubedlgui.Properties.Settings.Default.ListColumn5Width = listViewDownload.Columns[4].Width;
            youtubedlgui.Properties.Settings.Default.MaxDownloads = Decimal.ToInt32(numericUpDownMaxDownloads.Value);
            youtubedlgui.Properties.Settings.Default.MaxQuality = comboBoxMaxQuality.SelectedIndex;
            youtubedlgui.Properties.Settings.Default.AudioOnly = comboBoxAudioOnly.SelectedIndex;
            youtubedlgui.Properties.Settings.Default.playlist = checkBoxPlayList.Checked;
            youtubedlgui.Properties.Settings.Default.nocachedir = checkBoxNoCacheDir.Checked;
            youtubedlgui.Properties.Settings.Default.NoPart = checkBoxNoPart.Checked;
            youtubedlgui.Properties.Settings.Default.ffPlay = checkBoxFFPlay.Checked;
            youtubedlgui.Properties.Settings.Default.DownloadListItems = SerializeDownloadList();

            youtubedlgui.Properties.Settings.Default.Save();
        }

        private void comboBoxAudioOnly_SelectedIndexChanged(object sender, EventArgs e)
        {
            labelMaxVideoQuality.Visible = (comboBoxAudioOnly.SelectedIndex == 0);
            comboBoxMaxQuality.Visible = (comboBoxAudioOnly.SelectedIndex == 0);
        }

        private void listViewDownload_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableDisableContextMenuAndButtons();
        }

        private void toolStripMenuItemOpenFolder_Click(object sender, EventArgs e)
        {
            String FileToShow = "";
            if (strVideoSelected != "") FileToShow = strVideoSelected; else FileToShow = strVideoPartSelected;

            if (FileToShow != "") Process.Start("explorer.exe", "/select,\"" + FileToShow + "\"");
        }

        private void toolStripMenuItemRemoveDownload_Click(object sender, EventArgs e)
        {
            psSelected = (Process)(ListViewItemSelected.Tag);
            if (psSelected != null && !psSelected.HasExited)
            {
                MessageBox.Show(this, "Download is running. Can't remove it.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int lvs = ListViewItemSelected.Index;
            ListViewItem NextLvi = null;

            if (listViewDownload.Items.Count - 1 == lvs) lvs--;            

            listViewDownload.Items.Remove(ListViewItemSelected);

            if (lvs >= 0) NextLvi = listViewDownload.Items[lvs];

            if (NextLvi!=null) NextLvi.Selected = true;

            IconRefresh();
        }

        private void toolStripMenuItemCopyURL_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(ListViewItemSelected.Text);
        }

        private void toolStripMenuItemOpenInBrowser_Click(object sender, EventArgs e)
        {
            Process.Start(ListViewItemSelected.Text);
        }
    }
}
