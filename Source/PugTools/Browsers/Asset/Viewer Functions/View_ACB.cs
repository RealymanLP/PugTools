using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PugTools {
  internal class ViewACB {
    internal static List<ViewWEM> ParseACB(BinaryReader br) {
      List<ViewWEM> wems = new List<ViewWEM>();
      Int64 numWEMs = br.ReadInt64();

      for (Int32 intCount = 0; intCount < (Int32)numWEMs; intCount++) {
        ViewWEM wem = new ViewWEM();

        Byte value;
        StringBuilder sb = new StringBuilder();
        while ((value = br.ReadByte()) != 0x00) { sb.Append((Char)value); }

        wem.WemName = sb.ToString();
        wem.Length = br.ReadInt64();
        wem.Offset = br.ReadInt64();

        Int64 origPos = br.BaseStream.Position;

        br.BaseStream.Seek(wem.Offset, SeekOrigin.Begin);
        wem.Data = br.ReadBytes((Int32)wem.Length);

        br.BaseStream.Seek(origPos, SeekOrigin.Begin);
        wem.OggName = wem.WemName.Replace(".wem", ".ogg");
        wems.Add(wem);
      }

      return wems;
    }
  }
}
