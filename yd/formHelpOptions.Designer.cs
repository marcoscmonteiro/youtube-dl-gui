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
            this.textHelpOptions = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textHelpOptions
            // 
            this.textHelpOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textHelpOptions.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textHelpOptions.Location = new System.Drawing.Point(0, 0);
            this.textHelpOptions.Multiline = true;
            this.textHelpOptions.Name = "textHelpOptions";
            this.textHelpOptions.ReadOnly = true;
            this.textHelpOptions.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textHelpOptions.Size = new System.Drawing.Size(639, 450);
            this.textHelpOptions.TabIndex = 0;
            // 
            // formHelpOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 450);
            this.Controls.Add(this.textHelpOptions);
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

        private System.Windows.Forms.TextBox textHelpOptions;
    }
}