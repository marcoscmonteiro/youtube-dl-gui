namespace youtubedlgui
{
    partial class formHelpOptions
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formHelpOptions));
            this.textBoxFind = new System.Windows.Forms.TextBox();
            this.buttonFind = new System.Windows.Forms.Button();
            this.textHelpOptions = new System.Windows.Forms.RichTextBox();
            this.buttonDefaultOptions = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxOptions = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxFind
            // 
            this.textBoxFind.Location = new System.Drawing.Point(66, 29);
            this.textBoxFind.Name = "textBoxFind";
            this.textBoxFind.Size = new System.Drawing.Size(212, 20);
            this.textBoxFind.TabIndex = 2;
            this.textBoxFind.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFind_KeyPress);
            // 
            // buttonFind
            // 
            this.buttonFind.Cursor = System.Windows.Forms.Cursors.Default;
            this.buttonFind.Location = new System.Drawing.Point(284, 28);
            this.buttonFind.Name = "buttonFind";
            this.buttonFind.Size = new System.Drawing.Size(79, 21);
            this.buttonFind.TabIndex = 3;
            this.buttonFind.Text = "&Find / Next";
            this.buttonFind.UseVisualStyleBackColor = true;
            this.buttonFind.Click += new System.EventHandler(this.buttonFind_Click);
            // 
            // textHelpOptions
            // 
            this.textHelpOptions.AccessibleRole = System.Windows.Forms.AccessibleRole.MenuBar;
            this.textHelpOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textHelpOptions.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textHelpOptions.HideSelection = false;
            this.textHelpOptions.Location = new System.Drawing.Point(3, 52);
            this.textHelpOptions.Name = "textHelpOptions";
            this.textHelpOptions.ReadOnly = true;
            this.textHelpOptions.Size = new System.Drawing.Size(604, 424);
            this.textHelpOptions.TabIndex = 4;
            this.textHelpOptions.Text = "";
            // 
            // buttonDefaultOptions
            // 
            this.buttonDefaultOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDefaultOptions.Location = new System.Drawing.Point(499, 3);
            this.buttonDefaultOptions.Name = "buttonDefaultOptions";
            this.buttonDefaultOptions.Size = new System.Drawing.Size(108, 21);
            this.buttonDefaultOptions.TabIndex = 1;
            this.buttonDefaultOptions.Text = "Set Default Options";
            this.buttonDefaultOptions.UseVisualStyleBackColor = true;
            this.buttonDefaultOptions.Click += new System.EventHandler(this.buttonDefaultOptions_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Text to find:";
            // 
            // textBoxOptions
            // 
            this.textBoxOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOptions.Location = new System.Drawing.Point(66, 3);
            this.textBoxOptions.Name = "textBoxOptions";
            this.textBoxOptions.Size = new System.Drawing.Size(427, 20);
            this.textBoxOptions.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Options:";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 505);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(609, 22);
            this.statusStrip1.TabIndex = 8;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(451, 479);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 9;
            this.buttonSave.Text = "&Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(532, 479);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // formHelpOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(609, 527);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxOptions);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonDefaultOptions);
            this.Controls.Add(this.textHelpOptions);
            this.Controls.Add(this.buttonFind);
            this.Controls.Add(this.textBoxFind);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(625, 566);
            this.Name = "formHelpOptions";
            this.Text = "--Help Options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formHelpOptions_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.formHelpOptions_FormClosed);
            this.Load += new System.EventHandler(this.formHelpOptions_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBoxFind;
        private System.Windows.Forms.Button buttonFind;
        private System.Windows.Forms.RichTextBox textHelpOptions;
        private System.Windows.Forms.Button buttonDefaultOptions;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxOptions;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
    }
}