
namespace Magic_RDR
{
    partial class VolumeViewerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VolumeViewerForm));
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.modelViewer = new ModelViewer.ModelView();
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
            this.tabPageMapping = new System.Windows.Forms.TabPage();
            this.listViewShaders = new System.Windows.Forms.ListView();
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPageShaders = new System.Windows.Forms.TabPage();
            this.treeViewShaders = new System.Windows.Forms.TreeView();
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
            this.exportToXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.importTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setModelPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.saveRebuildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageGeometries.SuspendLayout();
            this.tabPageTextures.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageContainer)).BeginInit();
            this.tabPageMapping.SuspendLayout();
            this.tabPageShaders.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // elementHost1
            // 
            this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost1.Location = new System.Drawing.Point(0, 0);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(642, 533);
            this.elementHost1.TabIndex = 1;
            this.elementHost1.Child = this.modelViewer;
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
            this.tabControl.Controls.Add(this.tabPageMapping);
            this.tabControl.Controls.Add(this.tabPageShaders);
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
            this.listViewTextures.GridLines = true;
            this.listViewTextures.HideSelection = false;
            this.listViewTextures.Location = new System.Drawing.Point(3, 3);
            this.listViewTextures.Name = "listViewTextures";
            this.listViewTextures.Size = new System.Drawing.Size(223, 219);
            this.listViewTextures.TabIndex = 2;
            this.listViewTextures.UseCompatibleStateImageBehavior = false;
            this.listViewTextures.View = System.Windows.Forms.View.Details;
            this.listViewTextures.SelectedIndexChanged += new System.EventHandler(this.listViewTextures_SelectedIndexChanged);
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
            // tabPageMapping
            // 
            this.tabPageMapping.Controls.Add(this.listViewShaders);
            this.tabPageMapping.Location = new System.Drawing.Point(4, 34);
            this.tabPageMapping.Name = "tabPageMapping";
            this.tabPageMapping.Size = new System.Drawing.Size(229, 495);
            this.tabPageMapping.TabIndex = 2;
            this.tabPageMapping.Text = "Mapping";
            this.tabPageMapping.UseVisualStyleBackColor = true;
            // 
            // listViewShaders
            // 
            this.listViewShaders.Alignment = System.Windows.Forms.ListViewAlignment.Default;
            this.listViewShaders.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listViewShaders.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader9,
            this.columnHeader11,
            this.columnHeader6});
            this.listViewShaders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewShaders.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewShaders.FullRowSelect = true;
            this.listViewShaders.GridLines = true;
            this.listViewShaders.HideSelection = false;
            this.listViewShaders.Location = new System.Drawing.Point(0, 0);
            this.listViewShaders.Name = "listViewShaders";
            this.listViewShaders.Size = new System.Drawing.Size(229, 495);
            this.listViewShaders.TabIndex = 3;
            this.listViewShaders.UseCompatibleStateImageBehavior = false;
            this.listViewShaders.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Geometry";
            this.columnHeader9.Width = 68;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Texture";
            this.columnHeader11.Width = 85;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Embedded";
            this.columnHeader6.Width = 73;
            // 
            // tabPageShaders
            // 
            this.tabPageShaders.Controls.Add(this.treeViewShaders);
            this.tabPageShaders.Location = new System.Drawing.Point(4, 34);
            this.tabPageShaders.Name = "tabPageShaders";
            this.tabPageShaders.Size = new System.Drawing.Size(229, 495);
            this.tabPageShaders.TabIndex = 3;
            this.tabPageShaders.Text = "Shaders";
            this.tabPageShaders.UseVisualStyleBackColor = true;
            // 
            // treeViewShaders
            // 
            this.treeViewShaders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewShaders.Location = new System.Drawing.Point(0, 0);
            this.treeViewShaders.Name = "treeViewShaders";
            this.treeViewShaders.Size = new System.Drawing.Size(229, 495);
            this.treeViewShaders.TabIndex = 0;
            // 
            // checkBoxWireframe
            // 
            this.checkBoxWireframe.AutoSize = true;
            this.checkBoxWireframe.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxWireframe.Location = new System.Drawing.Point(187, 8);
            this.checkBoxWireframe.Name = "checkBoxWireframe";
            this.checkBoxWireframe.Size = new System.Drawing.Size(79, 17);
            this.checkBoxWireframe.TabIndex = 17;
            this.checkBoxWireframe.Text = "Wireframe";
            this.checkBoxWireframe.UseVisualStyleBackColor = true;
            this.checkBoxWireframe.CheckedChanged += new System.EventHandler(this.checkBoxWireframe_CheckedChanged);
            // 
            // checkBoxShowBounds
            // 
            this.checkBoxShowBounds.AutoSize = true;
            this.checkBoxShowBounds.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxShowBounds.Location = new System.Drawing.Point(84, 8);
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
            this.comboBoxSelectColor.Location = new System.Drawing.Point(742, 5);
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
            this.labelBackground.Location = new System.Drawing.Point(661, 9);
            this.labelBackground.Name = "labelBackground";
            this.labelBackground.Size = new System.Drawing.Size(75, 13);
            this.labelBackground.TabIndex = 22;
            this.labelBackground.Text = "Background :";
            // 
            // checkBoxVertices
            // 
            this.checkBoxVertices.AutoSize = true;
            this.checkBoxVertices.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxVertices.Location = new System.Drawing.Point(272, 8);
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
            this.exportToXMLToolStripMenuItem,
            this.toolStripSeparator1,
            this.importTextureToolStripMenuItem,
            this.setModelPositionToolStripMenuItem,
            this.toolStripSeparator2,
            this.saveRebuildToolStripMenuItem});
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Margin = new System.Windows.Forms.Padding(3);
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(62, 19);
            this.toolStripDropDownButton1.Text = "Options";
            // 
            // exportModelToolStripMenuItem
            // 
            this.exportModelToolStripMenuItem.Name = "exportModelToolStripMenuItem";
            this.exportModelToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.exportModelToolStripMenuItem.Text = "Export Model";
            this.exportModelToolStripMenuItem.Click += new System.EventHandler(this.exportModelToolStripMenuItem_Click);
            // 
            // exportCurrentTextureToolStripMenuItem
            // 
            this.exportCurrentTextureToolStripMenuItem.Name = "exportCurrentTextureToolStripMenuItem";
            this.exportCurrentTextureToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.exportCurrentTextureToolStripMenuItem.Text = "Export Texture";
            this.exportCurrentTextureToolStripMenuItem.Click += new System.EventHandler(this.exportCurrentTextureToolStripMenuItem_Click);
            // 
            // exportAllTexturesToolStripMenuItem
            // 
            this.exportAllTexturesToolStripMenuItem.Name = "exportAllTexturesToolStripMenuItem";
            this.exportAllTexturesToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.exportAllTexturesToolStripMenuItem.Text = "Export All Textures";
            this.exportAllTexturesToolStripMenuItem.Click += new System.EventHandler(this.exportAllTexturesToolStripMenuItem_Click);
            // 
            // exportToXMLToolStripMenuItem
            // 
            this.exportToXMLToolStripMenuItem.Name = "exportToXMLToolStripMenuItem";
            this.exportToXMLToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.exportToXMLToolStripMenuItem.Text = "Export to XML";
            this.exportToXMLToolStripMenuItem.Click += new System.EventHandler(this.exportToXMLToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(170, 6);
            // 
            // importTextureToolStripMenuItem
            // 
            this.importTextureToolStripMenuItem.Name = "importTextureToolStripMenuItem";
            this.importTextureToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.importTextureToolStripMenuItem.Text = "Import Texture";
            this.importTextureToolStripMenuItem.Click += new System.EventHandler(this.importTextureToolStripMenuItem_Click);
            // 
            // setModelPositionToolStripMenuItem
            // 
            this.setModelPositionToolStripMenuItem.Name = "setModelPositionToolStripMenuItem";
            this.setModelPositionToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.setModelPositionToolStripMenuItem.Text = "Set Model Position";
            this.setModelPositionToolStripMenuItem.Click += new System.EventHandler(this.setModelPositionToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(170, 6);
            // 
            // saveRebuildToolStripMenuItem
            // 
            this.saveRebuildToolStripMenuItem.Name = "saveRebuildToolStripMenuItem";
            this.saveRebuildToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.saveRebuildToolStripMenuItem.Text = "Rebuild";
            this.saveRebuildToolStripMenuItem.Click += new System.EventHandler(this.saveRebuildToolStripMenuItem_Click);
            // 
            // VolumeViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.checkBoxVertices);
            this.Controls.Add(this.labelBackground);
            this.Controls.Add(this.comboBoxSelectColor);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.checkBoxWireframe);
            this.Controls.Add(this.checkBoxShowBounds);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "VolumeViewerForm";
            this.Text = "MagicRDR - Model Viewer";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VolumeViewerForm_FormClosing);
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
            this.tabPageMapping.ResumeLayout(false);
            this.tabPageShaders.ResumeLayout(false);
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
        private System.Windows.Forms.Label labelPolygonsCount;
        private System.Windows.Forms.Label labelVerticesCount;
        private System.Windows.Forms.Label labelPosition;
        private System.Windows.Forms.Label labelBoundsMin;
        private System.Windows.Forms.Label labelBoundsMax;
        private System.Windows.Forms.Label labelGeometryCount;
        private System.Windows.Forms.Label labelParentMeshCount;
        private System.Windows.Forms.TableLayoutPanel panel1;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label labelCurrentMipmap;
        private System.Windows.Forms.PictureBox imageContainer;
        private System.Windows.Forms.ListView listViewTextures;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.CheckBox checkBoxWireframe;
        private System.Windows.Forms.ComboBox comboBoxSelectColor;
        private System.Windows.Forms.Label labelBackground;
        private System.Windows.Forms.CheckBox checkBoxVertices;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem exportModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportCurrentTextureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllTexturesToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxShowBounds;
        private System.Windows.Forms.TabPage tabPageMapping;
        private System.Windows.Forms.ListView listViewShaders;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ToolStripMenuItem exportToXMLToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPageShaders;
        private System.Windows.Forms.TreeView treeViewShaders;
        private System.Windows.Forms.ToolStripMenuItem importTextureToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem saveRebuildToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setModelPositionToolStripMenuItem;
    }
}