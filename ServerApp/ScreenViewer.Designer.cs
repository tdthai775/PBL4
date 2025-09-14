namespace ServerApp
{
    partial class ScreenView
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.PictureBox picScreen;
        private System.Windows.Forms.Button btnStopView;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.picScreen = new System.Windows.Forms.PictureBox();
            this.btnStopView = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picScreen)).BeginInit();
            this.SuspendLayout();
            // 
            // picScreen
            // 
            this.picScreen.Location = new System.Drawing.Point(16, 14);
            this.picScreen.Margin = new System.Windows.Forms.Padding(4);
            this.picScreen.Name = "picScreen";
            this.picScreen.Size = new System.Drawing.Size(1366, 768);
            this.picScreen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picScreen.TabIndex = 0;
            this.picScreen.TabStop = false;
            // 
            // btnStopView
            // 
            this.btnStopView.Location = new System.Drawing.Point(16, 800);
            this.btnStopView.Margin = new System.Windows.Forms.Padding(4);
            this.btnStopView.Name = "btnStopView";
            this.btnStopView.Size = new System.Drawing.Size(133, 37);
            this.btnStopView.TabIndex = 1;
            this.btnStopView.Text = "Stop Viewing";
            this.btnStopView.UseVisualStyleBackColor = true;
            this.btnStopView.Click += new System.EventHandler(this.btnStopView_Click);
            // 
            // ScreenView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1403, 850);
            this.Controls.Add(this.btnStopView);
            this.Controls.Add(this.picScreen);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ScreenView";
            this.Text = "Screen Viewer";
            ((System.ComponentModel.ISupportInitialize)(this.picScreen)).EndInit();
            this.ResumeLayout(false);

        }
    }
}