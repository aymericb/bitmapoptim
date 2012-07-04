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
        enum State
        {
            Ready,
            Scanning,
            Finished
        }

        private AppConfig AppConfig { get; set; }
        private string Path { get; set; }
        private BackgroundWorker Worker { get; set; }
        private State Status
        {
            get { return m_status; }
        }
        private List<ImageFile> Files { get; set; }    // TODO: check atomicity of get/set

        private State m_status; 

        #region Initialization and Destruction

        public MainWindow(AppConfig app_config)
        {
            InitializeComponent();

            this.AppConfig = app_config;
            this.Files = new List<ImageFile>();

            this.Worker = new BackgroundWorker();
            this.Worker.WorkerSupportsCancellation = true;
            this.Worker.DoWork += OnScanPaths;
            this.Worker.RunWorkerCompleted += OnScanFinished;

            this.listView.SetObjects(this.Files);
            this.colPath.AspectName = "Path";           // ### TODO threading, use mutex     

            SetStatus(State.Ready);            
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
            if (this.Status != State.Scanning)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Copy;
            }
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

        #region Status

        private void SetStatus(State status)
        {
            this.m_status = status;
            switch (this.m_status)
            {
                case State.Ready:
                    this.btnRescan.Enabled = false;
                    this.btnCancel.Enabled = false;
                    this.txtPath.Enabled = true;
                    this.btnBrowse.Enabled = true;
                    this.statusText.Text = "Ready. Select folder or files to analyse...";
                    this.statusProgress.Visible = false;
                    break;
                case State.Scanning:
                    this.btnRescan.Enabled = false;
                    this.btnCancel.Enabled = true;
                    this.txtPath.Enabled = false;
                    this.btnBrowse.Enabled = false;
                    this.statusText.Text = "Enumerating files...";
                    this.statusProgress.Visible = true;
                    break;
                case State.Finished:
                    this.btnRescan.Enabled = true;
                    this.btnCancel.Enabled = false;
                    this.txtPath.Enabled = true;
                    this.btnBrowse.Enabled = true;
                    //this.statusText.Text = "Ready";
                    this.statusProgress.Visible = false;
                    break;               
            }
        }

        #endregion

        #region Toolbar

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Worker.CancelAsync();
        }

        #endregion

        #region Background Worker control

        private void ScanPaths(string[] paths)
        {
            lock (this.Files)
            {
                this.Files.Clear();
            }
            this.listView.BuildList();
            SetStatus(State.Scanning);
            this.statusProgress.Style = ProgressBarStyle.Marquee;
            this.Worker.RunWorkerAsync(paths);            
        }


        private void ScanDirectory(BackgroundWorker worker, string path)
        {
            try
            {
                lock (this.Files)
                {
                    foreach (string file_path in Directory.EnumerateFiles(path, "*.png", SearchOption.TopDirectoryOnly))
                    {
                        this.Files.Add(new ImageFile(file_path));
                    }
                }
                this.listView.BuildList();
                foreach (string dir in Directory.EnumerateDirectories(path))
                {
                    if (worker.CancellationPending)
                        return;
                    ScanDirectory(worker, dir);
                }
            }
            catch (Exception)
            {
                // We ignore exceptions, which are probably caused by access exceptions.
            }
        }

        private void OnScanPaths(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            string[] paths = (string[])e.Argument;
           
            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    ScanDirectory(worker, path);
                }
                else
                {
                    // ### TODO: Removed hardcoded extensions            
                    if (path.ToLower().EndsWith(".png"))
                    {
                        lock (this.Files)
                        {
                            this.Files.Add(new ImageFile(path));
                        }
                    }
                }
            }
            this.listView.BuildList();
        }

        private void OnScanFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            SetStatus(State.Finished);
            if (e.Error != null)
            {
                statusText.Text = "Error: " + e.Error.Message;
                return;
            }

            statusText.Text = "Ready";
            
        }

        #endregion


    }
}
