using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using TorFile = TorArchive.File;

namespace PugTools {
  internal partial class NodeBrowser {
    private sealed class NodeMapPageChoice {
      public Int32 Index { get; set; }
      public Int64 SId { get; set; }
      public Int64 ParentId { get; set; }
      public Int64 Guid { get; set; }
      public String MapName { get; set; }
      public String DisplayName { get; set; }
      public NodeListItem Item { get; set; }
      public String ImagePath { get; set; }
      public Boolean HasImage { get; set; }
      public String DisplayText { get; set; }
      public Single MinX { get; set; }
      public Single MinY { get; set; }
      public Single MinZ { get; set; }
      public Single MaxX { get; set; }
      public Single MaxY { get; set; }
      public Single MaxZ { get; set; }
      public Single MiniMinX { get; set; }
      public Single MiniMinZ { get; set; }
      public Single MiniMaxX { get; set; }
      public Single MiniMaxZ { get; set; }
      public Boolean IsHeroic { get; set; }
      public Boolean MountAllowed { get; set; }
      public String ExplorationType { get; set; }
      public Int32 MiniMapColumns { get; set; }
      public Int32 MiniMapRows { get; set; }
      public Boolean HasMiniMap { get; set; }

      public override String ToString() {
        return String.IsNullOrWhiteSpace(DisplayText) ? (MapName ?? "(unnamed map)") : DisplayText;
      }
    }

    private Panel _nodeMapPagePanel;
    private ComboBox _nodeMapPageCombo;
    private Label _nodeMapPageLabel;
    private Boolean _nodeMapPageUpdating;
    private List<NodeMapPageChoice> _nodeMapPageChoices = new List<NodeMapPageChoice>();
    private NodeMapZoomPictureBox _nodeMapPagePreview;
    private Label _nodeMapPagePreviewLabel;
    private Label _nodeMapPageInfoLabel;
    private CheckBox _nodeMapPageMiniMapCheck;

