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
    }
}
