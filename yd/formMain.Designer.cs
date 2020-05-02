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
            this.contextMenuStripListView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemView = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemStop = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRetry = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBoxClipboardPaste = new System.Windows.Forms.CheckBox();
            this.buttonHelpOptions = new System.Windows.Forms.Button();
            this.buttonWorkDir = new System.Windows.Forms.Button();
            this.folderBrowserDialogWorkDir = new System.Windows.Forms.FolderBrowserDialog();
            this.contextMenuStripListView.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxOptions
            // 
            this.textBoxOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOptions.Location = new System.Drawing.Point(67, 64);
            this.textBoxOptions.Name = "textBoxOptions";
            this.textBoxOptions.Size = new System.Drawing.Size(645, 20);
            this.textBoxOptions.TabIndex = 3;
            this.textBoxOptions.Text = "--no-playlist --no-cache-dir";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Options:";
            // 
            // textBoxCommand
            // 
            this.textBoxCommand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCommand.Location = new System.Drawing.Point(67, 12);
            this.textBoxCommand.Name = "textBoxCommand";
            this.textBoxCommand.Size = new System.Drawing.Size(670, 20);
            this.textBoxCommand.TabIndex = 0;
            this.textBoxCommand.Text = "youtube-dl.exe";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Command:";
            // 
            // buttonDownload
            // 
            this.buttonDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownload.Location = new System.Drawing.Point(625, 115);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(112, 23);
            this.buttonDownload.TabIndex = 7;
            this.buttonDownload.Text = "&Download";
            this.buttonDownload.UseVisualStyleBackColor = true;
            this.buttonDownload.Click += new System.EventHandler(this.buttonDownload_Click);
            // 
            // textBoxURL
            // 
            this.textBoxURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxURL.Location = new System.Drawing.Point(67, 89);
            this.textBoxURL.Name = "textBoxURL";
            this.textBoxURL.Size = new System.Drawing.Size(564, 20);
            this.textBoxURL.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "URL:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Work. Dir.:";
            // 
            // textBoxWorkDir
            // 
            this.textBoxWorkDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxWorkDir.Location = new System.Drawing.Point(67, 38);
            this.textBoxWorkDir.Name = "textBoxWorkDir";
            this.textBoxWorkDir.Size = new System.Drawing.Size(645, 20);
            this.textBoxWorkDir.TabIndex = 1;
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
            this.listViewDownload.Location = new System.Drawing.Point(6, 144);
            this.listViewDownload.Name = "listViewDownload";
            this.listViewDownload.Size = new System.Drawing.Size(731, 232);
            this.listViewDownload.TabIndex = 8;
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
            this.Status.Width = 200;
            // 
            // File
            // 
            this.File.Text = "File";
            this.File.Width = 200;
            // 
            // contextMenuStripListView
            // 
            this.contextMenuStripListView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemView,
            this.toolStripMenuItemStop,
            this.toolStripMenuItemRetry});
            this.contextMenuStripListView.Name = "contextMenuStripListView";
            this.contextMenuStripListView.Size = new System.Drawing.Size(187, 70);
            // 
            // toolStripMenuItemView
            // 
            this.toolStripMenuItemView.Name = "toolStripMenuItemView";
            this.toolStripMenuItemView.Size = new System.Drawing.Size(186, 22);
            this.toolStripMenuItemView.Text = "Play Video Download";
            this.toolStripMenuItemView.Click += new System.EventHandler(this.toolStripMenuItemView_Click);
            // 
            // toolStripMenuItemStop
            // 
            this.toolStripMenuItemStop.Name = "toolStripMenuItemStop";
            this.toolStripMenuItemStop.Size = new System.Drawing.Size(186, 22);
            this.toolStripMenuItemStop.Text = "Stop Download";
            this.toolStripMenuItemStop.Click += new System.EventHandler(this.toolStripMenuItemStop_Click);
            // 
            // toolStripMenuItemRetry
            // 
            this.toolStripMenuItemRetry.Name = "toolStripMenuItemRetry";
            this.toolStripMenuItemRetry.Size = new System.Drawing.Size(186, 22);
            this.toolStripMenuItemRetry.Text = "Retry Download";
            this.toolStripMenuItemRetry.Click += new System.EventHandler(this.toolStripMenuItemRetry_Click);
            // 
            // checkBoxClipboardPaste
            // 
            this.checkBoxClipboardPaste.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxClipboardPaste.AutoSize = true;
            this.checkBoxClipboardPaste.Checked = true;
            this.checkBoxClipboardPaste.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxClipboardPaste.Location = new System.Drawing.Point(637, 92);
            this.checkBoxClipboardPaste.Name = "checkBoxClipboardPaste";
            this.checkBoxClipboardPaste.Size = new System.Drawing.Size(100, 17);
            this.checkBoxClipboardPaste.TabIndex = 6;
            this.checkBoxClipboardPaste.Text = "Clipboard Paste";
            this.checkBoxClipboardPaste.UseVisualStyleBackColor = true;
            // 
            // buttonHelpOptions
            // 
            this.buttonHelpOptions.Location = new System.Drawing.Point(713, 64);
            this.buttonHelpOptions.Name = "buttonHelpOptions";
            this.buttonHelpOptions.Size = new System.Drawing.Size(24, 20);
            this.buttonHelpOptions.TabIndex = 4;
            this.buttonHelpOptions.Text = "...";
            this.buttonHelpOptions.UseVisualStyleBackColor = true;
            this.buttonHelpOptions.Click += new System.EventHandler(this.buttonHelpOptions_Click);
            // 
            // buttonWorkDir
            // 
            this.buttonWorkDir.Location = new System.Drawing.Point(713, 38);
            this.buttonWorkDir.Name = "buttonWorkDir";
            this.buttonWorkDir.Size = new System.Drawing.Size(24, 20);
            this.buttonWorkDir.TabIndex = 2;
            this.buttonWorkDir.Text = "...";
            this.buttonWorkDir.UseVisualStyleBackColor = true;
            this.buttonWorkDir.Click += new System.EventHandler(this.buttonWorkDir_Click);
            // 
            // folderBrowserDialogWorkDir
            // 
            this.folderBrowserDialogWorkDir.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(745, 382);
            this.Controls.Add(this.buttonWorkDir);
            this.Controls.Add(this.buttonHelpOptions);
            this.Controls.Add(this.checkBoxClipboardPaste);
            this.Controls.Add(this.listViewDownload);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxWorkDir);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxURL);
            this.Controls.Add(this.buttonDownload);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxCommand);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxOptions);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "formMain";
            this.Text = "Youtube-DL GUI";
            this.Activated += new System.EventHandler(this.formMain_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.formMain_Load);
            this.contextMenuStripListView.ResumeLayout(false);
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
    }
}

