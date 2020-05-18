namespace youtubedlgui
{
    partial class formMain
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formMain));
            this.textBoxOptions = new System.Windows.Forms.TextBox();
            this.labelExtraOptions = new System.Windows.Forms.Label();
            this.textBoxCommand = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxURL = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxWorkDir = new System.Windows.Forms.TextBox();
            this.listViewDownload = new System.Windows.Forms.ListView();
            this.URL = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.File = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.WorkDir = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.YdlArguments = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageListStatus = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuStripListView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemPlayVideo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemOpenFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemStop = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRetry = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemViewLog = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRemoveDownload = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBoxClipboardPaste = new System.Windows.Forms.CheckBox();
            this.buttonHelpOptions = new System.Windows.Forms.Button();
            this.buttonWorkDir = new System.Windows.Forms.Button();
            this.folderBrowserDialogWorkDir = new System.Windows.Forms.FolderBrowserDialog();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelDownloads = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelDownloadsN = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelQueued = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelQueuedN = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelSucceed = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelSucceedN = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelError = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelErrorN = new System.Windows.Forms.ToolStripStatusLabel();
            this.numericUpDownMaxDownloads = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.timerMonitor = new System.Windows.Forms.Timer(this.components);
            this.groupBoxOptions = new System.Windows.Forms.GroupBox();
            this.checkBoxNoCacheDir = new System.Windows.Forms.CheckBox();
            this.checkBoxPlayList = new System.Windows.Forms.CheckBox();
            this.comboBoxAudioOnly = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.labelMaxVideoQuality = new System.Windows.Forms.Label();
            this.comboBoxMaxQuality = new System.Windows.Forms.ComboBox();
            this.toolTipHelp = new System.Windows.Forms.ToolTip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonOpenFolder = new System.Windows.Forms.Button();
            this.buttonViewLog = new System.Windows.Forms.Button();
            this.buttonDeleteVideo = new System.Windows.Forms.Button();
            this.buttonRetryDownload = new System.Windows.Forms.Button();
            this.buttonStopDownload = new System.Windows.Forms.Button();
            this.buttonPlayVideo = new System.Windows.Forms.Button();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.buttonRemoveDownload = new System.Windows.Forms.Button();
            this.contextMenuStripListView.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxDownloads)).BeginInit();
            this.groupBoxOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxOptions
            // 
            this.textBoxOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOptions.Location = new System.Drawing.Point(78, 37);
            this.textBoxOptions.Name = "textBoxOptions";
            this.textBoxOptions.Size = new System.Drawing.Size(661, 20);
            this.textBoxOptions.TabIndex = 6;
            // 
            // labelExtraOptions
            // 
            this.labelExtraOptions.AutoSize = true;
            this.labelExtraOptions.Location = new System.Drawing.Point(1, 40);
            this.labelExtraOptions.Name = "labelExtraOptions";
            this.labelExtraOptions.Size = new System.Drawing.Size(71, 13);
            this.labelExtraOptions.TabIndex = 1;
            this.labelExtraOptions.Text = "Extra options:";
            // 
            // textBoxCommand
            // 
            this.textBoxCommand.Enabled = false;
            this.textBoxCommand.Location = new System.Drawing.Point(78, 13);
            this.textBoxCommand.Name = "textBoxCommand";
            this.textBoxCommand.Size = new System.Drawing.Size(78, 20);
            this.textBoxCommand.TabIndex = 0;
            this.textBoxCommand.Text = "youtube-dl.exe";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Command:";
            // 
            // textBoxURL
            // 
            this.textBoxURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxURL.Location = new System.Drawing.Point(69, 7);
            this.textBoxURL.Name = "textBoxURL";
            this.textBoxURL.Size = new System.Drawing.Size(511, 20);
            this.textBoxURL.TabIndex = 0;
            this.toolTipHelp.SetToolTip(this.textBoxURL, "Press Enter to begin Download");
            this.textBoxURL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxURL_KeyPress);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "URL:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 36);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Work. Dir.:";
            // 
            // textBoxWorkDir
            // 
            this.textBoxWorkDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxWorkDir.Location = new System.Drawing.Point(69, 33);
            this.textBoxWorkDir.Name = "textBoxWorkDir";
            this.textBoxWorkDir.Size = new System.Drawing.Size(684, 20);
            this.textBoxWorkDir.TabIndex = 3;
            this.textBoxWorkDir.Text = "c:\\users\\marco\\Downloads";
            // 
            // listViewDownload
            // 
            this.listViewDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewDownload.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.URL,
            this.Status,
            this.File,
            this.WorkDir,
            this.YdlArguments});
            this.listViewDownload.FullRowSelect = true;
            this.listViewDownload.HideSelection = false;
            this.listViewDownload.Location = new System.Drawing.Point(6, 128);
            this.listViewDownload.Name = "listViewDownload";
            this.listViewDownload.Size = new System.Drawing.Size(770, 256);
            this.listViewDownload.SmallImageList = this.imageListStatus;
            this.listViewDownload.TabIndex = 14;
            this.listViewDownload.UseCompatibleStateImageBehavior = false;
            this.listViewDownload.View = System.Windows.Forms.View.Details;
            this.listViewDownload.SelectedIndexChanged += new System.EventHandler(this.listViewDownload_SelectedIndexChanged);
            this.listViewDownload.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listViewDownload_MouseClick);
            this.listViewDownload.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewDownload_MouseDoubleClick);
            // 
            // URL
            // 
            this.URL.Text = "URL";
            this.URL.Width = 200;
            // 
            // Status
            // 
            this.Status.Text = "Status";
            this.Status.Width = 311;
            // 
            // File
            // 
            this.File.Text = "File";
            this.File.Width = 239;
            // 
            // WorkDir
            // 
            this.WorkDir.Text = "Work. Dir.";
            // 
            // YdlArguments
            // 
            this.YdlArguments.Text = "Youtube-dl Arguments";
            // 
            // imageListStatus
            // 
            this.imageListStatus.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListStatus.ImageStream")));
            this.imageListStatus.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListStatus.Images.SetKeyName(0, "GreyBall");
            this.imageListStatus.Images.SetKeyName(1, "GreenArrow");
            this.imageListStatus.Images.SetKeyName(2, "GreenBall");
            this.imageListStatus.Images.SetKeyName(3, "RedX");
            this.imageListStatus.Images.SetKeyName(4, "RedBall");
            // 
            // contextMenuStripListView
            // 
            this.contextMenuStripListView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemPlayVideo,
            this.toolStripMenuItemDelete,
            this.toolStripMenuItemOpenFolder,
            this.toolStripSeparator1,
            this.toolStripMenuItemStop,
            this.toolStripMenuItemRetry,
            this.toolStripMenuItemViewLog,
            this.toolStripMenuItemRemoveDownload});
            this.contextMenuStripListView.Name = "contextMenuStripListView";
            this.contextMenuStripListView.Size = new System.Drawing.Size(140, 164);
            // 
            // toolStripMenuItemPlayVideo
            // 
            this.toolStripMenuItemPlayVideo.Image = global::youtubedlgui.Properties.Resources.PlayButton;
            this.toolStripMenuItemPlayVideo.Name = "toolStripMenuItemPlayVideo";
            this.toolStripMenuItemPlayVideo.Size = new System.Drawing.Size(139, 22);
            this.toolStripMenuItemPlayVideo.Text = "Play";
            this.toolStripMenuItemPlayVideo.Click += new System.EventHandler(this.toolStripMenuItemView_Click);
            // 
            // toolStripMenuItemDelete
            // 
            this.toolStripMenuItemDelete.Image = global::youtubedlgui.Properties.Resources.DeleteButton;
            this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
            this.toolStripMenuItemDelete.Size = new System.Drawing.Size(139, 22);
            this.toolStripMenuItemDelete.Text = "Delete";
            this.toolStripMenuItemDelete.Click += new System.EventHandler(this.toolStripMenuItemDelete_Click);
            // 
            // toolStripMenuItemOpenFolder
            // 
            this.toolStripMenuItemOpenFolder.Image = global::youtubedlgui.Properties.Resources.VideoFolder;
            this.toolStripMenuItemOpenFolder.Name = "toolStripMenuItemOpenFolder";
            this.toolStripMenuItemOpenFolder.Size = new System.Drawing.Size(139, 22);
            this.toolStripMenuItemOpenFolder.Text = "Open Folder";
            this.toolStripMenuItemOpenFolder.Click += new System.EventHandler(this.toolStripMenuItemOpenFolder_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(136, 6);
            // 
            // toolStripMenuItemStop
            // 
            this.toolStripMenuItemStop.Image = global::youtubedlgui.Properties.Resources.CloseButton;
            this.toolStripMenuItemStop.Name = "toolStripMenuItemStop";
            this.toolStripMenuItemStop.Size = new System.Drawing.Size(139, 22);
            this.toolStripMenuItemStop.Text = "Stop";
            this.toolStripMenuItemStop.Click += new System.EventHandler(this.toolStripMenuItemStop_Click);
            // 
            // toolStripMenuItemRetry
            // 
            this.toolStripMenuItemRetry.Image = global::youtubedlgui.Properties.Resources.RetryButton;
            this.toolStripMenuItemRetry.Name = "toolStripMenuItemRetry";
            this.toolStripMenuItemRetry.Size = new System.Drawing.Size(139, 22);
            this.toolStripMenuItemRetry.Text = "Retry";
            this.toolStripMenuItemRetry.Click += new System.EventHandler(this.toolStripMenuItemRetry_Click);
            // 
            // toolStripMenuItemViewLog
            // 
            this.toolStripMenuItemViewLog.Image = global::youtubedlgui.Properties.Resources.ViewLogButton;
            this.toolStripMenuItemViewLog.Name = "toolStripMenuItemViewLog";
            this.toolStripMenuItemViewLog.Size = new System.Drawing.Size(139, 22);
            this.toolStripMenuItemViewLog.Text = "View Log";
            this.toolStripMenuItemViewLog.Click += new System.EventHandler(this.toolStripMenuItemViewLog_Click);
            // 
            // toolStripMenuItemRemoveDownload
            // 
            this.toolStripMenuItemRemoveDownload.Image = global::youtubedlgui.Properties.Resources.DeleteButton;
            this.toolStripMenuItemRemoveDownload.Name = "toolStripMenuItemRemoveDownload";
            this.toolStripMenuItemRemoveDownload.Size = new System.Drawing.Size(139, 22);
            this.toolStripMenuItemRemoveDownload.Text = "Remove";
            this.toolStripMenuItemRemoveDownload.Click += new System.EventHandler(this.toolStripMenuItemRemoveDownload_Click);
            // 
            // checkBoxClipboardPaste
            // 
            this.checkBoxClipboardPaste.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxClipboardPaste.AutoSize = true;
            this.checkBoxClipboardPaste.Checked = true;
            this.checkBoxClipboardPaste.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxClipboardPaste.Location = new System.Drawing.Point(677, 10);
            this.checkBoxClipboardPaste.Name = "checkBoxClipboardPaste";
            this.checkBoxClipboardPaste.Size = new System.Drawing.Size(101, 17);
            this.checkBoxClipboardPaste.TabIndex = 2;
            this.checkBoxClipboardPaste.Text = "Auto Clip. Paste";
            this.checkBoxClipboardPaste.UseVisualStyleBackColor = true;
            // 
            // buttonHelpOptions
            // 
            this.buttonHelpOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHelpOptions.Location = new System.Drawing.Point(740, 37);
            this.buttonHelpOptions.Name = "buttonHelpOptions";
            this.buttonHelpOptions.Size = new System.Drawing.Size(24, 20);
            this.buttonHelpOptions.TabIndex = 7;
            this.buttonHelpOptions.Text = "...";
            this.buttonHelpOptions.UseVisualStyleBackColor = true;
            this.buttonHelpOptions.Click += new System.EventHandler(this.buttonHelpOptions_Click);
            // 
            // buttonWorkDir
            // 
            this.buttonWorkDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonWorkDir.Location = new System.Drawing.Point(754, 33);
            this.buttonWorkDir.Name = "buttonWorkDir";
            this.buttonWorkDir.Size = new System.Drawing.Size(24, 20);
            this.buttonWorkDir.TabIndex = 4;
            this.buttonWorkDir.Text = "...";
            this.buttonWorkDir.UseVisualStyleBackColor = true;
            this.buttonWorkDir.Click += new System.EventHandler(this.buttonWorkDir_Click);
            // 
            // folderBrowserDialogWorkDir
            // 
            this.folderBrowserDialogWorkDir.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Image = global::youtubedlgui.Properties.Resources.UpdateButton;
            this.buttonUpdate.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonUpdate.Location = new System.Drawing.Point(162, 10);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(72, 25);
            this.buttonUpdate.TabIndex = 1;
            this.buttonUpdate.Text = "Update";
            this.buttonUpdate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelDownloads,
            this.toolStripStatusLabelDownloadsN,
            this.toolStripStatusLabelQueued,
            this.toolStripStatusLabelQueuedN,
            this.toolStripStatusLabelSucceed,
            this.toolStripStatusLabelSucceedN,
            this.toolStripStatusLabelError,
            this.toolStripStatusLabelErrorN});
            this.statusStrip1.Location = new System.Drawing.Point(0, 416);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(784, 25);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabelDownloads
            // 
            this.toolStripStatusLabelDownloads.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)(((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabelDownloads.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            this.toolStripStatusLabelDownloads.Image = global::youtubedlgui.Properties.Resources.GreenArrow;
            this.toolStripStatusLabelDownloads.Margin = new System.Windows.Forms.Padding(5, 3, 0, 2);
            this.toolStripStatusLabelDownloads.Name = "toolStripStatusLabelDownloads";
            this.toolStripStatusLabelDownloads.Size = new System.Drawing.Size(89, 20);
            this.toolStripStatusLabelDownloads.Text = "Downloads:";
            this.toolStripStatusLabelDownloads.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // toolStripStatusLabelDownloadsN
            // 
            this.toolStripStatusLabelDownloadsN.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)(((System.Windows.Forms.ToolStripStatusLabelBorderSides.Top | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabelDownloadsN.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            this.toolStripStatusLabelDownloadsN.Margin = new System.Windows.Forms.Padding(-2, 3, 5, 2);
            this.toolStripStatusLabelDownloadsN.Name = "toolStripStatusLabelDownloadsN";
            this.toolStripStatusLabelDownloadsN.Size = new System.Drawing.Size(16, 20);
            this.toolStripStatusLabelDownloadsN.Text = "-";
            this.toolStripStatusLabelDownloadsN.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabelQueued
            // 
            this.toolStripStatusLabelQueued.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)(((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabelQueued.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.toolStripStatusLabelQueued.Image = global::youtubedlgui.Properties.Resources.GreyBall;
            this.toolStripStatusLabelQueued.Name = "toolStripStatusLabelQueued";
            this.toolStripStatusLabelQueued.Size = new System.Drawing.Size(72, 20);
            this.toolStripStatusLabelQueued.Text = "Queued:";
            // 
            // toolStripStatusLabelQueuedN
            // 
            this.toolStripStatusLabelQueuedN.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)(((System.Windows.Forms.ToolStripStatusLabelBorderSides.Top | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabelQueuedN.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.toolStripStatusLabelQueuedN.Margin = new System.Windows.Forms.Padding(-2, 3, 5, 2);
            this.toolStripStatusLabelQueuedN.Name = "toolStripStatusLabelQueuedN";
            this.toolStripStatusLabelQueuedN.Size = new System.Drawing.Size(16, 20);
            this.toolStripStatusLabelQueuedN.Text = "-";
            // 
            // toolStripStatusLabelSucceed
            // 
            this.toolStripStatusLabelSucceed.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)(((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabelSucceed.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.toolStripStatusLabelSucceed.Image = global::youtubedlgui.Properties.Resources.GreenBall;
            this.toolStripStatusLabelSucceed.Name = "toolStripStatusLabelSucceed";
            this.toolStripStatusLabelSucceed.Size = new System.Drawing.Size(74, 20);
            this.toolStripStatusLabelSucceed.Text = "Succeed:";
            // 
            // toolStripStatusLabelSucceedN
            // 
            this.toolStripStatusLabelSucceedN.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)(((System.Windows.Forms.ToolStripStatusLabelBorderSides.Top | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabelSucceedN.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.toolStripStatusLabelSucceedN.Margin = new System.Windows.Forms.Padding(-2, 3, 5, 2);
            this.toolStripStatusLabelSucceedN.Name = "toolStripStatusLabelSucceedN";
            this.toolStripStatusLabelSucceedN.Size = new System.Drawing.Size(16, 20);
            this.toolStripStatusLabelSucceedN.Text = "-";
            // 
            // toolStripStatusLabelError
            // 
            this.toolStripStatusLabelError.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)(((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabelError.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.toolStripStatusLabelError.Image = global::youtubedlgui.Properties.Resources.RedX;
            this.toolStripStatusLabelError.Name = "toolStripStatusLabelError";
            this.toolStripStatusLabelError.Size = new System.Drawing.Size(55, 20);
            this.toolStripStatusLabelError.Text = "Error:";
            // 
            // toolStripStatusLabelErrorN
            // 
            this.toolStripStatusLabelErrorN.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)(((System.Windows.Forms.ToolStripStatusLabelBorderSides.Top | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabelErrorN.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.toolStripStatusLabelErrorN.Margin = new System.Windows.Forms.Padding(-2, 3, 5, 2);
            this.toolStripStatusLabelErrorN.Name = "toolStripStatusLabelErrorN";
            this.toolStripStatusLabelErrorN.Size = new System.Drawing.Size(16, 20);
            this.toolStripStatusLabelErrorN.Text = "-";
            // 
            // numericUpDownMaxDownloads
            // 
            this.numericUpDownMaxDownloads.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownMaxDownloads.Location = new System.Drawing.Point(737, 392);
            this.numericUpDownMaxDownloads.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMaxDownloads.Name = "numericUpDownMaxDownloads";
            this.numericUpDownMaxDownloads.Size = new System.Drawing.Size(39, 20);
            this.numericUpDownMaxDownloads.TabIndex = 13;
            this.numericUpDownMaxDownloads.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(645, 395);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Max. Downloads:";
            // 
            // timerMonitor
            // 
            this.timerMonitor.Interval = 200;
            this.timerMonitor.Tick += new System.EventHandler(this.timerMonitor_Tick);
            // 
            // groupBoxOptions
            // 
            this.groupBoxOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOptions.Controls.Add(this.checkBoxNoCacheDir);
            this.groupBoxOptions.Controls.Add(this.checkBoxPlayList);
            this.groupBoxOptions.Controls.Add(this.comboBoxAudioOnly);
            this.groupBoxOptions.Controls.Add(this.label7);
            this.groupBoxOptions.Controls.Add(this.textBoxCommand);
            this.groupBoxOptions.Controls.Add(this.labelMaxVideoQuality);
            this.groupBoxOptions.Controls.Add(this.comboBoxMaxQuality);
            this.groupBoxOptions.Controls.Add(this.textBoxOptions);
            this.groupBoxOptions.Controls.Add(this.labelExtraOptions);
            this.groupBoxOptions.Controls.Add(this.label2);
            this.groupBoxOptions.Controls.Add(this.buttonUpdate);
            this.groupBoxOptions.Controls.Add(this.buttonHelpOptions);
            this.groupBoxOptions.Location = new System.Drawing.Point(6, 59);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Size = new System.Drawing.Size(770, 63);
            this.groupBoxOptions.TabIndex = 5;
            this.groupBoxOptions.TabStop = false;
            // 
            // checkBoxNoCacheDir
            // 
            this.checkBoxNoCacheDir.AutoSize = true;
            this.checkBoxNoCacheDir.Checked = true;
            this.checkBoxNoCacheDir.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxNoCacheDir.Location = new System.Drawing.Point(667, 13);
            this.checkBoxNoCacheDir.Name = "checkBoxNoCacheDir";
            this.checkBoxNoCacheDir.Size = new System.Drawing.Size(93, 17);
            this.checkBoxNoCacheDir.TabIndex = 5;
            this.checkBoxNoCacheDir.Text = "No Cache Dir.";
            this.checkBoxNoCacheDir.UseVisualStyleBackColor = true;
            // 
            // checkBoxPlayList
            // 
            this.checkBoxPlayList.AutoSize = true;
            this.checkBoxPlayList.Location = new System.Drawing.Point(596, 13);
            this.checkBoxPlayList.Name = "checkBoxPlayList";
            this.checkBoxPlayList.Size = new System.Drawing.Size(65, 17);
            this.checkBoxPlayList.TabIndex = 4;
            this.checkBoxPlayList.Text = "Play List";
            this.checkBoxPlayList.UseVisualStyleBackColor = true;
            // 
            // comboBoxAudioOnly
            // 
            this.comboBoxAudioOnly.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAudioOnly.FormattingEnabled = true;
            this.comboBoxAudioOnly.Items.AddRange(new object[] {
            "-",
            "Best",
            "aac",
            "flac",
            "mp3",
            "m4a",
            "opus",
            "vorbis",
            "wav"});
            this.comboBoxAudioOnly.Location = new System.Drawing.Point(305, 12);
            this.comboBoxAudioOnly.Name = "comboBoxAudioOnly";
            this.comboBoxAudioOnly.Size = new System.Drawing.Size(66, 21);
            this.comboBoxAudioOnly.TabIndex = 2;
            this.comboBoxAudioOnly.SelectedIndexChanged += new System.EventHandler(this.comboBoxAudioOnly_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(240, 17);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(59, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Audio only:";
            // 
            // labelMaxVideoQuality
            // 
            this.labelMaxVideoQuality.AutoSize = true;
            this.labelMaxVideoQuality.Location = new System.Drawing.Point(387, 17);
            this.labelMaxVideoQuality.Name = "labelMaxVideoQuality";
            this.labelMaxVideoQuality.Size = new System.Drawing.Size(98, 13);
            this.labelMaxVideoQuality.TabIndex = 11;
            this.labelMaxVideoQuality.Text = "Max. Video Quality:";
            // 
            // comboBoxMaxQuality
            // 
            this.comboBoxMaxQuality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMaxQuality.FormattingEnabled = true;
            this.comboBoxMaxQuality.Items.AddRange(new object[] {
            "Best",
            "2160p (4K)",
            "1080p (Full HD)",
            "720p (HD)",
            "480p (SD)",
            "360p",
            "240p",
            "Worst"});
            this.comboBoxMaxQuality.Location = new System.Drawing.Point(491, 12);
            this.comboBoxMaxQuality.Name = "comboBoxMaxQuality";
            this.comboBoxMaxQuality.Size = new System.Drawing.Size(99, 21);
            this.comboBoxMaxQuality.TabIndex = 3;
            this.toolTipHelp.SetToolTip(this.comboBoxMaxQuality, "Maximum Download Quality Allowed");
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 395);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Video:";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(277, 395);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(58, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Download:";
            // 
            // buttonOpenFolder
            // 
            this.buttonOpenFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOpenFolder.Enabled = false;
            this.buttonOpenFolder.Image = global::youtubedlgui.Properties.Resources.VideoFolder;
            this.buttonOpenFolder.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonOpenFolder.Location = new System.Drawing.Point(178, 388);
            this.buttonOpenFolder.Name = "buttonOpenFolder";
            this.buttonOpenFolder.Size = new System.Drawing.Size(89, 25);
            this.buttonOpenFolder.TabIndex = 9;
            this.buttonOpenFolder.Text = "Open Folder";
            this.buttonOpenFolder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonOpenFolder.UseVisualStyleBackColor = true;
            this.buttonOpenFolder.Click += new System.EventHandler(this.toolStripMenuItemOpenFolder_Click);
            // 
            // buttonViewLog
            // 
            this.buttonViewLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonViewLog.Enabled = false;
            this.buttonViewLog.Image = global::youtubedlgui.Properties.Resources.ViewLogButton;
            this.buttonViewLog.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonViewLog.Location = new System.Drawing.Point(462, 388);
            this.buttonViewLog.Name = "buttonViewLog";
            this.buttonViewLog.Size = new System.Drawing.Size(51, 25);
            this.buttonViewLog.TabIndex = 12;
            this.buttonViewLog.Text = "Log";
            this.buttonViewLog.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonViewLog.UseVisualStyleBackColor = true;
            this.buttonViewLog.Click += new System.EventHandler(this.toolStripMenuItemViewLog_Click);
            // 
            // buttonDeleteVideo
            // 
            this.buttonDeleteVideo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDeleteVideo.Enabled = false;
            this.buttonDeleteVideo.Image = global::youtubedlgui.Properties.Resources.DeleteButton;
            this.buttonDeleteVideo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonDeleteVideo.Location = new System.Drawing.Point(107, 388);
            this.buttonDeleteVideo.Name = "buttonDeleteVideo";
            this.buttonDeleteVideo.Size = new System.Drawing.Size(65, 25);
            this.buttonDeleteVideo.TabIndex = 8;
            this.buttonDeleteVideo.Text = "Delete";
            this.buttonDeleteVideo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonDeleteVideo.UseVisualStyleBackColor = true;
            this.buttonDeleteVideo.Click += new System.EventHandler(this.toolStripMenuItemDelete_Click);
            // 
            // buttonRetryDownload
            // 
            this.buttonRetryDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRetryDownload.Enabled = false;
            this.buttonRetryDownload.Image = global::youtubedlgui.Properties.Resources.RetryButton;
            this.buttonRetryDownload.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonRetryDownload.Location = new System.Drawing.Point(399, 388);
            this.buttonRetryDownload.Name = "buttonRetryDownload";
            this.buttonRetryDownload.Size = new System.Drawing.Size(57, 25);
            this.buttonRetryDownload.TabIndex = 11;
            this.buttonRetryDownload.Text = "Retry";
            this.buttonRetryDownload.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonRetryDownload.UseVisualStyleBackColor = true;
            this.buttonRetryDownload.Click += new System.EventHandler(this.toolStripMenuItemRetry_Click);
            // 
            // buttonStopDownload
            // 
            this.buttonStopDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonStopDownload.Enabled = false;
            this.buttonStopDownload.Image = global::youtubedlgui.Properties.Resources.CloseButton;
            this.buttonStopDownload.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonStopDownload.Location = new System.Drawing.Point(335, 388);
            this.buttonStopDownload.Name = "buttonStopDownload";
            this.buttonStopDownload.Size = new System.Drawing.Size(58, 25);
            this.buttonStopDownload.TabIndex = 10;
            this.buttonStopDownload.Text = "Stop";
            this.buttonStopDownload.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonStopDownload.UseVisualStyleBackColor = true;
            this.buttonStopDownload.Click += new System.EventHandler(this.toolStripMenuItemStop_Click);
            // 
            // buttonPlayVideo
            // 
            this.buttonPlayVideo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonPlayVideo.Enabled = false;
            this.buttonPlayVideo.Image = global::youtubedlgui.Properties.Resources.PlayButton;
            this.buttonPlayVideo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonPlayVideo.Location = new System.Drawing.Point(50, 388);
            this.buttonPlayVideo.Name = "buttonPlayVideo";
            this.buttonPlayVideo.Size = new System.Drawing.Size(51, 25);
            this.buttonPlayVideo.TabIndex = 7;
            this.buttonPlayVideo.Text = "Play";
            this.buttonPlayVideo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonPlayVideo.UseVisualStyleBackColor = true;
            this.buttonPlayVideo.Click += new System.EventHandler(this.toolStripMenuItemView_Click);
            // 
            // buttonDownload
            // 
            this.buttonDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownload.Image = global::youtubedlgui.Properties.Resources.DownloadButton;
            this.buttonDownload.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonDownload.Location = new System.Drawing.Point(586, 5);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(85, 25);
            this.buttonDownload.TabIndex = 1;
            this.buttonDownload.Text = "&Download";
            this.buttonDownload.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonDownload.UseVisualStyleBackColor = true;
            this.buttonDownload.Click += new System.EventHandler(this.buttonDownload_Click);
            // 
            // buttonRemoveDownload
            // 
            this.buttonRemoveDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRemoveDownload.Enabled = false;
            this.buttonRemoveDownload.Image = global::youtubedlgui.Properties.Resources.DeleteButton;
            this.buttonRemoveDownload.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonRemoveDownload.Location = new System.Drawing.Point(519, 388);
            this.buttonRemoveDownload.Name = "buttonRemoveDownload";
            this.buttonRemoveDownload.Size = new System.Drawing.Size(72, 25);
            this.buttonRemoveDownload.TabIndex = 16;
            this.buttonRemoveDownload.Text = "Remove";
            this.buttonRemoveDownload.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonRemoveDownload.UseVisualStyleBackColor = true;
            this.buttonRemoveDownload.Click += new System.EventHandler(this.toolStripMenuItemRemoveDownload_Click);
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 441);
            this.Controls.Add(this.buttonRemoveDownload);
            this.Controls.Add(this.buttonOpenFolder);
            this.Controls.Add(this.buttonViewLog);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonDeleteVideo);
            this.Controls.Add(this.buttonRetryDownload);
            this.Controls.Add(this.buttonStopDownload);
            this.Controls.Add(this.buttonPlayVideo);
            this.Controls.Add(this.checkBoxClipboardPaste);
            this.Controls.Add(this.numericUpDownMaxDownloads);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.groupBoxOptions);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.buttonWorkDir);
            this.Controls.Add(this.listViewDownload);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxWorkDir);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxURL);
            this.Controls.Add(this.buttonDownload);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(800, 480);
            this.Name = "formMain";
            this.Text = "Youtube-DL GUI";
            this.Activated += new System.EventHandler(this.formMain_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.formMain_FormClosed);
            this.Load += new System.EventHandler(this.formMain_Load);
            this.contextMenuStripListView.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxDownloads)).EndInit();
            this.groupBoxOptions.ResumeLayout(false);
            this.groupBoxOptions.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxOptions;
        private System.Windows.Forms.Label labelExtraOptions;
        private System.Windows.Forms.TextBox textBoxCommand;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.TextBox textBoxURL;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxWorkDir;
        private System.Windows.Forms.ListView listViewDownload;
        private System.Windows.Forms.ColumnHeader URL;
        private System.Windows.Forms.ColumnHeader Status;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripListView;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPlayVideo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStop;
        private System.Windows.Forms.ColumnHeader File;
        private System.Windows.Forms.CheckBox checkBoxClipboardPaste;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRetry;
        private System.Windows.Forms.Button buttonHelpOptions;
        private System.Windows.Forms.Button buttonWorkDir;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogWorkDir;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelDownloads;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelQueued;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxDownloads;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Timer timerMonitor;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemViewLog;
        private System.Windows.Forms.GroupBox groupBoxOptions;
        private System.Windows.Forms.ImageList imageListStatus;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelSucceed;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelError;
        private System.Windows.Forms.ToolTip toolTipHelp;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelDownloadsN;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelQueuedN;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelSucceedN;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelErrorN;
        private System.Windows.Forms.ComboBox comboBoxMaxQuality;
        private System.Windows.Forms.Label labelMaxVideoQuality;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox checkBoxNoCacheDir;
        private System.Windows.Forms.CheckBox checkBoxPlayList;
        private System.Windows.Forms.ComboBox comboBoxAudioOnly;
        private System.Windows.Forms.Button buttonPlayVideo;
        private System.Windows.Forms.Button buttonStopDownload;
        private System.Windows.Forms.Button buttonRetryDownload;
        private System.Windows.Forms.Button buttonDeleteVideo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonViewLog;
        private System.Windows.Forms.Button buttonOpenFolder;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpenFolder;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ColumnHeader WorkDir;
        private System.Windows.Forms.ColumnHeader YdlArguments;
        private System.Windows.Forms.Button buttonRemoveDownload;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRemoveDownload;
    }
}

