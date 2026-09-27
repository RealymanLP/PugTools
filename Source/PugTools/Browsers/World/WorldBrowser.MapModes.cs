using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FileFormats;
using SlimDX;

namespace PugTools {
  public partial class WorldBrowser {
    private Panel worldMapModePanel;
    private Label worldMapModeLabel;
    private Button worldMapScopeButton;
    private Button worldMapSourceButton;
    private Button worldMapCloseButton;
    private ComboBox worldMapPageCombo;
    private bool updatingWorldMapPageCombo;
    private ulong pendingMapLinkAreaId;
    private long pendingMapLinkMapNameSId;
    private long pendingMapLinkSubmapNameSId;
    private bool pendingMapLinkMiniMap;

    private void EnsureWorldMapModeControls() {
      if (worldMapModePanel != null || splitContainer3?.Panel1 == null) return;
      worldMapModePanel = new Panel {
        Size = new Size(760, 72),
        BackColor = Color.FromArgb(238, 12, 26, 32),
        Anchor = AnchorStyles.Top | AnchorStyles.Right,
        Padding = new Padding(6, 4, 6, 4),
        Visible = false
      };
      worldMapModeLabel = new Label {
        AutoEllipsis = true,
        ForeColor = Color.FromArgb(232, 245, 248),
        TextAlign = ContentAlignment.MiddleLeft,
        Location = new Point(8, 6),
        Size = new Size(180, 26)
      };
      worldMapScopeButton = new Button {
        Text = "World map",
        Location = new Point(193, 4),
        Size = new Size(118, 28),
        FlatStyle = FlatStyle.Flat,
        UseVisualStyleBackColor = false,
        BackColor = Color.FromArgb(24, 47, 57),
        ForeColor = Color.FromArgb(221, 241, 247),
        TabStop = false
      };
      worldMapScopeButton.FlatAppearance.BorderColor = Color.FromArgb(76, 145, 168);
      worldMapScopeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(36, 69, 82);
      worldMapScopeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(45, 83, 98);
      worldMapSourceButton = new Button {
        Text = "Original in-game map",
        Location = new Point(315, 4),
        Size = new Size(158, 28),
        FlatStyle = FlatStyle.Flat,
        UseVisualStyleBackColor = false,
        BackColor = Color.FromArgb(24, 47, 57),
        ForeColor = Color.FromArgb(221, 241, 247),
        TabStop = false
      };
      worldMapSourceButton.FlatAppearance.BorderColor = Color.FromArgb(76, 145, 168);
      worldMapSourceButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(36, 69, 82);
      worldMapSourceButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(45, 83, 98);
      worldMapCloseButton = new Button {
        Text = "×",
        Location = new Point(477, 4),
        Size = new Size(54, 28),
        FlatStyle = FlatStyle.Flat,
        UseVisualStyleBackColor = false,
        BackColor = Color.FromArgb(24, 47, 57),
        ForeColor = Color.FromArgb(221, 241, 247),
        TabStop = false
      };
      worldMapCloseButton.FlatAppearance.BorderColor = Color.FromArgb(76, 145, 168);
      worldMapCloseButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(36, 69, 82);
      worldMapCloseButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(45, 83, 98);
      worldMapPageCombo = new ComboBox {
        Location = new Point(8, 38),
        Size = new Size(744, 28),
        DropDownStyle = ComboBoxStyle.DropDownList,
        FlatStyle = FlatStyle.Flat,
        BackColor = Color.FromArgb(8, 17, 22),
        ForeColor = Color.FromArgb(221, 241, 247),
        IntegralHeight = false,
        DropDownHeight = 420,
        TabStop = false
      };
      worldMapPageCombo.SelectedIndexChanged += (_, __) => {
        if (updatingWorldMapPageCombo || panelRender == null || !panelRender.IsFullMapOpen) return;
        if (worldMapPageCombo.SelectedItem is WorldMapPageChoice choice) {
          bool changed = choice.SId != 0
            ? panelRender.SelectInteractiveMapPage(choice.SId)
            : panelRender.SelectInteractiveMapPageByGuid(choice.Guid);
          if (changed) ActivateWorldRenderInput();
        }
      };
      worldMapCloseButton.Click += (_, __) => { panelRender?.CloseInteractiveMap(); ActivateWorldRenderInput(); };
      worldMapScopeButton.Click += (_, __) => {
        if (panelRender == null || !panelRender.IsFullMapOpen) return;
        if (panelRender.InteractiveMapTravelMode) {
          SetStatusLabel("Taxi and quick-travel maps use the world scope so all destinations remain visible.");
        } else if (panelRender.InteractiveMapIsWorldScope && !panelRender.InteractiveMapHasAreaScope) {
          SetStatusLabel("No separate area map exists at the current height; the world map remains active.");
        } else panelRender.SetInteractiveMapWorldScope(!panelRender.InteractiveMapIsWorldScope);
        ActivateWorldRenderInput();
      };
      worldMapSourceButton.Click += (_, __) => {
        if (panelRender == null || !panelRender.IsFullMapOpen) return;
        panelRender.SetInteractiveMapOriginalArt(!panelRender.InteractiveMapPrefersOriginalArt);
        ActivateWorldRenderInput();
      };
      worldMapModePanel.Controls.Add(worldMapModeLabel);
      worldMapModePanel.Controls.Add(worldMapScopeButton);
      worldMapModePanel.Controls.Add(worldMapSourceButton);
      worldMapModePanel.Controls.Add(worldMapCloseButton);
      worldMapModePanel.Controls.Add(worldMapPageCombo);
      splitContainer3.Panel1.Controls.Add(worldMapModePanel);
      LayoutWorldMapModeControls();
      worldMapModePanel.BringToFront();
    }

