
using System.Windows.Forms;

namespace Magic_RDR
{
    partial class WorkForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkForm));
            this.loadingProgressBar = new System.Windows.Forms.ProgressBar();
            this.workingTextLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // loadingProgressBar
            // 
            this.loadingProgressBar.Location = new System.Drawing.Point(12, 42);
            this.loadingProgressBar.MarqueeAnimationSpeed = 20;
            this.loadingProgressBar.Name = "loadingProgressBar";
            this.loadingProgressBar.Size = new System.Drawing.Size(139, 23);
            this.loadingProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.loadingProgressBar.TabIndex = 3;
            // 
            // workingTextLabel
            // 
            this.workingTextLabel.AutoSize = true;
            this.workingTextLabel.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.workingTextLabel.Location = new System.Drawing.Point(33, 11);
            this.workingTextLabel.Name = "workingTextLabel";
            this.workingTextLabel.Size = new System.Drawing.Size(97, 25);
            this.workingTextLabel.TabIndex = 2;
            this.workingTextLabel.Text = "Loading...";
            // 
            // WorkForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(162, 77);
            this.Controls.Add(this.loadingProgressBar);
            this.Controls.Add(this.workingTextLabel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "WorkForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.WorkForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ProgressBar loadingProgressBar;
        private Label workingTextLabel;
    }
}