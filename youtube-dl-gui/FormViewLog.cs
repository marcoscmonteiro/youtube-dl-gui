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
    public partial class formViewLog : Form
    {
        public formViewLog()
        {
            InitializeComponent();
        }
        public formViewLog(StringBuilder sb)
        {
            InitializeComponent();
            textBoxLog.Text = sb.ToString();
            ShowDialog();
        }

        private void formViewLog_Load(object sender, EventArgs e)
        {

        }
    }
}