    private void LayoutWorldMapModeControls() {
      if (worldMapModePanel == null || splitContainer3?.Panel1 == null) return;
      int top = (worldToolbar?.Visible == true ? worldToolbar.Bottom : 0) + 7;
      worldMapModePanel.Left = Math.Max(4, splitContainer3.Panel1.ClientSize.Width - worldMapModePanel.Width - 9);
      worldMapModePanel.Top = Math.Max(4, top);
    }

    private sealed class WorldMapPageChoice {
      public long SId { get; set; }
      public long Guid { get; set; }
      public long ParentId { get; set; }
      public string Text { get; set; }
      public override string ToString() => Text ?? String.Empty;
    }

    private static string WorldMapPageDisplayName(View_AREA.InteractiveMapPageInfo page) {
      if (page == null) return String.Empty;
      if (!String.IsNullOrWhiteSpace(page.DisplayName) && !String.Equals(page.DisplayName, page.MapName, StringComparison.OrdinalIgnoreCase))
        return page.MapName + "  —  " + page.DisplayName;
      return page.MapName ?? String.Empty;
    }

    private void RebuildWorldMapPageCombo() {
      if (worldMapPageCombo == null || panelRender == null) return;
      List<View_AREA.InteractiveMapPageInfo> pages = panelRender.GetInteractiveMapPageInfos();
      long selected = panelRender.InteractiveMapSelectedPageSId;
      long selectedGuid = panelRender.InteractiveMapSelectedPageGuid;
      updatingWorldMapPageCombo = true;
      try {
        worldMapPageCombo.BeginUpdate();
        worldMapPageCombo.Items.Clear();
        if (pages.Count == 0) {
          worldMapPageCombo.Enabled = false;
          return;
        }

        var byParent = pages.GroupBy(p => p.ParentId).ToDictionary(g => g.Key, g => g
          .OrderByDescending(x => x.HasImage).ThenBy(x => x.MapName ?? String.Empty, StringComparer.OrdinalIgnoreCase).ToList());
        var visited = new HashSet<long>();
        Action<long,int> addChildren = null;
        addChildren = (parentId, depth) => {
          if (!byParent.TryGetValue(parentId, out List<View_AREA.InteractiveMapPageInfo> children)) return;
          foreach (View_AREA.InteractiveMapPageInfo page in children) {
            long key = page.SId != 0 ? page.SId : page.Guid;
            if (!visited.Add(key)) continue;
            string prefix = depth == 0 ? String.Empty : new string(' ', Math.Min(12, depth * 2)) + "└─ ";
            worldMapPageCombo.Items.Add(new WorldMapPageChoice { SId = page.SId, Guid = page.Guid, ParentId = page.ParentId, Text = prefix + WorldMapPageDisplayName(page) });
            addChildren(page.SId != 0 ? page.SId : page.Guid, depth + 1);
          }
        };

        // Root pages first, followed by any orphaned pages (old/Beta data can contain incomplete parent links).
        addChildren(0L, 0);
        foreach (View_AREA.InteractiveMapPageInfo page in pages.OrderBy(x => x.MapName ?? String.Empty, StringComparer.OrdinalIgnoreCase)) {
          long key = page.SId != 0 ? page.SId : page.Guid;
          if (visited.Contains(key)) continue;
          string text = "└─ " + WorldMapPageDisplayName(page);
          worldMapPageCombo.Items.Add(new WorldMapPageChoice { SId = page.SId, Guid = page.Guid, ParentId = page.ParentId, Text = text });
          visited.Add(key);
          addChildren(page.SId != 0 ? page.SId : page.Guid, 1);
        }

        int selectedIndex = -1;
        for (int i = 0; i < worldMapPageCombo.Items.Count; i++) {
          if (worldMapPageCombo.Items[i] is WorldMapPageChoice choice &&
              ((selected != 0 && choice.SId == selected) || (selected == 0 && selectedGuid != 0 && choice.Guid == selectedGuid))) {
            selectedIndex = i; break;
          }
        }
        if (selectedIndex >= 0) worldMapPageCombo.SelectedIndex = selectedIndex;
        else if (worldMapPageCombo.Items.Count > 0) worldMapPageCombo.SelectedIndex = 0;
        worldMapPageCombo.Enabled = worldMapPageCombo.Items.Count > 0 && !panelRender.InteractiveMapTravelMode;
      } finally {
        worldMapPageCombo.EndUpdate();
        updatingWorldMapPageCombo = false;
      }
    }

