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

        public formHelpOptions()
        {
            InitializeComponent();
        }

        public formMain ParentMainForm;

        private void formHelpOptions_Load(object sender, EventArgs e)
        {

        }

        public void setText(String text)
        {
            textHelpOptions.Text = text;
        }

        private void formHelpOptions_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void formHelpOptions_FormClosed(object sender, FormClosedEventArgs e)
        {
            ParentMainForm.formHelpOptionsInstance = null;
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
            ParentMainForm.SetDefaultOptions();
        }

        private void textBoxFind_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r') buttonFind_Click(sender, null);
        }
    }
}
