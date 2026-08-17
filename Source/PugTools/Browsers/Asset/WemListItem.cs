using System;
using System.Collections.Generic;
// using System.Diagnostics;
using System.Linq;

namespace PugTools {
  public class WemListItem { // Must be public for interop with ObjectListView
    public String Name { get; } // Must be public for interop with ObjectListView
    public Int32 Size { get; } // Must be public for interop with ObjectListView
    public String Value { get; } // Must be public for interop with ObjectListView

    internal List<WemListItem> Children { get; } = new List<WemListItem>();
    internal ViewWEM Obj { get; }

    internal WemListItem(FileFormat_BNK_STID_SoundBank bnk) {
      Name = bnk.Name;
    }
    internal WemListItem(String name, FileFormat_BNK_DIDX didx) {
      Name = name;
      Size = 0;
      Value = "";
      BnkIdDict dict = BnkIdDict.Instance;

      if (didx.Wems.Count > 0) {
        foreach (ViewWEM wem in didx.Wems) {
          _ = uint.TryParse(wem.WemName.Replace(".wem", ""), out UInt32 id);

          if (dict.Data.Keys.Contains(id)) wem.WemName = dict.Data[id] + ".wem";

          Children.Add(new WemListItem(wem.WemName.ToString(), wem));
        }
      }
    }
    internal WemListItem(String name, FileFormat_BNK_HIRC hirc) {
      Name = name;

      if (hirc.NumObject > 0) {
        Dictionary<UInt32, List<FileFormat_BNK_HIRC_Object>> hircDict =
          new Dictionary<UInt32, List<FileFormat_BNK_HIRC_Object>>();

        foreach (FileFormat_BNK_HIRC_Object obj in hirc.Objects) {
          if (!hircDict.Keys.Contains(obj.Type)) {
            List<FileFormat_BNK_HIRC_Object> objList = new List<FileFormat_BNK_HIRC_Object>{
              obj
            };
            hircDict.Add(obj.Type, objList);

          } else {
            hircDict[obj.Type].Add(obj);
          }
        }

        foreach (KeyValuePair<UInt32, List<FileFormat_BNK_HIRC_Object>> kvp in hircDict) {
          String displayName;

          switch (kvp.Key) {
            case 1:
              displayName = "1 - Settings";
              break;

            case 2:
              displayName = "2 - Sound SFX/Sound Voice";
              break;

            case 3:
              displayName = "3 - Event Action";
              break;

            case 4:
              displayName = "4 - Event";
              break;

            case 5:
              displayName = "5 - Random Container or Sequence Container";
              break;

            case 6:
              displayName = "6 - Switch Container";
              break;

            case 7:
              displayName = "7 - Actor-Mixer";
              break;

            case 8:
              displayName = "8 - Audio Bus";
              break;

            case 9:
              displayName = "9 - Blend Container";
              break;

            case 10:
              displayName = "10 - Music Segment";
              break;

            case 11:
              displayName = "11 - Music Track";
              break;

            case 12:
              displayName = "12 - Music Switch Container";
              break;

            case 13:
              displayName = "13 - Music Playlist Container";
              break;

            case 14:
              displayName = "14 - Attenuation";
              break;

            case 15:
              displayName = "15 - Dialogue Event";
              break;

            case 16:
              displayName = "16 - Motion Bus";
              break;

            case 17:
              displayName = "17 - Motion FX";
              break;

            case 18:
              displayName = "18 - Effect";
              break;

            case 20:
              displayName = "20 - Auxiliary Bus";
              break;

            default:
              displayName = "**UNKNOWN**";
              break;

          }

          Children.Add(new WemListItem(displayName, kvp.Value));
        }

        //Debug.WriteLine("pause here");
      }
    }
    internal WemListItem(String name, FileFormat_BNK_HIRC_Object objList) {
      if (objList == null) {
        throw new ArgumentNullException(nameof(objList));
      }

      Name = name;
      BnkIdDict dict = BnkIdDict.Instance;
      _ = uint.TryParse(Name, out UInt32 id);

      if (dict.Data.Keys.Contains(id)) Value = dict.Data[id];
    }
    internal WemListItem(String name, FileFormat_BNK_STID stid) {
      Name = name;
      _ = BnkIdDict.Instance;

      if (stid.NumSoundBanks > 0) {
        foreach (FileFormat_BNK_STID_SoundBank bnk in stid.SoundBanks) {
          Children.Add(new WemListItem(bnk));
        }
      }
    }
    internal WemListItem(String name, List<FileFormat_BNK_HIRC_Object> objList) {
      Name = name;

      foreach (FileFormat_BNK_HIRC_Object obj in objList) {
        FileFormat_BNK_HIRC_Object hircObj = obj;
        Children.Add(new WemListItem(obj.Id.ToString(), hircObj));
      }
    }
    internal WemListItem(String name, ViewWEM obj) {
      Name = name;
      Obj = obj;
      Size = Obj.Data.Length / 1024;
      Value = "";
    }
    internal static void ResetTreeListViewColumns(BrightIdeasSoftware.TreeListView tlv) {
      BrightIdeasSoftware.OLVColumn olvColumn1 = new BrightIdeasSoftware.OLVColumn();
      BrightIdeasSoftware.OLVColumn olvColumn2 = new BrightIdeasSoftware.OLVColumn();
      BrightIdeasSoftware.OLVColumn olvColumn3 = new BrightIdeasSoftware.OLVColumn();

      olvColumn1.AspectName = nameof(Name);
      olvColumn1.CellPadding = null;
      olvColumn1.Text = "Name";

      olvColumn2.AspectName = nameof(Value);
      olvColumn2.CellPadding = null;
      olvColumn2.Text = "Event / Action";
      olvColumn2.MinimumWidth = 90;

      olvColumn3.AspectName = nameof(Size);
      olvColumn3.CellPadding = null;
      olvColumn3.Text = "Size (KB)";
      olvColumn3.MinimumWidth = 60;

      tlv.Columns.Clear();
      tlv.Columns.Add(olvColumn1);
      tlv.Columns.Add(olvColumn2);
      tlv.Columns.Add(olvColumn3);
    }
  }
}
