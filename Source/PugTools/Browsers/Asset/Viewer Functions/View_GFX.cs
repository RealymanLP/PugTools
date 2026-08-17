using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace PugTools {
  internal static class ViewGFX {
    internal static MemoryStream DecompressGFX(BinaryReader br) {
      Byte[] header = br.ReadBytes(3);

      if ((header[0] == 67 && header[1] == 70 && header[2] == 88)
          || (header[0] == 67 && header[1] == 87 && header[2] == 83)) {
        //SmallVersion
        br.ReadByte();

        //Data Length
        UInt32 dataLength = br.ReadUInt32();

        Byte[] inflatedData = new Byte[dataLength];

        Inflater inf = new Inflater();
        inf.SetInput(br.ReadBytes((Int32)br.BaseStream.Length - 8));
        inf.Inflate(inflatedData);

        MemoryStream stream = new MemoryStream();
        stream.Write(inflatedData, 0, inflatedData.Length);

        return stream;
      } else {

        return null;
      }
    }
  }
}
