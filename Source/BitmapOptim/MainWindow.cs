/*
 * BitmapOptim - The Image Optimizer for Windows
 * 
 * Copyright (C) 2012 Aymeric Barthe
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

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
        private List<ImageFile> Files { get; set; }    

        private List<ImageFile> PendingFiles { get; set; } // Files that need to be added to ListView
        private Timer RefreshTimer { get; set; }
        private State m_status;
        

        #region Initialization and Destruction

        public MainWindow(AppConfig app_config)
        {
            InitializeComponent();            

            // Initialize application data
            this.AppConfig = app_config;
            this.Files = new List<ImageFile>();

            // Initialize Background Worker
            this.Worker = new BackgroundWorker();
            this.Worker.WorkerSupportsCancellation = true;
            this.Worker.DoWork += OnScanPaths;
            this.Worker.RunWorkerCompleted += OnScanFinished;

            // Initialize Refresh Timer
            this.RefreshTimer = new Timer();
            this.RefreshTimer.Interval = 500;
            this.RefreshTimer.Tick += OnRefreshListView;

            // Initialize GUI
            //this.PendingFiles = new List<ImageFile>();
            this.listView.SetObjects(this.Files);
            this.colPath.AspectName = "Path";           // ### TODO threading, use mutex     
            //this.colPath.AspectGetter = GetPath;
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
            this.listView.SetObjects(this.Files);
            SetStatus(State.Scanning);
            this.RefreshTimer.Start();
            this.statusProgress.Style = ProgressBarStyle.Marquee;
            this.Worker.RunWorkerAsync(paths);            
        }


        private void ScanDirectory(BackgroundWorker worker, string path)
        {
            try
            {
                List<ImageFile> pending_files = new List<ImageFile>();
                foreach (string file_path in Directory.EnumerateFiles(path, "*.png", SearchOption.TopDirectoryOnly))
                {
                    pending_files.Add(new ImageFile(file_path));
                }
                if (pending_files.Count > 0)
                {
                    lock (this.Files)
                    {
                        this.Files.AddRange(pending_files);
                        if (this.PendingFiles == null)
                            this.PendingFiles = pending_files;
                        else
                            this.PendingFiles.AddRange(pending_files);
                    }
                }
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
        }

        private void OnScanFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            this.RefreshTimer.Start();
            SetStatus(State.Finished);
            lock (this.Files)
            {
                this.listView.SetObjects(this.Files);
            }
            if (e.Error != null)
            {
                statusText.Text = "Error: " + e.Error.Message;
                return;
            }
            statusText.Text = "Ready";            
        }

        #endregion

        #region ObjectListView

        private void OnRefreshListView(Object myObject, EventArgs myEventArgs)
        {
            if (this.Status == State.Scanning)
            {
                lock (this.Files)
                {
                    if (this.PendingFiles != null)
                    {
                        this.listView.AddObjects(this.PendingFiles);
                        this.PendingFiles = null;
                    }
                }
            }
        }

        #endregion

    }
}
