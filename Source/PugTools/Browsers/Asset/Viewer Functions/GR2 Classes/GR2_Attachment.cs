using System;
using System.IO;

namespace FileFormats {
  public class GR2_Attachment {
    public SlimDX.Matrix attachMatrix;
    public String attachName = "";
    public String boneName = "";
    public UInt64 offsetAttachBoneName = 0;
    public UInt64 offsetAttachName = 0;

    public GR2_Attachment(BinaryReader br, Boolean is64Bit) {
      offsetAttachName = is64Bit ? br.ReadUInt64() : br.ReadUInt32();
      offsetAttachBoneName = is64Bit ? br.ReadUInt64() : br.ReadUInt32();
      attachMatrix = FileHelpers.ReadMatrix(br);
      attachName = FileHelpers.ReadString(br, offsetAttachName);
      boneName = FileHelpers.ReadString(br, offsetAttachBoneName);
    }
  }
}
