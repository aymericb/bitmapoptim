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
        private AppConfig AppConfig { get; set; }
        private string Path { get; set; }

        #region Initialization and Destruction

        public MainWindow(AppConfig app_config)
        {
            InitializeComponent();

            this.AppConfig = app_config;
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            AppConfig.RestoreWindowBounds(this, this.AppConfig.MainWindowBounds);
        }
        
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.AppConfig.MainWindowBounds = this.DesktopBounds;
        }

        #endregion

        #region GUI Folder browsing

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            // Legacy Win2k dialog selector sucks
            // ### TODO: Use new common control dialogs instead: 
            // http://stackoverflow.com/questions/600346/using-openfiledialog-for-directory-not-folderbrowserdialog

            FolderBrowserDialog folder_dialog = new FolderBrowserDialog();
            if (folder_dialog.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = folder_dialog.SelectedPath;
                SetPath(txtPath.Text);
            }
        }

        // Used to capture "return" key in TextEdit
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Return && this.ActiveControl == txtPath)
            {
                SetPath(txtPath.Text);
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void SetPath(string path)
        {
            // Clear errors
            errProvider.SetError(txtPath, "");

            // Check path exists
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                errProvider.SetError(txtPath, "Path does not exist");
                return;
            }

            // Check if the path is accessible for the user
            if (Directory.Exists(path) && !hasWriteAccessToFolder(path))
            {
                errProvider.SetError(txtPath, "Path is not accessible by user");
                return;
            }

            // Scan the path
            ScanPaths(new string[] { path });
        }

        
        static private bool hasWriteAccessToFolder(string folderPath)
        {
            try
            {
                // Attempt to get a list of security permissions from the folder. 
                // This will raise an exception if the path is read only or do not have access to view the permissions. 
                // From http://stackoverflow.com/questions/1410127/c-sharp-test-if-user-has-write-access-to-a-folder
                System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(folderPath);

                // However this does not work for "fake" folders that riddle Vista and 7
                // ### TODO: Find a better way
                Directory.EnumerateFiles(folderPath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void MainWindow_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length < 1)
            {
                return;
            }
            else if (files.Length == 1)
            {
                txtPath.Text = files[0];
                SetPath(txtPath.Text);
                return;
            }
            else
            {
                txtPath.Text = "";
                ScanPaths(files);
            }
        }

        #endregion

        #region Background Worker control

        private void ScanPaths(string[] paths)
        {

        }

        #endregion


    }
}
