using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace BitmapOptim
{
    public partial class MainWindow : Form
    {
        public MainWindow(AppConfig app_config)
        {
            InitializeComponent();

            m_app_config = app_config;
        }


        private AppConfig m_app_config;

        private void MainWindow_Load(object sender, EventArgs e)
        {
            AppConfig.RestoreWindowBounds(this, m_app_config.MainWindowBounds);
        }
        
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_app_config.MainWindowBounds = this.DesktopBounds;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            // Legacy Win2k dialog selector sucks
            // ### TODO: Use new common control dialogs instead: 
            // http://stackoverflow.com/questions/600346/using-openfiledialog-for-directory-not-folderbrowserdialog

            FolderBrowserDialog folder_dialog = new FolderBrowserDialog();
            if (folder_dialog.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = folder_dialog.SelectedPath;
            }
        }
    }
}
