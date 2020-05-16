using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace youtubedlgui
{
    public partial class formHelpOptions : Form
    {
        public String Options;
        public String HelpOptions;
        
        private Boolean Changed { get => textBoxOptions.Text != Options; }

        public formHelpOptions()
        {
            InitializeComponent();
        }

        private void formHelpOptions_Load(object sender, EventArgs e)
        {
            textHelpOptions.Text = HelpOptions;
            textBoxOptions.Text = Options;
        }

        private void formHelpOptions_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Changed) 
            {
                DialogResult dr = MessageBox.Show(this, "Save changes?", "Youtube-dl Extra Options was Changed", MessageBoxButtons.YesNoCancel);

                if (dr == DialogResult.Yes) Options = textBoxOptions.Text;
                if (dr == DialogResult.Cancel || dr == DialogResult.None) e.Cancel = true;            
            }
        }

        private void buttonFind_Click(object sender, EventArgs e)
        {
            int pos = textHelpOptions.Find(textBoxFind.Text, textHelpOptions.SelectionStart + textHelpOptions.SelectionLength, RichTextBoxFinds.None);
            if (pos == -1) { 
                if (textHelpOptions.SelectionStart == 0) { MessageBox.Show("Text not found.", "Atention", MessageBoxButtons.OK); return;  }
                DialogResult dr = MessageBox.Show("Text not found. Find from begining?", "Atention", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    textHelpOptions.SelectionStart = 0;
                    textHelpOptions.SelectionLength = 0;
                    buttonFind_Click(sender, e);
                }
            }

        }

        private void buttonDefaultOptions_Click(object sender, EventArgs e)
        {
            youtubedlgui.Properties.Settings.Default.Reset();
            textBoxOptions.Text = youtubedlgui.Properties.Settings.Default.Options;
        }

        private void textBoxFind_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r') buttonFind_Click(sender, null);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Options = textBoxOptions.Text;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/ytdl-org/youtube-dl/blob/master/README.md");
        }

        private void textHelpOptions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F) textBoxFind.Focus();
        }


        private void textBoxOptions_KeyPress(object sender, KeyPressEventArgs e)
        {
            buttonSave.Enabled = true;
        }

    }
}
