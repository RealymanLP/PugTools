namespace PugTools {
  partial class NodeBrowser {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NodeBrowser));
      // Background Workers
      this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
      this.backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
      this.backgroundWorker3 = new System.ComponentModel.BackgroundWorker();
      // Buttons
      this.btnExtractPath = new System.Windows.Forms.Button();
      this.btnClearSearch = new System.Windows.Forms.Button();
      this.btnExtract = new System.Windows.Forms.Button();
      this.btnFileFinder = new System.Windows.Forms.Button();
      this.btnFindNext = new System.Windows.Forms.Button();
      this.btnSearch = new System.Windows.Forms.Button();
      this.btnToggleCollapse = new System.Windows.Forms.Button();
      // Context Menu Items
      this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
      // Data Grid Views
      this.dataGridView1 = new System.Windows.Forms.DataGridView();
      // Image Lists
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      // Labels
      this.lblExtractPath = new System.Windows.Forms.Label();
      // Picture Box
      this.loadingSwirl1 = new System.Windows.Forms.PictureBox();
      // Split Containers
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.splitContainer3 = new System.Windows.Forms.SplitContainer();
      this.splitContainer4 = new System.Windows.Forms.SplitContainer();
      // Status Strip
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      // Text Boxes
      this.txtExtractPath = new System.Windows.Forms.TextBox();
      this.txtSearch = new System.Windows.Forms.TextBox();
      // Tool Strip Controls
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
      this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
      this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
      this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
      this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
      // Tree View Controls
      this.treeViewFast1 = new TreeViewFast.Controls.TreeViewFast();
      this.treeViewGrid1 = new BrightIdeasSoftware.TreeListView();
      this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
      this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
      this.olvColumn3 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
      //
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
      this.splitContainer4.Panel1.SuspendLayout();
      this.splitContainer4.Panel2.SuspendLayout();
      this.splitContainer4.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
      this.splitContainer3.Panel1.SuspendLayout();
      this.splitContainer3.Panel2.SuspendLayout();
      this.splitContainer3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.loadingSwirl1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.treeViewGrid1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
      this.contextMenuStrip1.SuspendLayout();
      this.contextMenuStrip2.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      this.toolStrip1.SuspendLayout();
      this.SuspendLayout();

      /////////////////////////////////////////////////////////////////////////////////////////////
      // SPLIT CONTAINERS

      //
      // splitContainer1
      //
      this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer1.IsSplitterFixed = true;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      this.splitContainer1.Size = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size;
      this.splitContainer1.SplitterDistance = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height - 22;
      this.splitContainer1.SplitterWidth = 1;
      this.splitContainer1.TabIndex = 0;
      this.splitContainer1.TabStop = false;
      //
      // splitContainer1.Panel1
      //
      this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
      //
      // splitContainer1.Panel2
      //
      this.splitContainer1.Panel2.Controls.Add(this.statusStrip1);
      //
      // splitContainer2
      //
      this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splitContainer2.Location = new System.Drawing.Point(0, 0);
      this.splitContainer2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
      this.splitContainer2.Name = "splitContainer2";
      this.splitContainer2.Size = new System.Drawing.Size(this.splitContainer1.Width, this.splitContainer1.Panel1.Height);
      this.splitContainer2.SplitterDistance = 405;
      this.splitContainer2.SplitterWidth = 1;
      this.splitContainer2.TabIndex = 0;
      this.splitContainer2.TabStop = false;
      //
      // splitContainer2.Panel1
      //
      this.splitContainer2.Panel1.Controls.Add(this.btnClearSearch);
      this.splitContainer2.Panel1.Controls.Add(this.btnFindNext);
      this.splitContainer2.Panel1.Controls.Add(this.btnSearch);
      this.splitContainer2.Panel1.Controls.Add(this.txtSearch);
      this.splitContainer2.Panel1.Controls.Add(this.treeViewFast1);
      //
      // splitContainer2.Panel2
      //
      this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
      //
      // splitContainer3
      //
      this.splitContainer3.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer3.Location = new System.Drawing.Point(0, 0);
      this.splitContainer3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
      this.splitContainer3.Name = "splitContainer3";
      this.splitContainer3.Size = new System.Drawing.Size(this.splitContainer2.Panel2.Width - 350, this.splitContainer2.Height);
      this.splitContainer3.SplitterDistance = this.splitContainer3.Width - 350;
      this.splitContainer3.SplitterWidth = 1;
      this.splitContainer3.TabIndex = 0;
      this.splitContainer3.TabStop = false;
      //
      // splitContainer3.Panel1
      //
      this.splitContainer3.Panel1.AutoScroll = true;
      this.splitContainer3.Panel1.Controls.Add(this.loadingSwirl1);
      this.splitContainer3.Panel1.Controls.Add(this.treeViewGrid1);
      this.splitContainer3.Panel1.Controls.Add(this.toolStrip1);
      //
      // splitContainer3.Panel2
      //
      this.splitContainer3.Panel2.Controls.Add(this.splitContainer4);
      //
      // splitContainer4
      //
      this.splitContainer4.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer4.Location = new System.Drawing.Point(0, 0);
      this.splitContainer4.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
      this.splitContainer4.Name = "splitContainer4";
      this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
      this.splitContainer4.Size = new System.Drawing.Size(350, this.splitContainer3.Height);
      this.splitContainer4.SplitterDistance = splitContainer4.Height - 160;
      this.splitContainer4.SplitterWidth = 1;
      this.splitContainer4.TabIndex = 0;
      this.splitContainer4.TabStop = false;
      //
      // splitContainer4.Panel1
      //
      this.splitContainer4.Panel1.Controls.Add(this.dataGridView1);
      //
      // splitContainer4.Panel2
      //
      this.splitContainer4.Panel2.Controls.Add(this.lblExtractPath);
      this.splitContainer4.Panel2.Controls.Add(this.txtExtractPath);
      this.splitContainer4.Panel2.Controls.Add(this.btnExtractPath);
      this.splitContainer4.Panel2.Controls.Add(this.btnExtract);
      this.splitContainer4.Panel2.Controls.Add(this.btnToggleCollapse);
      this.splitContainer4.Panel2.Controls.Add(this.btnFileFinder);

      /////////////////////////////////////////////////////////////////////////////////////////////
      // LEFT PANEL

      // 
      // txtSearch
      // 
      this.txtSearch.Enabled = false;
      this.txtSearch.Location = new System.Drawing.Point(34, 8);
      this.txtSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.txtSearch.Name = "txtSearch";
      this.txtSearch.Size = new System.Drawing.Size(335, 23);
      this.txtSearch.TabIndex = 0;
      this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtSearchKeyDown);
      // 
      // btnSearch
      // 
      this.btnSearch.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnSearch.Enabled = false;
      this.btnSearch.Location = new System.Drawing.Point(33, 37);
      this.btnSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnSearch.Name = "btnSearch";
      this.btnSearch.Size = new System.Drawing.Size(108, 27);
      this.btnSearch.TabIndex = 1;
      this.btnSearch.Text = "Search";
      this.btnSearch.UseVisualStyleBackColor = true;
      this.btnSearch.Click += new System.EventHandler(this.BtnSearchClick);
      // 
      // btnFindNext
      // 
      this.btnFindNext.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnFindNext.Enabled = false;
      this.btnFindNext.Location = new System.Drawing.Point(147, 37);
      this.btnFindNext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnFindNext.Name = "btnFindNext";
      this.btnFindNext.Size = new System.Drawing.Size(108, 27);
      this.btnFindNext.TabIndex = 2;
      this.btnFindNext.Text = "Find Next";
      this.btnFindNext.UseVisualStyleBackColor = true;
      this.btnFindNext.Click += new System.EventHandler(this.BtnFindNextClick);
      // 
      // btnClearSearch
      // 
      this.btnClearSearch.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnClearSearch.Enabled = false;
      this.btnClearSearch.Location = new System.Drawing.Point(261, 37);
      this.btnClearSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnClearSearch.Name = "btnClearSearch";
      this.btnClearSearch.Size = new System.Drawing.Size(108, 27);
      this.btnClearSearch.TabIndex = 3;
      this.btnClearSearch.Text = "Clear";
      this.btnClearSearch.UseVisualStyleBackColor = true;
      this.btnClearSearch.Click += new System.EventHandler(this.BtnClearSearchClick);
      // 
      // treeViewFast1
      // 
      this.treeViewFast1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewFast1AfterSelect);
      this.treeViewFast1.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.treeViewFast1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.treeViewFast1.ImageIndex = 0;
      this.treeViewFast1.ImageList = this.imageList1;
      this.treeViewFast1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TreeViewFast1KeyDown);
      this.treeViewFast1.Location = new System.Drawing.Point(0, 0);
      this.treeViewFast1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.treeViewFast1.MouseHover += new System.EventHandler(this.TreeViewFast1MouseHover);
      this.treeViewFast1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TreeViewFast1MouseUp);
      this.treeViewFast1.Name = "treeViewFast1";
      this.treeViewFast1.SelectedImageIndex = 0;
      this.treeViewFast1.Size = new System.Drawing.Size(350, this.splitContainer2.Panel1.Height - 92);
      this.treeViewFast1.TabIndex = 0;
      this.treeViewFast1.Visible = false;
      // 
      // imageList1
      // 
      this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "COMPUTER.ICO");
      this.imageList1.Images.SetKeyName(1, "Folder.ico");
      this.imageList1.Images.SetKeyName(2, "textdoc.ico");
      // 
      // contextMenuStrip1
      // 
      this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.toolStripMenuItem1
      });
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.contextMenuStrip1.Size = new System.Drawing.Size(111, 26);
      // 
      // contextMenuStrip2
      // 
      this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.toolStripMenuItem2
      });
      this.contextMenuStrip2.Name = "contextMenuStrip2";
      this.contextMenuStrip2.Size = new System.Drawing.Size(136, 26);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(110, 22);
      this.toolStripMenuItem1.Text = "Extract";
      this.toolStripMenuItem1.Click += new System.EventHandler(this.ToolStripMenuItem1Click);
      // 
      // tooStripMenuItem2
      // 
      this.toolStripMenuItem2.Name = "toolStripMenuItem2";
      this.toolStripMenuItem2.Size = new System.Drawing.Size(135, 22);
      this.toolStripMenuItem2.Text = "Go to Node";
      this.toolStripMenuItem2.Click += new System.EventHandler(this.ToolStripMenuItem2Click);

      /////////////////////////////////////////////////////////////////////////////////////////////
      // CENTER PANEL

      // 
      // loadingSwirl1
      // 
      this.loadingSwirl1.BackColor = System.Drawing.Color.White;
      this.loadingSwirl1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.loadingSwirl1.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.loadingSwirl1.Cursor = System.Windows.Forms.Cursors.Default;
      this.loadingSwirl1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.loadingSwirl1.Image = global::PugTools.Properties.Resources.LoadingSwirl;
      this.loadingSwirl1.Location = new System.Drawing.Point(0, 0);
      this.loadingSwirl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.loadingSwirl1.Name = "loadingSwirl1";
      this.loadingSwirl1.Size = this.splitContainer3.Panel1.Size;
      this.loadingSwirl1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.loadingSwirl1.TabIndex = 3;
      this.loadingSwirl1.TabStop = false;
      this.loadingSwirl1.Visible = false;
      //
      // toolStrip1
      //
      this.toolStrip1.CanOverflow = false;
      this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Top;
      this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.toolStripLabel1,
        this.toolStripTextBox1,
        this.toolStripButton1,
        this.toolStripButton2,
        this.toolStripButton3
      });
      this.toolStrip1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Visible = false;
      //
      // toolStripLabel1
      //
      this.toolStripLabel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.toolStripLabel1.Name = "toolStripLabel1";
      this.toolStripLabel1.Text = "Filter:";
      //
      // toolStripTextBox1
      //
      this.toolStripTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.toolStripTextBox1.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.toolStripTextBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ToolStripTextBox1KeyDown);
      this.toolStripTextBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.toolStripTextBox1.Name = "toolStripTextBox1";
      this.toolStripTextBox1.Size = new System.Drawing.Size(200, 25);
      //
      // toolStripButton1
      //
      this.toolStripButton1.BackColor = System.Drawing.SystemColors.ControlLight;
      this.toolStripButton1.Click += new System.EventHandler(this.ToolStripButton1Click);
      this.toolStripButton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.toolStripButton1.Name = "toolStripButton1";
      this.toolStripButton1.Text = "Filter";
      this.toolStripButton1.Size = new System.Drawing.Size(50, 25);
      //
      // toolStripButton2
      //
      this.toolStripButton2.BackColor = System.Drawing.SystemColors.ControlLight;
      this.toolStripButton2.Click += new System.EventHandler(this.ToolStripButton2Click);
      this.toolStripButton2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.toolStripButton2.Name = "toolStripButton2";
      this.toolStripButton2.Text = "Clear";
      this.toolStripButton2.Size = new System.Drawing.Size(50, 25);
      //
      // toolStripButton3
      //
      this.toolStripButton3.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
      this.toolStripButton3.Click += new System.EventHandler(this.ToolStripButton3Click);
      this.toolStripButton3.Font = new System.Drawing.Font("Webdings", 13F);
      this.toolStripButton3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.toolStripButton3.MouseEnter += new System.EventHandler(this.ToolStripButton3MouseEnter);
      this.toolStripButton3.MouseLeave += new System.EventHandler(this.ToolStripButton3MouseLeave);
      this.toolStripButton3.Name = "toolStripButton3";
      this.toolStripButton3.Text = "r";
      this.toolStripButton3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.toolStripButton3.ToolTipText = "Close";
      this.toolStripButton3.Size = new System.Drawing.Size(22, 22);
      //
      // treeViewGrid1
      //
      this.treeViewGrid1.AllColumns.Add(this.olvColumn1);
      this.treeViewGrid1.AllColumns.Add(this.olvColumn2);
      this.treeViewGrid1.AllColumns.Add(this.olvColumn3);
      this.treeViewGrid1.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.treeViewGrid1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        this.olvColumn1,
        this.olvColumn2,
        this.olvColumn3
      });
      this.treeViewGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeViewGrid1.GridLines = true;
      this.treeViewGrid1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TreeViewGrid1KeyDown);
      this.treeViewGrid1.Location = new System.Drawing.Point(0, 0);
      this.treeViewGrid1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
      this.treeViewGrid1.MouseHover += new System.EventHandler(this.TreeViewGrid1MouseHover);
      this.treeViewGrid1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TreeViewGrid1MouseUp);
      this.treeViewGrid1.Name = "treeViewGrid1";
      this.treeViewGrid1.OwnerDraw = true;
      this.treeViewGrid1.ShowGroups = false;
      this.treeViewGrid1.SelectedIndexChanged += new System.EventHandler(this.TreeViewGrid1SelectedIndexChanged);
      this.treeViewGrid1.Size = this.splitContainer3.Panel1.Size;
      this.treeViewGrid1.TabIndex = 4;
      this.treeViewGrid1.UseCompatibleStateImageBehavior = false;
      this.treeViewGrid1.UseFiltering = true;
      this.treeViewGrid1.View = System.Windows.Forms.View.Details;
      this.treeViewGrid1.VirtualMode = true;
      this.treeViewGrid1.Visible = false;
      // 
      // olvColumn1
      // 
      this.olvColumn1.AspectName = nameof(NodeListItem.DisplayName);
      this.olvColumn1.CellPadding = null;
      this.olvColumn1.Text = "Name";
      this.olvColumn1.Width = this.splitContainer3.Panel1.Width / 3;
      // 
      // olvColumn2
      // 
      this.olvColumn2.AspectName = nameof(NodeListItem.Type);
      this.olvColumn2.CellPadding = null;
      this.olvColumn2.Text = "Type";
      this.olvColumn2.Width = this.splitContainer3.Panel1.Width / 3;
      // 
      // olvColumn3
      // 
      this.olvColumn3.AspectName = nameof(NodeListItem.DisplayValue);
      this.olvColumn3.CellPadding = null;
      this.olvColumn3.Text = "Value";
      this.olvColumn3.Width = this.splitContainer3.Panel1.Width / 3;
      this.olvColumn3.FillsFreeSpace = true;

      /////////////////////////////////////////////////////////////////////////////////////////////
      //  RIGHT PANEL

      // 
      // dataGridView1
      // 
      this.dataGridView1.AllowUserToAddRows = false;
      this.dataGridView1.AllowUserToDeleteRows = false;
      this.dataGridView1.AllowUserToResizeRows = false;
      this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView1.Cursor = System.Windows.Forms.Cursors.Default;
      this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView1.Enabled = false;
      this.dataGridView1.Location = new System.Drawing.Point(0, 0);
      this.dataGridView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.dataGridView1.Name = "dataGridView1";
      this.dataGridView1.RowHeadersVisible = false;
      this.dataGridView1.Size = this.splitContainer4.Panel1.Size;
      this.dataGridView1.TabIndex = 0;
      this.dataGridView1.VirtualMode = true;
      // 
      // lblExtractPath
      // 
      this.lblExtractPath.AutoSize = true;
      this.lblExtractPath.Cursor = System.Windows.Forms.Cursors.Default;
      this.lblExtractPath.Location = new System.Drawing.Point(0, 5);
      this.lblExtractPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.lblExtractPath.Name = "lblExtractPath";
      this.lblExtractPath.Size = new System.Drawing.Size(70, 15);
      this.lblExtractPath.TabIndex = 3;
      this.lblExtractPath.Text = "Extract Path";
      // 
      // txtExtractPath
      // 
      this.txtExtractPath.Enabled = false;
      this.txtExtractPath.Location = new System.Drawing.Point(4, 23);
      this.txtExtractPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.txtExtractPath.Name = "txtExtractPath";
      this.txtExtractPath.Size = new System.Drawing.Size(240, 23);
      this.txtExtractPath.TabIndex = 2;
      // 
      // btnExtractPath
      // 
      this.btnExtractPath.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnExtractPath.Enabled = false;
      this.btnExtractPath.Location = new System.Drawing.Point(250, 22);
      this.btnExtractPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnExtractPath.Name = "btnExtractPath";
      this.btnExtractPath.Size = new System.Drawing.Size(27, 25);
      this.btnExtractPath.TabIndex = 5;
      this.btnExtractPath.Text = "...";
      this.btnExtractPath.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.btnExtractPath.UseVisualStyleBackColor = true;
      this.btnExtractPath.Click += new System.EventHandler(this.BtnExtractPathClick);
      // 
      // btnExtract
      // 
      this.btnExtract.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnExtract.Enabled = false;
      this.btnExtract.Location = new System.Drawing.Point(4, 53);
      this.btnExtract.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnExtract.Name = "btnExtract";
      this.btnExtract.Size = new System.Drawing.Size(160, 27);
      this.btnExtract.TabIndex = 1;
      this.btnExtract.Text = "Extract Node";
      this.btnExtract.UseVisualStyleBackColor = true;
      this.btnExtract.Click += new System.EventHandler(this.BtnExtractClick);
      // 
      // btnToggleCollapse
      // 
      this.btnToggleCollapse.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnToggleCollapse.Enabled = false;
      this.btnToggleCollapse.Location = new System.Drawing.Point(4, 87);
      this.btnToggleCollapse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnToggleCollapse.Name = "btnToggleCollapse";
      this.btnToggleCollapse.Size = new System.Drawing.Size(160, 27);
      this.btnToggleCollapse.TabIndex = 6;
      this.btnToggleCollapse.Text = "Collapse Child Nodes";
      this.btnToggleCollapse.UseVisualStyleBackColor = true;
      this.btnToggleCollapse.Click += new System.EventHandler(this.BtnToggleNodesClick);
      // 
      // btnFileFinder
      // 
      this.btnFileFinder.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnFileFinder.Enabled = false;
      this.btnFileFinder.Location = new System.Drawing.Point(4, 121);
      this.btnFileFinder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnFileFinder.Name = "btnFileFinder";
      this.btnFileFinder.Size = new System.Drawing.Size(160, 27);
      this.btnFileFinder.TabIndex = 7;
      this.btnFileFinder.Text = "Run File Name Finder";
      this.btnFileFinder.UseVisualStyleBackColor = true;
      this.btnFileFinder.Click += new System.EventHandler(this.BtnFileFinderClick);

      /////////////////////////////////////////////////////////////////////////////////////////////
      // STATUS BAR

      // 
      // statusStrip1
      // 
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.toolStripStatusLabel1,
        this.toolStripProgressBar1,
      });
      this.statusStrip1.Location = new System.Drawing.Point(0, 1015);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
      this.statusStrip1.Size = new System.Drawing.Size(1196, 22);
      this.statusStrip1.TabIndex = 1;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // toolStripStatusLabel1
      // 
      this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
      this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
      // 
      // toolStripProgressBar1
      // 
      this.toolStripProgressBar1.ForeColor = System.Drawing.Color.Lime;
      this.toolStripProgressBar1.Name = "toolStripProgressBar1";
      this.toolStripProgressBar1.Size = new System.Drawing.Size(117, 18);
      this.toolStripProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
      this.toolStripProgressBar1.Visible = false;

      /////////////////////////////////////////////////////////////////////////////////////////////
      // BACKGROUND

      // 
      // backgroundWorker1
      // 
      this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker1Run);
      this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker1Completed);
      // 
      // backgroundWorker2
      // 
      this.backgroundWorker2.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker2Run);
      this.backgroundWorker2.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundWorker2Progress);
      this.backgroundWorker2.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker2Completed);
      this.backgroundWorker2.WorkerReportsProgress = true;
      //
      // backgroundWorker3
      //
      this.backgroundWorker3.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker3Run);
      this.backgroundWorker3.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker3Completed);

      /////////////////////////////////////////////////////////////////////////////////////////////
      // FORM

      // 
      // NodeBrowser
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1904, 1041);
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.NodeBrowserFormClosed);
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NodeBrowserFormClosing);
      this.Controls.Add(this.splitContainer1);
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "NodeBrowser";
      this.Resize += new System.EventHandler(this.NodeBrowserFormResize);
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Node Browser";
      this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
      //
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
      this.splitContainer2.ResumeLayout(false);
      this.splitContainer3.Panel1.ResumeLayout(false);
      this.splitContainer3.Panel1.PerformLayout();
      this.splitContainer3.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
      this.splitContainer3.ResumeLayout(false);
      this.splitContainer4.Panel1.ResumeLayout(false);
      this.splitContainer4.Panel1.PerformLayout();
      this.splitContainer4.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
      this.splitContainer4.ResumeLayout(false);
      //
      ((System.ComponentModel.ISupportInitialize)(this.loadingSwirl1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.treeViewGrid1)).EndInit();
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.contextMenuStrip1.ResumeLayout(false);
      this.contextMenuStrip2.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    // Background Workers
    private System.ComponentModel.BackgroundWorker backgroundWorker1;
    private System.ComponentModel.BackgroundWorker backgroundWorker2;
    private System.ComponentModel.BackgroundWorker backgroundWorker3;
    // Buttons
    private System.Windows.Forms.Button btnExtractPath;
    private System.Windows.Forms.Button btnClearSearch;
    private System.Windows.Forms.Button btnExtract;
    private System.Windows.Forms.Button btnFileFinder;
    private System.Windows.Forms.Button btnFindNext;
    private System.Windows.Forms.Button btnSearch;
    private System.Windows.Forms.Button btnToggleCollapse;
    // Context Menu Items
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
    // Data Grid View
    private System.Windows.Forms.DataGridView dataGridView1;
    // Image List
    private System.Windows.Forms.ImageList imageList1;
    // Labels
    private System.Windows.Forms.Label lblExtractPath;
    // Picture Box
    private System.Windows.Forms.PictureBox loadingSwirl1;
    // Split Containers
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.SplitContainer splitContainer3;
    private System.Windows.Forms.SplitContainer splitContainer4;
    // Status Strip
    private System.Windows.Forms.StatusStrip statusStrip1;
    // Text
    private System.Windows.Forms.TextBox txtExtractPath;
    private System.Windows.Forms.TextBox txtSearch;
    // Tool Strip Controls
    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.ToolStripButton toolStripButton1;
    private System.Windows.Forms.ToolStripButton toolStripButton2;
    private System.Windows.Forms.ToolStripButton toolStripButton3;
    private System.Windows.Forms.ToolStripLabel toolStripLabel1;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
    private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
    // Tree Views
    private TreeViewFast.Controls.TreeViewFast treeViewFast1;
    private BrightIdeasSoftware.TreeListView treeViewGrid1;
    private BrightIdeasSoftware.OLVColumn olvColumn1;
    private BrightIdeasSoftware.OLVColumn olvColumn2;
    private BrightIdeasSoftware.OLVColumn olvColumn3;
  }
}
