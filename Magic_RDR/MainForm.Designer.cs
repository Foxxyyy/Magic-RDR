
using System.Windows.Forms;

namespace Magic_RDR
{
    partial class MainForm
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            ""}, 5, System.Drawing.Color.Empty, System.Drawing.Color.Empty, new System.Drawing.Font("Arial", 9F));
            this.treeView = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.fileOptions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.importButton = new System.Windows.Forms.ToolStripMenuItem();
            this.importDirectoryButton = new System.Windows.Forms.ToolStripMenuItem();
            this.createDirectoryButton = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.replaceFileButton = new System.Windows.Forms.ToolStripMenuItem();
            this.removeFileButton = new System.Windows.Forms.ToolStripMenuItem();
            this.removeDirectoryButton = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.extractFileButton = new System.Windows.Forms.ToolStripMenuItem();
            this.extractResourceButton = new System.Windows.Forms.ToolStripMenuItem();
            this.extractTheseFilesButton = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.viewFilePropertiesButton = new System.Windows.Forms.ToolStripMenuItem();
            this.viewHexButton = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.copyPathButton = new System.Windows.Forms.ToolStripMenuItem();
            this.accessDirectoryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.fileNameStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.currentDirectoryLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.asNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hashStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.creditsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.listView = new Magic_RDR.RPF.ListViewNF();
            this.NameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TypeColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SizeColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.changeSAVLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileOptions.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.BackColor = System.Drawing.SystemColors.Control;
            this.treeView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeView.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeView.FullRowSelect = true;
            this.treeView.HideSelection = false;
            this.treeView.HotTracking = true;
            this.treeView.Indent = 28;
            this.treeView.ItemHeight = 25;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.treeView.Name = "treeView";
            treeNode2.Name = "Node0";
            treeNode2.NodeFont = new System.Drawing.Font("Arial", 9F);
            treeNode2.Text = "";
            this.treeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode2});
            this.treeView.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.treeView.ShowLines = false;
            this.treeView.ShowPlusMinus = false;
            this.treeView.Size = new System.Drawing.Size(205, 445);
            this.treeView.TabIndex = 0;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            this.treeView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseUp);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "folder.png");
            this.imageList1.Images.SetKeyName(1, "palette.png");
            this.imageList1.Images.SetKeyName(2, "film.png");
            this.imageList1.Images.SetKeyName(3, "music.png");
            this.imageList1.Images.SetKeyName(4, "page_word.png");
            this.imageList1.Images.SetKeyName(5, "script.png");
            this.imageList1.Images.SetKeyName(6, "bricks.png");
            this.imageList1.Images.SetKeyName(7, "chart_pie.png");
            this.imageList1.Images.SetKeyName(8, "user.png");
            // 
            // fileOptions
            // 
            this.fileOptions.Font = new System.Drawing.Font("Arial", 11F);
            this.fileOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importButton,
            this.importDirectoryButton,
            this.createDirectoryButton,
            this.toolStripSeparator3,
            this.replaceFileButton,
            this.removeFileButton,
            this.removeDirectoryButton,
            this.toolStripSeparator4,
            this.extractFileButton,
            this.extractResourceButton,
            this.extractTheseFilesButton,
            this.toolStripSeparator5,
            this.viewFilePropertiesButton,
            this.viewHexButton,
            this.toolStripSeparator6,
            this.copyPathButton,
            this.accessDirectoryMenuItem});
            this.fileOptions.Name = "fileOptions";
            this.fileOptions.Size = new System.Drawing.Size(195, 314);
            this.fileOptions.Opening += new System.ComponentModel.CancelEventHandler(this.fileOptions_Opening);
            // 
            // importButton
            // 
            this.importButton.Image = global::Magic_RDR.Properties.Resources.page_add;
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(194, 22);
            this.importButton.Text = "Import File";
            this.importButton.Click += new System.EventHandler(this.importButton_Click);
            // 
            // importDirectoryButton
            // 
            this.importDirectoryButton.Image = ((System.Drawing.Image)(resources.GetObject("importDirectoryButton.Image")));
            this.importDirectoryButton.Name = "importDirectoryButton";
            this.importDirectoryButton.Size = new System.Drawing.Size(194, 22);
            this.importDirectoryButton.Text = "Import Directory";
            this.importDirectoryButton.Click += new System.EventHandler(this.importDirectoryButton_Click);
            // 
            // createDirectoryButton
            // 
            this.createDirectoryButton.Image = global::Magic_RDR.Properties.Resources.folder_add;
            this.createDirectoryButton.Name = "createDirectoryButton";
            this.createDirectoryButton.Size = new System.Drawing.Size(194, 22);
            this.createDirectoryButton.Text = "Create Directory";
            this.createDirectoryButton.Click += new System.EventHandler(this.createDirectoryButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(191, 6);
            // 
            // replaceFileButton
            // 
            this.replaceFileButton.Image = global::Magic_RDR.Properties.Resources.page_copy;
            this.replaceFileButton.Name = "replaceFileButton";
            this.replaceFileButton.Size = new System.Drawing.Size(194, 22);
            this.replaceFileButton.Text = "Replace File";
            this.replaceFileButton.Click += new System.EventHandler(this.replaceFileButton_Click);
            // 
            // removeFileButton
            // 
            this.removeFileButton.Image = global::Magic_RDR.Properties.Resources.page_delete;
            this.removeFileButton.Name = "removeFileButton";
            this.removeFileButton.Size = new System.Drawing.Size(194, 22);
            this.removeFileButton.Text = "Remove File";
            this.removeFileButton.Click += new System.EventHandler(this.removeFileButton_Click);
            // 
            // removeDirectoryButton
            // 
            this.removeDirectoryButton.Image = ((System.Drawing.Image)(resources.GetObject("removeDirectoryButton.Image")));
            this.removeDirectoryButton.Name = "removeDirectoryButton";
            this.removeDirectoryButton.Size = new System.Drawing.Size(194, 22);
            this.removeDirectoryButton.Text = "Remove Directory";
            this.removeDirectoryButton.Click += new System.EventHandler(this.removeDirectoryButton_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(191, 6);
            // 
            // extractFileButton
            // 
            this.extractFileButton.Image = global::Magic_RDR.Properties.Resources.page_go;
            this.extractFileButton.Name = "extractFileButton";
            this.extractFileButton.Size = new System.Drawing.Size(194, 22);
            this.extractFileButton.Text = "Extract File";
            this.extractFileButton.Click += new System.EventHandler(this.extractFileButton_Click);
            // 
            // extractResourceButton
            // 
            this.extractResourceButton.Image = global::Magic_RDR.Properties.Resources.package_go;
            this.extractResourceButton.Name = "extractResourceButton";
            this.extractResourceButton.Size = new System.Drawing.Size(194, 22);
            this.extractResourceButton.Text = "Extract Resource";
            this.extractResourceButton.Click += new System.EventHandler(this.extractResourceButton_Click);
            // 
            // extractTheseFilesButton
            // 
            this.extractTheseFilesButton.Image = global::Magic_RDR.Properties.Resources.folder_go;
            this.extractTheseFilesButton.Name = "extractTheseFilesButton";
            this.extractTheseFilesButton.Size = new System.Drawing.Size(194, 22);
            this.extractTheseFilesButton.Text = "Extract Directory";
            this.extractTheseFilesButton.Click += new System.EventHandler(this.extractTheseFilesButton_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(191, 6);
            // 
            // viewFilePropertiesButton
            // 
            this.viewFilePropertiesButton.Image = global::Magic_RDR.Properties.Resources.page_edit;
            this.viewFilePropertiesButton.Name = "viewFilePropertiesButton";
            this.viewFilePropertiesButton.Size = new System.Drawing.Size(194, 22);
            this.viewFilePropertiesButton.Text = "View Properties";
            this.viewFilePropertiesButton.Click += new System.EventHandler(this.viewFilePropertiesButton_Click);
            // 
            // viewHexButton
            // 
            this.viewHexButton.Image = ((System.Drawing.Image)(resources.GetObject("viewHexButton.Image")));
            this.viewHexButton.Name = "viewHexButton";
            this.viewHexButton.Size = new System.Drawing.Size(194, 22);
            this.viewHexButton.Text = "View Hex";
            this.viewHexButton.Click += new System.EventHandler(this.viewHexButton_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(191, 6);
            // 
            // copyPathButton
            // 
            this.copyPathButton.Image = global::Magic_RDR.Properties.Resources.page_paste;
            this.copyPathButton.Name = "copyPathButton";
            this.copyPathButton.Size = new System.Drawing.Size(194, 22);
            this.copyPathButton.Text = "Copy Path";
            this.copyPathButton.Click += new System.EventHandler(this.copyPathButton_Click);
            // 
            // accessDirectoryMenuItem
            // 
            this.accessDirectoryMenuItem.Image = global::Magic_RDR.Properties.Resources.folder_find;
            this.accessDirectoryMenuItem.Name = "accessDirectoryMenuItem";
            this.accessDirectoryMenuItem.Size = new System.Drawing.Size(194, 22);
            this.accessDirectoryMenuItem.Text = "Access Directory";
            this.accessDirectoryMenuItem.Click += new System.EventHandler(this.accessDirectoryMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusStrip1.AutoSize = false;
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileNameStatusLabel,
            this.currentDirectoryLabel});
            this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.statusStrip1.Location = new System.Drawing.Point(0, 481);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip1.Size = new System.Drawing.Size(784, 30);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // fileNameStatusLabel
            // 
            this.fileNameStatusLabel.BackColor = System.Drawing.SystemColors.Control;
            this.fileNameStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileNameStatusLabel.Image = global::Magic_RDR.Properties.Resources.accept;
            this.fileNameStatusLabel.Margin = new System.Windows.Forms.Padding(3, 3, 0, 2);
            this.fileNameStatusLabel.Name = "fileNameStatusLabel";
            this.fileNameStatusLabel.Size = new System.Drawing.Size(132, 25);
            this.fileNameStatusLabel.Text = "Made by Im Foxxyyy";
            // 
            // currentDirectoryLabel
            // 
            this.currentDirectoryLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.currentDirectoryLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.currentDirectoryLabel.Name = "currentDirectoryLabel";
            this.currentDirectoryLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.currentDirectoryLabel.Size = new System.Drawing.Size(0, 25);
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 100);
            this.panel1.TabIndex = 0;
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.toolStripSeparator,
            this.saveToolStripMenuItem,
            this.reloadToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileToolStripMenuItem.Image = global::Magic_RDR.Properties.Resources.file;
            this.fileToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(48, 23);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripMenuItem.Image")));
            this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(180, 20);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(177, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem1,
            this.asNewToolStripMenuItem});
            this.saveToolStripMenuItem.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.saveToolStripMenuItem.Image = global::Magic_RDR.Properties.Resources.save;
            this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // saveToolStripMenuItem1
            // 
            this.saveToolStripMenuItem1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.saveToolStripMenuItem1.Image = global::Magic_RDR.Properties.Resources.script_save;
            this.saveToolStripMenuItem1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripMenuItem1.Name = "saveToolStripMenuItem1";
            this.saveToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem1.Size = new System.Drawing.Size(187, 22);
            this.saveToolStripMenuItem1.Text = "Current";
            this.saveToolStripMenuItem1.Click += new System.EventHandler(this.saveToolStripMenuItem1_Click);
            // 
            // asNewToolStripMenuItem
            // 
            this.asNewToolStripMenuItem.Image = global::Magic_RDR.Properties.Resources.script_go;
            this.asNewToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.asNewToolStripMenuItem.Name = "asNewToolStripMenuItem";
            this.asNewToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.S)));
            this.asNewToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.asNewToolStripMenuItem.Text = "As New";
            this.asNewToolStripMenuItem.Click += new System.EventHandler(this.asNewToolStripMenuItem_Click);
            // 
            // reloadToolStripMenuItem
            // 
            this.reloadToolStripMenuItem.Image = global::Magic_RDR.Properties.Resources._1200px_OOjs_UI_icon_reload_svg;
            this.reloadToolStripMenuItem.Name = "reloadToolStripMenuItem";
            this.reloadToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.reloadToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.reloadToolStripMenuItem.Text = "Reload";
            this.reloadToolStripMenuItem.Click += new System.EventHandler(this.reloadToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exitToolStripMenuItem.Image = global::Magic_RDR.Properties.Resources.close;
            this.exitToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Close";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hashStripMenuItem,
            this.changeSAVLanguageToolStripMenuItem,
            this.toolStripSeparator2,
            this.settingsToolStripMenuItem});
            this.toolsToolStripMenuItem.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("toolsToolStripMenuItem.Image")));
            this.toolsToolStripMenuItem.Margin = new System.Windows.Forms.Padding(4, 3, 0, 0);
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(80, 23);
            this.toolsToolStripMenuItem.Text = "Options";
            // 
            // hashStripMenuItem
            // 
            this.hashStripMenuItem.Image = global::Magic_RDR.Properties.Resources.comment_edit;
            this.hashStripMenuItem.Name = "hashStripMenuItem";
            this.hashStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.hashStripMenuItem.Text = "Hash Generator";
            this.hashStripMenuItem.Click += new System.EventHandler(this.hashStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(205, 6);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Image = global::Magic_RDR.Properties.Resources.cog;
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // creditsToolStripMenuItem
            // 
            this.creditsToolStripMenuItem.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.creditsToolStripMenuItem.Image = global::Magic_RDR.Properties.Resources.cog;
            this.creditsToolStripMenuItem.Margin = new System.Windows.Forms.Padding(-2, 3, 0, 0);
            this.creditsToolStripMenuItem.Name = "creditsToolStripMenuItem";
            this.creditsToolStripMenuItem.Size = new System.Drawing.Size(76, 23);
            this.creditsToolStripMenuItem.Text = "Credits";
            this.creditsToolStripMenuItem.Click += new System.EventHandler(this.creditsToolStripMenuItem_Click);
            // 
            // menuStrip
            // 
            this.menuStrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.menuStrip.AutoSize = false;
            this.menuStrip.BackColor = System.Drawing.SystemColors.Control;
            this.menuStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.creditsToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.menuStrip.Size = new System.Drawing.Size(784, 30);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip1";
            // 
            // searchBox
            // 
            this.searchBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchBox.BackColor = System.Drawing.SystemColors.Window;
            this.searchBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.searchBox.ForeColor = System.Drawing.Color.Silver;
            this.searchBox.Location = new System.Drawing.Point(591, 5);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(186, 23);
            this.searchBox.TabIndex = 6;
            this.searchBox.Text = "Search files...";
            this.searchBox.Visible = false;
            this.searchBox.TextChanged += new System.EventHandler(this.searchBox_TextChanged);
            this.searchBox.Enter += new System.EventHandler(this.searchBox_Enter);
            this.searchBox.Leave += new System.EventHandler(this.searchBox_Leave);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Cursor = System.Windows.Forms.Cursors.VSplit;
            this.splitContainer1.Enabled = false;
            this.splitContainer1.Location = new System.Drawing.Point(0, 33);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listView);
            this.splitContainer1.Size = new System.Drawing.Size(784, 445);
            this.splitContainer1.SplitterDistance = 205;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 7;
            // 
            // listView
            // 
            this.listView.Alignment = System.Windows.Forms.ListViewAlignment.Default;
            this.listView.BackColor = System.Drawing.SystemColors.Control;
            this.listView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.NameColumn,
            this.TypeColumn,
            this.SizeColumn});
            this.listView.ContextMenuStrip = this.fileOptions;
            this.listView.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listView.FullRowSelect = true;
            this.listView.HideSelection = false;
            this.listView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem2});
            this.listView.Location = new System.Drawing.Point(0, 0);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(574, 445);
            this.listView.TabIndex = 2;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_ColumnClick);
            this.listView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listView_KeyDown);
            this.listView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView_MouseDoubleClick);
            this.listView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listView_MouseMove);
            // 
            // NameColumn
            // 
            this.NameColumn.Text = "Name";
            this.NameColumn.Width = 300;
            // 
            // TypeColumn
            // 
            this.TypeColumn.Text = "Type";
            this.TypeColumn.Width = 150;
            // 
            // SizeColumn
            // 
            this.SizeColumn.Tag = "Numeric";
            this.SizeColumn.Text = "Size";
            this.SizeColumn.Width = 110;
            // 
            // changeSAVLanguageToolStripMenuItem
            // 
            this.changeSAVLanguageToolStripMenuItem.Image = global::Magic_RDR.Properties.Resources.application_view_gallery;
            this.changeSAVLanguageToolStripMenuItem.Name = "changeSAVLanguageToolStripMenuItem";
            this.changeSAVLanguageToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.changeSAVLanguageToolStripMenuItem.Text = "Change .SAV language";
            this.changeSAVLanguageToolStripMenuItem.Click += new System.EventHandler(this.changeSAVLanguageToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(784, 511);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.searchBox);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MagicRDR | Xbox 360";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.fileOptions.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private Magic_RDR.RPF.ListViewNF listView;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel fileNameStatusLabel;
        private System.Windows.Forms.ColumnHeader NameColumn;
        private System.Windows.Forms.ColumnHeader TypeColumn;
        private System.Windows.Forms.ColumnHeader SizeColumn;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ContextMenuStrip fileOptions;
        private System.Windows.Forms.ToolStripMenuItem importButton;
        private System.Windows.Forms.ToolStripMenuItem createDirectoryButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem replaceFileButton;
        private System.Windows.Forms.ToolStripMenuItem extractFileButton;
        private System.Windows.Forms.ToolStripMenuItem removeFileButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem extractTheseFilesButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem viewFilePropertiesButton;
        private System.Windows.Forms.ToolStripMenuItem extractResourceButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem copyPathButton;
        private Panel panel1;
        private ToolStripMenuItem accessDirectoryMenuItem;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem1;
        private ToolStripMenuItem asNewToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem hashStripMenuItem;
        private ToolStripMenuItem creditsToolStripMenuItem;
        private MenuStrip menuStrip;
        private TextBox searchBox;
        private SplitContainer splitContainer1;
        private ToolStripStatusLabel currentDirectoryLabel;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem viewHexButton;
        private ToolStripMenuItem importDirectoryButton;
        private ToolStripMenuItem removeDirectoryButton;
        private ToolStripMenuItem reloadToolStripMenuItem;
        private ToolStripMenuItem changeSAVLanguageToolStripMenuItem;
    }
}

