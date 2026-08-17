using System;
using System.IO;

namespace FileFormats {
  public class GR2_Bone_Skeleton {
    public Int32 boneIndex;
    public String boneName;
    public UInt64 offsetBoneName;
    public SlimDX.Matrix parent;
    public Int32 parentBoneIndex;
    public SlimDX.Matrix root;

    public GR2_Bone_Skeleton(BinaryReader br, Int32 index, Boolean is64Bit) {
      offsetBoneName = is64Bit ? br.ReadUInt64() : br.ReadUInt32();
      parentBoneIndex = br.ReadInt32();

      if (is64Bit)
        br.ReadUInt32(); // unknown/padding field in the 64-bit skeleton bone record

      parent = FileHelpers.ReadMatrix(br, true);
      root = FileHelpers.ReadMatrix(br, true);

      boneName = FileHelpers.ReadString(br, offsetBoneName);
      boneIndex = index;
    }
  }
}
