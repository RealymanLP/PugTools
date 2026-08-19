using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SlimDX;

namespace FileFormats {
  public static class FileHelpers {
    public static Single ByteToFloat(Byte byteValue) {
      return byteValue / 255.0F;
    }
    // public static Single ByteToNormal(Byte byteValue) {
    //   return (Single)((byteValue - 127.5) / 127.5);
    // }
    public static UInt32 GetFNV1Hash(String name) {
      if (name == null || name == "") return 0;

      const UInt32 Fnv1Prime = unchecked(16777619);
      const UInt32 Fnv1OffsetBasis = unchecked(2166136261);

      UInt32 hash = Fnv1OffsetBasis;
      Char[] arName = name.ToLower().ToArray();

      for (Int32 i = 0; i < arName.Length; i++) {
        unchecked {
          hash *= Fnv1Prime;
          hash ^= arName[i];
        }
      }

      Int32 mask = 1 << 31;
      hash = (UInt32)((hash >> 32) ^ (hash & mask));

      return hash;
    }
    public static Matrix ReadMatrix(BinaryReader br, Boolean invert = false) {
      Matrix temp = new Matrix {
        M11 = br.ReadSingle(),
        M12 = br.ReadSingle(),
        M13 = br.ReadSingle(),
        M14 = br.ReadSingle(),

        M21 = br.ReadSingle(),
        M22 = br.ReadSingle(),
        M23 = br.ReadSingle(),
        M24 = br.ReadSingle(),

        M31 = br.ReadSingle(),
        M32 = br.ReadSingle(),
        M33 = br.ReadSingle(),
        M34 = br.ReadSingle(),

        M41 = br.ReadSingle(),
        M42 = br.ReadSingle(),
        M43 = br.ReadSingle(),
        M44 = br.ReadSingle()
      };

      if (invert) temp.Invert();

      return temp;
    }
    public static String ReadString(BinaryReader br, UInt32 offset) {
      return ReadString(br, (UInt64)offset);
    }

    public static String ReadString(BinaryReader br, UInt64 offset) {
      Int64 originalPosition = br.BaseStream.Position;

      try {
        if (offset >= (UInt64)br.BaseStream.Length)
          throw new InvalidDataException($"String offset 0x{offset:X} is outside the stream.");

        br.BaseStream.Seek((Int64)offset, SeekOrigin.Begin);

        List<Byte> strBytes = new List<Byte>();
        while (br.BaseStream.Position < br.BaseStream.Length) {
          Byte b = br.ReadByte();
          if (b == 0) break;
          strBytes.Add(b);
        }

        if (br.BaseStream.Position >= br.BaseStream.Length && (strBytes.Count == 0 || strBytes[strBytes.Count - 1] != 0))
          throw new EndOfStreamException($"String at offset 0x{offset:X} is not NUL terminated.");

        return Encoding.ASCII.GetString(strBytes.ToArray());
      } finally {
        br.BaseStream.Seek(originalPosition, SeekOrigin.Begin);
      }
    }
    public static UInt16 ReverseBytes(UInt16 value) {
      return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
    }
    public static UInt32 ReverseBytes(UInt32 value) {
      return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
             (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
    }
    public static UInt64 ReverseBytes(UInt64 value) {
      return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
             (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
             (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
             (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
    }
    // public static Vector2 StringToVec2(String value) {
    //   String[] temp = value.Split(',');
    //   return new Vector2(Single.Parse(temp[0]), Single.Parse(temp[1]));
    // }
    // public static Vector3 StringToVec3(String value) {
    //   String[] temp = value.Split(',');
    //   return new Vector3(Single.Parse(temp[0]), Single.Parse(temp[1]), Single.Parse(temp[2]));
    // }
    public static Vector4 StringToVec4(String value) {
      String[] temp = value.Split(',');

      if (temp.Length == 4)
        return new Vector4(
          Single.Parse(temp[0]),
          Single.Parse(temp[1]),
          Single.Parse(temp[2]),
          Single.Parse(temp[3])
        );

      else if (temp.Length == 3)
        return new Vector4(
          Single.Parse(temp[0]),
          Single.Parse(temp[1]),
          Single.Parse(temp[2]),
          0
        );

      else
        return new Vector4();
    }
  }
}
