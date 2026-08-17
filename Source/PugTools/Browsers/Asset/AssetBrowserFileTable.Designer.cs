namespace PugTools {
  partial class AssetBrowserFileTable {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssetBrowserFileTable));
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.splitContainer3 = new System.Windows.Forms.SplitContainer();
      //
      this.treeViewFast1 = new TreeViewFast.Controls.TreeViewFast();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      //
      this.loadingSwirl1 = new System.Windows.Forms.PictureBox();
      this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
      this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
      this.treeListView1 = new BrightIdeasSoftware.TreeListView();
      //
      this.dataGridView1 = new System.Windows.Forms.DataGridView();
      //
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
      this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
      //
      this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
      //
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      //
      ((System.ComponentModel.ISupportInitialize)(this.loadingSwirl1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.treeListView1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
      this.statusStrip1.SuspendLayout();
      this.SuspendLayout();
      //
      // SPLIT CONTAINERS /////////////////////////////////////////////////////////////////////////
      //
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
      this.splitContainer2.SplitterDistance = 365;
      this.splitContainer2.SplitterWidth = 1;
      this.splitContainer2.TabIndex = 0;
      this.splitContainer2.TabStop = false;
      //
      // splitContainer2.Panel1
      //
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
      this.splitContainer3.Panel1.Controls.Add(this.treeListView1);
      //
      // splitContainer3.Panel2
      //
      this.splitContainer3.Panel2.Controls.Add(this.dataGridView1);
      //
      // LEFT PANEL ///////////////////////////////////////////////////////////////////////////////
      //
      // 
      // treeViewFast1
      // 
      this.treeViewFast1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewFast1_AfterSelect);
      this.treeViewFast1.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.treeViewFast1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeViewFast1.ImageIndex = 0;
      this.treeViewFast1.ImageList = this.imageList1;
      this.treeViewFast1.Location = new System.Drawing.Point(0, 0);
      this.treeViewFast1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.treeViewFast1.Name = "treeViewFast1";
      this.treeViewFast1.SelectedImageIndex = 0;
      this.treeViewFast1.Size = new System.Drawing.Size(365, this.splitContainer2.Panel1.Height);
      this.treeViewFast1.TabIndex = 1;
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
      // CENTER PANEL /////////////////////////////////////////////////////////////////////////////
      //
      // 
      // treeListView1
      // 
      this.treeListView1.AllColumns.Add(this.olvColumn1);
      this.treeListView1.AllColumns.Add(this.olvColumn2);
      this.treeListView1.BackColor = System.Drawing.SystemColors.Window;
      this.treeListView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.treeListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        this.olvColumn1,
        this.olvColumn2
      });
      this.treeListView1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeListView1.GridLines = true;
      this.treeListView1.Location = new System.Drawing.Point(0, 0);
      this.treeListView1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
      this.treeListView1.Name = "treeListView1";
      this.treeListView1.OwnerDraw = true;
      this.treeListView1.ShowGroups = false;
      this.treeListView1.Size = this.splitContainer3.Panel1.Size; // new System.Drawing.Size(1200, 998);
      this.treeListView1.UseCompatibleStateImageBehavior = false;
      this.treeListView1.View = System.Windows.Forms.View.Details;
      this.treeListView1.VirtualMode = true;
      this.treeListView1.Visible = false;
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
      // loadingSwirl1
      // 
      this.loadingSwirl1.BackColor = System.Drawing.Color.White;
      this.loadingSwirl1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.loadingSwirl1.BorderStyle = System.Windows.Forms.BorderStyle.None;
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
      // RIGHT PANEL //////////////////////////////////////////////////////////////////////////////
      //
      // 
      // dataGridView1
      // 
      this.dataGridView1.AllowUserToAddRows = false;
      this.dataGridView1.AllowUserToDeleteRows = false;
      this.dataGridView1.AllowUserToResizeRows = false;
      this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView1.Enabled = false;
      this.dataGridView1.Location = new System.Drawing.Point(0, 0);
      this.dataGridView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.dataGridView1.Name = "dataGridView1";
      this.dataGridView1.RowHeadersVisible = false;
      this.dataGridView1.Size = new System.Drawing.Size(350, this.splitContainer3.Panel2.Height);
      this.dataGridView1.TabIndex = 0;
      this.dataGridView1.VirtualMode = true;
      //
      // STATUSBAR ////////////////////////////////////////////////////////////////////////////////
      //
      // 
      // statusStrip1
      // 
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.toolStripStatusLabel1,
        this.toolStripStatusLabel3,
        this.toolStripProgressBar1,
        this.toolStripStatusLabel2
      });
      this.statusStrip1.Location = new System.Drawing.Point(0, 998);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
      this.statusStrip1.Size = new System.Drawing.Size(this.splitContainer1.Width, 22);
      this.statusStrip1.TabIndex = 1;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // toolStripStatusLabel1
      // 
      this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
      this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
      // 
      // toolStripStatusLabel3
      // 
      this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
      this.toolStripStatusLabel3.Size = new System.Drawing.Size(0, 17);
      // 
      // toolStripProgressBar1
      // 
      this.toolStripProgressBar1.ForeColor = System.Drawing.Color.Lime;
      this.toolStripProgressBar1.Name = "toolStripProgressBar1";
      this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
      this.toolStripProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
      this.toolStripProgressBar1.Visible = false;
      // 
      // toolStripStatusLabel2
      // 
      this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
      this.toolStripStatusLabel2.Size = new System.Drawing.Size(0, 17);
      //
      // BACKGROUND ///////////////////////////////////////////////////////////////////////////////
      //
      // 
      // backgroundWorker1
      // 
      this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker1_DoWork);
      this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker1_RunWorkerCompleted);
      //
      // FORM /////////////////////////////////////////////////////////////////////////////////////
      //
      // 
      // AssetBrowserFileTable
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size;
      this.Controls.Add(this.splitContainer1);
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AssetBrowserFileTable_FormClosed);
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AssetBrowserFileTable_FormClosing);
      this.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
      this.Name = "AssetBrowserFileTable";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Asset File Table Browser ";
      this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
      //
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      //
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
      this.splitContainer2.ResumeLayout(false);
      //
      this.splitContainer3.Panel1.ResumeLayout(false);
      this.splitContainer3.Panel1.PerformLayout();
      this.splitContainer3.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
      this.splitContainer3.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.loadingSwirl1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.treeListView1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion


    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.SplitContainer splitContainer3;
    //
    private TreeViewFast.Controls.TreeViewFast treeViewFast1;
    private System.Windows.Forms.ImageList imageList1;
    //
    private BrightIdeasSoftware.TreeListView treeListView1;
    private BrightIdeasSoftware.OLVColumn olvColumn1;
    private BrightIdeasSoftware.OLVColumn olvColumn2;
    private System.Windows.Forms.PictureBox loadingSwirl1;
    //
    private System.Windows.Forms.DataGridView dataGridView1;
    //
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
    //
    private System.ComponentModel.BackgroundWorker backgroundWorker1;
  }
  /*
  partial class AssetBrowserFileTable {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssetBrowserFileTable));
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.treeViewFast1 = new TreeViewFast.Controls.TreeViewFast();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.treeListView1 = new BrightIdeasSoftware.TreeListView();
      this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
      this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
      this.pictureBox2 = new System.Windows.Forms.PictureBox();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
      this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
      this.dataGridView1 = new System.Windows.Forms.DataGridView();
      this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.treeListView1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
      this.statusStrip1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
      this.SuspendLayout();
      // 
      // splitContainer1
      // 
      this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.treeViewFast1);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
      this.splitContainer1.Size = new System.Drawing.Size(1904, 1041);
      this.splitContainer1.SplitterDistance = 350;
      this.splitContainer1.SplitterWidth = 5;
      this.splitContainer1.TabIndex = 0;
      this.splitContainer1.TabStop = false;
      // 
      // treeViewFast1
      // 
      this.treeViewFast1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewFast1_AfterSelect);
      this.treeViewFast1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeViewFast1.ImageIndex = 0;
      this.treeViewFast1.ImageList = this.imageList1;
      this.treeViewFast1.Location = new System.Drawing.Point(0, 0);
      this.treeViewFast1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.treeViewFast1.Name = "treeViewFast1";
      this.treeViewFast1.SelectedImageIndex = 0;
      this.treeViewFast1.Size = new System.Drawing.Size(346, 952);
      this.treeViewFast1.TabIndex = 1;
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
      // splitContainer2
      // 
      this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer2.Location = new System.Drawing.Point(0, 0);
      this.splitContainer2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.splitContainer2.Name = "splitContainer2";
      // 
      // splitContainer2.Panel1
      // 
      this.splitContainer2.Panel1.AutoScroll = true;
      this.splitContainer2.Panel1.Controls.Add(this.treeListView1);
      this.splitContainer2.Panel1.Controls.Add(this.pictureBox2);
      this.splitContainer2.Panel1.Controls.Add(this.statusStrip1);
      // 
      // splitContainer2.Panel2
      // 
      this.splitContainer2.Panel2.Controls.Add(this.dataGridView1);
      this.splitContainer2.Size = new System.Drawing.Size(1549, 1041);
      this.splitContainer2.SplitterDistance = 1200;
      this.splitContainer2.SplitterWidth = 5;
      this.splitContainer2.TabIndex = 0;
      this.splitContainer2.TabStop = false;
      // 
      // treeListView1
      // 
      this.treeListView1.AllColumns.Add(this.olvColumn1);
      this.treeListView1.AllColumns.Add(this.olvColumn2);
      this.treeListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top
      | System.Windows.Forms.AnchorStyles.Bottom)
      | System.Windows.Forms.AnchorStyles.Left)
      | System.Windows.Forms.AnchorStyles.Right)));
      this.treeListView1.BackColor = System.Drawing.SystemColors.Window;
      this.treeListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        this.olvColumn1,
        this.olvColumn2
      });
      this.treeListView1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeListView1.GridLines = true;
      this.treeListView1.Location = new System.Drawing.Point(0, 0);
      this.treeListView1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
      this.treeListView1.Name = "treeListView1";
      this.treeListView1.OwnerDraw = true;
      this.treeListView1.ShowGroups = false;
      this.treeListView1.Size = new System.Drawing.Size(1200, 998);
      this.treeListView1.UseCompatibleStateImageBehavior = false;
      this.treeListView1.View = System.Windows.Forms.View.Details;
      this.treeListView1.VirtualMode = true;
      this.treeListView1.Visible = false;
      // 
      // olvColumn1
      // 
      this.olvColumn1.AspectName = "name";
      this.olvColumn1.CellPadding = null;
      this.olvColumn1.Text = "Name";
      this.olvColumn1.Width = 167;
      // 
      // olvColumn2
      // 
      this.olvColumn2.AspectName = "displayValue";
      this.olvColumn2.CellPadding = null;
      this.olvColumn2.Text = "Value";
      this.olvColumn2.Width = 230;
      // 
      // pictureBox2
      // 
      this.pictureBox2.BackColor = System.Drawing.Color.White;
      this.pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureBox2.Image = global::PugTools.Properties.Resources.LoadingSwirl;
      this.pictureBox2.Location = new System.Drawing.Point(0, 0);
      this.pictureBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.pictureBox2.Name = "pictureBox2";
      this.pictureBox2.Size = new System.Drawing.Size(1200, 998);
      this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.pictureBox2.TabIndex = 3;
      this.pictureBox2.TabStop = false;
      this.pictureBox2.Visible = false;
      // 
      // statusStrip1
      // 
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.toolStripStatusLabel1,
        this.toolStripStatusLabel3,
        this.toolStripProgressBar1,
        this.toolStripStatusLabel2
      });
      this.statusStrip1.Location = new System.Drawing.Point(0, 998);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
      this.statusStrip1.Size = new System.Drawing.Size(1200, 22);
      this.statusStrip1.TabIndex = 1;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // toolStripStatusLabel1
      // 
      this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
      this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
      // 
      // toolStripStatusLabel3
      // 
      this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
      this.toolStripStatusLabel3.Size = new System.Drawing.Size(0, 17);
      // 
      // toolStripProgressBar1
      // 
      this.toolStripProgressBar1.ForeColor = System.Drawing.Color.Lime;
      this.toolStripProgressBar1.Name = "toolStripProgressBar1";
      this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
      this.toolStripProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
      this.toolStripProgressBar1.Visible = false;
      // 
      // toolStripStatusLabel2
      // 
      this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
      this.toolStripStatusLabel2.Size = new System.Drawing.Size(0, 17);
      // 
      // dataGridView1
      // 
      this.dataGridView1.AllowUserToAddRows = false;
      this.dataGridView1.AllowUserToDeleteRows = false;
      this.dataGridView1.AllowUserToResizeRows = false;
      this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridView1.Enabled = false;
      this.dataGridView1.Location = new System.Drawing.Point(0, 0);
      this.dataGridView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.dataGridView1.Name = "dataGridView1";
      this.dataGridView1.RowHeadersVisible = false;
      this.dataGridView1.Size = new System.Drawing.Size(340, 998);
      this.dataGridView1.TabIndex = 0;
      this.dataGridView1.VirtualMode = true;
      // 
      // backgroundWorker1
      // 
      this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker1_DoWork);
      this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker1_RunWorkerCompleted);
      // 
      // AssetBrowserFileTable
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1904, 1041);
      this.Controls.Add(this.splitContainer1);
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "AssetBrowserFileTable";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Asset File Table Browser ";
      this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AssetBrowserFileTable_FormClosed);
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AssetBrowserFileTable_FormClosing);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel1.PerformLayout();
      this.splitContainer2.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
      this.splitContainer2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.treeListView1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer1;
    private TreeViewFast.Controls.TreeViewFast treeViewFast1;
    private System.Windows.Forms.DataGridView dataGridView1;
    private System.Windows.Forms.ImageList imageList1;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    private System.Windows.Forms.PictureBox pictureBox2;
    private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
    private System.ComponentModel.BackgroundWorker backgroundWorker1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
    private BrightIdeasSoftware.TreeListView treeListView1;
    private BrightIdeasSoftware.OLVColumn olvColumn1;
    private BrightIdeasSoftware.OLVColumn olvColumn2;
  }  
  */
}
