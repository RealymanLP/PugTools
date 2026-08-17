using System;
using System.Windows.Forms;
using BrightIdeasSoftware;
using TorArchive;

namespace PugTools {
  public class FileListItem { // Must be public for interop with ObjectListView
    public UInt16 CompressedMethod { get; } // Must be public for interop with ObjectListView
    public String Directory { get; } // Must be public for interop with ObjectListView
    public String Extension { get; } // Must be public for interop with ObjectListView
    public Boolean IsCompressed { get; } // Must be public for interop with ObjectListView
    public String Name { get; } // Must be public for interop with ObjectListView
    public UInt64 Offset { get; } // Must be public for interop with ObjectListView
    public UInt64 SizeCompressed { get; } // Must be public for interop with ObjectListView
    public UInt64 SizeUncompressed { get; } // Must be public for interop with ObjectListView

    internal FileListItem(HashFileInfo hashInfo, FileInfo info) {
      CompressedMethod = info.CompressionMethod;
      Directory = hashInfo.IsNamed ? hashInfo.Directory : "Unknown";
      Extension = hashInfo.Extension.ToUpper();
      IsCompressed = info.IsCompressed;
      Name = hashInfo.FileName;
      Offset = info.Offset;
      SizeCompressed = info.CompressedSize;
      SizeUncompressed = info.UncompressedSize;
    }

    internal static void ResetTreeListViewColumns(TreeListView tlv) {
      OLVColumn olvColumn1 = new OLVColumn();
      OLVColumn olvColumn2 = new OLVColumn();
      OLVColumn olvColumn3 = new OLVColumn();
      OLVColumn olvColumn4 = new OLVColumn();
      OLVColumn olvColumn5 = new OLVColumn();
      OLVColumn olvColumn6 = new OLVColumn();
      OLVColumn olvColumn7 = new OLVColumn();
      OLVColumn olvColumn8 = new OLVColumn();

      olvColumn1.AspectName = nameof(Name);
      olvColumn1.CellPadding = null;
      olvColumn1.Text = "Name";

      olvColumn2.AspectName = nameof(Extension);
      olvColumn2.CellPadding = null;
      olvColumn2.Text = "File Type";

      olvColumn3.AspectName = nameof(Directory);
      olvColumn3.CellPadding = null;
      olvColumn3.Text = "Directory";

      olvColumn4.AspectName = nameof(Offset);
      olvColumn4.CellPadding = null;
      olvColumn4.Text = "Offset";

      olvColumn5.AspectName = nameof(SizeUncompressed);
      olvColumn5.CellPadding = null;
      olvColumn5.Text = "Size";

      olvColumn6.AspectName = nameof(SizeCompressed);
      olvColumn6.CellPadding = null;
      olvColumn6.Text = "Compressed Size";

      olvColumn7.AspectName = nameof(IsCompressed);
      olvColumn7.CellPadding = null;
      olvColumn7.Text = "Is Compressed";

      olvColumn8.AspectName = nameof(CompressedMethod);
      olvColumn8.CellPadding = null;
      olvColumn8.Text = "Compressed Method";

      tlv.Columns.Clear();
      tlv.Columns.Add(olvColumn1);
      tlv.Columns.Add(olvColumn2);
      tlv.Columns.Add(olvColumn3);
      tlv.Columns.Add(olvColumn4);
      tlv.Columns.Add(olvColumn5);
      tlv.Columns.Add(olvColumn6);
      tlv.Columns.Add(olvColumn7);
      tlv.Columns.Add(olvColumn8);
    }
  }
}
