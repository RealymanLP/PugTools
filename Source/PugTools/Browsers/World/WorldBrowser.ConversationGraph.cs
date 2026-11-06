using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using GomLib.Models;

namespace PugTools {
  // The conversation as a graph, left to right, like the strip Jedipedia draws above a playing conversation: the line
  // that plays is the spine, every answer the player could give hangs below it, the lines already played are lit and
  // the line playing now is ringed. It lives in the playback window and can be hidden there.
  public partial class WorldBrowser {
    private sealed class WorldConversationGraphNode {
      public long Id;
      public DialogNode Node;
      public bool IsStart, IsChoice, IsAmbient, IsEmpty;
      public int Col, Lane;
      public readonly List<WorldConversationGraphEdge> Out = new List<WorldConversationGraphEdge>();
    }

    private sealed class WorldConversationGraphEdge {
      public WorldConversationGraphNode From, To;
      // Back closes a loop (drawn apart, and left out of the column order); Tree is the edge that first reached To.
      public bool Back, Tree;
    }

    private sealed class WorldConversationGraphView : Panel {
      private const int ColW = 38, LaneH = 24, Pad = 26, MaxNodes = 3000, MaxDepth = 1200;

      private readonly Func<WorldConversationGraphNode, string> tipOf;
      private readonly ToolTip tip = new ToolTip { InitialDelay = 250, ReshowDelay = 100, AutoPopDelay = 20000 };
      private readonly List<WorldConversationGraphNode> nodes = new List<WorldConversationGraphNode>();
      private readonly Dictionary<long, WorldConversationGraphNode> byId = new Dictionary<long, WorldConversationGraphNode>();
      private readonly HashSet<long> visited = new HashSet<long>();
      private readonly HashSet<string> taken = new HashSet<string>();
      private readonly HashSet<long> stack = new HashSet<long>();
      private readonly HashSet<long> occupied = new HashSet<long>();
      private readonly List<WorldConversationGraphNode> pre = new List<WorldConversationGraphNode>();
      private readonly List<WorldConversationGraphNode> post = new List<WorldConversationGraphNode>();
      private Conversation conversation;
      private long current;
      private WorldConversationGraphNode hot;
      private bool truncated;
      private float scale = 1.25f;
      private int maxCol, maxLane;

      public event Action<WorldConversationGraphNode> NodeClicked;
      public event Action<float> ZoomChanged;

      public WorldConversationGraphView(Func<WorldConversationGraphNode, string> tipOf) {
        this.tipOf = tipOf;
        DoubleBuffered = true;
        ResizeRedraw = true;
        AutoScroll = true;
        BackColor = Color.FromArgb(8, 19, 23);
        SetStyle(ControlStyles.Selectable, true);
        TabStop = true;
      }

      protected override void Dispose(bool disposing) {
        if (disposing) tip.Dispose();
        base.Dispose(disposing);
      }

      public bool Truncated { get { return truncated; } }

      public void Build(Conversation source, List<long> roots) {
        conversation = source;
        nodes.Clear(); byId.Clear(); visited.Clear(); taken.Clear(); stack.Clear(); occupied.Clear(); pre.Clear(); post.Clear();
        current = 0; hot = null; truncated = false;
        tip.SetToolTip(this, String.Empty);
        if (source?.NodeLookup == null || source.NodeLookup.Count == 0) { AutoScrollMinSize = Size.Empty; Invalidate(); return; }

        // The conversation's beginning is a place, not a line: every root hangs from it.
        var start = new WorldConversationGraphNode { Id = 0, IsStart = true };
        nodes.Add(start); byId[0] = start; stack.Add(0); pre.Add(start);
        foreach (long root in roots ?? new List<long>()) Expand(start, root, 1);
        stack.Remove(0); post.Add(start);

        // Columns: the longest way from the beginning, loops left out. A reverse post-order walk is a topological order
        // of what remains.
        for (int i = post.Count - 1; i >= 0; i--) {
          WorldConversationGraphNode n = post[i];
          foreach (WorldConversationGraphEdge e in n.Out)
            if (!e.Back && e.To.Col < n.Col + 1) e.To.Col = n.Col + 1;
        }

        // Lanes: a node goes on the first lane at or below its parent's where the way from the parent is clear, so
        // the first answer continues the line and the others hang beneath it.
        Occupy(0, 0, 0);
        foreach (WorldConversationGraphNode n in pre) {
          foreach (WorldConversationGraphEdge e in n.Out) {
            if (!e.Tree) continue;
            WorldConversationGraphNode child = e.To;
            int lane = n.Lane;
            while (lane < 5000 && !RunFree(lane, n.Col + 1, child.Col)) lane++;
            child.Lane = lane;
            Occupy(lane, n.Col + 1, child.Col);
          }
        }
        maxCol = nodes.Max(n => n.Col); maxLane = nodes.Max(n => n.Lane);
        UpdateExtent();
        AutoScrollPosition = new Point(0, 0);
        Invalidate();
      }

