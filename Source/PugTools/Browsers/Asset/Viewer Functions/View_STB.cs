using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PugTools {
  internal class STB_Entry {
    internal Int64 ID { get; set; }
    internal Int32 Length { get; set; }
    internal Int32 Offset { get; set; }
    internal String StringValue { get; set; }
  }
  internal class ViewSTB {
    internal static List<STB_Entry> ParseSTB(BinaryReader br) {
      List<STB_Entry> entries = new List<STB_Entry>();
      Byte[] header = br.ReadBytes(3);

      if (header[0] == 1 && header[1] == 0 && header[2] == 0) {
        Int32 numStrings = br.ReadInt32();

        for (Int32 intCount = 0; intCount < numStrings; intCount++) {
          STB_Entry entry = new STB_Entry{ ID = br.ReadInt64()};

          br.ReadByte();
          br.ReadByte();
          br.ReadInt32();

          entry.Length = br.ReadInt32();
          entry.Offset = br.ReadInt32();

          br.ReadInt32();

          entries.Add(entry);
        }

        foreach (STB_Entry entry in entries) {
          ReadStringByLength(br, entry);
        }
      }

      return entries;
    }

    private static void ReadStringByLength(BinaryReader br, STB_Entry entry) {
      br.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

      Byte[] b = br.ReadBytes(entry.Length);
      entry.StringValue = Encoding.UTF8.GetString(b);

      return;
    }
  }
}
