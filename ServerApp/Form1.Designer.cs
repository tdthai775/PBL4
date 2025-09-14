namespace ServerApp
{
    partial class Form1
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.gbSystemInformation = new System.Windows.Forms.GroupBox();
            this.txtDisk = new System.Windows.Forms.Label();
            this.txtRam = new System.Windows.Forms.Label();
            this.txtCpu = new System.Windows.Forms.Label();
            this.gbBulkActions = new System.Windows.Forms.GroupBox();
            this.btnSystemInfomation = new System.Windows.Forms.Button();
            this.btnStream = new System.Windows.Forms.Button();
            this.btnRestartSelected = new System.Windows.Forms.Button();
            this.btnShutdownSelected = new System.Windows.Forms.Button();
            this.gbServerControl = new System.Windows.Forms.GroupBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnStopServer = new System.Windows.Forms.Button();
            this.btnStartServer = new System.Windows.Forms.Button();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.gbClientList = new System.Windows.Forms.GroupBox();
            this.dgvListClients = new System.Windows.Forms.DataGridView();
            this.pnlLeft.SuspendLayout();
            this.gbSystemInformation.SuspendLayout();
            this.gbBulkActions.SuspendLayout();
            this.gbServerControl.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.gbClientList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvListClients)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.White;
            this.pnlLeft.Controls.Add(this.gbSystemInformation);
            this.pnlLeft.Controls.Add(this.gbBulkActions);
            this.pnlLeft.Controls.Add(this.gbServerControl);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(15);
            this.pnlLeft.Size = new System.Drawing.Size(425, 596);
            this.pnlLeft.TabIndex = 0;
            // 
            // gbSystemInformation
            // 
            this.gbSystemInformation.Controls.Add(this.txtDisk);
            this.gbSystemInformation.Controls.Add(this.txtRam);
            this.gbSystemInformation.Controls.Add(this.txtCpu);
            this.gbSystemInformation.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbSystemInformation.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.gbSystemInformation.Location = new System.Drawing.Point(18, 419);
            this.gbSystemInformation.Name = "gbSystemInformation";
            this.gbSystemInformation.Size = new System.Drawing.Size(389, 165);
            this.gbSystemInformation.TabIndex = 4;
            this.gbSystemInformation.TabStop = false;
            this.gbSystemInformation.Text = "📊 System Information";
            // 
            // txtDisk
            // 
            this.txtDisk.AutoSize = true;
            this.txtDisk.Location = new System.Drawing.Point(19, 125);
            this.txtDisk.Name = "txtDisk";
            this.txtDisk.Size = new System.Drawing.Size(56, 23);
            this.txtDisk.TabIndex = 6;
            this.txtDisk.Text = "DISK :";
            // 
            // txtRam
            // 
            this.txtRam.AutoSize = true;
            this.txtRam.Location = new System.Drawing.Point(18, 40);
            this.txtRam.Name = "txtRam";
            this.txtRam.Size = new System.Drawing.Size(49, 23);
            this.txtRam.TabIndex = 5;
            this.txtRam.Text = "RAM:";
            // 
            // txtCpu
            // 
            this.txtCpu.AutoSize = true;
            this.txtCpu.Location = new System.Drawing.Point(18, 84);
            this.txtCpu.Name = "txtCpu";
            this.txtCpu.Size = new System.Drawing.Size(52, 23);
            this.txtCpu.TabIndex = 4;
            this.txtCpu.Text = "CPU :";
            // 
            // gbBulkActions
            // 
            this.gbBulkActions.Controls.Add(this.btnSystemInfomation);
            this.gbBulkActions.Controls.Add(this.btnStream);
            this.gbBulkActions.Controls.Add(this.btnRestartSelected);
            this.gbBulkActions.Controls.Add(this.btnShutdownSelected);
            this.gbBulkActions.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.gbBulkActions.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.gbBulkActions.Location = new System.Drawing.Point(33, 196);
            this.gbBulkActions.Name = "gbBulkActions";
            this.gbBulkActions.Size = new System.Drawing.Size(339, 214);
            this.gbBulkActions.TabIndex = 1;
            this.gbBulkActions.TabStop = false;
            this.gbBulkActions.Text = "🎮 Điều khiển ";
            // 
            // btnSystemInfomation
            // 
            this.btnSystemInfomation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(140)))), ((int)(((byte)(22)))));
            this.btnSystemInfomation.FlatAppearance.BorderSize = 0;
            this.btnSystemInfomation.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSystemInfomation.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSystemInfomation.ForeColor = System.Drawing.Color.White;
            this.btnSystemInfomation.Location = new System.Drawing.Point(48, 161);
            this.btnSystemInfomation.Name = "btnSystemInfomation";
            this.btnSystemInfomation.Size = new System.Drawing.Size(249, 36);
            this.btnSystemInfomation.TabIndex = 4;
            this.btnSystemInfomation.Text = "System Infomation";
            this.btnSystemInfomation.UseVisualStyleBackColor = false;
            this.btnSystemInfomation.Click += new System.EventHandler(this.btnSystemInfomation_Click);
            // 
            // btnStream
            // 
            this.btnStream.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(140)))), ((int)(((byte)(22)))));
            this.btnStream.FlatAppearance.BorderSize = 0;
            this.btnStream.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStream.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStream.ForeColor = System.Drawing.Color.White;
            this.btnStream.Location = new System.Drawing.Point(48, 119);
            this.btnStream.Name = "btnStream";
            this.btnStream.Size = new System.Drawing.Size(249, 36);
            this.btnStream.TabIndex = 3;
            this.btnStream.Text = "⏹️ Stream";
            this.btnStream.UseVisualStyleBackColor = false;
            this.btnStream.Click += new System.EventHandler(this.btnStream_Click);
            // 
            // btnRestartSelected
            // 
            this.btnRestartSelected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(144)))), ((int)(((byte)(255)))));
            this.btnRestartSelected.FlatAppearance.BorderSize = 0;
            this.btnRestartSelected.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestartSelected.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRestartSelected.ForeColor = System.Drawing.Color.White;
            this.btnRestartSelected.Location = new System.Drawing.Point(48, 74);
            this.btnRestartSelected.Name = "btnRestartSelected";
            this.btnRestartSelected.Size = new System.Drawing.Size(249, 39);
            this.btnRestartSelected.TabIndex = 1;
            this.btnRestartSelected.Text = "🔄 Khởi động lại (Chọn)";
            this.btnRestartSelected.UseVisualStyleBackColor = false;
            this.btnRestartSelected.Click += new System.EventHandler(this.btnRestartSelected_Click);
            // 
            // btnShutdownSelected
            // 
            this.btnShutdownSelected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(140)))), ((int)(((byte)(22)))));
            this.btnShutdownSelected.FlatAppearance.BorderSize = 0;
            this.btnShutdownSelected.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShutdownSelected.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnShutdownSelected.ForeColor = System.Drawing.Color.White;
            this.btnShutdownSelected.Location = new System.Drawing.Point(48, 33);
            this.btnShutdownSelected.Name = "btnShutdownSelected";
            this.btnShutdownSelected.Size = new System.Drawing.Size(249, 35);
            this.btnShutdownSelected.TabIndex = 0;
            this.btnShutdownSelected.Text = "🔌 Tắt máy (Chọn)";
            this.btnShutdownSelected.UseVisualStyleBackColor = false;
            this.btnShutdownSelected.Click += new System.EventHandler(this.btnShutdownSelected_Click);
            // 
            // gbServerControl
            // 
            this.gbServerControl.Controls.Add(this.btnRefresh);
            this.gbServerControl.Controls.Add(this.btnStopServer);
            this.gbServerControl.Controls.Add(this.btnStartServer);
            this.gbServerControl.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.gbServerControl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.gbServerControl.Location = new System.Drawing.Point(33, 15);
            this.gbServerControl.Name = "gbServerControl";
            this.gbServerControl.Size = new System.Drawing.Size(339, 167);
            this.gbServerControl.TabIndex = 0;
            this.gbServerControl.TabStop = false;
            this.gbServerControl.Text = "⚙️ Điều khiển Server";
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(144)))), ((int)(((byte)(255)))));
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Location = new System.Drawing.Point(48, 117);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(249, 35);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "🔄 Làm mới danh sách";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnStopServer
            // 
            this.btnStopServer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(34)))), ((int)(((byte)(45)))));
            this.btnStopServer.Enabled = false;
            this.btnStopServer.FlatAppearance.BorderSize = 0;
            this.btnStopServer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopServer.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStopServer.ForeColor = System.Drawing.Color.White;
            this.btnStopServer.Location = new System.Drawing.Point(48, 76);
            this.btnStopServer.Name = "btnStopServer";
            this.btnStopServer.Size = new System.Drawing.Size(249, 35);
            this.btnStopServer.TabIndex = 3;
            this.btnStopServer.Text = "🛑 Dừng Server";
            this.btnStopServer.UseVisualStyleBackColor = false;
            this.btnStopServer.Click += new System.EventHandler(this.btnStopServer_Click);
            // 
            // btnStartServer
            // 
            this.btnStartServer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(82)))), ((int)(((byte)(196)))), ((int)(((byte)(26)))));
            this.btnStartServer.FlatAppearance.BorderSize = 0;
            this.btnStartServer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartServer.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStartServer.ForeColor = System.Drawing.Color.White;
            this.btnStartServer.Location = new System.Drawing.Point(48, 35);
            this.btnStartServer.Name = "btnStartServer";
            this.btnStartServer.Size = new System.Drawing.Size(249, 35);
            this.btnStartServer.TabIndex = 2;
            this.btnStartServer.Text = "🚀 Khởi động Server";
            this.btnStartServer.UseVisualStyleBackColor = false;
            this.btnStartServer.Click += new System.EventHandler(this.btnStartServer_Click);
            // 
            // pnlRight
            // 
            this.pnlRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.pnlRight.Controls.Add(this.gbClientList);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(425, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Padding = new System.Windows.Forms.Padding(15);
            this.pnlRight.Size = new System.Drawing.Size(357, 596);
            this.pnlRight.TabIndex = 1;
            // 
            // gbClientList
            // 
            this.gbClientList.Controls.Add(this.dgvListClients);
            this.gbClientList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbClientList.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.gbClientList.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.gbClientList.Location = new System.Drawing.Point(15, 15);
            this.gbClientList.Name = "gbClientList";
            this.gbClientList.Padding = new System.Windows.Forms.Padding(10);
            this.gbClientList.Size = new System.Drawing.Size(327, 566);
            this.gbClientList.TabIndex = 0;
            this.gbClientList.TabStop = false;
            this.gbClientList.Text = "👥 Quản lý Clients";
            // 
            // dgvListClients
            // 
            this.dgvListClients.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            this.dgvListClients.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvListClients.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvListClients.GridColor = System.Drawing.SystemColors.ButtonHighlight;
            this.dgvListClients.Location = new System.Drawing.Point(13, 40);
            this.dgvListClients.Name = "dgvListClients";
            this.dgvListClients.RowHeadersWidth = 51;
            this.dgvListClients.RowTemplate.Height = 24;
            this.dgvListClients.Size = new System.Drawing.Size(298, 509);
            this.dgvListClients.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ClientSize = new System.Drawing.Size(782, 596);
            this.Controls.Add(this.pnlRight);
            this.Controls.Add(this.pnlLeft);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PBL4 Server Management";
            this.pnlLeft.ResumeLayout(false);
            this.gbSystemInformation.ResumeLayout(false);
            this.gbSystemInformation.PerformLayout();
            this.gbBulkActions.ResumeLayout(false);
            this.gbServerControl.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            this.gbClientList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvListClients)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.GroupBox gbServerControl;
        private System.Windows.Forms.Button btnStartServer;
        private System.Windows.Forms.Button btnStopServer;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.GroupBox gbBulkActions;
        private System.Windows.Forms.Button btnShutdownSelected;
        private System.Windows.Forms.Button btnRestartSelected;
        private System.Windows.Forms.Button btnStream;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.GroupBox gbClientList;
        private System.Windows.Forms.GroupBox gbSystemInformation;
        private System.Windows.Forms.DataGridView dgvListClients;
        private System.Windows.Forms.Label txtCpu;
        private System.Windows.Forms.Label txtDisk;
        private System.Windows.Forms.Label txtRam;
        private System.Windows.Forms.Button btnSystemInfomation;
    }
}