    internal void RefreshWorldMapModeControls() {
      if (_closing || IsDisposed || Disposing) return;
      if (InvokeRequired) {
        try { BeginInvoke(new Action(RefreshWorldMapModeControls)); } catch { }
        return;
      }
      EnsureWorldMapModeControls();
      if (worldMapModePanel == null) return;
      bool active = worldFullMapActive && panelRender?.IsFullMapOpen == true;
      worldMapModePanel.Visible = active;
      if (!active) return;
      LayoutWorldMapModeControls();
      RebuildWorldMapPageCombo();

      bool travel = panelRender.InteractiveMapTravelMode;
      bool world = panelRender.InteractiveMapIsWorldScope;
      bool hasArea = panelRender.InteractiveMapHasAreaScope;
      bool originalPreference = panelRender.InteractiveMapPrefersOriginalArt;
      bool originalAvailable = panelRender.InteractiveMapOriginalArtAvailable;
      bool originalActive = panelRender.InteractiveMapOriginalArtActive;
      string page = world ? panelRender.InteractiveMapWorldName : panelRender.InteractiveMapAreaName;
      string scope = world ? "World map" : "Area" + (String.IsNullOrWhiteSpace(page) ? String.Empty : ": " + page);
      worldMapModeLabel.Text = scope + " · " + (originalActive ? "in-game" : "PugTools");
      worldMapModeLabel.ForeColor = originalPreference && !originalAvailable ? Color.Khaki : Color.FromArgb(232, 245, 248);

      // Keep the controls visually enabled even on travel/world-only maps. A disabled WinForms button ignores custom
      // foreground colours and became almost unreadable on the dark map overlay. Clicks simply explain why scope is fixed.
      worldMapScopeButton.Enabled = true;
      worldMapScopeButton.Text = travel || (world && !hasArea) ? "World map" : world ? "Area map" : "World map";
      worldMapScopeButton.Width = 118;
      worldMapScopeButton.Tag = null;
      worldMapScopeButton.BackColor = Color.FromArgb(24, 47, 57);
      worldMapScopeButton.ForeColor = Color.FromArgb(221, 241, 247);

      worldMapSourceButton.Enabled = true;
      worldMapSourceButton.Text = originalPreference ? "PugTools map" : "Original in-game map";
      worldMapSourceButton.BackColor = originalActive ? Color.FromArgb(62, 54, 24) : Color.FromArgb(24, 47, 57);
      worldMapSourceButton.ForeColor = originalPreference && !originalAvailable ? Color.Khaki : Color.FromArgb(232, 245, 248);
      worldMapSourceButton.Tag = originalAvailable ? null : "No original SWTOR art is available for this map page.";
      var tip = worldMapNoteToolTip;
      if (tip != null) {
        tip.SetToolTip(worldMapScopeButton, travel ? "Travel maps always use the world scope." : hasArea ? "Switch between the current SWTOR submap and the world map." : "No separate area map exists here.");
        tip.SetToolTip(worldMapSourceButton, originalAvailable ? "Switch between the generated PugTools map and the original SWTOR map art." : "No original SWTOR art exists for this page; the generated map remains visible.");
      }
      worldMapModePanel.BringToFront();
    }

