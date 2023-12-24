
using System.Windows.Forms;

namespace Magic_RDR
{
    partial class PropertiesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertiesForm));
            this.propertiesOflabel = new System.Windows.Forms.Label();
            this.propertiesOfNameLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.fileInfoPanel = new System.Windows.Forms.Panel();
            this.fileInfoSize = new System.Windows.Forms.Label();
            this.fileResourceStart = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.fileResourceType = new System.Windows.Forms.NumericUpDown();
            this.FileResourceFlag2 = new System.Windows.Forms.TextBox();
            this.FileResourceFlag1 = new System.Windows.Forms.TextBox();
            this.resourceFlag2Label = new System.Windows.Forms.Label();
            this.resourceFlag1Label = new System.Windows.Forms.Label();
            this.fileInfoSizeNameLabel = new System.Windows.Forms.Label();
            this.isFileResource = new System.Windows.Forms.CheckBox();
            this.applyChangesButton = new System.Windows.Forms.Button();
            this.fileCompressed = new System.Windows.Forms.CheckBox();
            this.fileInfoPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fileResourceType)).BeginInit();
            this.SuspendLayout();
            // 
            // propertiesOflabel
            // 
            this.propertiesOflabel.AutoSize = true;
            this.propertiesOflabel.Font = new System.Drawing.Font("Segoe UI", 18F);
            this.propertiesOflabel.Location = new System.Drawing.Point(12, 8);
            this.propertiesOflabel.Name = "propertiesOflabel";
            this.propertiesOflabel.Size = new System.Drawing.Size(246, 32);
            this.propertiesOflabel.TabIndex = 0;
            this.propertiesOflabel.Text = "Properties Of (file/dir)";
            // 
            // propertiesOfNameLabel
            // 
            this.propertiesOfNameLabel.AutoSize = true;
            this.propertiesOfNameLabel.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Italic);
            this.propertiesOfNameLabel.Location = new System.Drawing.Point(30, 35);
            this.propertiesOfNameLabel.Name = "propertiesOfNameLabel";
            this.propertiesOfNameLabel.Size = new System.Drawing.Size(53, 30);
            this.propertiesOfNameLabel.TabIndex = 1;
            this.propertiesOfNameLabel.Text = "N/A";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 18F);
            this.label1.Location = new System.Drawing.Point(12, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 32);
            this.label1.TabIndex = 2;
            this.label1.Text = "Info";
            // 
            // fileInfoPanel
            // 
            this.fileInfoPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.fileInfoPanel.Controls.Add(this.fileInfoSize);
            this.fileInfoPanel.Controls.Add(this.fileResourceStart);
            this.fileInfoPanel.Controls.Add(this.label3);
            this.fileInfoPanel.Controls.Add(this.label2);
            this.fileInfoPanel.Controls.Add(this.fileResourceType);
            this.fileInfoPanel.Controls.Add(this.FileResourceFlag2);
            this.fileInfoPanel.Controls.Add(this.FileResourceFlag1);
            this.fileInfoPanel.Controls.Add(this.resourceFlag2Label);
            this.fileInfoPanel.Controls.Add(this.resourceFlag1Label);
            this.fileInfoPanel.Controls.Add(this.fileInfoSizeNameLabel);
            this.fileInfoPanel.Location = new System.Drawing.Point(15, 105);
            this.fileInfoPanel.Name = "fileInfoPanel";
            this.fileInfoPanel.Size = new System.Drawing.Size(357, 173);
            this.fileInfoPanel.TabIndex = 3;
            this.fileInfoPanel.Visible = false;
            // 
            // fileInfoSize
            // 
            this.fileInfoSize.AutoSize = true;
            this.fileInfoSize.Font = new System.Drawing.Font("Arial", 14F);
            this.fileInfoSize.Location = new System.Drawing.Point(156, 12);
            this.fileInfoSize.Name = "fileInfoSize";
            this.fileInfoSize.Size = new System.Drawing.Size(52, 22);
            this.fileInfoSize.TabIndex = 22;
            this.fileInfoSize.Text = "Size:";
            // 
            // fileResourceStart
            // 
            this.fileResourceStart.Location = new System.Drawing.Point(160, 128);
            this.fileResourceStart.MaxLength = 8;
            this.fileResourceStart.Name = "fileResourceStart";
            this.fileResourceStart.ReadOnly = true;
            this.fileResourceStart.Size = new System.Drawing.Size(186, 26);
            this.fileResourceStart.TabIndex = 21;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 13F);
            this.label3.Location = new System.Drawing.Point(12, 131);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(135, 21);
            this.label3.TabIndex = 20;
            this.label3.Text = "Resource Start:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 13F);
            this.label2.Location = new System.Drawing.Point(12, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(137, 21);
            this.label2.TabIndex = 19;
            this.label2.Text = "Resource Type:";
            // 
            // fileResourceType
            // 
            this.fileResourceType.AutoSize = true;
            this.fileResourceType.Location = new System.Drawing.Point(160, 40);
            this.fileResourceType.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.fileResourceType.Name = "fileResourceType";
            this.fileResourceType.Size = new System.Drawing.Size(186, 26);
            this.fileResourceType.TabIndex = 18;
            // 
            // FileResourceFlag2
            // 
            this.FileResourceFlag2.Location = new System.Drawing.Point(160, 98);
            this.FileResourceFlag2.MaxLength = 8;
            this.FileResourceFlag2.Name = "FileResourceFlag2";
            this.FileResourceFlag2.Size = new System.Drawing.Size(186, 26);
            this.FileResourceFlag2.TabIndex = 17;
            this.FileResourceFlag2.TextChanged += new System.EventHandler(this.FlagBoxes_TextChanged);
            // 
            // FileResourceFlag1
            // 
            this.FileResourceFlag1.Location = new System.Drawing.Point(160, 69);
            this.FileResourceFlag1.MaxLength = 8;
            this.FileResourceFlag1.Name = "FileResourceFlag1";
            this.FileResourceFlag1.Size = new System.Drawing.Size(186, 26);
            this.FileResourceFlag1.TabIndex = 16;
            this.FileResourceFlag1.TextChanged += new System.EventHandler(this.FlagBoxes_TextChanged);
            // 
            // resourceFlag2Label
            // 
            this.resourceFlag2Label.AutoSize = true;
            this.resourceFlag2Label.Font = new System.Drawing.Font("Arial", 13F);
            this.resourceFlag2Label.Location = new System.Drawing.Point(12, 101);
            this.resourceFlag2Label.Name = "resourceFlag2Label";
            this.resourceFlag2Label.Size = new System.Drawing.Size(142, 21);
            this.resourceFlag2Label.TabIndex = 15;
            this.resourceFlag2Label.Text = "Resource Flag2:";
            // 
            // resourceFlag1Label
            // 
            this.resourceFlag1Label.AutoSize = true;
            this.resourceFlag1Label.Font = new System.Drawing.Font("Arial", 13F);
            this.resourceFlag1Label.Location = new System.Drawing.Point(12, 72);
            this.resourceFlag1Label.Name = "resourceFlag1Label";
            this.resourceFlag1Label.Size = new System.Drawing.Size(142, 21);
            this.resourceFlag1Label.TabIndex = 14;
            this.resourceFlag1Label.Text = "Resource Flag1:";
            // 
            // fileInfoSizeNameLabel
            // 
            this.fileInfoSizeNameLabel.AutoSize = true;
            this.fileInfoSizeNameLabel.Font = new System.Drawing.Font("Arial", 14F);
            this.fileInfoSizeNameLabel.Location = new System.Drawing.Point(12, 12);
            this.fileInfoSizeNameLabel.Name = "fileInfoSizeNameLabel";
            this.fileInfoSizeNameLabel.Size = new System.Drawing.Size(52, 22);
            this.fileInfoSizeNameLabel.TabIndex = 0;
            this.fileInfoSizeNameLabel.Text = "Size:";
            // 
            // isFileResource
            // 
            this.isFileResource.AutoSize = true;
            this.isFileResource.Location = new System.Drawing.Point(154, 80);
            this.isFileResource.Name = "isFileResource";
            this.isFileResource.Size = new System.Drawing.Size(94, 22);
            this.isFileResource.TabIndex = 18;
            this.isFileResource.Text = "Resource";
            this.isFileResource.UseVisualStyleBackColor = true;
            this.isFileResource.Visible = false;
            // 
            // applyChangesButton
            // 
            this.applyChangesButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.applyChangesButton.Location = new System.Drawing.Point(15, 284);
            this.applyChangesButton.Name = "applyChangesButton";
            this.applyChangesButton.Size = new System.Drawing.Size(357, 30);
            this.applyChangesButton.TabIndex = 19;
            this.applyChangesButton.Text = "Apply";
            this.applyChangesButton.UseVisualStyleBackColor = true;
            this.applyChangesButton.Click += new System.EventHandler(this.applyChangesButton_Click);
            // 
            // fileCompressed
            // 
            this.fileCompressed.AutoSize = true;
            this.fileCompressed.Location = new System.Drawing.Point(254, 80);
            this.fileCompressed.Name = "fileCompressed";
            this.fileCompressed.Size = new System.Drawing.Size(118, 22);
            this.fileCompressed.TabIndex = 20;
            this.fileCompressed.Text = "Compressed";
            this.fileCompressed.UseVisualStyleBackColor = true;
            this.fileCompressed.Visible = false;
            this.fileCompressed.CheckedChanged += new System.EventHandler(this.fileCompressed_CheckedChanged);
            // 
            // PropertiesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(388, 326);
            this.Controls.Add(this.fileCompressed);
            this.Controls.Add(this.applyChangesButton);
            this.Controls.Add(this.isFileResource);
            this.Controls.Add(this.fileInfoPanel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.propertiesOfNameLabel);
            this.Controls.Add(this.propertiesOflabel);
            this.Font = new System.Drawing.Font("Arial", 12F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PropertiesForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MagicRDR | Properties";
            this.fileInfoPanel.ResumeLayout(false);
            this.fileInfoPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fileResourceType)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label propertiesOflabel;
        private Label propertiesOfNameLabel;
        private Label label1;
        private Panel fileInfoPanel;
        private Label fileInfoSizeNameLabel;
        private TextBox FileResourceFlag2;
        private TextBox FileResourceFlag1;
        private Label resourceFlag2Label;
        private Label resourceFlag1Label;
        private CheckBox isFileResource;
        private Button applyChangesButton;
        private Label label2;
        private NumericUpDown fileResourceType;
        private CheckBox fileCompressed;
        private TextBox fileResourceStart;
        private Label label3;
        private Label fileInfoSize;
    }
}