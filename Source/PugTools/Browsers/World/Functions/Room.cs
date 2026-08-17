using System.Collections.Generic;

using DataView;

namespace xxx {
  public class Room {
    public string name;
    public Dictionary<ulong, Instance> instances;

    public Room(DataView.DataView dv, string roomName) {
      _ = dv;
      name = roomName;
    }
  }
}