    private static bool IsMapLinkNote(AreaMapNote note) {
      if (note == null) return false;
      if (note.MapLinkAreaId != 0 || note.MapLinkMapNameSId != 0 || note.MapLinkSubmapNameSId != 0) return true;
      string key = note.Icon ?? String.Empty;
      return key.IndexOf("maplink", StringComparison.OrdinalIgnoreCase) >= 0 ||
        key.IndexOf("map_link", StringComparison.OrdinalIgnoreCase) >= 0 ||
        key.IndexOf("exit", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    internal bool TryOpenMapLinkForMapNote(AreaMapNote note, bool fromMiniMap) {
      if (!IsMapLinkNote(note) || panelRender == null) return false;
      UpdateWorldMapNoteToolTip(null, Point.Empty, fromMiniMap ? miniMapPicture : null);
      if (note.MapLinkAreaId == 0 && note.MapLinkMapNameSId == 0 && note.MapLinkSubmapNameSId == 0) {
        SetStatusLabel("Map exit symbol found, but this client entry does not contain a linked map target.");
        return true;
      }

      ulong targetArea = note.MapLinkAreaId != 0 ? note.MapLinkAreaId : currentAreaId;
      if (targetArea == 0) targetArea = currentAreaId;
      if (targetArea == currentAreaId) {
        bool opened = fromMiniMap
          ? panelRender.OpenMiniMapLink(note.MapLinkMapNameSId, note.MapLinkSubmapNameSId)
          : panelRender.OpenInteractiveMapLink(note.MapLinkMapNameSId, note.MapLinkSubmapNameSId);
        if (!opened) SetStatusLabel("Map exit found, but the linked map page is not available in this area.");
        else if (fromMiniMap) {
          miniMapTitle.Text = "Minimap — rendering…";
          miniMapPicture?.Invalidate();
        }
        return true;
      }

      TreeNode targetNode = FindMapLinkWorldAreaTreeNode(treeViewFast1?.Nodes, targetArea);
      if (targetNode == null) {
        SetStatusLabel("Map exit points to area " + targetArea.ToString(System.Globalization.CultureInfo.InvariantCulture) +
          ", but that area is not available in the current World Browser tree.");
        return true;
      }

      pendingMapLinkAreaId = targetArea;
      pendingMapLinkMapNameSId = note.MapLinkMapNameSId;
      pendingMapLinkSubmapNameSId = note.MapLinkSubmapNameSId;
      pendingMapLinkMiniMap = fromMiniMap;
      SetStatusLabel("Opening linked area…");
      targetNode.EnsureVisible();
      treeViewFast1.SelectedNode = targetNode;
      return true;
    }

    private static TreeNode FindMapLinkWorldAreaTreeNode(TreeNodeCollection nodes, ulong areaId) {
      if (nodes == null || areaId == 0) return null;
      foreach (TreeNode node in nodes) {
        if (node?.Tag is NodeAsset tag && tag.dynObject is TorArchive.HashFileInfo &&
          UInt64.TryParse((tag.id ?? String.Empty).Trim('/'), out ulong id) && id == areaId) return node;
        TreeNode child = FindMapLinkWorldAreaTreeNode(node?.Nodes, areaId);
        if (child != null) return child;
      }
      return null;
    }

    private void ApplyPendingMapLinkAfterAreaLoad() {
      if (pendingMapLinkAreaId == 0 || pendingMapLinkAreaId != currentAreaId || panelRender == null) return;
      bool mini = pendingMapLinkMiniMap;
      long mapId = pendingMapLinkMapNameSId, submapId = pendingMapLinkSubmapNameSId;
      pendingMapLinkAreaId = 0; pendingMapLinkMapNameSId = 0; pendingMapLinkSubmapNameSId = 0; pendingMapLinkMiniMap = false;

      bool opened;
      if (mini) {
        if (btnWorldMiniMap != null) btnWorldMiniMap.Checked = true;
        SetMiniMapVisible(true);
        opened = panelRender.OpenMiniMapLink(mapId, submapId);
        if (opened && miniMapTitle != null) miniMapTitle.Text = "Minimap — rendering…";
      } else opened = panelRender.OpenInteractiveMapLink(mapId, submapId);
      if (!opened) SetStatusLabel("Linked area loaded, but the requested map page is not available in its local map data.");
    }

    // Map taxi pins are an interaction surface too.  Resolve the physical terminal from the pin's world position,
    // then reuse exactly the same reachable-route builder and route-map UI as a click on the droid in the 3D world.
    internal bool TryOpenTaxiMapForMapNote(AreaMapNote note) {
      if (note == null || panelRender == null) return false;
      string icon = note.Icon ?? String.Empty;
      bool taxi = String.Equals(note.ServiceKind, "Taxi", StringComparison.OrdinalIgnoreCase) ||
        icon.IndexOf("taxi", StringComparison.OrdinalIgnoreCase) >= 0;
      if (!taxi) return false;

      List<WorldTaxiRouteInfo> graph;
      lock (worldTaxiRoutesSync) graph = worldTaxiRoutes
        .Where(r => r?.FromTaxiGom == true && r.Path?.Points != null && r.Path.Points.Count >= 2)
        .ToList();
      if (graph.Count == 0) return false;

      string source = TaxiInferTerminalSpec(note.Position, graph);
      List<WorldTaxiRouteInfo> routes = !String.IsNullOrWhiteSpace(source)
        ? graph.Where(r => String.Equals(r.SourceFqn, source, StringComparison.OrdinalIgnoreCase)).ToList()
        : new List<WorldTaxiRouteInfo>();

      if (routes.Count == 0) {
        // Older/static taxi mpn rows do not carry taxTerminalSpec.  A route's physical first point is authoritative
        // enough here because the user clicked a taxi-classified map symbol, not an arbitrary nearby object.
        float bestD2 = TaxiPhysicalEndpointRadius * TaxiPhysicalEndpointRadius;
        string nearestSource = null;
        foreach (WorldTaxiRouteInfo candidate in graph) {
          AreaPathPoint start = candidate.Reversed ? candidate.Path.Points[candidate.Path.Points.Count - 1] : candidate.Path.Points[0];
          float d2 = (start.Position - note.Position).LengthSquared();
          if (d2 < bestD2) { bestD2 = d2; nearestSource = candidate.SourceFqn; }
        }
        if (!String.IsNullOrWhiteSpace(nearestSource))
          routes = graph.Where(r => String.Equals(r.SourceFqn, nearestSource, StringComparison.OrdinalIgnoreCase)).ToList();
      }
      if (routes.Count == 0) {
        SetStatusLabel("Taxi map symbol found, but no local route starts at this terminal.");
        return true;
      }

      List<WorldTaxiRouteInfo> reachable = BuildReachableTaxiRoutes(routes, note.Position);
      if (reachable.Count > 0) routes = reachable;
      string label = !String.IsNullOrWhiteSpace(note.Label) ? note.Label :
        routes.Select(r => r.SourceLabel).FirstOrDefault(x => !String.IsNullOrWhiteSpace(x)) ?? "Taxi terminal";
      panelRender.OpenTaxiRouteMap(routes, label);
      UpdateWorldMapNoteToolTip(null, Point.Empty);
      ActivateWorldRenderInput();
      return true;
    }
  }
}
