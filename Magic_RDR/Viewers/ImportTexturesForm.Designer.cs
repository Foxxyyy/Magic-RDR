namespace Magic_RDR.Viewers
{
    partial class ImportTexturesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportTexturesForm));
            this.listView = new System.Windows.Forms.ListView();
            this.columnTexture = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnDimensions = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnFormat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.validateButton = new System.Windows.Forms.ToolStripButton();
            this.addTextureButton = new System.Windows.Forms.ToolStripButton();
            this.addDirectoryButton = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.addedTextureLabel = new System.Windows.Forms.Label();
            this.correctTextureLabel = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.infoLabel = new System.Windows.Forms.Label();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView
            // 
            this.listView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnTexture,
            this.columnDimensions,
            this.columnFormat});
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(0, 0);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(516, 695);
            this.listView.TabIndex = 0;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            // 
            // columnTexture
            // 
            this.columnTexture.Text = "Texture";
            this.columnTexture.Width = 308;
            // 
            // columnDimensions
            // 
            this.columnDimensions.Text = "Dimensions";
            this.columnDimensions.Width = 109;
            // 
            // columnFormat
            // 
            this.columnFormat.Text = "Pixel Format";
            this.columnFormat.Width = 91;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.validateButton,
            this.addTextureButton,
            this.addDirectoryButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1023, 26);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // validateButton
            // 
            this.validateButton.Image = global::Magic_RDR.Properties.Resources.accept;
            this.validateButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.validateButton.Margin = new System.Windows.Forms.Padding(4, 4, 0, 2);
            this.validateButton.Name = "validateButton";
            this.validateButton.Size = new System.Drawing.Size(67, 20);
            this.validateButton.Text = "Rebuild";
            this.validateButton.Click += new System.EventHandler(this.validateButton_Click);
            // 
            // addTextureButton
            // 
            this.addTextureButton.Image = global::Magic_RDR.Properties.Resources.page_add;
            this.addTextureButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addTextureButton.Margin = new System.Windows.Forms.Padding(4, 4, 0, 2);
            this.addTextureButton.Name = "addTextureButton";
            this.addTextureButton.Size = new System.Drawing.Size(90, 20);
            this.addTextureButton.Text = "Add Texture";
            this.addTextureButton.Click += new System.EventHandler(this.addTextureButton_Click);
            // 
            // addDirectoryButton
            // 
            this.addDirectoryButton.Image = global::Magic_RDR.Properties.Resources.folder_add;
            this.addDirectoryButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addDirectoryButton.Margin = new System.Windows.Forms.Padding(4, 4, 0, 2);
            this.addDirectoryButton.Name = "addDirectoryButton";
            this.addDirectoryButton.Size = new System.Drawing.Size(100, 20);
            this.addDirectoryButton.Text = "Add Directory";
            this.addDirectoryButton.Click += new System.EventHandler(this.addDirectoryButton_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 26);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.addedTextureLabel);
            this.splitContainer1.Panel2.Controls.Add(this.correctTextureLabel);
            this.splitContainer1.Panel2.Controls.Add(this.richTextBox1);
            this.splitContainer1.Size = new System.Drawing.Size(1023, 695);
            this.splitContainer1.SplitterDistance = 516;
            this.splitContainer1.TabIndex = 1;
            // 
            // addedTextureLabel
            // 
            this.addedTextureLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.addedTextureLabel.AutoSize = true;
            this.addedTextureLabel.BackColor = System.Drawing.SystemColors.Window;
            this.addedTextureLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.addedTextureLabel.Location = new System.Drawing.Point(383, 671);
            this.addedTextureLabel.Name = "addedTextureLabel";
            this.addedTextureLabel.Size = new System.Drawing.Size(108, 15);
            this.addedTextureLabel.TabIndex = 4;
            this.addedTextureLabel.Text = "Added Textures : 0";
            // 
            // correctTextureLabel
            // 
            this.correctTextureLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.correctTextureLabel.AutoSize = true;
            this.correctTextureLabel.BackColor = System.Drawing.SystemColors.Window;
            this.correctTextureLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.correctTextureLabel.Location = new System.Drawing.Point(260, 671);
            this.correctTextureLabel.Name = "correctTextureLabel";
            this.correctTextureLabel.Size = new System.Drawing.Size(112, 15);
            this.correctTextureLabel.TabIndex = 3;
            this.correctTextureLabel.Text = "Correct Textures : 0";
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(503, 695);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            // 
            // infoLabel
            // 
            this.infoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.infoLabel.AutoSize = true;
            this.infoLabel.BackColor = System.Drawing.SystemColors.Control;
            this.infoLabel.Location = new System.Drawing.Point(624, 9);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(397, 13);
            this.infoLabel.TabIndex = 2;
            this.infoLabel.Text = "Imported textures are added here, only those with the same name can be replaced.";
            // 
            // ImportTexturesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1023, 721);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ImportTexturesForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MagicRDR - Import Textures Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportTexturesForm_FormClosing);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader columnTexture;
        private System.Windows.Forms.ColumnHeader columnDimensions;
        private System.Windows.Forms.ColumnHeader columnFormat;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton addTextureButton;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label correctTextureLabel;
        private System.Windows.Forms.Label addedTextureLabel;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ToolStripButton validateButton;
        private System.Windows.Forms.ToolStripButton addDirectoryButton;
        private System.Windows.Forms.Label infoLabel;
    }
}