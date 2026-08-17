using System;
using TorArchive;

namespace PugTools {
  internal class TreeListItem {
    internal String Id { get; set; }
    internal String DisplayName { get; set; }
    internal HashFileInfo HashInfo { get; set; }
    internal String ParentId { get; set; }

    internal TreeListItem(String id, String parent, String display, HashFileInfo hashInfo) {
      Id = id;
      ParentId = parent;
      DisplayName = display;
      HashInfo = hashInfo;
    }
  }
}
