namespace PugTools {
  partial class AssetBrowser {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssetBrowser));
      // Background Workers
      this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
      this.backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
      this.backgroundWorker3 = new System.ComponentModel.BackgroundWorker();
      // Buttons
      this.btnSearch = new System.Windows.Forms.Button();
      this.btnFindNext = new System.Windows.Forms.Button();
      this.btnClearSearch = new System.Windows.Forms.Button();
      this.btnExtractPath = new System.Windows.Forms.Button();
      // Data Grid Views
      this.dataGridView1 = new System.Windows.Forms.DataGridView();
      // Hex Boxes
      this.hexBox1 = new Be.Windows.Forms.HexBox();
      // Image Lists
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      // Labels
      this.lblExtractPath = new System.Windows.Forms.Label();
      // Panels
      this.renderPanel = new System.Windows.Forms.Panel();
      // Picture Boxes
      this.loadingSwirl1 = new System.Windows.Forms.PictureBox();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      // Split Containers
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.splitContainer3 = new System.Windows.Forms.SplitContainer();
      this.splitContainer4 = new System.Windows.Forms.SplitContainer();
      // Status Strip
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
      this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
      // Text Boxes
      this.txtSearch = new System.Windows.Forms.TextBox();
      this.txtExtractPath = new System.Windows.Forms.TextBox();
      // Tool Strip Controls
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.toolStrip1Label1 = new System.Windows.Forms.ToolStripLabel();
      this.toolStrip1Button1 = new System.Windows.Forms.ToolStripButton();
      this.toolStrip1Button2 = new System.Windows.Forms.ToolStripButton();
      this.toolStrip1Button3 = new System.Windows.Forms.ToolStripButton();
      this.toolStrip1ProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
      // Tree View Controls
      this.treeViewFast1 = new TreeViewFast.Controls.TreeViewFast();
      this.treeViewGrid1 = new BrightIdeasSoftware.TreeListView();
      this.olvColumn1 = new BrightIdeasSoftware.OLVColumn();
      this.olvColumn2 = new BrightIdeasSoftware.OLVColumn();
      // Web Browsers
      this.webBrowser1 = new System.Windows.Forms.WebBrowser();

      //-----------------------------------------------------------------------------------------// 

      this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.extractByExtensionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.extractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.txtRawView = new System.Windows.Forms.TextBox();
      this.btnPreview = new System.Windows.Forms.Button();
      // this.btnAudioStop = new System.Windows.Forms.Button();
      this.btnExtract = new System.Windows.Forms.Button();
      this.btnSaveTxtHash = new System.Windows.Forms.Button();
      this.btnViewRaw = new System.Windows.Forms.Button();
      this.btnViewHex = new System.Windows.Forms.Button();
      this.btnFindFileNames = new System.Windows.Forms.Button();
      this.btnTestHashFile = new System.Windows.Forms.Button();
      this.btnFileTable = new System.Windows.Forms.Button();
      this.btnHashStatus = new System.Windows.Forms.Button();
      this.btnHelp = new System.Windows.Forms.Button();
      //
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
      this.splitContainer3.Panel1.SuspendLayout();
      this.splitContainer3.Panel2.SuspendLayout();
      this.splitContainer3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
      this.splitContainer4.Panel1.SuspendLayout();
      this.splitContainer4.Panel2.SuspendLayout();
      this.splitContainer4.SuspendLayout();
      //
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.loadingSwirl1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.treeViewGrid1)).BeginInit();
      this.contextMenuStrip1.SuspendLayout();
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
      this.splitContainer2.SplitterDistance = 350;
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
      this.splitContainer3.Panel1.Controls.Add(this.hexBox1);
      this.splitContainer3.Panel1.Controls.Add(this.pictureBox1);
      this.splitContainer3.Panel1.Controls.Add(this.loadingSwirl1);
      this.splitContainer3.Panel1.Controls.Add(this.renderPanel);
      this.splitContainer3.Panel1.Controls.Add(this.treeViewGrid1);
      this.splitContainer3.Panel1.Controls.Add(this.txtRawView);
      this.splitContainer3.Panel1.Controls.Add(this.webBrowser1);
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
      this.splitContainer4.SplitterDistance = splitContainer4.Height - 260;
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
      this.splitContainer4.Panel2.Controls.Add(this.btnPreview);
      // this.splitContainer4.Panel2.Controls.Add(this.btnAudioStop);
      this.splitContainer4.Panel2.Controls.Add(this.btnExtract);
      this.splitContainer4.Panel2.Controls.Add(this.btnSaveTxtHash);
      this.splitContainer4.Panel2.Controls.Add(this.btnViewRaw);
      this.splitContainer4.Panel2.Controls.Add(this.btnViewHex);
      this.splitContainer4.Panel2.Controls.Add(this.btnFindFileNames);
      this.splitContainer4.Panel2.Controls.Add(this.btnTestHashFile);
      this.splitContainer4.Panel2.Controls.Add(this.btnFileTable);
      this.splitContainer4.Panel2.Controls.Add(this.btnHashStatus);
      this.splitContainer4.Panel2.Controls.Add(this.btnHelp);

      /////////////////////////////////////////////////////////////////////////////////////////////
      // LEFT PANEL

      // 
      // txtSearch
      // 
      this.txtSearch.Enabled = false;
      this.txtSearch.Location = new System.Drawing.Point(9, 8);
      this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtSearchKeyDown);
      this.txtSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.txtSearch.Name = "txtSearch";
      this.txtSearch.Size = new System.Drawing.Size(335, 23);
      this.txtSearch.TabIndex = 0;
      // 
      // btnSearch
      // 
      this.btnSearch.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnSearch.Enabled = false;
      this.btnSearch.Location = new System.Drawing.Point(8, 37);
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
      this.btnFindNext.Location = new System.Drawing.Point(122, 37);
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
      this.btnClearSearch.Location = new System.Drawing.Point(236, 37);
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
        this.extractToolStripMenuItem,
        this.extractByExtensionToolStripMenuItem});
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.contextMenuStrip1.Size = new System.Drawing.Size(181, 48);
      // 
      // extractToolStripMenuItem
      // 
      this.extractToolStripMenuItem.Name = "extractToolStripMenuItem";
      this.extractToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
      this.extractToolStripMenuItem.Text = "Extract";
      this.extractToolStripMenuItem.Click += new System.EventHandler(this.ExtractToolStripMenuItemClick);
      // 
      // extractByExtensionToolStripMenuItem
      // 
      this.extractByExtensionToolStripMenuItem.Name = "extractByExtensionToolStripMenuItem";
      this.extractByExtensionToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
      this.extractByExtensionToolStripMenuItem.Text = "Extract By Extension";
      this.extractByExtensionToolStripMenuItem.Click += new System.EventHandler(this.ExtractByExtensionToolStripMenuItemClick);

      /////////////////////////////////////////////////////////////////////////////////////////////
      // CENTER PANEL

      // 
      // hexBox1
      // 
      this.hexBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.hexBox1.ColumnInfoVisible = true;
      this.hexBox1.CurrentLineChanged += new System.EventHandler(this.HexBoxPositionChanged);
      this.hexBox1.CurrentPositionInLineChanged += new System.EventHandler(this.HexBoxPositionChanged);
      this.hexBox1.Cursor = System.Windows.Forms.Cursors.Default;
      this.hexBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.hexBox1.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.hexBox1.LineInfoVisible = true;
      this.hexBox1.Location = new System.Drawing.Point(0, 0);
      this.hexBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.hexBox1.Name = "hexBox1";
      this.hexBox1.ReadOnly = true;
      this.hexBox1.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
      this.hexBox1.Size = this.splitContainer3.Panel1.Size;
      this.hexBox1.StringViewVisible = true;
      this.hexBox1.TabIndex = 1;
      this.hexBox1.UseFixedBytesPerLine = true;
      this.hexBox1.Visible = false;
      this.hexBox1.VScrollBarVisible = true;
      // 
      // pictureBox1
      // 
      this.pictureBox1.BackColor = System.Drawing.Color.White;
      this.pictureBox1.BackgroundImage = global::PugTools.Properties.Resources.Transparent;
      this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Default;
      this.pictureBox1.Location = new System.Drawing.Point(0, 0);
      this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = this.splitContainer3.Panel1.Size;
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pictureBox1.TabIndex = 2;
      this.pictureBox1.TabStop = false;
      this.pictureBox1.Visible = false;
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
      // renderPanel
      // 
      this.renderPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.renderPanel.Cursor = System.Windows.Forms.Cursors.Default;
      this.renderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.renderPanel.Location = new System.Drawing.Point(0, 0);
      this.renderPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.renderPanel.MouseHover += new System.EventHandler(this.RenderPanelMouseHover);
      this.renderPanel.Name = "renderPanel";
      this.renderPanel.Resize += new System.EventHandler(this.RenderPanelResize);
      this.renderPanel.Size = this.splitContainer3.Panel1.Size;
      this.renderPanel.TabIndex = 4;
      this.renderPanel.Visible = false;
      //
      // treeViewGrid1
      //
      this.treeViewGrid1.AllColumns.Add(this.olvColumn1);
      this.treeViewGrid1.AllColumns.Add(this.olvColumn2);
      this.treeViewGrid1.BackColor = System.Drawing.SystemColors.Window;
      this.treeViewGrid1.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.treeViewGrid1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        this.olvColumn1,
        this.olvColumn2
      });
      this.treeViewGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeViewGrid1.GridLines = true;
      this.treeViewGrid1.Location = new System.Drawing.Point(0, 0);
      this.treeViewGrid1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
      this.treeViewGrid1.Name = "treeItemView";
      this.treeViewGrid1.OwnerDraw = true;
      this.treeViewGrid1.ShowGroups = false;
      this.treeViewGrid1.SelectionChanged += new System.EventHandler(this.TreeViewGrid1SelectedIndexChanged);
      this.treeViewGrid1.Size = this.splitContainer3.Panel1.Size;
      this.treeViewGrid1.UseCompatibleStateImageBehavior = false;
      this.treeViewGrid1.View = System.Windows.Forms.View.Details;
      this.treeViewGrid1.VirtualMode = true;
      this.treeViewGrid1.Visible = false;
      // 
      // olvColumn1
      // 
      this.olvColumn1.AspectName = "name";
      this.olvColumn1.CellPadding = null;
      this.olvColumn1.Text = "Name";
      this.olvColumn1.Width = this.splitContainer3.Panel1.Width / 2;
      // 
      // olvColumn2
      // 
      this.olvColumn2.AspectName = "displayValue";
      this.olvColumn2.CellPadding = null;
      this.olvColumn2.Text = "Value";
      this.olvColumn2.Width = this.splitContainer3.Panel1.Width / 2;
      // 
      // txtRawView
      // 
      this.txtRawView.BackColor = System.Drawing.Color.White;
      this.txtRawView.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.txtRawView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtRawView.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.txtRawView.ForeColor = System.Drawing.Color.Black;
      this.txtRawView.Location = new System.Drawing.Point(0, 0);
      this.txtRawView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.txtRawView.Multiline = true;
      this.txtRawView.Name = "txtRawView";
      this.txtRawView.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtRawView.Size = this.splitContainer3.Panel1.Size;
      this.txtRawView.TabIndex = 6;
      this.txtRawView.Visible = false;
      // 
      // webBrowser1
      // 
      this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.webBrowser1.Location = new System.Drawing.Point(0, 0);
      this.webBrowser1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.webBrowser1.MinimumSize = new System.Drawing.Size(23, 23);
      this.webBrowser1.Name = "webBrowser1";
      this.webBrowser1.Size = this.splitContainer3.Panel1.Size;
      this.webBrowser1.TabIndex = 7;
      this.webBrowser1.Visible = false;

      // ADUIO TOOL STRIP /////////////////////////////////////////////////////////////////////////
      //
      // toolStrip1
      //
      this.toolStrip1.CanOverflow = false;
      this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Top;
      this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.toolStrip1Label1,
        new System.Windows.Forms.ToolStripSeparator(),
        this.toolStrip1Button1,
        this.toolStrip1Button2,
        new System.Windows.Forms.ToolStripSeparator(),
        this.toolStrip1Button3,
        new System.Windows.Forms.ToolStripSeparator(),
        this.toolStrip1ProgressBar1,
        new System.Windows.Forms.ToolStripSeparator()
      });
      this.toolStrip1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Visible = false;
      //
      // toolStrip1Label1
      //
      this.toolStrip1Label1.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
      this.toolStrip1Label1.Name = "toolStrip1Label1";
      this.toolStrip1Label1.Text = "00:00/00:00";
      this.toolStrip1Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      //
      // toolStrip1Button1
      //
      this.toolStrip1Button1.AutoSize = true;
      this.toolStrip1Button1.Click += new System.EventHandler(this.ToolStrip1Button1Click);
      this.toolStrip1Button1.Enabled = false;
      this.toolStrip1Button1.Font = new System.Drawing.Font("Webdings", 13F);
      this.toolStrip1Button1.Name = "toolStrip1Button1";
      this.toolStrip1Button1.Text = ";";
      this.toolStrip1Button1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.toolStrip1Button1.ToolTipText = "Pause";
      //
      // toolStrip1Button2
      //
      this.toolStrip1Button2.AutoSize = true;
      this.toolStrip1Button2.Click += new System.EventHandler(this.ToolStrip1Button2Click);
      this.toolStrip1Button2.Enabled = false;
      this.toolStrip1Button2.Font = new System.Drawing.Font("Webdings", 13F);
      this.toolStrip1Button2.Name = "toolStrip1Button2";
      this.toolStrip1Button2.Text = "<";
      this.toolStrip1Button2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.toolStrip1Button2.ToolTipText = "Stop";
      //
      // toolStrip1Button3
      //
      this.toolStrip1Button3.AutoSize = true;
      this.toolStrip1Button3.Click += new System.EventHandler(this.ToolStrip1Button3Click);
      this.toolStrip1Button3.Enabled = false;
      this.toolStrip1Button3.Font = new System.Drawing.Font("Webdings", 13F);
      this.toolStrip1Button3.Name = "toolStrip1Button3";
      this.toolStrip1Button3.Text = "X";
      this.toolStrip1Button3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.toolStrip1Button3.ToolTipText = "Mute";
      //
      // toolStrip1ProgressBar1
      //
      this.toolStrip1ProgressBar1.Enabled = false;
      this.toolStrip1ProgressBar1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
      this.toolStrip1ProgressBar1.Name = "toolStrip1ProgressBar1";
      this.toolStrip1ProgressBar1.Size = new System.Drawing.Size(250, 0);
      this.toolStrip1ProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;

      /////////////////////////////////////////////////////////////////////////////////////////////
      // RIGHT PANEL

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
      this.lblExtractPath.TabIndex = 0;
      this.lblExtractPath.Text = "Extract Path";
      // 
      // txtExtractPath
      // 
      this.txtExtractPath.Enabled = false;
      this.txtExtractPath.Location = new System.Drawing.Point(4, 23);
      this.txtExtractPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.txtExtractPath.Name = "txtExtractPath";
      this.txtExtractPath.Size = new System.Drawing.Size(240, 23);
      this.txtExtractPath.TabIndex = 0;
      // 
      // btnChooseExtract
      // 
      this.btnExtractPath.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnExtractPath.Enabled = false;
      this.btnExtractPath.Location = new System.Drawing.Point(247, 22);
      this.btnExtractPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnExtractPath.Name = "btnChooseExtract";
      this.btnExtractPath.Size = new System.Drawing.Size(38, 25);
      this.btnExtractPath.TabIndex = 1;
      this.btnExtractPath.Text = "...";
      this.btnExtractPath.UseVisualStyleBackColor = true;
      this.btnExtractPath.Click += new System.EventHandler(this.BtnChooseExtractClick);
      // 
      // btnPreview
      // 
      this.btnPreview.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnPreview.Enabled = false;
      this.btnPreview.Location = new System.Drawing.Point(6, 53);
      this.btnPreview.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnPreview.Name = "btnPreview";
      this.btnPreview.Size = new System.Drawing.Size(114, 27);
      this.btnPreview.TabIndex = 2;
      this.btnPreview.Text = "Auto Preview On";
      this.btnPreview.UseVisualStyleBackColor = true;
      this.btnPreview.Click += new System.EventHandler(this.BtnPreviewClick);
      // 
      // btnAudioStop
      // 
      // this.btnAudioStop.Cursor = System.Windows.Forms.Cursors.Default;
      // this.btnAudioStop.Enabled = false;
      // this.btnAudioStop.Location = new System.Drawing.Point(127, 53);
      // this.btnAudioStop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      // this.btnAudioStop.Name = "btnAudioStop";
      // this.btnAudioStop.Size = new System.Drawing.Size(114, 27);
      // this.btnAudioStop.TabIndex = 3;
      // this.btnAudioStop.Text = "Stop Audio";
      // this.btnAudioStop.UseVisualStyleBackColor = true;
      // this.btnAudioStop.Click += new System.EventHandler(this.BtnAudioStopClick);
      // 
      // btnExtract
      // 
      this.btnExtract.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnExtract.Enabled = false;
      this.btnExtract.Location = new System.Drawing.Point(6, 87);
      this.btnExtract.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnExtract.Name = "btnExtract";
      this.btnExtract.Size = new System.Drawing.Size(114, 27);
      this.btnExtract.TabIndex = 4;
      this.btnExtract.Text = "Extract Object";
      this.btnExtract.UseVisualStyleBackColor = true;
      this.btnExtract.Click += new System.EventHandler(this.BtnExtractClick);
      // 
      // btnSaveTxtHash
      // 
      this.btnSaveTxtHash.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnSaveTxtHash.Enabled = false;
      this.btnSaveTxtHash.Location = new System.Drawing.Point(127, 87);
      this.btnSaveTxtHash.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnSaveTxtHash.Name = "btnSaveTxtHash";
      this.btnSaveTxtHash.Size = new System.Drawing.Size(114, 27);
      this.btnSaveTxtHash.TabIndex = 5;
      this.btnSaveTxtHash.Text = "Save Hash File";
      this.btnSaveTxtHash.UseVisualStyleBackColor = true;
      this.btnSaveTxtHash.Click += new System.EventHandler(this.BtnSaveTxtHashClick);
      // 
      // btnViewRaw
      // 
      this.btnViewRaw.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnViewRaw.Enabled = false;
      this.btnViewRaw.Location = new System.Drawing.Point(6, 120);
      this.btnViewRaw.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnViewRaw.Name = "btnViewRaw";
      this.btnViewRaw.Size = new System.Drawing.Size(114, 27);
      this.btnViewRaw.TabIndex = 6;
      this.btnViewRaw.Text = "View RAW Data";
      this.btnViewRaw.UseVisualStyleBackColor = true;
      this.btnViewRaw.Click += new System.EventHandler(this.BtnViewRawClick);
      // 
      // btnViewHex
      // 
      this.btnViewHex.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnViewHex.Enabled = false;
      this.btnViewHex.Location = new System.Drawing.Point(127, 120);
      this.btnViewHex.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnViewHex.Name = "btnViewHex";
      this.btnViewHex.Size = new System.Drawing.Size(114, 27);
      this.btnViewHex.TabIndex = 7;
      this.btnViewHex.Text = "View HEX";
      this.btnViewHex.UseVisualStyleBackColor = true;
      this.btnViewHex.Click += new System.EventHandler(this.BtnViewHexClick);
      // 
      // btnFindFileNames
      // 
      this.btnFindFileNames.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnFindFileNames.Enabled = false;
      this.btnFindFileNames.Location = new System.Drawing.Point(6, 153);
      this.btnFindFileNames.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnFindFileNames.Name = "btnFindFileNames";
      this.btnFindFileNames.Size = new System.Drawing.Size(114, 27);
      this.btnFindFileNames.TabIndex = 8;
      this.btnFindFileNames.Text = "Find File Names";
      this.btnFindFileNames.UseVisualStyleBackColor = true;
      this.btnFindFileNames.Click += new System.EventHandler(this.BtnFindFileNamesClick);
      // 
      // btnTestFile
      // 
      this.btnTestHashFile.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnTestHashFile.Enabled = false;
      this.btnTestHashFile.Location = new System.Drawing.Point(127, 153);
      this.btnTestHashFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnTestHashFile.Name = "btnTestFile";
      this.btnTestHashFile.Size = new System.Drawing.Size(114, 27);
      this.btnTestHashFile.TabIndex = 9;
      this.btnTestHashFile.Text = "Test Hash File";
      this.btnTestHashFile.UseVisualStyleBackColor = true;
      this.btnTestHashFile.Click += new System.EventHandler(this.BtnTestHashFileClick);
      // 
      // btnFileTable
      // 
      this.btnFileTable.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnFileTable.Enabled = false;
      this.btnFileTable.Location = new System.Drawing.Point(6, 186);
      this.btnFileTable.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnFileTable.Name = "btnFileTable";
      this.btnFileTable.Size = new System.Drawing.Size(114, 27);
      this.btnFileTable.TabIndex = 10;
      this.btnFileTable.Text = "File Table";
      this.btnFileTable.UseVisualStyleBackColor = true;
      this.btnFileTable.Click += new System.EventHandler(this.BtnFileTableClick);
      // 
      // btnHashStatus
      // 
      this.btnHashStatus.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnHashStatus.Enabled = false;
      this.btnHashStatus.Location = new System.Drawing.Point(127, 186);
      this.btnHashStatus.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnHashStatus.Name = "btnHashStatus";
      this.btnHashStatus.Size = new System.Drawing.Size(114, 27);
      this.btnHashStatus.TabIndex = 11;
      this.btnHashStatus.Text = "Hash Status";
      this.btnHashStatus.UseVisualStyleBackColor = true;
      this.btnHashStatus.Click += new System.EventHandler(this.BtnHashStatusClick);
      // 
      // btnHelp
      // 
      this.btnHelp.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnHelp.Enabled = false;
      this.btnHelp.Location = new System.Drawing.Point(127, 53); // new System.Drawing.Point(6, 219);
      this.btnHelp.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnHelp.Name = "btnHelp";
      this.btnHelp.Size = new System.Drawing.Size(114, 27);
      this.btnHelp.TabIndex = 12;
      this.btnHelp.Text = "Help";
      this.btnHelp.UseVisualStyleBackColor = true;
      this.btnHelp.Click += new System.EventHandler(this.BtnHelpClick);

      /////////////////////////////////////////////////////////////////////////////////////////////
      // STATUS BAR
      //

      // 
      // statusStrip1
      // 
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.toolStripStatusLabel1,
        this.toolStripProgressBar1,
        this.toolStripStatusLabel2
      });
      this.statusStrip1.Location = new System.Drawing.Point(0, 0);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);
      this.statusStrip1.Size = new System.Drawing.Size(this.splitContainer1.Width, 22);
      this.statusStrip1.TabIndex = 8;
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
      // 
      // toolStripStatusLabel2
      // 
      this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
      this.toolStripStatusLabel2.Size = new System.Drawing.Size(0, 17);

      /////////////////////////////////////////////////////////////////////////////////////////////
      // BACKGROUND
      //

      // 
      // backgroundWorker1
      // 
      this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker1Run);
      this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker1Completed);
      // 
      // backgroundWorker2
      // 
      this.backgroundWorker2.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker2Run);
      this.backgroundWorker2.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundWorker2ProgressChanged);
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

      // 
      // AssetBrowser
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size; // new System.Drawing.Size(1904, 1041);
      this.Controls.Add(this.splitContainer1);
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AssetBrowserFormClosed);
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AssetBrowserFormClosing);
      this.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
      this.Name = "AssetBrowser";
      this.Resize += new System.EventHandler(this.AssetBrowserFormResize);
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Asset Browser";
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
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.loadingSwirl1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.treeViewGrid1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.contextMenuStrip1.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    // Background Workers
    private System.ComponentModel.BackgroundWorker backgroundWorker1;
    private System.ComponentModel.BackgroundWorker backgroundWorker2;
    private System.ComponentModel.BackgroundWorker backgroundWorker3;
    // Buttons
    private System.Windows.Forms.Button btnSearch;
    private System.Windows.Forms.Button btnFindNext;
    private System.Windows.Forms.Button btnClearSearch;
    private System.Windows.Forms.Button btnExtractPath;
    // Data Grid Views
    private System.Windows.Forms.DataGridView dataGridView1;
    // Hex Boxes
    private Be.Windows.Forms.HexBox hexBox1;
    // Image Lists
    private System.Windows.Forms.ImageList imageList1;
    // Labels
    private System.Windows.Forms.Label lblExtractPath;
    // Picture Boxes
    private System.Windows.Forms.PictureBox loadingSwirl1;
    private System.Windows.Forms.PictureBox pictureBox1;
    // Render Panels
    private System.Windows.Forms.Panel renderPanel;
    // Split Containers
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.SplitContainer splitContainer3;
    private System.Windows.Forms.SplitContainer splitContainer4;
    // Status Strip
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
    // Tool Strip 1 - Audio
    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.ToolStripLabel toolStrip1Label1;
    private System.Windows.Forms.ToolStripButton toolStrip1Button1;
    private System.Windows.Forms.ToolStripButton toolStrip1Button2;
    private System.Windows.Forms.ToolStripButton toolStrip1Button3;
    private System.Windows.Forms.ToolStripProgressBar toolStrip1ProgressBar1;
    // Tree Views
    private TreeViewFast.Controls.TreeViewFast treeViewFast1;
    private BrightIdeasSoftware.TreeListView treeViewGrid1;
    private BrightIdeasSoftware.OLVColumn olvColumn1;
    private BrightIdeasSoftware.OLVColumn olvColumn2;
    // Text Boxes
    private System.Windows.Forms.TextBox txtExtractPath;
    private System.Windows.Forms.TextBox txtRawView;
    private System.Windows.Forms.TextBox txtSearch;
    // Web Browser
    private System.Windows.Forms.WebBrowser webBrowser1;

    //-------------------------------------------------------------------------------------------//

    private System.Windows.Forms.ToolStripMenuItem extractToolStripMenuItem;
    private System.Windows.Forms.Button btnPreview;
    // private System.Windows.Forms.Button btnAudioStop;
    private System.Windows.Forms.Button btnExtract;
    private System.Windows.Forms.Button btnSaveTxtHash;
    private System.Windows.Forms.Button btnViewRaw;
    private System.Windows.Forms.Button btnViewHex;
    private System.Windows.Forms.Button btnFindFileNames;
    private System.Windows.Forms.Button btnTestHashFile;
    private System.Windows.Forms.Button btnFileTable;
    private System.Windows.Forms.Button btnHashStatus;
    private System.Windows.Forms.Button btnHelp;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    private System.Windows.Forms.ToolStripMenuItem extractByExtensionToolStripMenuItem;
  }
}
