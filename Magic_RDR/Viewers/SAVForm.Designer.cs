namespace Magic_RDR.Viewers
{
    partial class SAVForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SAVForm));
            this.languageComboBox = new System.Windows.Forms.ComboBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.currentLanguageLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // languageComboBox
            // 
            this.languageComboBox.FormattingEnabled = true;
            this.languageComboBox.Items.AddRange(new object[] {
            "English",
            "French",
            "Korean",
            "Portuguese",
            "Russian"});
            this.languageComboBox.Location = new System.Drawing.Point(21, 48);
            this.languageComboBox.Name = "languageComboBox";
            this.languageComboBox.Size = new System.Drawing.Size(225, 21);
            this.languageComboBox.TabIndex = 1;
            // 
            // saveButton
            // 
            this.saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.saveButton.Location = new System.Drawing.Point(21, 96);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(225, 35);
            this.saveButton.TabIndex = 2;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // currentLanguageLabel
            // 
            this.currentLanguageLabel.AutoSize = true;
            this.currentLanguageLabel.Location = new System.Drawing.Point(18, 20);
            this.currentLanguageLabel.Name = "currentLanguageLabel";
            this.currentLanguageLabel.Size = new System.Drawing.Size(131, 13);
            this.currentLanguageLabel.TabIndex = 3;
            this.currentLanguageLabel.Text = "Current language : English";
            // 
            // SAVForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(263, 149);
            this.Controls.Add(this.currentLanguageLabel);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.languageComboBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SAVForm";
            this.Text = "RDR .SAV Editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox languageComboBox;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Label currentLanguageLabel;
    }
}