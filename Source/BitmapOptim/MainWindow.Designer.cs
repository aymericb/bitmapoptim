namespace BitmapOptim
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.statusStip = new System.Windows.Forms.StatusStrip();
            this.statusText = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.errProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.statusStip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStip
            // 
            this.statusStip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusText});
            this.statusStip.Location = new System.Drawing.Point(0, 347);
            this.statusStip.Name = "statusStip";
            this.statusStip.Size = new System.Drawing.Size(670, 22);
            this.statusStip.TabIndex = 0;
            this.statusStip.Text = "statusStrip1";
            // 
            // statusText
            // 
            this.statusText.Name = "statusText";
            this.statusText.Size = new System.Drawing.Size(0, 17);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(583, 10);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtPath
            // 
            this.txtPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPath.Location = new System.Drawing.Point(12, 12);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(554, 20);
            this.txtPath.TabIndex = 2;
            // 
            // errProvider
            // 
            this.errProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errProvider.ContainerControl = this;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(670, 369);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.statusStip);
            this.MinimumSize = new System.Drawing.Size(500, 300);
            this.Name = "MainWindow";
            this.Text = "BitmapOptim";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.statusStip.ResumeLayout(false);
            this.statusStip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStip;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.ToolStripStatusLabel statusText;
        private System.Windows.Forms.ErrorProvider errProvider;
    }
}

