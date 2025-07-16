namespace YouTubeDownloader
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.Button btnGetInfo;
        private System.Windows.Forms.ComboBox cmbStreams;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnCancel; // <--- thêm mới
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            txtUrl = new TextBox();
            btnGetInfo = new Button();
            cmbStreams = new ComboBox();
            btnDownload = new Button();
            btnCancel = new Button();
            progressBar = new ProgressBar();
            lblStatus = new Label();
            SuspendLayout();
            // 
            // txtUrl
            // 
            txtUrl.Location = new Point(12, 12);
            txtUrl.Name = "txtUrl";
            txtUrl.Size = new Size(460, 27);
            txtUrl.TabIndex = 0;
            // 
            // btnGetInfo
            // 
            btnGetInfo.Location = new Point(478, 12);
            btnGetInfo.Name = "btnGetInfo";
            btnGetInfo.Size = new Size(100, 27);
            btnGetInfo.TabIndex = 1;
            btnGetInfo.Text = "Lấy thông tin";
            btnGetInfo.UseVisualStyleBackColor = true;
            btnGetInfo.Click += btnGetInfo_Click;
            // 
            // cmbStreams
            // 
            cmbStreams.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStreams.FormattingEnabled = true;
            cmbStreams.Location = new Point(12, 50);
            cmbStreams.Name = "cmbStreams";
            cmbStreams.Size = new Size(460, 28);
            cmbStreams.TabIndex = 2;
            // 
            // btnDownload
            // 
            btnDownload.Location = new Point(478, 50);
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new Size(100, 28);
            btnDownload.TabIndex = 3;
            btnDownload.Text = "Tải Video";
            btnDownload.UseVisualStyleBackColor = true;
            btnDownload.Click += btnDownload_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(478, 88);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 25);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "Dừng tải";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(12, 90);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(460, 23);
            progressBar.TabIndex = 4;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(12, 120);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(91, 20);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "Trạng thái: ...";
            // 
            // Form1
            // 
            BackColor = SystemColors.Control;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            ClientSize = new Size(590, 150);
            Controls.Add(btnCancel);
            Controls.Add(lblStatus);
            Controls.Add(progressBar);
            Controls.Add(btnDownload);
            Controls.Add(cmbStreams);
            Controls.Add(btnGetInfo);
            Controls.Add(txtUrl);
            ForeColor = SystemColors.ControlText;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "Form1";
            Text = "YouTube Video Downloader";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
