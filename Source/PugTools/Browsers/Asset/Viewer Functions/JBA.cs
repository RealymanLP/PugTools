using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace FileFormats {
  public sealed class JBAAnimation {
    public float Length { get; internal set; }
    public float FPS { get; internal set; }
    public int BlockCount { get; internal set; }
    public int BoneCount { get; internal set; }
    public List<string> BoneNames { get; } = new List<string>();
    internal List<JBA_Bone> Bones { get; } = new List<JBA_Bone>();
    internal List<JBA_Block> Blocks { get; } = new List<JBA_Block>();

    public int FrameCount => FPS > 0 ? Math.Max(1, (int)Math.Round(Length * FPS)) : 1;

    public JBAFrame Sample(float time) {
      if (Bones.Count == 0) return new JBAFrame(0, new List<JBATransform>());
      int frame = FPS > 0 ? Math.Max(0, (int)Math.Floor(time * FPS + 0.5f)) : 0;
      frame = Math.Min(frame, Math.Max(0, FrameCount - 1));
      var result = new List<JBATransform>(Bones.Count);
      for (int i = 0; i < Bones.Count; i++) result.Add(new JBATransform(Vector3.Zero, Quaternion.Identity));

      foreach (JBA_Block block in Blocks) {
        if (block.Layout == null || block.Layout.Count == 0) continue;
        int localFrame = frame - block.StartFrame;
        if (localFrame < 0) continue;
        for (int b = 0; b < block.BoneCount && b < Bones.Count; b++) {
          JBA_KeyLayout k = block.Layout[b];
          int r = k.RotationCount == 0 ? 0 : Math.Min(k.RotationCount - 1, localFrame);
          int t = k.TranslationCount == 0 ? 0 : Math.Min(k.TranslationCount - 1, localFrame);
          Vector3 pos = Vector3.Zero;
          Quaternion rot = Quaternion.Identity;
          if (k.RotationCount > 0 && k.Rotations != null && r < k.Rotations.Count) rot = k.Rotations[r];
          if (k.TranslationCount > 0 && k.Translations != null && t < k.Translations.Count) pos = k.Translations[t];
          result[b] = new JBATransform(pos, rot);
        }
      }
      return new JBAFrame(frame, result);
    }
  }

  public readonly struct JBATransform {
    public readonly Vector3 Translation;
    public readonly Quaternion Rotation;
    public JBATransform(Vector3 translation, Quaternion rotation) { Translation = translation; Rotation = rotation; }
  }

  public sealed class JBAFrame {
    public int Frame { get; }
    public IReadOnlyList<JBATransform> Bones { get; }
    internal JBAFrame(int frame, IReadOnlyList<JBATransform> bones) { Frame = frame; Bones = bones; }
  }

  internal sealed class JBA_Bone {
    public Vector3 TranslationStride, TranslationBase, RotationStride, RotationBase;
  }
  internal sealed class JBA_Block {
    public int StartFrame, Size, BoneCount;
    public List<JBA_KeyLayout> Layout = new List<JBA_KeyLayout>();
  }
  internal sealed class JBA_KeyLayout {
    public int RotationCount, TranslationCount;
    public List<Quaternion> Rotations = new List<Quaternion>();
    public List<Vector3> Translations = new List<Vector3>();
  }

  public static class JBAReader {
    public static JBAAnimation Read(BinaryReader br) {
      if (br == null) throw new ArgumentNullException(nameof(br));
      Stream s = br.BaseStream;
      long start = s.Position;
      if (s.Length - start < 0x28) throw new InvalidDataException("JBA file is too short.");

      br.ReadUInt32();
      var a = new JBAAnimation {
        Length = br.ReadSingle(),
        FPS = br.ReadSingle(),
        BlockCount = checked((int)br.ReadUInt32())
      };
      br.ReadUInt32(); br.ReadUInt32();
      a.BoneCount = checked((int)br.ReadUInt32());
      br.ReadUInt32(); br.ReadUInt32(); br.ReadUInt32();

      if (a.BlockCount < 0 || a.BlockCount > 4096 || a.BoneCount < 0 || a.BoneCount > 4096)
        throw new InvalidDataException("JBA contains unreasonable block/bone counts.");

      var headers = new List<(int startFrame, int size)>();
      for (int i = 0; i < a.BlockCount; i++) headers.Add((checked((int)br.ReadUInt32()), checked((int)br.ReadUInt32())));
      br.BaseStream.Seek(4L * a.BlockCount, SeekOrigin.Current);
      Align(br.BaseStream, 128);

      for (int i = 0; i < a.BoneCount; i++) {
        a.Bones.Add(new JBA_Bone {
          TranslationStride = ReadV3(br), TranslationBase = ReadV3(br),
          RotationStride = ReadV3(br), RotationBase = ReadV3(br)
        });
      }
      Align(br.BaseStream, 128);

      foreach (var h in headers) {
        long blockStart = br.BaseStream.Position;
        if (h.size < 8 || blockStart + h.size > br.BaseStream.Length) throw new EndOfStreamException("Invalid JBA block size.");
        var block = new JBA_Block { StartFrame = h.startFrame, Size = h.size, BoneCount = checked((int)br.ReadUInt32()) };
        br.ReadUInt32();
        if (block.BoneCount < 0 || block.BoneCount > a.BoneCount) throw new InvalidDataException("Invalid JBA block bone count.");
        var layout = new uint[block.BoneCount * 4];
        for (int i = 0; i < layout.Length; i++) layout[i] = br.ReadUInt32();
        for (int b = 0; b < block.BoneCount; b++) {
          var k = new JBA_KeyLayout { RotationCount = checked((int)layout[b * 4]), TranslationCount = checked((int)layout[b * 4 + 2]) };
          if (k.RotationCount > 10000 || k.TranslationCount > 10000) throw new InvalidDataException("Invalid JBA keyframe count.");
          for (int r = 0; r < k.RotationCount; r++) k.Rotations.Add(ReadRotation(br, a.Bones[b].RotationBase, a.Bones[b].RotationStride));
          for (int t = 0; t < k.TranslationCount; t++) k.Translations.Add(ReadTranslation(br, a.Bones[b].TranslationBase, a.Bones[b].TranslationStride));
          block.Layout.Add(k);
        }
        a.Blocks.Add(block);
        br.BaseStream.Seek(blockStart + h.size, SeekOrigin.Begin);
        Align(br.BaseStream, 128);
      }

      // Bone-name table is optional in malformed/partial runtime files. Try to read the standard table.
      if (br.BaseStream.Position + 20 <= br.BaseStream.Length) {
        long p = br.BaseStream.Position;
        uint count = br.ReadUInt32();
        if (count <= a.BoneCount && p + 20 + count * 8 <= br.BaseStream.Length) {
          br.ReadUInt32(); uint idxOff = br.ReadUInt32(); uint offOff = br.ReadUInt32(); uint namesOff = br.ReadUInt32();
          if (idxOff + count * 4 <= br.BaseStream.Length && offOff + count * 4 <= br.BaseStream.Length && namesOff < br.BaseStream.Length) {
            br.BaseStream.Seek(offOff, SeekOrigin.Begin);
            uint[] offs = new uint[count];
            for (int i = 0; i < count; i++) offs[i] = br.ReadUInt32();
            br.BaseStream.Seek(namesOff, SeekOrigin.Begin);
            for (int i = 0; i < count; i++) {
              br.BaseStream.Seek(namesOff + offs[i], SeekOrigin.Begin);
              a.BoneNames.Add(ReadCString(br));
            }
          }
        }
      }
      while (a.BoneNames.Count < a.BoneCount) a.BoneNames.Add("bone_" + a.BoneNames.Count);
      return a;
    }

    private static Vector3 ReadV3(BinaryReader br) => new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
    private static Vector3 ReadTranslation(BinaryReader br, Vector3 b, Vector3 s) {
      uint v = br.ReadUInt32();
      return new Vector3(b.X + ((v >> 21) & 0x7ff) * s.X, b.Y + ((v >> 10) & 0x7ff) * s.Y, b.Z + (v & 0x3ff) * s.Z);
    }
    private static Quaternion ReadRotation(BinaryReader br, Vector3 b, Vector3 s) {
      ushort xRaw = br.ReadUInt16(); ushort yRaw = br.ReadUInt16(); ushort zRaw = br.ReadUInt16();
      float x = b.X + (xRaw & 0x7fff) * s.X, y = b.Y + yRaw * s.Y, z = b.Z + zRaw * s.Z;
      float d = x*x + y*y + z*z; float w = d > 1f ? 0f : (float)Math.Sqrt(Math.Max(0f, 1f-d));
      if ((xRaw & 0x8000) != 0) w = -w;
      return Quaternion.Normalize(new Quaternion(x,y,z,w));
    }
    private static string ReadCString(BinaryReader br) { var bytes = new List<byte>(); byte b; while ((b = br.ReadByte()) != 0 && bytes.Count < 4096) bytes.Add(b); return Encoding.UTF8.GetString(bytes.ToArray()); }
    private static void Align(Stream s, int a) { long p = s.Position; long n = (p + a - 1) / a * a; if (n <= s.Length) s.Position = n; }
  }
}
