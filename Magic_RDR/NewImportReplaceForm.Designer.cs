
using System.Windows.Forms;

namespace Magic_RDR
{
    partial class NewImportReplaceForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewImportReplaceForm));
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.importTab = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.importFileResourcePanel = new System.Windows.Forms.Panel();
            this.importResourceFlag2 = new System.Windows.Forms.TextBox();
            this.importResourceFlag1 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.importResourceVersionLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.importResourceType = new System.Windows.Forms.NumericUpDown();
            this.importFileButton = new System.Windows.Forms.Button();
            this.fileImportPanel = new System.Windows.Forms.Panel();
            this.import_Append = new System.Windows.Forms.RadioButton();
            this.import_Before = new System.Windows.Forms.RadioButton();
            this.import_After = new System.Windows.Forms.RadioButton();
            this.importOpenFile = new System.Windows.Forms.Button();
            this.importFileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.replaceTab = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.replaceFileResourcePanel = new System.Windows.Forms.Panel();
            this.replaceResourceFlag2 = new System.Windows.Forms.TextBox();
            this.replaceResourceFlag1 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.replaceResourceVersionLabel = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.replaceResourceType = new System.Windows.Forms.NumericUpDown();
            this.replaceButton = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.replaceReplaceNameCheck = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.replacingFileName = new System.Windows.Forms.TextBox();
            this.replaceFileOpen = new System.Windows.Forms.Button();
            this.replacingWithFileName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.mainTabControl.SuspendLayout();
            this.importTab.SuspendLayout();
            this.panel1.SuspendLayout();
            this.importFileResourcePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.importResourceType)).BeginInit();
            this.fileImportPanel.SuspendLayout();
            this.replaceTab.SuspendLayout();
            this.panel2.SuspendLayout();
            this.replaceFileResourcePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.replaceResourceType)).BeginInit();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTabControl
            // 
            this.mainTabControl.Controls.Add(this.importTab);
            this.mainTabControl.Controls.Add(this.replaceTab);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.mainTabControl.Location = new System.Drawing.Point(0, 0);
            this.mainTabControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(502, 501);
            this.mainTabControl.TabIndex = 0;
            // 
            // importTab
            // 
            this.importTab.Controls.Add(this.panel1);
            this.importTab.Location = new System.Drawing.Point(4, 30);
            this.importTab.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.importTab.Name = "importTab";
            this.importTab.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.importTab.Size = new System.Drawing.Size(494, 467);
            this.importTab.TabIndex = 0;
            this.importTab.Text = "Import";
            this.importTab.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.importFileResourcePanel);
            this.panel1.Controls.Add(this.importFileButton);
            this.panel1.Controls.Add(this.fileImportPanel);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(9, 7);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(471, 451);
            this.panel1.TabIndex = 1;
            // 
            // importFileResourcePanel
            // 
            this.importFileResourcePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.importFileResourcePanel.Controls.Add(this.importResourceFlag2);
            this.importFileResourcePanel.Controls.Add(this.importResourceFlag1);
            this.importFileResourcePanel.Controls.Add(this.label4);
            this.importFileResourcePanel.Controls.Add(this.label3);
            this.importFileResourcePanel.Controls.Add(this.importResourceVersionLabel);
            this.importFileResourcePanel.Controls.Add(this.label5);
            this.importFileResourcePanel.Controls.Add(this.importResourceType);
            this.importFileResourcePanel.Enabled = false;
            this.importFileResourcePanel.Location = new System.Drawing.Point(16, 220);
            this.importFileResourcePanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.importFileResourcePanel.Name = "importFileResourcePanel";
            this.importFileResourcePanel.Size = new System.Drawing.Size(435, 167);
            this.importFileResourcePanel.TabIndex = 5;
            // 
            // importResourceFlag2
            // 
            this.importResourceFlag2.Location = new System.Drawing.Point(154, 107);
            this.importResourceFlag2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.importResourceFlag2.MaxLength = 8;
            this.importResourceFlag2.Name = "importResourceFlag2";
            this.importResourceFlag2.Size = new System.Drawing.Size(275, 29);
            this.importResourceFlag2.TabIndex = 13;
            this.importResourceFlag2.TextChanged += new System.EventHandler(this.FlagBoxes_TextChanged);
            // 
            // importResourceFlag1
            // 
            this.importResourceFlag1.Location = new System.Drawing.Point(154, 73);
            this.importResourceFlag1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.importResourceFlag1.MaxLength = 8;
            this.importResourceFlag1.Name = "importResourceFlag1";
            this.importResourceFlag1.Size = new System.Drawing.Size(275, 29);
            this.importResourceFlag1.TabIndex = 12;
            this.importResourceFlag1.TextChanged += new System.EventHandler(this.FlagBoxes_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 108);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(119, 21);
            this.label4.TabIndex = 11;
            this.label4.Text = "Resource Flag2:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 74);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(119, 21);
            this.label3.TabIndex = 9;
            this.label3.Text = "Resource Flag1:";
            // 
            // importResourceVersionLabel
            // 
            this.importResourceVersionLabel.AutoSize = true;
            this.importResourceVersionLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.importResourceVersionLabel.Location = new System.Drawing.Point(10, 6);
            this.importResourceVersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.importResourceVersionLabel.Name = "importResourceVersionLabel";
            this.importResourceVersionLabel.Size = new System.Drawing.Size(198, 25);
            this.importResourceVersionLabel.TabIndex = 6;
            this.importResourceVersionLabel.Text = "Resource Version: N/A";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 39);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(113, 21);
            this.label5.TabIndex = 5;
            this.label5.Text = "Resource Type:";
            // 
            // importResourceType
            // 
            this.importResourceType.Location = new System.Drawing.Point(154, 38);
            this.importResourceType.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.importResourceType.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.importResourceType.Name = "importResourceType";
            this.importResourceType.Size = new System.Drawing.Size(275, 29);
            this.importResourceType.TabIndex = 4;
            // 
            // importFileButton
            // 
            this.importFileButton.Location = new System.Drawing.Point(16, 400);
            this.importFileButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.importFileButton.Name = "importFileButton";
            this.importFileButton.Size = new System.Drawing.Size(435, 40);
            this.importFileButton.TabIndex = 4;
            this.importFileButton.Text = "Import";
            this.importFileButton.UseVisualStyleBackColor = true;
            this.importFileButton.Click += new System.EventHandler(this.importFileButton_Click);
            // 
            // fileImportPanel
            // 
            this.fileImportPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.fileImportPanel.Controls.Add(this.import_Append);
            this.fileImportPanel.Controls.Add(this.import_Before);
            this.fileImportPanel.Controls.Add(this.import_After);
            this.fileImportPanel.Controls.Add(this.importOpenFile);
            this.fileImportPanel.Controls.Add(this.importFileName);
            this.fileImportPanel.Controls.Add(this.label2);
            this.fileImportPanel.Location = new System.Drawing.Point(16, 46);
            this.fileImportPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.fileImportPanel.Name = "fileImportPanel";
            this.fileImportPanel.Size = new System.Drawing.Size(435, 161);
            this.fileImportPanel.TabIndex = 2;
            // 
            // import_Append
            // 
            this.import_Append.AutoSize = true;
            this.import_Append.Checked = true;
            this.import_Append.Location = new System.Drawing.Point(16, 127);
            this.import_Append.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.import_Append.Name = "import_Append";
            this.import_Append.Size = new System.Drawing.Size(131, 25);
            this.import_Append.TabIndex = 5;
            this.import_Append.TabStop = true;
            this.import_Append.Text = "Append To End";
            this.import_Append.UseVisualStyleBackColor = true;
            // 
            // import_Before
            // 
            this.import_Before.AutoSize = true;
            this.import_Before.Enabled = false;
            this.import_Before.Location = new System.Drawing.Point(16, 100);
            this.import_Before.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.import_Before.Name = "import_Before";
            this.import_Before.Size = new System.Drawing.Size(115, 25);
            this.import_Before.TabIndex = 4;
            this.import_Before.TabStop = true;
            this.import_Before.Text = "Before [FILE]";
            this.import_Before.UseVisualStyleBackColor = true;
            // 
            // import_After
            // 
            this.import_After.AutoSize = true;
            this.import_After.Enabled = false;
            this.import_After.Location = new System.Drawing.Point(16, 74);
            this.import_After.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.import_After.Name = "import_After";
            this.import_After.Size = new System.Drawing.Size(104, 25);
            this.import_After.TabIndex = 3;
            this.import_After.TabStop = true;
            this.import_After.Text = "After [FILE]";
            this.import_After.UseVisualStyleBackColor = true;
            // 
            // importOpenFile
            // 
            this.importOpenFile.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.importOpenFile.Location = new System.Drawing.Point(380, 38);
            this.importOpenFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.importOpenFile.Name = "importOpenFile";
            this.importOpenFile.Size = new System.Drawing.Size(49, 27);
            this.importOpenFile.TabIndex = 2;
            this.importOpenFile.Text = "...";
            this.importOpenFile.UseVisualStyleBackColor = true;
            this.importOpenFile.Click += new System.EventHandler(this.importOpenFile_Click);
            // 
            // importFileName
            // 
            this.importFileName.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.importFileName.Location = new System.Drawing.Point(4, 38);
            this.importFileName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.importFileName.Name = "importFileName";
            this.importFileName.ReadOnly = true;
            this.importFileName.Size = new System.Drawing.Size(367, 25);
            this.importFileName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 13F);
            this.label2.Location = new System.Drawing.Point(10, 6);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 25);
            this.label2.TabIndex = 0;
            this.label2.Text = "File Import";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 18F);
            this.label1.Location = new System.Drawing.Point(9, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 32);
            this.label1.TabIndex = 0;
            this.label1.Text = "Import";
            // 
            // replaceTab
            // 
            this.replaceTab.Controls.Add(this.panel2);
            this.replaceTab.Location = new System.Drawing.Point(4, 30);
            this.replaceTab.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.replaceTab.Name = "replaceTab";
            this.replaceTab.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.replaceTab.Size = new System.Drawing.Size(494, 467);
            this.replaceTab.TabIndex = 1;
            this.replaceTab.Text = "Replace";
            this.replaceTab.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.replaceFileResourcePanel);
            this.panel2.Controls.Add(this.replaceButton);
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Controls.Add(this.label11);
            this.panel2.Location = new System.Drawing.Point(9, 9);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(471, 453);
            this.panel2.TabIndex = 2;
            // 
            // replaceFileResourcePanel
            // 
            this.replaceFileResourcePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.replaceFileResourcePanel.Controls.Add(this.replaceResourceFlag2);
            this.replaceFileResourcePanel.Controls.Add(this.replaceResourceFlag1);
            this.replaceFileResourcePanel.Controls.Add(this.label6);
            this.replaceFileResourcePanel.Controls.Add(this.label7);
            this.replaceFileResourcePanel.Controls.Add(this.replaceResourceVersionLabel);
            this.replaceFileResourcePanel.Controls.Add(this.label9);
            this.replaceFileResourcePanel.Controls.Add(this.replaceResourceType);
            this.replaceFileResourcePanel.Enabled = false;
            this.replaceFileResourcePanel.Location = new System.Drawing.Point(16, 220);
            this.replaceFileResourcePanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.replaceFileResourcePanel.Name = "replaceFileResourcePanel";
            this.replaceFileResourcePanel.Size = new System.Drawing.Size(435, 167);
            this.replaceFileResourcePanel.TabIndex = 5;
            // 
            // replaceResourceFlag2
            // 
            this.replaceResourceFlag2.Location = new System.Drawing.Point(154, 107);
            this.replaceResourceFlag2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.replaceResourceFlag2.MaxLength = 8;
            this.replaceResourceFlag2.Name = "replaceResourceFlag2";
            this.replaceResourceFlag2.Size = new System.Drawing.Size(275, 29);
            this.replaceResourceFlag2.TabIndex = 13;
            this.replaceResourceFlag2.TextChanged += new System.EventHandler(this.FlagBoxes_TextChanged);
            // 
            // replaceResourceFlag1
            // 
            this.replaceResourceFlag1.Location = new System.Drawing.Point(154, 73);
            this.replaceResourceFlag1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.replaceResourceFlag1.MaxLength = 8;
            this.replaceResourceFlag1.Name = "replaceResourceFlag1";
            this.replaceResourceFlag1.Size = new System.Drawing.Size(275, 29);
            this.replaceResourceFlag1.TabIndex = 12;
            this.replaceResourceFlag1.TextChanged += new System.EventHandler(this.FlagBoxes_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 108);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(119, 21);
            this.label6.TabIndex = 11;
            this.label6.Text = "Resource Flag2:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 74);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(119, 21);
            this.label7.TabIndex = 9;
            this.label7.Text = "Resource Flag1:";
            // 
            // replaceResourceVersionLabel
            // 
            this.replaceResourceVersionLabel.AutoSize = true;
            this.replaceResourceVersionLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.replaceResourceVersionLabel.Location = new System.Drawing.Point(10, 6);
            this.replaceResourceVersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.replaceResourceVersionLabel.Name = "replaceResourceVersionLabel";
            this.replaceResourceVersionLabel.Size = new System.Drawing.Size(198, 25);
            this.replaceResourceVersionLabel.TabIndex = 6;
            this.replaceResourceVersionLabel.Text = "Resource Version: N/A";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 39);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(113, 21);
            this.label9.TabIndex = 5;
            this.label9.Text = "Resource Type:";
            // 
            // replaceResourceType
            // 
            this.replaceResourceType.Location = new System.Drawing.Point(154, 38);
            this.replaceResourceType.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.replaceResourceType.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.replaceResourceType.Name = "replaceResourceType";
            this.replaceResourceType.Size = new System.Drawing.Size(275, 29);
            this.replaceResourceType.TabIndex = 4;
            // 
            // replaceButton
            // 
            this.replaceButton.Location = new System.Drawing.Point(16, 400);
            this.replaceButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.replaceButton.Name = "replaceButton";
            this.replaceButton.Size = new System.Drawing.Size(435, 40);
            this.replaceButton.TabIndex = 4;
            this.replaceButton.Text = "Replace";
            this.replaceButton.UseVisualStyleBackColor = true;
            this.replaceButton.Click += new System.EventHandler(this.replaceButton_Click);
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.replaceReplaceNameCheck);
            this.panel4.Controls.Add(this.label12);
            this.panel4.Controls.Add(this.replacingFileName);
            this.panel4.Controls.Add(this.replaceFileOpen);
            this.panel4.Controls.Add(this.replacingWithFileName);
            this.panel4.Controls.Add(this.label10);
            this.panel4.Location = new System.Drawing.Point(16, 46);
            this.panel4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(435, 149);
            this.panel4.TabIndex = 2;
            // 
            // replaceReplaceNameCheck
            // 
            this.replaceReplaceNameCheck.AutoSize = true;
            this.replaceReplaceNameCheck.Location = new System.Drawing.Point(223, 74);
            this.replaceReplaceNameCheck.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.replaceReplaceNameCheck.Name = "replaceReplaceNameCheck";
            this.replaceReplaceNameCheck.Size = new System.Drawing.Size(129, 25);
            this.replaceReplaceNameCheck.TabIndex = 11;
            this.replaceReplaceNameCheck.Text = "Replace Name";
            this.replaceReplaceNameCheck.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Segoe UI", 13F);
            this.label12.Location = new System.Drawing.Point(10, 74);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(49, 25);
            this.label12.TabIndex = 4;
            this.label12.Text = "With";
            // 
            // replacingFileName
            // 
            this.replacingFileName.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.replacingFileName.Location = new System.Drawing.Point(7, 38);
            this.replacingFileName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.replacingFileName.Name = "replacingFileName";
            this.replacingFileName.ReadOnly = true;
            this.replacingFileName.Size = new System.Drawing.Size(366, 25);
            this.replacingFileName.TabIndex = 3;
            // 
            // replaceFileOpen
            // 
            this.replaceFileOpen.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.replaceFileOpen.Location = new System.Drawing.Point(380, 106);
            this.replaceFileOpen.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.replaceFileOpen.Name = "replaceFileOpen";
            this.replaceFileOpen.Size = new System.Drawing.Size(49, 27);
            this.replaceFileOpen.TabIndex = 2;
            this.replaceFileOpen.Text = "...";
            this.replaceFileOpen.UseVisualStyleBackColor = true;
            this.replaceFileOpen.Click += new System.EventHandler(this.replaceFileOpen_Click);
            // 
            // replacingWithFileName
            // 
            this.replacingWithFileName.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.replacingWithFileName.Location = new System.Drawing.Point(7, 106);
            this.replacingWithFileName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.replacingWithFileName.Name = "replacingWithFileName";
            this.replacingWithFileName.ReadOnly = true;
            this.replacingWithFileName.Size = new System.Drawing.Size(366, 25);
            this.replacingWithFileName.TabIndex = 1;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Segoe UI", 13F);
            this.label10.Location = new System.Drawing.Point(10, 5);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(88, 25);
            this.label10.TabIndex = 0;
            this.label10.Text = "Replacing";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Segoe UI", 18F);
            this.label11.Location = new System.Drawing.Point(7, 3);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(96, 32);
            this.label11.TabIndex = 0;
            this.label11.Text = "Replace";
            // 
            // NewImportReplaceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(502, 501);
            this.Controls.Add(this.mainTabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewImportReplaceForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MagicRDR | Import/Replace";
            this.mainTabControl.ResumeLayout(false);
            this.importTab.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.importFileResourcePanel.ResumeLayout(false);
            this.importFileResourcePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.importResourceType)).EndInit();
            this.fileImportPanel.ResumeLayout(false);
            this.fileImportPanel.PerformLayout();
            this.replaceTab.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.replaceFileResourcePanel.ResumeLayout(false);
            this.replaceFileResourcePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.replaceResourceType)).EndInit();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TabControl mainTabControl;
        private TabPage importTab;
        private TabPage replaceTab;
        private Panel panel1;
        private Label label1;
        private Panel fileImportPanel;
        private RadioButton import_Append;
        private RadioButton import_Before;
        private RadioButton import_After;
        private Button importOpenFile;
        private TextBox importFileName;
        private Label label2;
        private Button importFileButton;
        private Panel importFileResourcePanel;
        private TextBox importResourceFlag2;
        private TextBox importResourceFlag1;
        private Label label4;
        private Label label3;
        private Label importResourceVersionLabel;
        private Label label5;
        private NumericUpDown importResourceType;
        private Panel panel2;
        private Panel replaceFileResourcePanel;
        private TextBox replaceResourceFlag2;
        private TextBox replaceResourceFlag1;
        private Label label6;
        private Label label7;
        private Label replaceResourceVersionLabel;
        private Label label9;
        private NumericUpDown replaceResourceType;
        private Button replaceButton;
        private Panel panel4;
        private Button replaceFileOpen;
        private TextBox replacingWithFileName;
        private Label label10;
        private Label label11;
        private Label label12;
        private TextBox replacingFileName;
        private CheckBox replaceReplaceNameCheck;
    }
}