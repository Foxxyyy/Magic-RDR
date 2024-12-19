
namespace Magic_RDR.Viewers
{
    partial class AudioViewerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AudioViewerForm));
            this.audioPlayer = new AxWMPLib.AxWindowsMediaPlayer();
            this.listView = new System.Windows.Forms.ListView();
            this.columnHeaderAudio = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.exportButton = new System.Windows.Forms.ToolStripButton();
            this.exportAllButton = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.audioPlayer)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // audioPlayer
            // 
            this.audioPlayer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.audioPlayer.Enabled = true;
            this.audioPlayer.Location = new System.Drawing.Point(0, 30);
            this.audioPlayer.Name = "audioPlayer";
            this.audioPlayer.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("audioPlayer.OcxState")));
            this.audioPlayer.Size = new System.Drawing.Size(647, 464);
            this.audioPlayer.TabIndex = 0;
            // 
            // listView
            // 
            this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView.BackColor = System.Drawing.SystemColors.Control;
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderAudio});
            this.listView.FullRowSelect = true;
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(653, 30);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(231, 464);
            this.listView.TabIndex = 1;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // columnHeaderAudio
            // 
            this.columnHeaderAudio.Text = "Audio Files";
            this.columnHeaderAudio.Width = 235;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportButton,
            this.exportAllButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(884, 26);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // exportButton
            // 
            this.exportButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.exportButton.Image = ((System.Drawing.Image)(resources.GetObject("exportButton.Image")));
            this.exportButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.exportButton.Margin = new System.Windows.Forms.Padding(5, 5, 0, 2);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(45, 19);
            this.exportButton.Text = "Export";
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // exportAllButton
            // 
            this.exportAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.exportAllButton.Image = ((System.Drawing.Image)(resources.GetObject("exportAllButton.Image")));
            this.exportAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.exportAllButton.Margin = new System.Windows.Forms.Padding(5, 5, 0, 2);
            this.exportAllButton.Name = "exportAllButton";
            this.exportAllButton.Size = new System.Drawing.Size(62, 19);
            this.exportAllButton.Text = "Export All";
            this.exportAllButton.Click += new System.EventHandler(this.exportAllButton_Click);
            // 
            // AudioViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(884, 494);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.listView);
            this.Controls.Add(this.audioPlayer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AudioViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MagicRDR - Audio Player";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AudioViewerForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.audioPlayer)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AxWMPLib.AxWindowsMediaPlayer audioPlayer;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader columnHeaderAudio;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton exportButton;
        private System.Windows.Forms.ToolStripButton exportAllButton;
    }
}
