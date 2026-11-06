using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GomLib.Models;

namespace PugTools {
  public partial class WorldBrowser {
    private sealed class WorldConversationNavigatorEntry {
      public WorldNpcPlacement Placement;
      public string Label;
      public string RoomName => Placement?.Room?.RoomName ?? String.Empty;
      public string Fqn => Placement?.SourceFqn ?? String.Empty;
      public string ConversationName => Placement?.Interaction?.Conversation ?? String.Empty;
      public string ConversationId => Placement?.Interaction?.ConversationId.ToString() ?? String.Empty;
      public override string ToString() => Label;
    }

    private sealed class WorldConversationListComparer : System.Collections.IComparer {
      private readonly int column;
      private readonly bool descending;
      public WorldConversationListComparer(int column, bool descending) { this.column = column; this.descending = descending; }
      public int Compare(object x, object y) {
        var a = x as ListViewItem;
        var b = y as ListViewItem;
        if (a == null || b == null) return 0;
        int result;
        if (column == 4 && a.Tag is WorldConversationNavigatorEntry ea && b.Tag is WorldConversationNavigatorEntry eb) {
          result = ea.Placement.Interaction.ConversationId.CompareTo(eb.Placement.Interaction.ConversationId);
        } else {
          string av = column < a.SubItems.Count ? a.SubItems[column].Text : String.Empty;
          string bv = column < b.SubItems.Count ? b.SubItems[column].Text : String.Empty;
          result = StringComparer.CurrentCultureIgnoreCase.Compare(av, bv);
        }
        return descending ? -result : result;
      }
    }

    private void ShowWorldConversationNavigator() {
      if (currentDom == null) return;
      var entries = worldNpcPlacements
        .Where(p => p?.Interaction?.HasConversation == true)
        .Select(p => new WorldConversationNavigatorEntry {
          Placement = p,
          Label = (!String.IsNullOrWhiteSpace(p.Title)
            ? p.Title + " — " + (p.Name ?? p.SourceFqn ?? "NPC")
            : (p.Name ?? p.SourceFqn ?? "NPC"))
        })
        .GroupBy(e => (e.Placement.SourceFqn ?? "") + "|" + e.Placement.Interaction.ConversationId + "|" + e.Placement.Interaction.Conversation)
        .Select(g => g.First())
        .ToList();

      using (var dialog = new Form {
        Text = "Area conversations",
        StartPosition = FormStartPosition.CenterParent,
        FormBorderStyle = FormBorderStyle.Sizable,
        MinimizeBox = false,
        MaximizeBox = true,
        ShowInTaskbar = false,
        ClientSize = new Size(900, 500),
        MinimumSize = new Size(650, 360)
      }) {
        var search = new TextBox { Dock = DockStyle.Top, PlaceholderText = "Search NPC, room, FQN, title or conversation…" };
        var list = new ListView {
          Dock = DockStyle.Fill,
          View = View.Details,
          FullRowSelect = true,
          GridLines = true,
          HideSelection = false,
          MultiSelect = false,
          Sorting = SortOrder.None
        };
        list.Columns.Add("NPC / title", 230);
        list.Columns.Add("Room", 150);
        list.Columns.Add("FQN", 310);
        list.Columns.Add("Conversation", 180);
        list.Columns.Add("ID", 85);
        var hint = new Label { Dock = DockStyle.Bottom, Height = 34, TextAlign = ContentAlignment.MiddleLeft,
          Text = "Choose a conversation and double-click or press Start. Click a column header to sort." };
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(4) };
        var close = new Button { Text = "Close", DialogResult = DialogResult.Cancel, Width = 90 };
        var start = new Button { Text = "Start conversation", Width = 140 };
        buttons.Controls.Add(close);
        buttons.Controls.Add(start);
        List<WorldConversationNavigatorEntry> visible = new List<WorldConversationNavigatorEntry>();
        int sortColumn = 1;
        bool sortDescending = false;
        void ApplySort() {
          list.ListViewItemSorter = new WorldConversationListComparer(sortColumn, sortDescending);
          list.Sort();
        }
        void Rebuild() {
          string q = (search.Text ?? "").Trim();
          visible = entries.Where(e => q.Length == 0 ||
            e.Label.IndexOf(q, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
            e.Fqn.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
            e.RoomName.IndexOf(q, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
            e.ConversationName.IndexOf(q, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
            e.ConversationId.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
          list.BeginUpdate();
          list.Items.Clear();
          foreach (var entry in visible) {
            var item = new ListViewItem(entry.Label);
            item.SubItems.Add(entry.RoomName);
            item.SubItems.Add(entry.Fqn);
            item.SubItems.Add(entry.ConversationName);
            item.SubItems.Add(entry.ConversationId);
            item.Tag = entry;
            list.Items.Add(item);
          }
          ApplySort();
          list.EndUpdate();
          hint.Text = visible.Count + " conversation entr" + (visible.Count == 1 ? "y" : "ies") +
            " from the loaded area. Click a column header to sort; NPC selection in the 3D view is not required.";
          start.Enabled = list.SelectedItems.Count > 0;
        }
        void Launch() {
          if (list.SelectedItems.Count == 0) return;
          var item = list.SelectedItems[0];
          var entry = item.Tag as WorldConversationNavigatorEntry;
          if (entry == null) return;
          if (!TryLoadWorldConversation(entry.Placement.Interaction, out Conversation conversation, out string error)) {
            MessageBox.Show(dialog, error ?? "The conversation could not be loaded.", "Conversation navigator", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
          }
          try {
            // Keep the actual AREA placement as the conversation initiator. This is important for
            // staging marks, speaker resolution and conversations whose camera targets are relative
            // to the NPC instance; the user does not have to select that instance in the viewport.
            StartWorldConversationPlayback(conversation, entry.Placement);
            dialog.Close();
          } catch (Exception ex) {
            MessageBox.Show(dialog,
              "The conversation was loaded, but playback could not be started.\r\n\r\n" + ex.Message,
              "Conversation navigator", MessageBoxButtons.OK, MessageBoxIcon.Warning);
          }
        }
        search.TextChanged += (_, __) => Rebuild();
        list.DoubleClick += (_, __) => Launch();
        list.ColumnClick += (_, e) => {
          if (sortColumn == e.Column) sortDescending = !sortDescending;
          else { sortColumn = e.Column; sortDescending = false; }
          ApplySort();
        };
        list.SelectedIndexChanged += (_, __) => start.Enabled = list.SelectedItems.Count > 0;
        start.Click += (_, __) => Launch();
        dialog.Controls.Add(list);
        dialog.Controls.Add(hint);
        dialog.Controls.Add(buttons);
        dialog.Controls.Add(search);
        dialog.AcceptButton = start;
        dialog.CancelButton = close;
        dialog.Shown += (_, __) => { Rebuild(); search.Focus(); };
        start.Enabled = false;
        dialog.ShowDialog(this);
      }
    }
  }
}
