
namespace Magic_RDR
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.backgroundTextureComboBox = new System.Windows.Forms.ComboBox();
            this.imageSizeModeLabel = new System.Windows.Forms.Label();
            this.sizeModeComboBox = new System.Windows.Forms.ComboBox();
            this.checkBoxUseLastRPF = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxUseCustomColor = new System.Windows.Forms.CheckBox();
            this.saveLabel = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.groupBoxListView = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.sortColumnComboBox = new System.Windows.Forms.ComboBox();
            this.sortOrderLabel = new System.Windows.Forms.Label();
            this.sortOrderComboBox = new System.Windows.Forms.ComboBox();
            this.groupBoxTreeView = new System.Windows.Forms.GroupBox();
            this.checkBoxShowPlusMinus = new System.Windows.Forms.CheckBox();
            this.checkBoxShowLines = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBoxListView.SuspendLayout();
            this.groupBoxTreeView.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.checkBoxUseLastRPF);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.saveLabel);
            this.panel1.Controls.Add(this.saveButton);
            this.panel1.Controls.Add(this.groupBoxListView);
            this.panel1.Controls.Add(this.groupBoxTreeView);
            this.panel1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel1.Location = new System.Drawing.Point(10, 10);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(318, 376);
            this.panel1.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.backgroundTextureComboBox);
            this.groupBox2.Controls.Add(this.imageSizeModeLabel);
            this.groupBox2.Controls.Add(this.sizeModeComboBox);
            this.groupBox2.Location = new System.Drawing.Point(6, 214);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(304, 70);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Texture Viewer";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(174, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Background :";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // backgroundTextureComboBox
            // 
            this.backgroundTextureComboBox.FormattingEnabled = true;
            this.backgroundTextureComboBox.Items.AddRange(new object[] {
            "Black",
            "White",
            "Red",
            "Green",
            "Blue",
            "Transparent"});
            this.backgroundTextureComboBox.Location = new System.Drawing.Point(177, 40);
            this.backgroundTextureComboBox.Name = "backgroundTextureComboBox";
            this.backgroundTextureComboBox.Size = new System.Drawing.Size(121, 21);
            this.backgroundTextureComboBox.TabIndex = 2;
            this.backgroundTextureComboBox.Text = "Black";
            this.backgroundTextureComboBox.SelectedIndexChanged += new System.EventHandler(this.backgroundTextureComboBox_SelectedIndexChanged);
            // 
            // imageSizeModeLabel
            // 
            this.imageSizeModeLabel.AutoSize = true;
            this.imageSizeModeLabel.Location = new System.Drawing.Point(5, 23);
            this.imageSizeModeLabel.Name = "imageSizeModeLabel";
            this.imageSizeModeLabel.Size = new System.Drawing.Size(63, 13);
            this.imageSizeModeLabel.TabIndex = 1;
            this.imageSizeModeLabel.Text = "Size Mode :";
            this.imageSizeModeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // sizeModeComboBox
            // 
            this.sizeModeComboBox.FormattingEnabled = true;
            this.sizeModeComboBox.Items.AddRange(new object[] {
            "AutoSize",
            "Zoom",
            "CenterImage",
            "StretchImage"});
            this.sizeModeComboBox.Location = new System.Drawing.Point(7, 40);
            this.sizeModeComboBox.Name = "sizeModeComboBox";
            this.sizeModeComboBox.Size = new System.Drawing.Size(121, 21);
            this.sizeModeComboBox.TabIndex = 0;
            this.sizeModeComboBox.Text = "Auto-Size";
            this.sizeModeComboBox.SelectedIndexChanged += new System.EventHandler(this.sizeModeComboBox_SelectedIndexChanged);
            // 
            // checkBoxUseLastRPF
            // 
            this.checkBoxUseLastRPF.AutoSize = true;
            this.checkBoxUseLastRPF.Location = new System.Drawing.Point(12, 290);
            this.checkBoxUseLastRPF.Name = "checkBoxUseLastRPF";
            this.checkBoxUseLastRPF.Size = new System.Drawing.Size(138, 17);
            this.checkBoxUseLastRPF.TabIndex = 3;
            this.checkBoxUseLastRPF.Text = "Use last .RPF at launch";
            this.checkBoxUseLastRPF.UseVisualStyleBackColor = true;
            this.checkBoxUseLastRPF.CheckedChanged += new System.EventHandler(this.checkBoxUseLastRPF_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBoxUseCustomColor);
            this.groupBox1.Location = new System.Drawing.Point(6, 158);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(304, 48);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Model Viewer";
            // 
            // checkBoxUseCustomColor
            // 
            this.checkBoxUseCustomColor.AutoSize = true;
            this.checkBoxUseCustomColor.Location = new System.Drawing.Point(6, 22);
            this.checkBoxUseCustomColor.Name = "checkBoxUseCustomColor";
            this.checkBoxUseCustomColor.Size = new System.Drawing.Size(110, 17);
            this.checkBoxUseCustomColor.TabIndex = 2;
            this.checkBoxUseCustomColor.Text = "Use Custom Color";
            this.checkBoxUseCustomColor.UseVisualStyleBackColor = true;
            this.checkBoxUseCustomColor.CheckedChanged += new System.EventHandler(this.checkBoxUseCustomColor_CheckedChanged);
            // 
            // saveLabel
            // 
            this.saveLabel.AutoSize = true;
            this.saveLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.saveLabel.Location = new System.Drawing.Point(79, 322);
            this.saveLabel.Name = "saveLabel";
            this.saveLabel.Size = new System.Drawing.Size(162, 15);
            this.saveLabel.TabIndex = 4;
            this.saveLabel.Text = "This will only apply on restart";
            // 
            // saveButton
            // 
            this.saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.saveButton.Location = new System.Drawing.Point(7, 340);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(303, 28);
            this.saveButton.TabIndex = 3;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // groupBoxListView
            // 
            this.groupBoxListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxListView.Controls.Add(this.label2);
            this.groupBoxListView.Controls.Add(this.sortColumnComboBox);
            this.groupBoxListView.Controls.Add(this.sortOrderLabel);
            this.groupBoxListView.Controls.Add(this.sortOrderComboBox);
            this.groupBoxListView.Location = new System.Drawing.Point(6, 82);
            this.groupBoxListView.Name = "groupBoxListView";
            this.groupBoxListView.Size = new System.Drawing.Size(304, 70);
            this.groupBoxListView.TabIndex = 2;
            this.groupBoxListView.TabStop = false;
            this.groupBoxListView.Text = "List View";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(174, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Sort Column :";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // sortColumnComboBox
            // 
            this.sortColumnComboBox.FormattingEnabled = true;
            this.sortColumnComboBox.Items.AddRange(new object[] {
            "Name",
            "Type",
            "Size"});
            this.sortColumnComboBox.Location = new System.Drawing.Point(177, 40);
            this.sortColumnComboBox.Name = "sortColumnComboBox";
            this.sortColumnComboBox.Size = new System.Drawing.Size(121, 21);
            this.sortColumnComboBox.TabIndex = 2;
            this.sortColumnComboBox.Text = "Name";
            this.sortColumnComboBox.SelectedIndexChanged += new System.EventHandler(this.sortColumnComboBox_SelectedIndexChanged);
            // 
            // sortOrderLabel
            // 
            this.sortOrderLabel.AutoSize = true;
            this.sortOrderLabel.Location = new System.Drawing.Point(5, 23);
            this.sortOrderLabel.Name = "sortOrderLabel";
            this.sortOrderLabel.Size = new System.Drawing.Size(61, 13);
            this.sortOrderLabel.TabIndex = 1;
            this.sortOrderLabel.Text = "Sort Order :";
            this.sortOrderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // sortOrderComboBox
            // 
            this.sortOrderComboBox.FormattingEnabled = true;
            this.sortOrderComboBox.Items.AddRange(new object[] {
            "Ascending",
            "Descending"});
            this.sortOrderComboBox.Location = new System.Drawing.Point(7, 40);
            this.sortOrderComboBox.Name = "sortOrderComboBox";
            this.sortOrderComboBox.Size = new System.Drawing.Size(121, 21);
            this.sortOrderComboBox.TabIndex = 0;
            this.sortOrderComboBox.Text = "Ascending";
            this.sortOrderComboBox.SelectedIndexChanged += new System.EventHandler(this.sortOrderComboBox_SelectedIndexChanged);
            // 
            // groupBoxTreeView
            // 
            this.groupBoxTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTreeView.BackColor = System.Drawing.SystemColors.Control;
            this.groupBoxTreeView.Controls.Add(this.checkBoxShowPlusMinus);
            this.groupBoxTreeView.Controls.Add(this.checkBoxShowLines);
            this.groupBoxTreeView.Location = new System.Drawing.Point(6, 6);
            this.groupBoxTreeView.Name = "groupBoxTreeView";
            this.groupBoxTreeView.Size = new System.Drawing.Size(304, 70);
            this.groupBoxTreeView.TabIndex = 1;
            this.groupBoxTreeView.TabStop = false;
            this.groupBoxTreeView.Text = "Tree View";
            // 
            // checkBoxShowPlusMinus
            // 
            this.checkBoxShowPlusMinus.AutoSize = true;
            this.checkBoxShowPlusMinus.Checked = true;
            this.checkBoxShowPlusMinus.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowPlusMinus.Location = new System.Drawing.Point(6, 46);
            this.checkBoxShowPlusMinus.Name = "checkBoxShowPlusMinus";
            this.checkBoxShowPlusMinus.Size = new System.Drawing.Size(107, 17);
            this.checkBoxShowPlusMinus.TabIndex = 1;
            this.checkBoxShowPlusMinus.Text = "Show Plus-Minus";
            this.checkBoxShowPlusMinus.UseVisualStyleBackColor = true;
            this.checkBoxShowPlusMinus.CheckedChanged += new System.EventHandler(this.checkBoxShowPlusMinus_CheckedChanged);
            // 
            // checkBoxShowLines
            // 
            this.checkBoxShowLines.AutoSize = true;
            this.checkBoxShowLines.Checked = true;
            this.checkBoxShowLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowLines.Location = new System.Drawing.Point(6, 22);
            this.checkBoxShowLines.Name = "checkBoxShowLines";
            this.checkBoxShowLines.Size = new System.Drawing.Size(81, 17);
            this.checkBoxShowLines.TabIndex = 0;
            this.checkBoxShowLines.Text = "Show Lines";
            this.checkBoxShowLines.UseVisualStyleBackColor = true;
            this.checkBoxShowLines.CheckedChanged += new System.EventHandler(this.checkBoxShowLines_CheckedChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(340, 398);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MagicRDR - Settings";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxListView.ResumeLayout(false);
            this.groupBoxListView.PerformLayout();
            this.groupBoxTreeView.ResumeLayout(false);
            this.groupBoxTreeView.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBoxListView;
        private System.Windows.Forms.Label sortOrderLabel;
        private System.Windows.Forms.ComboBox sortOrderComboBox;
        private System.Windows.Forms.GroupBox groupBoxTreeView;
        private System.Windows.Forms.CheckBox checkBoxShowPlusMinus;
        private System.Windows.Forms.CheckBox checkBoxShowLines;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Label saveLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxUseCustomColor;
        private System.Windows.Forms.CheckBox checkBoxUseLastRPF;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label imageSizeModeLabel;
        private System.Windows.Forms.ComboBox sizeModeComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox backgroundTextureComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox sortColumnComboBox;
    }
}