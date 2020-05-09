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
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxCommand = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.textBoxURL = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxWorkDir = new System.Windows.Forms.TextBox();
            this.listViewDownload = new System.Windows.Forms.ListView();
            this.URL = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.File = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageListStatus = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuStripListView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemView = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemStop = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRetry = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemDeletePartial = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemViewLog = new System.Windows.Forms.ToolStripMenuItem();
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
            this.groupBoxInteface = new System.Windows.Forms.GroupBox();
            this.toolTipHelp = new System.Windows.Forms.ToolTip(this.components);
            this.comboBoxMaxQuality = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.contextMenuStripListView.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxDownloads)).BeginInit();
            this.groupBoxOptions.SuspendLayout();
            this.groupBoxInteface.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxOptions
            // 
            this.textBoxOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOptions.Location = new System.Drawing.Point(65, 36);
            this.textBoxOptions.Name = "textBoxOptions";
            this.textBoxOptions.Size = new System.Drawing.Size(462, 20);
            this.textBoxOptions.TabIndex = 2;
            this.textBoxOptions.Text = "--no-playlist --no-cache-dir";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Options:";
            // 
            // textBoxCommand
            // 
            this.textBoxCommand.Enabled = false;
            this.textBoxCommand.Location = new System.Drawing.Point(65, 10);
            this.textBoxCommand.Name = "textBoxCommand";
            this.textBoxCommand.Size = new System.Drawing.Size(84, 20);
            this.textBoxCommand.TabIndex = 0;
            this.textBoxCommand.Text = "youtube-dl.exe";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Command:";
            // 
            // buttonDownload
            // 
            this.buttonDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownload.Location = new System.Drawing.Point(651, 4);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(85, 23);
            this.buttonDownload.TabIndex = 2;
            this.buttonDownload.Text = "&Download";
            this.buttonDownload.UseVisualStyleBackColor = true;
            this.buttonDownload.Click += new System.EventHandler(this.buttonDownload_Click);
            // 
            // textBoxURL
            // 
            this.textBoxURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxURL.Location = new System.Drawing.Point(69, 7);
            this.textBoxURL.Name = "textBoxURL";
            this.textBoxURL.Size = new System.Drawing.Size(390, 20);
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
            this.textBoxWorkDir.Size = new System.Drawing.Size(644, 20);
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
            this.File});
            this.listViewDownload.HideSelection = false;
            this.listViewDownload.Location = new System.Drawing.Point(6, 126);
            this.listViewDownload.Name = "listViewDownload";
            this.listViewDownload.Size = new System.Drawing.Size(730, 207);
            this.listViewDownload.SmallImageList = this.imageListStatus;
            this.listViewDownload.TabIndex = 7;
            this.listViewDownload.UseCompatibleStateImageBehavior = false;
            this.listViewDownload.View = System.Windows.Forms.View.Details;
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
            this.File.Width = 200;
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
            this.toolStripMenuItemView,
            this.toolStripMenuItemStop,
            this.toolStripMenuItemRetry,
            this.ToolStripMenuItemDeletePartial,
            this.toolStripMenuItemViewLog});
            this.contextMenuStripListView.Name = "contextMenuStripListView";
            this.contextMenuStripListView.Size = new System.Drawing.Size(201, 114);
            // 
            // toolStripMenuItemView
            // 
            this.toolStripMenuItemView.Name = "toolStripMenuItemView";
            this.toolStripMenuItemView.Size = new System.Drawing.Size(200, 22);
            this.toolStripMenuItemView.Text = "Play Video";
            this.toolStripMenuItemView.Click += new System.EventHandler(this.toolStripMenuItemView_Click);
            // 
            // toolStripMenuItemStop
            // 
            this.toolStripMenuItemStop.Name = "toolStripMenuItemStop";
            this.toolStripMenuItemStop.Size = new System.Drawing.Size(200, 22);
            this.toolStripMenuItemStop.Text = "Stop Download";
            this.toolStripMenuItemStop.Click += new System.EventHandler(this.toolStripMenuItemStop_Click);
            // 
            // toolStripMenuItemRetry
            // 
            this.toolStripMenuItemRetry.Name = "toolStripMenuItemRetry";
            this.toolStripMenuItemRetry.Size = new System.Drawing.Size(200, 22);
            this.toolStripMenuItemRetry.Text = "Retry Download";
            this.toolStripMenuItemRetry.Click += new System.EventHandler(this.toolStripMenuItemRetry_Click);
            // 
            // ToolStripMenuItemDeletePartial
            // 
            this.ToolStripMenuItemDeletePartial.Name = "ToolStripMenuItemDeletePartial";
            this.ToolStripMenuItemDeletePartial.Size = new System.Drawing.Size(200, 22);
            this.ToolStripMenuItemDeletePartial.Text = "Delete partial Download";
            this.ToolStripMenuItemDeletePartial.Click += new System.EventHandler(this.ToolStripMenuItemDeletePartial_Click);
            // 
            // toolStripMenuItemViewLog
            // 
            this.toolStripMenuItemViewLog.Name = "toolStripMenuItemViewLog";
            this.toolStripMenuItemViewLog.Size = new System.Drawing.Size(200, 22);
            this.toolStripMenuItemViewLog.Text = "View Log";
            this.toolStripMenuItemViewLog.Click += new System.EventHandler(this.toolStripMenuItemViewLog_Click);
            // 
            // checkBoxClipboardPaste
            // 
            this.checkBoxClipboardPaste.AutoSize = true;
            this.checkBoxClipboardPaste.Checked = true;
            this.checkBoxClipboardPaste.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxClipboardPaste.Location = new System.Drawing.Point(10, 13);
            this.checkBoxClipboardPaste.Name = "checkBoxClipboardPaste";
            this.checkBoxClipboardPaste.Size = new System.Drawing.Size(150, 17);
            this.checkBoxClipboardPaste.TabIndex = 0;
            this.checkBoxClipboardPaste.Text = "Auto URL Clipboard Paste";
            this.checkBoxClipboardPaste.UseVisualStyleBackColor = true;
            // 
            // buttonHelpOptions
            // 
            this.buttonHelpOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHelpOptions.Location = new System.Drawing.Point(528, 36);
            this.buttonHelpOptions.Name = "buttonHelpOptions";
            this.buttonHelpOptions.Size = new System.Drawing.Size(24, 20);
            this.buttonHelpOptions.TabIndex = 5;
            this.buttonHelpOptions.Text = "...";
            this.buttonHelpOptions.UseVisualStyleBackColor = true;
            this.buttonHelpOptions.Click += new System.EventHandler(this.buttonHelpOptions_Click);
            // 
            // buttonWorkDir
            // 
            this.buttonWorkDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonWorkDir.Location = new System.Drawing.Point(714, 33);
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
            this.buttonUpdate.Location = new System.Drawing.Point(155, 9);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(72, 22);
            this.buttonUpdate.TabIndex = 1;
            this.buttonUpdate.Text = "Update";
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
            this.statusStrip1.Location = new System.Drawing.Point(0, 336);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(744, 25);
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
            this.numericUpDownMaxDownloads.Location = new System.Drawing.Point(10, 35);
            this.numericUpDownMaxDownloads.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMaxDownloads.Name = "numericUpDownMaxDownloads";
            this.numericUpDownMaxDownloads.Size = new System.Drawing.Size(39, 20);
            this.numericUpDownMaxDownloads.TabIndex = 1;
            this.numericUpDownMaxDownloads.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(55, 39);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Max. Downloads";
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
            this.groupBoxOptions.Controls.Add(this.textBoxCommand);
            this.groupBoxOptions.Controls.Add(this.textBoxOptions);
            this.groupBoxOptions.Controls.Add(this.label1);
            this.groupBoxOptions.Controls.Add(this.label2);
            this.groupBoxOptions.Controls.Add(this.buttonUpdate);
            this.groupBoxOptions.Controls.Add(this.buttonHelpOptions);
            this.groupBoxOptions.Location = new System.Drawing.Point(6, 59);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Size = new System.Drawing.Size(558, 61);
            this.groupBoxOptions.TabIndex = 5;
            this.groupBoxOptions.TabStop = false;
            // 
            // groupBoxInteface
            // 
            this.groupBoxInteface.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxInteface.Controls.Add(this.checkBoxClipboardPaste);
            this.groupBoxInteface.Controls.Add(this.numericUpDownMaxDownloads);
            this.groupBoxInteface.Controls.Add(this.label5);
            this.groupBoxInteface.Location = new System.Drawing.Point(569, 59);
            this.groupBoxInteface.Name = "groupBoxInteface";
            this.groupBoxInteface.Size = new System.Drawing.Size(168, 61);
            this.groupBoxInteface.TabIndex = 6;
            this.groupBoxInteface.TabStop = false;
            // 
            // comboBoxMaxQuality
            // 
            this.comboBoxMaxQuality.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
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
            this.comboBoxMaxQuality.Location = new System.Drawing.Point(534, 7);
            this.comboBoxMaxQuality.Name = "comboBoxMaxQuality";
            this.comboBoxMaxQuality.Size = new System.Drawing.Size(111, 21);
            this.comboBoxMaxQuality.TabIndex = 1;
            this.toolTipHelp.SetToolTip(this.comboBoxMaxQuality, "Maximum Download Quality Allowed");
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(465, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(68, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Max. Quality:";
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 361);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.comboBoxMaxQuality);
            this.Controls.Add(this.groupBoxInteface);
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
            this.MinimumSize = new System.Drawing.Size(760, 400);
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
            this.groupBoxInteface.ResumeLayout(false);
            this.groupBoxInteface.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxOptions;
        private System.Windows.Forms.Label label1;
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
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemView;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStop;
        private System.Windows.Forms.ColumnHeader File;
        private System.Windows.Forms.CheckBox checkBoxClipboardPaste;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRetry;
        private System.Windows.Forms.Button buttonHelpOptions;
        private System.Windows.Forms.Button buttonWorkDir;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogWorkDir;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemDeletePartial;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelDownloads;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelQueued;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxDownloads;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Timer timerMonitor;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemViewLog;
        private System.Windows.Forms.GroupBox groupBoxOptions;
        private System.Windows.Forms.GroupBox groupBoxInteface;
        private System.Windows.Forms.ImageList imageListStatus;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelSucceed;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelError;
        private System.Windows.Forms.ToolTip toolTipHelp;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelDownloadsN;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelQueuedN;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelSucceedN;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelErrorN;
        private System.Windows.Forms.ComboBox comboBoxMaxQuality;
        private System.Windows.Forms.Label label6;
    }
}