      private void UpdateExtent() {
        AutoScrollMinSize = new Size((int)(Pad * 2 + maxCol * ColW * scale), (int)(Pad * 2 + maxLane * LaneH * scale));
      }

      public float Zoom {
        get { return scale; }
        set { scale = Math.Max(.6f, Math.Min(3.5f, value)); if (nodes.Count > 0) { UpdateExtent(); Invalidate(); } }
      }

      // Zoom about a point of the visible area, so the part under the mouse stays under it.
      public void ZoomBy(float factor, Point anchor) {
        float before = scale;
        float after = Math.Max(.6f, Math.Min(3.5f, before * factor));
        if (Math.Abs(after - before) < .001f || nodes.Count == 0) return;
        float ratio = after / before;
        Point at = new Point(-AutoScrollPosition.X, -AutoScrollPosition.Y);
        scale = after;
        UpdateExtent();
        AutoScrollPosition = new Point(Math.Max(0, (int)((at.X + anchor.X) * ratio - anchor.X)), Math.Max(0, (int)((at.Y + anchor.Y) * ratio - anchor.Y)));
        Invalidate();
        ZoomChanged?.Invoke(scale);
      }

      private void Expand(WorldConversationGraphNode parent, long raw, int depth) {
        long id = ResolveWorldConversationLinkTarget(conversation, raw, out _);
        DialogNode dialog = null;
        if (id == 0 || conversation.NodeLookup == null || !conversation.NodeLookup.TryGetValue(id, out dialog) || dialog == null) return;
        if (byId.TryGetValue(id, out WorldConversationGraphNode existing)) {
          // Reached again: a rejoin, or the closing of a loop when the node is still on the way down.
          parent.Out.Add(new WorldConversationGraphEdge { From = parent, To = existing, Back = stack.Contains(id) });
          return;
        }
        if (nodes.Count >= MaxNodes || depth > MaxDepth) { truncated = true; return; }
        var child = new WorldConversationGraphNode { Id = id, Node = dialog, IsChoice = dialog.IsPlayerNode, IsAmbient = dialog.IsAmbient, IsEmpty = dialog.IsEmpty };
        nodes.Add(child); byId[id] = child;
        parent.Out.Add(new WorldConversationGraphEdge { From = parent, To = child, Tree = true });
        stack.Add(id); pre.Add(child);
        if (dialog.ChildIds != null) foreach (int next in dialog.ChildIds) Expand(child, next, depth + 1);
        stack.Remove(id); post.Add(child);
      }

      private static long Cell(int col, int lane) { return (long)col * 100000L + lane; }
      private bool RunFree(int lane, int from, int to) {
        for (int c = from; c <= to; c++) if (occupied.Contains(Cell(c, lane))) return false;
        return true;
      }
      private void Occupy(int lane, int from, int to) { for (int c = from; c <= to; c++) occupied.Add(Cell(c, lane)); }

      private PointF Where(WorldConversationGraphNode n) { return new PointF(Pad + n.Col * ColW * scale, Pad + n.Lane * LaneH * scale); }

      // The node the playback is on now; it also becomes part of the walked line.
      public void SetCurrent(long id) {
        long previous = current;
        current = id;
        if (id != 0) {
          visited.Add(id);
          taken.Add(previous.ToString(CultureInfo.InvariantCulture) + ">" + id.ToString(CultureInfo.InvariantCulture));
          if (byId.TryGetValue(id, out WorldConversationGraphNode n)) {
            PointF p = Where(n);
            AutoScrollPosition = new Point(Math.Max(0, (int)p.X - ClientSize.Width / 2), Math.Max(0, (int)p.Y - ClientSize.Height / 2));
          }
        }
        Invalidate();
      }

      public void ClearCurrent() { current = 0; Invalidate(); }

      protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);
        if (nodes.Count == 0) return;
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TranslateTransform(AutoScrollPosition.X, AutoScrollPosition.Y);
        using (var untaken = new Pen(Color.FromArgb(95, 122, 138), 1.2f) { DashStyle = DashStyle.Dash })
        using (var loop = new Pen(Color.FromArgb(190, 140, 90), 1.2f) { DashStyle = DashStyle.Dot })
        using (var lit = new Pen(Color.FromArgb(88, 205, 130), 2.6f)) {
          foreach (WorldConversationGraphNode n in nodes)
            foreach (WorldConversationGraphEdge edge in n.Out) {
              bool walked = taken.Contains(n.Id.ToString(CultureInfo.InvariantCulture) + ">" + edge.To.Id.ToString(CultureInfo.InvariantCulture));
              DrawEdge(g, edge, walked ? lit : edge.Back ? loop : untaken);
            }
        }
        foreach (WorldConversationGraphNode n in nodes) DrawNode(g, n);
      }

