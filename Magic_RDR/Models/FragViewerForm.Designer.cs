namespace Magic_RDR
{
    partial class FragViewerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FragViewerForm));
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageGeometries = new System.Windows.Forms.TabPage();
            this.labelBoundsMax = new System.Windows.Forms.Label();
            this.labelBoundsMin = new System.Windows.Forms.Label();
            this.labelPosition = new System.Windows.Forms.Label();
            this.labelPolygonsCount = new System.Windows.Forms.Label();
            this.labelVerticesCount = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.TableLayoutPanel();
            this.labelParentMeshCount = new System.Windows.Forms.Label();
            this.labelGeometryCount = new System.Windows.Forms.Label();
            this.tabPageTextures = new System.Windows.Forms.TabPage();
            this.labelCurrentMipmap = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.imageContainer = new System.Windows.Forms.PictureBox();
            this.listViewTextures = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPageSkeleton = new System.Windows.Forms.TabPage();
            this.listViewSkeleton = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.modelViewer = new ModelViewer.ModelView();
            this.checkBoxShowGrid = new System.Windows.Forms.CheckBox();
            this.comboBoxGridSize = new System.Windows.Forms.ComboBox();
            this.labelGridSize = new System.Windows.Forms.Label();
            this.checkBoxWireframe = new System.Windows.Forms.CheckBox();
            this.checkBoxShowBounds = new System.Windows.Forms.CheckBox();
            this.comboBoxSelectColor = new System.Windows.Forms.ComboBox();
            this.labelBackground = new System.Windows.Forms.Label();
            this.checkBoxVertices = new System.Windows.Forms.CheckBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.exportModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportCurrentTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rebuildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageGeometries.SuspendLayout();
            this.tabPageTextures.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageContainer)).BeginInit();
            this.tabPageSkeleton.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(0, 32);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.AutoScroll = true;
            this.splitContainer.Panel1.Controls.Add(this.tabControl);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.elementHost1);
            this.splitContainer.Size = new System.Drawing.Size(884, 533);
            this.splitContainer.SplitterDistance = 237;
            this.splitContainer.SplitterWidth = 5;
            this.splitContainer.TabIndex = 1;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageGeometries);
            this.tabControl.Controls.Add(this.tabPageTextures);
            this.tabControl.Controls.Add(this.tabPageSkeleton);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl.ItemSize = new System.Drawing.Size(60, 30);
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(237, 533);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageGeometries
            // 
            this.tabPageGeometries.AutoScroll = true;
            this.tabPageGeometries.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPageGeometries.Controls.Add(this.labelBoundsMax);
            this.tabPageGeometries.Controls.Add(this.labelBoundsMin);
            this.tabPageGeometries.Controls.Add(this.labelPosition);
            this.tabPageGeometries.Controls.Add(this.labelPolygonsCount);
            this.tabPageGeometries.Controls.Add(this.labelVerticesCount);
            this.tabPageGeometries.Controls.Add(this.panel1);
            this.tabPageGeometries.Controls.Add(this.labelParentMeshCount);
            this.tabPageGeometries.Controls.Add(this.labelGeometryCount);
            this.tabPageGeometries.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabPageGeometries.Location = new System.Drawing.Point(4, 34);
            this.tabPageGeometries.Name = "tabPageGeometries";
            this.tabPageGeometries.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGeometries.Size = new System.Drawing.Size(229, 495);
            this.tabPageGeometries.TabIndex = 0;
            this.tabPageGeometries.Text = "Geometries";
            this.tabPageGeometries.UseVisualStyleBackColor = true;
            // 
            // labelBoundsMax
            // 
            this.labelBoundsMax.AutoSize = true;
            this.labelBoundsMax.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBoundsMax.Location = new System.Drawing.Point(5, 168);
            this.labelBoundsMax.Name = "labelBoundsMax";
            this.labelBoundsMax.Size = new System.Drawing.Size(87, 15);
            this.labelBoundsMax.TabIndex = 8;
            this.labelBoundsMax.Text = "* Bounds Max :";
            // 
            // labelBoundsMin
            // 
            this.labelBoundsMin.AutoSize = true;
            this.labelBoundsMin.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBoundsMin.Location = new System.Drawing.Point(5, 145);
            this.labelBoundsMin.Name = "labelBoundsMin";
            this.labelBoundsMin.Size = new System.Drawing.Size(85, 15);
            this.labelBoundsMin.TabIndex = 7;
            this.labelBoundsMin.Text = "* Bounds Min :";
            // 
            // labelPosition
            // 
            this.labelPosition.AutoSize = true;
            this.labelPosition.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPosition.Location = new System.Drawing.Point(5, 122);
            this.labelPosition.Name = "labelPosition";
            this.labelPosition.Size = new System.Drawing.Size(64, 15);
            this.labelPosition.TabIndex = 6;
            this.labelPosition.Text = "* Position :";
            // 
            // labelPolygonsCount
            // 
            this.labelPolygonsCount.AutoSize = true;
            this.labelPolygonsCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPolygonsCount.Location = new System.Drawing.Point(5, 91);
            this.labelPolygonsCount.Name = "labelPolygonsCount";
            this.labelPolygonsCount.Size = new System.Drawing.Size(106, 15);
            this.labelPolygonsCount.TabIndex = 5;
            this.labelPolygonsCount.Text = "* Polygons Count :";
            // 
            // labelVerticesCount
            // 
            this.labelVerticesCount.AutoSize = true;
            this.labelVerticesCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVerticesCount.Location = new System.Drawing.Point(5, 68);
            this.labelVerticesCount.Name = "labelVerticesCount";
            this.labelVerticesCount.Size = new System.Drawing.Size(97, 15);
            this.labelVerticesCount.TabIndex = 4;
            this.labelVerticesCount.Text = "* Vertices Count :";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.OutsetDouble;
            this.panel1.ColumnCount = 1;
            this.panel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.panel1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.panel1.Location = new System.Drawing.Point(3, 200);
            this.panel1.Name = "panel1";
            this.panel1.RowCount = 1;
            this.panel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panel1.Size = new System.Drawing.Size(218, 287);
            this.panel1.TabIndex = 3;
            // 
            // labelParentMeshCount
            // 
            this.labelParentMeshCount.AutoSize = true;
            this.labelParentMeshCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelParentMeshCount.Location = new System.Drawing.Point(5, 37);
            this.labelParentMeshCount.Name = "labelParentMeshCount";
            this.labelParentMeshCount.Size = new System.Drawing.Size(123, 15);
            this.labelParentMeshCount.TabIndex = 1;
            this.labelParentMeshCount.Text = "* Parent Mesh Count :";
            // 
            // labelGeometryCount
            // 
            this.labelGeometryCount.AutoSize = true;
            this.labelGeometryCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelGeometryCount.Location = new System.Drawing.Point(5, 14);
            this.labelGeometryCount.Name = "labelGeometryCount";
            this.labelGeometryCount.Size = new System.Drawing.Size(109, 15);
            this.labelGeometryCount.TabIndex = 0;
            this.labelGeometryCount.Text = "* Geometry Count :";
            // 
            // tabPageTextures
            // 
            this.tabPageTextures.AutoScroll = true;
            this.tabPageTextures.Controls.Add(this.labelCurrentMipmap);
            this.tabPageTextures.Controls.Add(this.trackBar1);
            this.tabPageTextures.Controls.Add(this.imageContainer);
            this.tabPageTextures.Controls.Add(this.listViewTextures);
            this.tabPageTextures.Location = new System.Drawing.Point(4, 34);
            this.tabPageTextures.Name = "tabPageTextures";
            this.tabPageTextures.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTextures.Size = new System.Drawing.Size(229, 495);
            this.tabPageTextures.TabIndex = 1;
            this.tabPageTextures.Text = "Textures";
            this.tabPageTextures.UseVisualStyleBackColor = true;
            // 
            // labelCurrentMipmap
            // 
            this.labelCurrentMipmap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelCurrentMipmap.AutoSize = true;
            this.labelCurrentMipmap.Location = new System.Drawing.Point(3, 227);
            this.labelCurrentMipmap.Name = "labelCurrentMipmap";
            this.labelCurrentMipmap.Size = new System.Drawing.Size(0, 15);
            this.labelCurrentMipmap.TabIndex = 4;
            // 
            // trackBar1
            // 
            this.trackBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.trackBar1.AutoSize = false;
            this.trackBar1.Location = new System.Drawing.Point(0, 245);
            this.trackBar1.Maximum = 30;
            this.trackBar1.Minimum = 1;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(210, 22);
            this.trackBar1.TabIndex = 3;
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.trackBar1.Value = 1;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // imageContainer
            // 
            this.imageContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.imageContainer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.imageContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.imageContainer.Location = new System.Drawing.Point(0, 273);
            this.imageContainer.MaximumSize = new System.Drawing.Size(210, 210);
            this.imageContainer.Name = "imageContainer";
            this.imageContainer.Size = new System.Drawing.Size(210, 210);
            this.imageContainer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imageContainer.TabIndex = 0;
            this.imageContainer.TabStop = false;
            // 
            // listViewTextures
            // 
            this.listViewTextures.Alignment = System.Windows.Forms.ListViewAlignment.Default;
            this.listViewTextures.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listViewTextures.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader4,
            this.columnHeader5});
            this.listViewTextures.Dock = System.Windows.Forms.DockStyle.Top;
            this.listViewTextures.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewTextures.FullRowSelect = true;
            this.listViewTextures.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewTextures.HideSelection = false;
            this.listViewTextures.Location = new System.Drawing.Point(3, 3);
            this.listViewTextures.Name = "listViewTextures";
            this.listViewTextures.Size = new System.Drawing.Size(223, 219);
            this.listViewTextures.TabIndex = 2;
            this.listViewTextures.UseCompatibleStateImageBehavior = false;
            this.listViewTextures.View = System.Windows.Forms.View.Details;
            this.listViewTextures.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 125;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Size";
            this.columnHeader2.Width = 55;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Format";
            this.columnHeader4.Width = 50;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Mipmaps";
            this.columnHeader5.Width = 62;
            // 
            // tabPageSkeleton
            // 
            this.tabPageSkeleton.AutoScroll = true;
            this.tabPageSkeleton.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPageSkeleton.Controls.Add(this.listViewSkeleton);
            this.tabPageSkeleton.Location = new System.Drawing.Point(4, 34);
            this.tabPageSkeleton.Name = "tabPageSkeleton";
            this.tabPageSkeleton.Size = new System.Drawing.Size(229, 495);
            this.tabPageSkeleton.TabIndex = 2;
            this.tabPageSkeleton.Text = "Skeleton";
            this.tabPageSkeleton.UseVisualStyleBackColor = true;
            // 
            // listViewSkeleton
            // 
            this.listViewSkeleton.Alignment = System.Windows.Forms.ListViewAlignment.Default;
            this.listViewSkeleton.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewSkeleton.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader6,
            this.columnHeader7});
            this.listViewSkeleton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewSkeleton.FullRowSelect = true;
            this.listViewSkeleton.GridLines = true;
            this.listViewSkeleton.HideSelection = false;
            this.listViewSkeleton.Location = new System.Drawing.Point(0, 0);
            this.listViewSkeleton.Name = "listViewSkeleton";
            this.listViewSkeleton.Size = new System.Drawing.Size(227, 493);
            this.listViewSkeleton.TabIndex = 0;
            this.listViewSkeleton.UseCompatibleStateImageBehavior = false;
            this.listViewSkeleton.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Bones";
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Parent";
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Offset";
            this.columnHeader7.Width = 110;
            // 
            // elementHost1
            // 
            this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost1.Location = new System.Drawing.Point(0, 0);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(642, 533);
            this.elementHost1.TabIndex = 0;
            this.elementHost1.Child = this.modelViewer;
            // 
            // checkBoxShowGrid
            // 
            this.checkBoxShowGrid.AutoSize = true;
            this.checkBoxShowGrid.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxShowGrid.Location = new System.Drawing.Point(114, 7);
            this.checkBoxShowGrid.Name = "checkBoxShowGrid";
            this.checkBoxShowGrid.Size = new System.Drawing.Size(80, 17);
            this.checkBoxShowGrid.TabIndex = 20;
            this.checkBoxShowGrid.Text = "Show Grid";
            this.checkBoxShowGrid.UseVisualStyleBackColor = true;
            this.checkBoxShowGrid.CheckedChanged += new System.EventHandler(this.checkBoxShowGrid_CheckedChanged);
            // 
            // comboBoxGridSize
            // 
            this.comboBoxGridSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxGridSize.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxGridSize.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxGridSize.FormattingEnabled = true;
            this.comboBoxGridSize.Items.AddRange(new object[] {
            "3",
            "8",
            "10",
            "15",
            "30",
            "100",
            "500",
            "1000"});
            this.comboBoxGridSize.Location = new System.Drawing.Point(757, 5);
            this.comboBoxGridSize.Name = "comboBoxGridSize";
            this.comboBoxGridSize.Size = new System.Drawing.Size(121, 21);
            this.comboBoxGridSize.TabIndex = 19;
            this.comboBoxGridSize.Text = "8";
            this.comboBoxGridSize.SelectedIndexChanged += new System.EventHandler(this.comboBoxGridSize_SelectedIndexChanged);
            // 
            // labelGridSize
            // 
            this.labelGridSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelGridSize.AutoSize = true;
            this.labelGridSize.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelGridSize.Location = new System.Drawing.Point(694, 9);
            this.labelGridSize.Name = "labelGridSize";
            this.labelGridSize.Size = new System.Drawing.Size(58, 13);
            this.labelGridSize.TabIndex = 18;
            this.labelGridSize.Text = "Grid Size :";
            // 
            // checkBoxWireframe
            // 
            this.checkBoxWireframe.AutoSize = true;
            this.checkBoxWireframe.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxWireframe.Location = new System.Drawing.Point(304, 7);
            this.checkBoxWireframe.Name = "checkBoxWireframe";
            this.checkBoxWireframe.Size = new System.Drawing.Size(79, 17);
            this.checkBoxWireframe.TabIndex = 17;
            this.checkBoxWireframe.Text = "Wireframe";
            this.checkBoxWireframe.UseVisualStyleBackColor = true;
            this.checkBoxWireframe.CheckedChanged += new System.EventHandler(this.checkBoxWireframeChanged);
            // 
            // checkBoxShowBounds
            // 
            this.checkBoxShowBounds.AutoSize = true;
            this.checkBoxShowBounds.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxShowBounds.Location = new System.Drawing.Point(200, 7);
            this.checkBoxShowBounds.Name = "checkBoxShowBounds";
            this.checkBoxShowBounds.Size = new System.Drawing.Size(97, 17);
            this.checkBoxShowBounds.TabIndex = 10;
            this.checkBoxShowBounds.Text = "Show Bounds";
            this.checkBoxShowBounds.UseVisualStyleBackColor = true;
            this.checkBoxShowBounds.CheckedChanged += new System.EventHandler(this.checkBoxShowBounds_CheckedChanged);
            // 
            // comboBoxSelectColor
            // 
            this.comboBoxSelectColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxSelectColor.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxSelectColor.FormattingEnabled = true;
            this.comboBoxSelectColor.Items.AddRange(new object[] {
            "Color 1",
            "Color 2",
            "Default"});
            this.comboBoxSelectColor.Location = new System.Drawing.Point(558, 5);
            this.comboBoxSelectColor.Name = "comboBoxSelectColor";
            this.comboBoxSelectColor.Size = new System.Drawing.Size(130, 21);
            this.comboBoxSelectColor.TabIndex = 21;
            this.comboBoxSelectColor.Text = "Select Colors";
            this.comboBoxSelectColor.SelectedIndexChanged += new System.EventHandler(this.comboBoxSelectColor_SelectedIndexChanged);
            // 
            // labelBackground
            // 
            this.labelBackground.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelBackground.AutoSize = true;
            this.labelBackground.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBackground.Location = new System.Drawing.Point(477, 8);
            this.labelBackground.Name = "labelBackground";
            this.labelBackground.Size = new System.Drawing.Size(75, 13);
            this.labelBackground.TabIndex = 22;
            this.labelBackground.Text = "Background :";
            // 
            // checkBoxVertices
            // 
            this.checkBoxVertices.AutoSize = true;
            this.checkBoxVertices.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxVertices.Location = new System.Drawing.Point(391, 7);
            this.checkBoxVertices.Name = "checkBoxVertices";
            this.checkBoxVertices.Size = new System.Drawing.Size(65, 17);
            this.checkBoxVertices.TabIndex = 23;
            this.checkBoxVertices.Text = "Vertices";
            this.checkBoxVertices.UseVisualStyleBackColor = true;
            this.checkBoxVertices.CheckedChanged += new System.EventHandler(this.checkBoxVertices_CheckedChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolStrip1.Location = new System.Drawing.Point(2, 2);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(79, 29);
            this.toolStrip1.TabIndex = 25;
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportModelToolStripMenuItem,
            this.exportCurrentTextureToolStripMenuItem,
            this.exportAllTexturesToolStripMenuItem,
            this.importTextureToolStripMenuItem,
            this.rebuildToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Margin = new System.Windows.Forms.Padding(3);
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(70, 20);
            this.toolStripDropDownButton1.Text = "Export";
            // 
            // exportModelToolStripMenuItem
            // 
            this.exportModelToolStripMenuItem.Name = "exportModelToolStripMenuItem";
            this.exportModelToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exportModelToolStripMenuItem.Text = "Export Model";
            this.exportModelToolStripMenuItem.Click += new System.EventHandler(this.exportModelToolStripMenuItem_Click);
            // 
            // exportCurrentTextureToolStripMenuItem
            // 
            this.exportCurrentTextureToolStripMenuItem.Name = "exportCurrentTextureToolStripMenuItem";
            this.exportCurrentTextureToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exportCurrentTextureToolStripMenuItem.Text = "Export Texture";
            this.exportCurrentTextureToolStripMenuItem.Click += new System.EventHandler(this.exportCurrentTextureToolStripMenuItem_Click);
            // 
            // exportAllTexturesToolStripMenuItem
            // 
            this.exportAllTexturesToolStripMenuItem.Name = "exportAllTexturesToolStripMenuItem";
            this.exportAllTexturesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exportAllTexturesToolStripMenuItem.Text = "Export All Textures";
            this.exportAllTexturesToolStripMenuItem.Click += new System.EventHandler(this.exportAllTexturesToolStripMenuItem_Click);
            // 
            // importTextureToolStripMenuItem
            // 
            this.importTextureToolStripMenuItem.Name = "importTextureToolStripMenuItem";
            this.importTextureToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.importTextureToolStripMenuItem.Text = "Import Texture";
            this.importTextureToolStripMenuItem.Click += new System.EventHandler(this.importTextureToolStripMenuItem_Click);
            // 
            // rebuildToolStripMenuItem
            // 
            this.rebuildToolStripMenuItem.Name = "rebuildToolStripMenuItem";
            this.rebuildToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.rebuildToolStripMenuItem.Text = "Rebuild";
            this.rebuildToolStripMenuItem.Click += new System.EventHandler(this.rebuildToolStripMenuItem_Click);
            // 
            // FragViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.checkBoxVertices);
            this.Controls.Add(this.labelBackground);
            this.Controls.Add(this.comboBoxSelectColor);
            this.Controls.Add(this.comboBoxGridSize);
            this.Controls.Add(this.checkBoxShowGrid);
            this.Controls.Add(this.labelGridSize);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.checkBoxWireframe);
            this.Controls.Add(this.checkBoxShowBounds);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FragViewerForm";
            this.Text = "MagicRDR - Model Viewer";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ModelViewerForm_FormClosing);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPageGeometries.ResumeLayout(false);
            this.tabPageGeometries.PerformLayout();
            this.tabPageTextures.ResumeLayout(false);
            this.tabPageTextures.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageContainer)).EndInit();
            this.tabPageSkeleton.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private ModelViewer.ModelView modelViewer;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageGeometries;
        private System.Windows.Forms.TabPage tabPageTextures;
        private System.Windows.Forms.TabPage tabPageSkeleton;
        private System.Windows.Forms.Label labelGeometryCount;
        private System.Windows.Forms.Label labelParentMeshCount;
        private System.Windows.Forms.TableLayoutPanel panel1;
        private System.Windows.Forms.Label labelPolygonsCount;
        private System.Windows.Forms.Label labelVerticesCount;
        private System.Windows.Forms.Label labelPosition;
        private System.Windows.Forms.Label labelBoundsMin;
        private System.Windows.Forms.Label labelBoundsMax;
        private System.Windows.Forms.PictureBox imageContainer;
        private System.Windows.Forms.ListView listViewTextures;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.CheckBox checkBoxShowBounds;
        private System.Windows.Forms.ListView listViewSkeleton;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.CheckBox checkBoxWireframe;
        private System.Windows.Forms.ComboBox comboBoxGridSize;
        private System.Windows.Forms.Label labelGridSize;
        private System.Windows.Forms.CheckBox checkBoxShowGrid;
        private System.Windows.Forms.ComboBox comboBoxSelectColor;
        private System.Windows.Forms.Label labelBackground;
        private System.Windows.Forms.CheckBox checkBoxVertices;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem exportModelToolStripMenuItem;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label labelCurrentMipmap;
        private System.Windows.Forms.ToolStripMenuItem exportCurrentTextureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importTextureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rebuildToolStripMenuItem;
    }
}