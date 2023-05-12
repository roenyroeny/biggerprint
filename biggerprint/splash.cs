using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;

namespace biggerprint
{
    public partial class splash : Form
    {
        float progress = 0.0f;
        string log = "";

        public splash()
        {
            InitializeComponent();
        }

        private volatile float foo;
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            log += "Loading Settings...\n";
            Settings.LoadSettings();
            log += "Enumerating Printers...\n";
            PrintDocument pdoc = new PrintDocument();
            Settings.pageSize = Utility.Unfreedom(pdoc.DefaultPageSettings.PrintableArea.Size);
            PrinterSettings printerSettings = new PrinterSettings();

            log += $"Printer found : {printerSettings.PrinterName}\n";
            log += $"Fetching default settings...\n";
            foo = pdoc.DefaultPageSettings.PrintableArea.Size.Width; // this forces the implemenation to go and find printers 

            progress = 1.0f;
        }

        private void splash_Load(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (progress >= 1.0f)
            {
                Close();
                return;
            }
            progressBar1.Value = (int)(progress * 100.0f);
            label1.Text = log;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