      private void DrawEdge(Graphics g, WorldConversationGraphEdge edge, Pen pen) {
        PointF a = Where(edge.From), b = Where(edge.To);
        if (a.Y == b.Y && !edge.Back) { g.DrawLine(pen, a, b); return; }
        if (a.Y == b.Y) {
          // A loop back along one lane goes round beneath it.
          g.DrawBezier(pen, a, new PointF(a.X, a.Y + LaneH * scale * .9f), new PointF(b.X, b.Y + LaneH * scale * .9f), b);
          return;
        }
        float mid = (a.X + b.X) / 2f;
        g.DrawBezier(pen, a, new PointF(mid, a.Y), new PointF(mid, b.Y), b);
      }

      private void DrawNode(Graphics g, WorldConversationGraphNode n) {
        PointF p = Where(n);
        bool isCurrent = n.Id == current && current != 0;
        bool walked = visited.Contains(n.Id);
        Color fill = walked ? Color.FromArgb(88, 205, 130) : n.IsChoice ? Color.FromArgb(69, 153, 201) : Color.FromArgb(120, 135, 142);
        float k = Math.Max(1f, scale);
        float r = (isCurrent ? 6.5f : n.IsStart ? 4.5f : 4f) * k;
        var box = new RectangleF(p.X - r, p.Y - r, r * 2, r * 2);
        if (n.IsStart || n.IsEmpty || n.IsAmbient) {
          using (var pen = new Pen(fill, 1.6f)) g.DrawEllipse(pen, box);
        } else {
          using (var brush = new SolidBrush(fill)) g.FillEllipse(brush, box);
        }
        if (isCurrent) using (var ring = new Pen(Color.White, 2f)) g.DrawEllipse(ring, p.X - r - 2.5f * k, p.Y - r - 2.5f * k, (r + 2.5f * k) * 2, (r + 2.5f * k) * 2);
      }

      private WorldConversationGraphNode HitTest(Point client) {
        float x = client.X - AutoScrollPosition.X, y = client.Y - AutoScrollPosition.Y;
        float reach = Math.Max(10f, 7f * scale);
        WorldConversationGraphNode best = null; float bestDistance = reach * reach;
        foreach (WorldConversationGraphNode n in nodes) {
          PointF p = Where(n);
          float d = (p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y);
          if (d <= bestDistance) { best = n; bestDistance = d; }
        }
        return best;
      }

      protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        WorldConversationGraphNode hit = HitTest(e.Location);
        if (hit == hot) return;
        hot = hit;
        Cursor = hit != null ? Cursors.Hand : Cursors.Default;
        tip.SetToolTip(this, hit == null ? String.Empty : tipOf(hit));
      }

      protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); Focus(); }

      protected override void OnMouseClick(MouseEventArgs e) {
        base.OnMouseClick(e);
        WorldConversationGraphNode hit = HitTest(e.Location);
        if (hit != null) NodeClicked?.Invoke(hit);
      }

      // The graph is wide rather than tall, so the wheel moves it sideways; with Ctrl it zooms.
      protected override void OnMouseWheel(MouseEventArgs e) {
        if ((ModifierKeys & Keys.Control) == Keys.Control) { ZoomBy(e.Delta > 0 ? 1.15f : 1f / 1.15f, e.Location); return; }
        AutoScrollPosition = new Point(Math.Max(0, -AutoScrollPosition.X - e.Delta), -AutoScrollPosition.Y);
      }
    }

    private Panel worldConversationGraphHost;
    private WorldConversationGraphView worldConversationGraphView;
    private Label worldConversationGraphCaption;
    private Button worldConversationGraphToggle;
    private bool worldConversationGraphVisible = true;
    private int worldConversationGraphHeight = 190;
    private float worldConversationGraphZoom = 1.25f;

    private Panel BuildWorldConversationGraphHost() {
      worldConversationGraphView = new WorldConversationGraphView(n => n.IsStart || n.Node == null
        ? "Conversation start"
        : WorldConversationNodeTooltip(worldConversationPlaybackConversation, n.Node, null)) { Dock = DockStyle.Fill, Zoom = worldConversationGraphZoom };
      worldConversationGraphView.ZoomChanged += zoom => worldConversationGraphZoom = zoom;
      worldConversationGraphCaption = new Label {
        Dock = DockStyle.Fill, Padding = new Padding(8, 4, 6, 2), AutoEllipsis = true,
        ForeColor = Color.FromArgb(147, 178, 188), BackColor = Color.FromArgb(8, 19, 23)
      };
      // A click on a node plays the conversation from that node on (the same entry the tree preview's "play from here"
      // uses); the line it carries is shown underneath. The start point itself has nothing to play.
      worldConversationGraphView.NodeClicked += n => {
        if (worldConversationGraphCaption != null)
          worldConversationGraphCaption.Text = n.IsStart || n.Node == null ? "Conversation start"
            : WorldConversationNodeLabel(worldConversationPlaybackConversation, n.Node);
        if (!n.IsStart && n.Node != null && worldConversationPlaybackConversation != null) _ = PlayWorldConversationNodeAsync(n.Id);
      };

      // Zoom buttons next to the caption, and a grip along the lower edge to make the whole strip taller.
      Button smaller = WorldDarkActionButton("−", 28); smaller.Dock = DockStyle.Right;
      Button larger = WorldDarkActionButton("+", 28); larger.Dock = DockStyle.Right;
      smaller.Click += (_, __) => worldConversationGraphView?.ZoomBy(1f / 1.25f, new Point(worldConversationGraphView.ClientSize.Width / 2, worldConversationGraphView.ClientSize.Height / 2));
      larger.Click += (_, __) => worldConversationGraphView?.ZoomBy(1.25f, new Point(worldConversationGraphView.ClientSize.Width / 2, worldConversationGraphView.ClientSize.Height / 2));
      var captionRow = new Panel { Dock = DockStyle.Bottom, Height = 28, BackColor = Color.FromArgb(8, 19, 23) };
      captionRow.Controls.Add(worldConversationGraphCaption);
      captionRow.Controls.Add(smaller);
      captionRow.Controls.Add(larger);

      worldConversationGraphHost = new Panel {
        Dock = DockStyle.Top, Height = Math.Max(90, worldConversationGraphHeight), Visible = worldConversationGraphVisible, BackColor = Color.FromArgb(8, 19, 23)
      };
      var grip = new Panel { Dock = DockStyle.Bottom, Height = 7, Cursor = Cursors.SizeNS, BackColor = Color.FromArgb(30, 58, 70) };
      int dragFrom = 0, dragHeight = 0; bool dragging = false;
      grip.MouseDown += (_, e) => { if (e.Button != MouseButtons.Left) return; dragging = true; dragFrom = Cursor.Position.Y; dragHeight = worldConversationGraphHost.Height; };
      grip.MouseMove += (_, __) => {
        if (!dragging || worldConversationGraphHost == null) return;
        // The speaker line, the text, the answers and the buttons keep about 270 pixels of the window.
        int room = worldConversationPlaybackForm == null ? 600 : worldConversationPlaybackForm.ClientSize.Height - 270;
        worldConversationGraphHeight = Math.Max(90, Math.Min(Math.Max(120, room), dragHeight + Cursor.Position.Y - dragFrom));
        worldConversationGraphHost.Height = worldConversationGraphHeight;
      };
      grip.MouseUp += (_, __) => dragging = false;
      grip.MouseCaptureChanged += (_, __) => dragging = false;
      worldConversationGraphHost.Controls.Add(worldConversationGraphView);
      worldConversationGraphHost.Controls.Add(captionRow);
      worldConversationGraphHost.Controls.Add(grip);
      return worldConversationGraphHost;
    }

    private void ToggleWorldConversationGraph() {
      worldConversationGraphVisible = !worldConversationGraphVisible;
      if (worldConversationGraphHost != null) worldConversationGraphHost.Visible = worldConversationGraphVisible;
      if (worldConversationGraphToggle != null) worldConversationGraphToggle.Text = worldConversationGraphVisible ? "Hide graph" : "Show graph";
    }

    private void ResetWorldConversationGraph(Conversation conversation) {
      if (worldConversationGraphView == null) return;
      List<KeyValuePair<int, long>> roots = conversation?.RootNodes?.OrderBy(x => x.Key).ToList() ?? new List<KeyValuePair<int, long>>();
      if (roots.Count == 0 && conversation != null) roots = InferWorldConversationRoots(conversation);
      worldConversationGraphView.Build(conversation, roots.Select(x => x.Value).ToList());
      if (worldConversationGraphCaption != null)
        worldConversationGraphCaption.Text = worldConversationGraphView.Truncated ? "Graph truncated: the conversation is larger than it draws." : String.Empty;
    }
  }
}