    private void InitializeNodeMapPageUi() {
      if (_nodeMapPagePanel != null || splitContainer3?.Panel1 == null) return;

      // Use explicit bounds for the two rows. WinForms docking order can otherwise
      // make a Fill control extend over the information row on some DPI/font sizes.
      _nodeMapPagePanel = new Panel {
        Dock = DockStyle.Top,
        Height = 74,
        Padding = new Padding(8, 4, 8, 4),
        Visible = false,
        BackColor = SystemColors.Control
      };

      TableLayoutPanel selectorRow = new TableLayoutPanel {
        Location = new Point(8, 4),
        Size = new Size(1, 28),
        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        ColumnCount = 3,
        RowCount = 1,
        Margin = new Padding(0),
        Padding = new Padding(0)
      };
      selectorRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82f));
      selectorRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
      selectorRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64f));
      selectorRow.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
      _nodeMapPagePanel.Controls.Add(selectorRow);

      _nodeMapPageLabel = new Label {
        Dock = DockStyle.Fill,
        AutoSize = false,
        Text = "Map page:",
        TextAlign = ContentAlignment.MiddleLeft,
        Margin = new Padding(0)
      };
      selectorRow.Controls.Add(_nodeMapPageLabel, 0, 0);

      _nodeMapPageCombo = new ComboBox {
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList,
        IntegralHeight = false,
        DropDownHeight = 420,
        FormattingEnabled = true,
        Margin = new Padding(0, 2, 4, 2)
      };
      _nodeMapPageCombo.SelectedIndexChanged += NodeMapPageComboSelectedIndexChanged;
      selectorRow.Controls.Add(_nodeMapPageCombo, 1, 0);

      _nodeMapPageMiniMapCheck = new CheckBox {
        Dock = DockStyle.Fill,
        AutoSize = false,
        Text = "Tiles",
        TextAlign = ContentAlignment.MiddleCenter,
        Checked = false,
        Margin = new Padding(0)
      };
      _nodeMapPageMiniMapCheck.CheckedChanged += NodeMapPageMiniMapCheckChanged;
      selectorRow.Controls.Add(_nodeMapPageMiniMapCheck, 2, 0);

      // Keep this row physically below the selector. Do not dock it into the
      // remaining space, because the ComboBox can otherwise cover it.
      _nodeMapPageInfoLabel = new Label {
        Location = new Point(8, 36),
        Size = new Size(1, 30),
        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        AutoEllipsis = true,
        TextAlign = ContentAlignment.MiddleLeft,
        ForeColor = SystemColors.GrayText,
        Text = String.Empty
      };
      _nodeMapPagePanel.Controls.Add(_nodeMapPageInfoLabel);
      _nodeMapPageInfoLabel.BringToFront();

      splitContainer3.Panel1.Controls.Add(_nodeMapPagePanel);
      _nodeMapPagePanel.BringToFront();

      _nodeMapPagePreview = new NodeMapZoomPictureBox {
        Dock = DockStyle.Fill,
        BackColor = Color.Black,
        Visible = false
      };
      _nodeMapPagePreview.MouseEnter += delegate { _nodeMapPagePreview.Focus(); };
      _nodeMapPagePreviewLabel = new Label {
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleCenter,
        ForeColor = Color.Gainsboro,
        BackColor = Color.Black,
        Text = "No map image available",
        Visible = false
      };
      _nodePreviewModelPanel?.Controls.Add(_nodeMapPagePreview);
      _nodePreviewModelPanel?.Controls.Add(_nodeMapPagePreviewLabel);
      _nodeMapPagePreviewLabel?.BringToFront();
    }

    private void DisposeNodeMapPageUi() {
      try {
        if (_nodeMapPageCombo != null) _nodeMapPageCombo.SelectedIndexChanged -= NodeMapPageComboSelectedIndexChanged;
        if (_nodeMapPageMiniMapCheck != null) _nodeMapPageMiniMapCheck.CheckedChanged -= NodeMapPageMiniMapCheckChanged;
        _nodeMapPageCombo?.Dispose();
        _nodeMapPagePanel?.Dispose();
      } catch { }
      DisposeNodeMapPagePreviewImage();
      try { _nodeMapPagePreview?.Dispose(); } catch { }
      try { _nodeMapPagePreviewLabel?.Dispose(); } catch { }
      _nodeMapPagePreview = null;
      _nodeMapPagePreviewLabel = null;
      _nodeMapPageCombo = null;
      _nodeMapPagePanel = null;
      _nodeMapPageLabel = null;
      _nodeMapPageInfoLabel = null;
      _nodeMapPageMiniMapCheck = null;
      _nodeMapPageChoices = new List<NodeMapPageChoice>();
    }

    private Boolean IsWorldMapDataNode(NodeAsset asset) {
      String id = asset?.id ?? String.Empty;
      return id.StartsWith("world.areas.", StringComparison.OrdinalIgnoreCase)
        && id.EndsWith(".mapdata", StringComparison.OrdinalIgnoreCase);
    }

    private static NodeListItem FindNodeField(NodeListItem parent, params String[] names) {
      if (parent?.children == null || names == null) return null;
      foreach (NodeListItem child in parent.children) {
        String name = child?.Name?.ToString();
        String display = child?.DisplayName;
        String fieldId = child?.FieldId;
        foreach (String wanted in names) {
          if (String.Equals(name, wanted, StringComparison.OrdinalIgnoreCase)
              || String.Equals(display, wanted, StringComparison.OrdinalIgnoreCase)
              || String.Equals(fieldId, wanted, StringComparison.OrdinalIgnoreCase)) return child;
        }
      }
      return null;
    }

    private static Int64 NodeMapPageInt64(NodeListItem item) {
      if (item == null) return 0L;
      Object value = item.value;
      if (value == null) return 0L;
      try {
        if (value is UInt64 u) return unchecked((Int64)u);
        if (value is Int64 i) return i;
        if (value is UInt32 u32) return u32;
        if (value is Int32 i32) return i32;
        return Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
      } catch { return 0L; }
    }

    private static String NodeMapPageString(NodeListItem item) {
      if (item?.value == null) return String.Empty;
      return item.value.ToString() ?? String.Empty;
    }

    private static Boolean NodeMapPageBool(NodeListItem item) {
      if (item?.value == null) return false;
      try { return Convert.ToBoolean(item.value, System.Globalization.CultureInfo.InvariantCulture); } catch { return false; }
    }

    private static Single[] NodeMapPageVector(NodeListItem item) {
      Single[] result = new Single[] { 0f, 0f, 0f };
      if (item?.value is IEnumerable values) {
        Int32 index = 0;
        foreach (Object value in values) {
          if (index >= 3) break;
          try { result[index] = Convert.ToSingle(value, System.Globalization.CultureInfo.InvariantCulture); } catch { }
          index++;
        }
      }
      return result;
    }

    private void FindNodeMapPageMiniMapCounts(String mapName, UInt64 areaId, out Int32 columns, out Int32 rows) {
      columns = 0;
      rows = 0;
      if (_currentAssets == null || String.IsNullOrWhiteSpace(mapName) || areaId == 0) return;
      const Int32 maxParts = 99;
      const Int32 trailingMisses = 8;
      Int32 rowMisses = 0;
      for (Int32 row = 0; row < maxParts && rowMisses < trailingMisses; row++) {
        Boolean found = false;
        for (Int32 col = 0; col < maxParts; col++) {
          String path = String.Format("/resources/world/areas/{0}/minimaps/{1}_{2:00}_{3:00}_r.dds", areaId, mapName, col, row);
          try { using TorFile file = _currentAssets.FindFile(path); if (file != null) { found = true; break; } } catch { }
        }
        if (found) { rows = row + 1; rowMisses = 0; } else rowMisses++;
      }
      Int32 colMisses = 0;
      for (Int32 col = 0; col < maxParts && colMisses < trailingMisses; col++) {
        Boolean found = false;
        for (Int32 row = 0; row < Math.Max(rows, 1); row++) {
          String path = String.Format("/resources/world/areas/{0}/minimaps/{1}_{2:00}_{3:00}_r.dds", areaId, mapName, col, row);
          try { using TorFile file = _currentAssets.FindFile(path); if (file != null) { found = true; break; } } catch { }
        }
        if (found) { columns = col + 1; colMisses = 0; } else colMisses++;
      }
    }

    private List<NodeMapPageChoice> BuildNodeMapPageChoices() {
      List<NodeMapPageChoice> result = new List<NodeMapPageChoice>();
      if (_rootList == null) return result;

      NodeListItem listRoot = _rootList.OfType<NodeListItem>()
        .FirstOrDefault(x => String.Equals(x?.Name?.ToString(), "mapDataContainerMapDataList", StringComparison.OrdinalIgnoreCase)
          || String.Equals(x?.DisplayName, "mapDataContainerMapDataList", StringComparison.OrdinalIgnoreCase)
          || String.Equals(x?.FieldId, "4611686042955270002", StringComparison.OrdinalIgnoreCase));
      if (listRoot == null || listRoot.children == null) return result;

      Int32 index = 0;
      foreach (NodeListItem rawPage in listRoot.children) {
        if (rawPage == null) { index++; continue; }
        NodeListItem mapName = FindNodeField(rawPage, "mapName", "4611686020073980011");
        String name = NodeMapPageString(mapName);
        if (String.IsNullOrWhiteSpace(name)) { index++; continue; }

        NodeListItem mapSid = FindNodeField(rawPage, "mapNameSId", "4611686141823655043");
        NodeListItem parentSid = FindNodeField(rawPage, "mapParentNameSId", "4611686141823655046");
        NodeListItem guid = FindNodeField(rawPage, "mapPageGUID", "4611686020668180062");
        NodeListItem minCoord = FindNodeField(rawPage, "mapPageMinCoord");
        NodeListItem maxCoord = FindNodeField(rawPage, "mapPageMaxCoord");
        NodeListItem miniMinCoord = FindNodeField(rawPage, "mapPageMiniMinCoord");
        NodeListItem miniMaxCoord = FindNodeField(rawPage, "mapPageMiniMaxCoord");
        NodeListItem heroic = FindNodeField(rawPage, "mapIsHeroic");
        NodeListItem mount = FindNodeField(rawPage, "mapMountAllowed");
        NodeListItem exploration = FindNodeField(rawPage, "mapExplorationType");
        Single[] min = NodeMapPageVector(minCoord);
        Single[] max = NodeMapPageVector(maxCoord);
        Single[] miniMin = NodeMapPageVector(miniMinCoord);
        Single[] miniMax = NodeMapPageVector(miniMaxCoord);
        UInt64 areaId = ExtractNodeMapAreaId();
        String imagePath = ResolveNodeMapPageImagePath(areaId, name, out Boolean hasImage);
        FindNodeMapPageMiniMapCounts(name, areaId, out Int32 miniColumns, out Int32 miniRows);

        result.Add(new NodeMapPageChoice {
          Index = index,
          MapName = name.Trim(),
          DisplayName = ResolveNodeMapPageDisplayName(name, NodeMapPageInt64(guid)),
          SId = NodeMapPageInt64(mapSid),
          ParentId = NodeMapPageInt64(parentSid),
          Guid = NodeMapPageInt64(guid),
          Item = rawPage,
          ImagePath = imagePath,
          HasImage = hasImage,
          MinX = min[0], MinY = min[1], MinZ = min[2],
          MaxX = max[0], MaxY = max[1], MaxZ = max[2],
          MiniMinX = miniMin[0], MiniMinZ = miniMin[2],
          MiniMaxX = miniMax[0], MiniMaxZ = miniMax[2],
          IsHeroic = NodeMapPageBool(heroic),
          MountAllowed = NodeMapPageBool(mount),
          ExplorationType = NodeMapPageString(exploration),
          MiniMapColumns = miniColumns,
          MiniMapRows = miniRows,
          HasMiniMap = miniColumns > 0 && miniRows > 0
        });
        index++;
      }
      return result;
    }

    private String ResolveNodeMapPageDisplayName(String mapName, Int64 guid) {
      if (String.IsNullOrWhiteSpace(mapName)) return String.Empty;
      try {
        var strings = _currentDom?.StringTable?.Find("str.sys.worldmap");
        String localized = strings?.GetText(guid, "MapPage." + mapName);
        if (!String.IsNullOrWhiteSpace(localized)) return localized.Trim();

        // A German STB can legitimately have no translation for a newly added map page.
        // Match the rest of PugTools/Jedipedia behavior and fall back to English rather
        // than leaving the selector with only the internal name.
        String english = strings?.GetText(guid, "MapPage." + mapName, "enMale");
        return String.IsNullOrWhiteSpace(english) ? String.Empty : english.Trim();
      } catch { return String.Empty; }
    }

    private void RefreshNodeMapPageUi(NodeAsset asset) {
      if (_nodeMapPagePanel == null || _nodeMapPageCombo == null) return;

      _nodeMapPageUpdating = true;
      try {
        _nodeMapPageChoices = IsWorldMapDataNode(asset) ? BuildNodeMapPageChoices() : new List<NodeMapPageChoice>();
        _nodeMapPageCombo.Items.Clear();
        foreach (NodeMapPageChoice choice in BuildNodeMapPageDisplayOrder(_nodeMapPageChoices))
          _nodeMapPageCombo.Items.Add(choice);

        _nodeMapPagePanel.Visible = _nodeMapPageChoices.Count > 0;
        if (_nodeMapPageChoices.Count > 0) {
          NodeMapPageChoice selected = _nodeMapPageChoices.FirstOrDefault();
          if (selected != null) {
            Int32 comboIndex = _nodeMapPageCombo.Items.IndexOf(selected);
            if (comboIndex >= 0) _nodeMapPageCombo.SelectedIndex = comboIndex;
            UpdateNodeMapPageInfo(selected);
            if (_nodeMapPageMiniMapCheck != null) _nodeMapPageMiniMapCheck.Checked = false;
            ShowNodeMapPagePreview(selected);
          }
        } else {
          HideNodeMapPagePreview();
        }
      } finally {
        _nodeMapPageUpdating = false;
      }
    }

    private static List<NodeMapPageChoice> BuildNodeMapPageDisplayOrder(List<NodeMapPageChoice> pages) {
      List<NodeMapPageChoice> result = new List<NodeMapPageChoice>();
      if (pages == null || pages.Count == 0) return result;

      Dictionary<Int64, List<NodeMapPageChoice>> children = new Dictionary<Int64, List<NodeMapPageChoice>>();
      foreach (NodeMapPageChoice page in pages) {
        Int64 parent = page.ParentId;
        if (!children.TryGetValue(parent, out List<NodeMapPageChoice> list)) {
          list = new List<NodeMapPageChoice>();
          children[parent] = list;
        }
        list.Add(page);
      }
      foreach (List<NodeMapPageChoice> list in children.Values)
        list.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.MapName, b.MapName));

      HashSet<NodeMapPageChoice> visited = new HashSet<NodeMapPageChoice>();
      Action<Int64, Int32> append = null;
      append = (parent, depth) => {
        if (!children.TryGetValue(parent, out List<NodeMapPageChoice> list)) return;
        foreach (NodeMapPageChoice page in list) {
          if (!visited.Add(page)) continue;
          String localized = String.IsNullOrWhiteSpace(page.DisplayName) ? page.MapName : page.DisplayName.Trim();
          String indent = new String(' ', Math.Min(24, depth * 3));
          page.DisplayText = indent + (depth > 0 ? "└─ " : "") + page.MapName +
            (String.Equals(localized, page.MapName, StringComparison.OrdinalIgnoreCase) ? String.Empty : "  —  " + localized);
          result.Add(page);
          append(page.SId != 0 ? page.SId : page.Guid, depth + 1);
        }
      };

      append(0L, 0);
      // Legacy/Beta files can contain pages whose parent was not preserved. Keep them visible instead of dropping them.
      foreach (NodeMapPageChoice page in pages.OrderBy(x => x.MapName, StringComparer.OrdinalIgnoreCase)) {
        if (!visited.Contains(page)) {
          String localized = String.IsNullOrWhiteSpace(page.DisplayName) ? page.MapName : page.DisplayName.Trim();
          page.DisplayText = (String.Equals(localized, page.MapName, StringComparison.OrdinalIgnoreCase)
            ? page.MapName : page.MapName + "  —  " + localized);
          result.Add(page);
          visited.Add(page);
          append(page.SId != 0 ? page.SId : page.Guid, 1);
        }
      }
      return result;
    }

    private UInt64 ExtractNodeMapAreaId() {
      String id = treeViewFast1?.SelectedNode?.Tag is NodeAsset asset ? asset.id : null;
      if (String.IsNullOrWhiteSpace(id)) return 0;
      String[] parts = id.Split('.');
      return parts.Length >= 3 && UInt64.TryParse(parts[2], out UInt64 areaId) ? areaId : 0;
    }

    private String ResolveNodeMapPageImagePath(UInt64 areaId, String mapName, out Boolean hasImage) {
      hasImage = false;
      if (areaId == 0 || String.IsNullOrWhiteSpace(mapName) || _currentAssets == null) return null;
      String safeName = mapName.Trim().Replace('\\', '/').Trim('/');
      if (safeName.IndexOf('/') >= 0) safeName = safeName.Substring(safeName.LastIndexOf('/') + 1);
      String[] candidates = {
        $"/resources/world/areas/{areaId}/{safeName}_r.dds",
        $"/resources/world/livecontent/systemgenerated/{areaId}/{safeName}_r.dds",
        $"/resources/world/areas/{areaId}/{safeName}.dds",
        $"/resources/world/livecontent/systemgenerated/{areaId}/{safeName}.dds"
      };
      foreach (String candidate in candidates) {
        try { using TorFile file = _currentAssets.FindFile(candidate); if (file != null) { hasImage = true; return candidate; } } catch { }
      }
      return candidates[0];
    }

    private void DisposeNodeMapPagePreviewImage() {
      if (_nodeMapPagePreview == null) return;
      Image old = _nodeMapPagePreview.Image;
      _nodeMapPagePreview.Image = null;
      try { old?.Dispose(); } catch { }
    }

    private void HideNodeMapPagePreview() {
      _nodeMapPagePreviewActive = false;
      DisposeNodeMapPagePreviewImage();
      if (_nodeMapPagePreview != null) _nodeMapPagePreview.Visible = false;
      if (_nodeMapPagePreviewLabel != null) _nodeMapPagePreviewLabel.Visible = false;
      if (_nodePreviewContentSplit != null) _nodePreviewContentSplit.Panel2Collapsed = true;
    }

    private void ShowNodeMapPagePreview(NodeMapPageChoice choice) {
      if (choice == null || _nodePreviewContentSplit == null || _nodePreviewModelPanel == null) return;
      try { StopNodePreviewRenderer(false); } catch { }
      _nodeMapPagePreviewActive = true;
      _nodePreviewContentSplit.Panel2Collapsed = false;
      ResizeNodePreviewLayout();
      _nodePreviewModelLabel.Text = "Map preview — " + (String.IsNullOrWhiteSpace(choice.DisplayName) ? choice.MapName : choice.DisplayName);
      DisposeNodeMapPagePreviewImage();
      _nodeMapPagePreviewLabel.Text = choice.HasImage ? "Loading map image..." : "No map image available for this map page";
      _nodeMapPagePreviewLabel.Visible = true;
      _nodeMapPagePreview.Visible = false;
      if (_nodeMapPageMiniMapCheck?.Checked == true) {
        if (ShowNodeMapPageMiniMap(choice)) {
          _nodeMapPagePreview.Visible = true;
          _nodeMapPagePreviewLabel.Visible = false;
        } else {
          _nodeMapPagePreviewLabel.Text = "Minimap tiles could not be loaded";
        }
        return;
      }
      if (!choice.HasImage || _currentAssets == null || String.IsNullOrWhiteSpace(choice.ImagePath)) return;
      try {
        using TorFile file = _currentAssets.FindFile(choice.ImagePath);
        if (file == null) return;
        using Stream input = file.OpenCopyInMemory();
        lock (NodePreviewDevIlLock) {
          DevIL.ImageImporter importer = new DevIL.ImageImporter();
          DevIL.Image image = importer.LoadImageFromStream(DevIL.ImageType.Dds, input);
          using MemoryStream output = new MemoryStream();
          DevIL.ImageExporter exporter = new DevIL.ImageExporter();
          exporter.SaveImageToStream(image, DevIL.ImageType.Png, output);
          output.Position = 0;
          using Bitmap temporary = new Bitmap(output);
          using Bitmap decoded = new Bitmap(temporary);
          _nodeMapPagePreview.SetImage(new Bitmap(decoded), true);
        }
        _nodeMapPagePreview.Visible = true;
        _nodeMapPagePreviewLabel.Visible = false;
      } catch {
        _nodeMapPagePreviewLabel.Text = "Map image could not be decoded";
      }
    }

    private void UpdateNodeMapPageInfo(NodeMapPageChoice choice) {
      if (_nodeMapPageInfoLabel == null) return;
      if (choice == null) { _nodeMapPageInfoLabel.Text = String.Empty; return; }
      String image = choice.HasImage ? "image: yes" : "image: no";
      String flags = (choice.IsHeroic ? "Heroic" : "Normal") + ", " + (choice.MountAllowed ? "mount" : "no mount");
      String exploration = String.IsNullOrWhiteSpace(choice.ExplorationType) ? String.Empty : " | exploration: " + choice.ExplorationType;
      String mini = choice.HasMiniMap ? String.Format(" | minimap: {0}x{1}", choice.MiniMapColumns, choice.MiniMapRows) : " | minimap: none";
      if (_nodeMapPageMiniMapCheck != null) _nodeMapPageMiniMapCheck.Enabled = choice.HasMiniMap;
      _nodeMapPageInfoLabel.Text = String.Format(
        "SId {0} | GUID {1} | Parent {2} | Bounds ({3:0.##},{4:0.##},{5:0.##}) → ({6:0.##},{7:0.##},{8:0.##}) | {9}{10} | {11}",
        choice.SId, choice.Guid, choice.ParentId,
        choice.MinX, choice.MinY, choice.MinZ, choice.MaxX, choice.MaxY, choice.MaxZ,
        flags, exploration, image + mini
      );
    }

    private Bitmap LoadNodeMapPageTile(String path) {
      if (_currentAssets == null) return null;
      try {
        using TorFile file = _currentAssets.FindFile(path);
        if (file == null) return null;
        using Stream input = file.OpenCopyInMemory();
        lock (NodePreviewDevIlLock) {
          DevIL.ImageImporter importer = new DevIL.ImageImporter();
          DevIL.Image image = importer.LoadImageFromStream(DevIL.ImageType.Dds, input);
          using MemoryStream output = new MemoryStream();
          DevIL.ImageExporter exporter = new DevIL.ImageExporter();
          exporter.SaveImageToStream(image, DevIL.ImageType.Png, output);
          output.Position = 0;
          using Bitmap decoded = new Bitmap(output);
          return new Bitmap(decoded);
        }
      } catch { return null; }
    }

    private Boolean ShowNodeMapPageMiniMap(NodeMapPageChoice choice) {
      if (choice == null || !choice.HasMiniMap || _currentAssets == null) return false;
      Int32 columns = Math.Min(99, Math.Max(1, choice.MiniMapColumns));
      Int32 rows = Math.Min(99, Math.Max(1, choice.MiniMapRows));
      Bitmap first = null;
      for (Int32 row = 0; row < rows && first == null; row++)
        for (Int32 col = 0; col < columns && first == null; col++)
          first = LoadNodeMapPageTile(String.Format("/resources/world/areas/{0}/minimaps/{1}_{2:00}_{3:00}_r.dds", ExtractNodeMapAreaId(), choice.MapName, col, row));
      if (first == null) return false;

      Int32 tileWidth = first.Width;
      Int32 tileHeight = first.Height;
      const Int32 maxDimension = 8192;
      Single scale = Math.Min(1f, Math.Min((Single)maxDimension / Math.Max(1, columns * tileWidth), (Single)maxDimension / Math.Max(1, rows * tileHeight)));
      Int32 cellWidth = Math.Max(1, (Int32)Math.Round(tileWidth * scale));
      Int32 cellHeight = Math.Max(1, (Int32)Math.Round(tileHeight * scale));
      Bitmap mosaic = new Bitmap(Math.Max(1, columns * cellWidth), Math.Max(1, rows * cellHeight), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      using (Graphics g = Graphics.FromImage(mosaic)) {
        g.Clear(Color.FromArgb(28, 28, 28));
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        for (Int32 row = 0; row < rows; row++) {
          for (Int32 col = 0; col < columns; col++) {
            String path = String.Format("/resources/world/areas/{0}/minimaps/{1}_{2:00}_{3:00}_r.dds", ExtractNodeMapAreaId(), choice.MapName, col, row);
            using Bitmap tile = LoadNodeMapPageTile(path);
            Rectangle target = new Rectangle(col * cellWidth, row * cellHeight, cellWidth, cellHeight);
            if (tile != null) g.DrawImage(tile, target);
            else {
              using Pen pen = new Pen(Color.FromArgb(70, 180, 180, 180));
              g.DrawRectangle(pen, target);
              using Font font = new Font(FontFamily.GenericSansSerif, Math.Max(7f, Math.Min(14f, cellWidth / 8f)));
              TextRenderer.DrawText(g, "?", font, target, Color.DimGray, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
          }
        }
      }
      try { first.Dispose(); } catch { }
      DisposeNodeMapPagePreviewImage();
      _nodeMapPagePreview.SetCoordinateFrame(choice.MiniMinX, choice.MiniMinZ, choice.MiniMaxX, choice.MiniMaxZ);
      _nodeMapPagePreview.SetImage(mosaic, true);
      _nodeMapPagePreviewLabel.Text = String.Format("Minimap tiles: {0} x {1}", columns, rows);
      return true;
    }

    private void NodeMapPageMiniMapCheckChanged(Object sender, EventArgs e) {
      if (_nodeMapPageUpdating || _nodeMapPageCombo?.SelectedItem is not NodeMapPageChoice choice) return;
      try { ShowNodeMapPagePreview(choice); } catch { }
    }

    private void NodeMapPageComboSelectedIndexChanged(Object sender, EventArgs e) {
      if (_nodeMapPageUpdating || _nodeMapPageCombo?.SelectedItem is not NodeMapPageChoice choice) return;
      try {
        UpdateNodeMapPageInfo(choice);
        ShowNodeMapPagePreview(choice);
        if (choice.Item != null) {
          treeViewGrid1.SelectObject(choice.Item, true);
        }
      } catch {
        // Selection is only a convenience overlay; never let malformed Beta map data break the Node Browser.
      }
    }
  }
}
