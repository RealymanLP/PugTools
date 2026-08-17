using System;
using System.IO;

namespace PugTools {
  internal static class ViewSCPT {
    internal static MemoryStream DecryptSCPT(BinaryReader br) {
      Byte[] data;

      UInt64 header = br.ReadUInt32();

      if (header.ToString() == "1414546259") {
        //SmallVersion
        UInt16 smallVer = br.ReadUInt16();
        //BigVersion
        UInt16 bigVer = br.ReadUInt16();
        //Unknown
        br.ReadUInt64();

        if (bigVer == 5 && smallVer == 5) {
          //Read ID
          br.ReadUInt64();
          //Read IsEncrypted 
          Boolean encrypted = br.ReadBoolean();
          //Unknown
          br.ReadUInt64();
          //Data Length
          UInt32 dataLength = br.ReadUInt32();
          data = br.ReadBytes((Int32)dataLength);

          if (encrypted) {
            Byte[] decryptedData = new Byte[data.Length];
            UInt32 unk = 0x35;

            for (Int32 i = 0; i < data.Length; i++) {
              decryptedData[i] = (Byte)(data[i] ^ unk);
              unk += 0x36;
            }

            data = decryptedData;
          }

          MemoryStream stream = new MemoryStream();
          stream.Write(data, 0, data.Length);

          return stream;
        }
      }

      return null;
    }
  }
}
