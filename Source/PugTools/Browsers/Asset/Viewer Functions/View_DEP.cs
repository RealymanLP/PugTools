using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using nsHashDictionary;

namespace PugTools {
  internal class DEP_Entry {
    internal List<String> Dependencies { get; set; }
    internal String Filename { get; set; }
    internal UInt64 ID { get; set; }
    internal Int32 IntDepencies { get; set; }
    internal Int32 StrLength { get; set; }
    internal Int32 StrOffset { get; set; }
    internal String Type { get; set; }
    internal String Value { get; set; }

    internal DEP_Entry() {
      Dependencies = new List<String>();
    }
    public override String ToString() {
      return String.Format("Entry {0}: {1}", ID, Value);
    }
  }
  internal static class ViewDEP {
    internal static List<DEP_Entry> Read(BinaryReader br, HashDictionary hashDict) {
      UInt32 header = br.ReadUInt32();

      if (header.ToString() == "1") {
        //Read length (4 bytes)                
        br.ReadInt32();
        //Read number of entries                
        Int32 numEntries = br.ReadInt32();
        //Read length2                
        br.ReadInt32();
        //Read number of entries2                
        br.ReadInt32();
        //Parse definitions
        List<DEP_Entry> entries = new List<DEP_Entry>();

        for (Int32 i = 0; i < numEntries; i++) {
          DEP_Entry entry = new DEP_Entry();

          UInt32 sh = br.ReadUInt32();
          UInt32 ph = br.ReadUInt32();

          HashData data = hashDict.SearchHashList(ph, sh);

          if (data != null && data.FileName != "") {
            entry.ID = (UInt64)data.Ph << 32 | data.Sh;
            entry.Filename = data.FileName;
          } else {
            if (data == null) {
              entry.ID = (UInt64)ph << 32 | sh;
              entry.Filename = entry.ID.ToString();
            } else {
              entry.ID = (UInt64)data.Ph << 32 | data.Sh;
              entry.Filename = entry.ID.ToString();
            }

          }

          if (entry.Filename.Contains('.')) {
            entry.Type = entry.Filename.Split('.').Last().ToUpper();
          } else {
            entry.Type = "Unknown";
          }

          entry.IntDepencies = br.ReadInt16();

          if (entry.IntDepencies > 0) {
            for (Int32 c = 0; c < entry.IntDepencies; c++) {
              UInt32 dep_sh = br.ReadUInt32();
              UInt32 dep_ph = br.ReadUInt32();

              HashData depData = hashDict.SearchHashList(dep_ph, dep_sh);
              String dependency;

              if (depData != null) {
                dependency = depData.FileName;

                if (dependency == "")
                  dependency = String.Format("{0}", (UInt64)dep_ph << 32 | dep_sh);
              } else {
                dependency = String.Format("{0}", (UInt64)dep_ph << 32 | dep_sh);
              }

              entry.Dependencies.Add(dependency);
            }
          }

          entries.Add(entry);
        }

        return entries;
      } else {
        return null;
      }
    }
  }
}
