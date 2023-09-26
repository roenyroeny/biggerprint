using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace biggerprint
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new splash().ShowDialog();

            var form = new Main();

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
                form.ImportNewDocument(args[1]);

            Application.Run(form);
        }
    }
}
