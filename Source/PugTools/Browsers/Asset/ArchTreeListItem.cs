using System;
using TorArchive;

namespace PugTools {
  internal class ArchTreeListItem {
    internal Archive Arch { get; }
    internal String DisplayName { get; }
    internal String Id { get; }
    internal String ParentId { get; }

    internal ArchTreeListItem(String id, String parent, String display, Archive arch) {
      Id = id;
      ParentId = parent;
      DisplayName = display;
      Arch = arch;
    }
  }
}
