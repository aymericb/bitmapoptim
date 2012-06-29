using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BitmapOptim
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Initialize config
            AppConfig config = AppConfig.Load<AppConfig>("BitmapOptim");

            // Initialize GUI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Start GUI application
            MainWindow win = new MainWindow(config);
            Application.Run(win);

            // Save config
            AppConfig.Save<AppConfig>("BitmapOptim", config);
        }
    }
}
