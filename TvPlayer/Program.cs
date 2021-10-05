using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TvPlayer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!System.IO.File.Exists("settings.db"))
            {
                if (MessageBox.Show("Error.\nauto creating the settings.db.\nPlease update the TvShowsRoot in the settings.db file", "", MessageBoxButtons.OK) == DialogResult.OK)
                    Settings.Instance.Save();
                Application.Exit();
                return;
            }


            Application.Run(new Form1());

        }
    }
}
