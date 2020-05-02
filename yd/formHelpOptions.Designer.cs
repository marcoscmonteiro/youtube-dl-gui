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
            this.SuspendLayout();
            // 
            // textBoxFind
            // 
            this.textBoxFind.Location = new System.Drawing.Point(76, 2);
            this.textBoxFind.Name = "textBoxFind";
            this.textBoxFind.Size = new System.Drawing.Size(196, 20);
            this.textBoxFind.TabIndex = 0;
            this.textBoxFind.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFind_KeyPress);
            // 
            // buttonFind
            // 
            this.buttonFind.Cursor = System.Windows.Forms.Cursors.Default;
            this.buttonFind.Location = new System.Drawing.Point(278, 2);
            this.buttonFind.Name = "buttonFind";
            this.buttonFind.Size = new System.Drawing.Size(37, 21);
            this.buttonFind.TabIndex = 1;
            this.buttonFind.Text = "&Find";
            this.buttonFind.UseVisualStyleBackColor = true;
            this.buttonFind.Click += new System.EventHandler(this.buttonFind_Click);
            // 
            // textHelpOptions
            // 
            this.textHelpOptions.AccessibleRole = System.Windows.Forms.AccessibleRole.MenuBar;
            this.textHelpOptions.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textHelpOptions.HideSelection = false;
            this.textHelpOptions.Location = new System.Drawing.Point(0, 28);
            this.textHelpOptions.Name = "textHelpOptions";
            this.textHelpOptions.ReadOnly = true;
            this.textHelpOptions.Size = new System.Drawing.Size(607, 499);
            this.textHelpOptions.TabIndex = 3;
            this.textHelpOptions.Text = "";
            // 
            // buttonDefaultOptions
            // 
            this.buttonDefaultOptions.Location = new System.Drawing.Point(499, 2);
            this.buttonDefaultOptions.Name = "buttonDefaultOptions";
            this.buttonDefaultOptions.Size = new System.Drawing.Size(108, 21);
            this.buttonDefaultOptions.TabIndex = 2;
            this.buttonDefaultOptions.Text = "Set Default Options";
            this.buttonDefaultOptions.UseVisualStyleBackColor = true;
            this.buttonDefaultOptions.Click += new System.EventHandler(this.buttonDefaultOptions_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Text to find:";
            // 
            // formHelpOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(609, 527);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonDefaultOptions);
            this.Controls.Add(this.textHelpOptions);
            this.Controls.Add(this.buttonFind);
            this.Controls.Add(this.textBoxFind);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
    }
}