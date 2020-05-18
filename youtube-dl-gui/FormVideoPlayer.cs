using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace youtubedlgui
{


    public partial class formVideoPlayer : Form
    {

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public String File;
        private Process ffplay;

        public formVideoPlayer()
        {
            InitializeComponent();
        }

        private void formVideoPlayer_Load(object sender, EventArgs e)
        {
            // start ffplay 
            ffplay = new Process
            {
                StartInfo =
            {
                FileName = AppContext.BaseDirectory + "ffplay",
                Arguments = "\"" + File + "\"",
                // hides the command window
                CreateNoWindow = true, 
                // redirect input, output, and error streams..
                RedirectStandardError = false,
                RedirectStandardOutput = false,
                UseShellExecute = false
            }
            };

            ffplay.EnableRaisingEvents = true;
            //ffplay.OutputDataReceived += (o, e) => Debug.WriteLine(e.Data ?? "NULL", "ffplay");
            //ffplay.ErrorDataReceived += (o, e) => Debug.WriteLine(e.Data ?? "NULL", "ffplay");
            //ffplay.Exited += (o, e) => Debug.WriteLine("Exited", "ffplay");
            ffplay.Start();

            Thread.Sleep(1000); // you need to wait/check the process started, then...

            //while (Process.GetProcessById(ffplay.Id)==null) { }

            // child, new parent
            // make 'this' the parent of ffmpeg (presuming you are in scope of a Form or Control)
            SetParent(ffplay.MainWindowHandle, this.Handle);

            // window, x, y, width, height, repaint
            // move the ffplayer window to the top-left corner and set the size to 320x280
            MoveWindow(ffplay.MainWindowHandle, 0, 0, this.Width, this.Height, true);
        }

        private void formVideoPlayer_Resize(object sender, EventArgs e)
        {
            MoveWindow(ffplay.MainWindowHandle, 0, 0, this.Width, this.Height, true);
        }

        private void formVideoPlayer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (ffplay != null) ffplay.Kill();
        }
    }
}
