namespace youtubedlgui
{
    partial class formUpdate
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formUpdate));
            this.textBoxUpdate = new System.Windows.Forms.TextBox();
            this.buttonClose = new System.Windows.Forms.Button();
            this.timerVerifyUpdate = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // textBoxUpdate
            // 
            this.textBoxUpdate.Location = new System.Drawing.Point(10, 10);
            this.textBoxUpdate.Multiline = true;
            this.textBoxUpdate.Name = "textBoxUpdate";
            this.textBoxUpdate.ReadOnly = true;
            this.textBoxUpdate.Size = new System.Drawing.Size(481, 199);
            this.textBoxUpdate.TabIndex = 0;
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(178, 219);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(146, 29);
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Text = "&Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // timerVerifyUpdate
            // 
            this.timerVerifyUpdate.Tick += new System.EventHandler(this.timerVerifyUpdate_Tick);
            // 
            // formUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 260);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.textBoxUpdate);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "formUpdate";
            this.Text = "Youtube-dl Update";
            this.Activated += new System.EventHandler(this.formUpdate_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formUpdate_FormClosing);
            this.Load += new System.EventHandler(this.formUpdate_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxUpdate;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Timer timerVerifyUpdate;
    }
}