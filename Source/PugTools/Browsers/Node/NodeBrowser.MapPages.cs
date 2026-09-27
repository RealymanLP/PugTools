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

    private void InitializeNodeMapPageUi() {
      if (_nodeMapPagePanel != null || splitContainer3?.Panel1 == null) return;

      _nodeMapPagePanel = new Panel {
        Dock = DockStyle.Top,
        Height = 36,
        Padding = new Padding(8, 4, 8, 4),
        Visible = false,
        BackColor = SystemColors.Control
      };

      _nodeMapPageLabel = new Label {
        AutoSize = false,
        Width = 82,
        Dock = DockStyle.Left,
        Text = "Map page:",
        TextAlign = ContentAlignment.MiddleLeft
      };
      _nodeMapPagePanel.Controls.Add(_nodeMapPageLabel);

      _nodeMapPageCombo = new ComboBox {
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList,
        IntegralHeight = false,
        DropDownHeight = 420,
        FormattingEnabled = true
      };
      _nodeMapPageCombo.SelectedIndexChanged += NodeMapPageComboSelectedIndexChanged;
      _nodeMapPagePanel.Controls.Add(_nodeMapPageCombo);

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
        String imagePath = ResolveNodeMapPageImagePath(ExtractNodeMapAreaId(), name, out Boolean hasImage);

        result.Add(new NodeMapPageChoice {
          Index = index,
          MapName = name.Trim(),
          DisplayName = ResolveNodeMapPageDisplayName(name, NodeMapPageInt64(guid)),
          SId = NodeMapPageInt64(mapSid),
          ParentId = NodeMapPageInt64(parentSid),
          Guid = NodeMapPageInt64(guid),
          Item = rawPage,
          ImagePath = imagePath,
          HasImage = hasImage
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
        return String.IsNullOrWhiteSpace(localized) ? String.Empty : localized.Trim();
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

    private void NodeMapPageComboSelectedIndexChanged(Object sender, EventArgs e) {
      if (_nodeMapPageUpdating || _nodeMapPageCombo?.SelectedItem is not NodeMapPageChoice choice) return;
      try {
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
