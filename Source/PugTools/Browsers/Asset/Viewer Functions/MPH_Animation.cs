using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace FileFormats {
  public sealed class JBARig {
    public List<JBARigBone> Bones { get; } = new List<JBARigBone>();
    public Int32[] AnimToRig { get; internal set; } = Array.Empty<Int32>();
    public String Source { get; internal set; } = String.Empty;
  }

  public sealed class JBARigBone {
    public String Name { get; internal set; } = String.Empty;
    public Int32 Parent { get; internal set; } = -1;
    public Vector3 BindTranslation { get; internal set; }
    public Quaternion BindRotation { get; internal set; } = Quaternion.Identity;
  }

  internal sealed class MPHAnimationEntry {
    public UInt32 AnimIndex;
    public UInt32 BoneMappingIndex;
  }

  internal sealed class MPHAnimationSet {
    public UInt32 RigSectionIndex;
    public List<MPHAnimationEntry> Entries = new List<MPHAnimationEntry>();
  }

  internal sealed class MPHAnimationList {
    public List<MPHAnimationSet> Sets = new List<MPHAnimationSet>();
    public List<String> Names = new List<String>();
  }

  internal sealed class MPHRigSection {
    public UInt32 Index;
    public List<JBARigBone> Bones = new List<JBARigBone>();
  }

  internal sealed class MPHMapSection {
    public UInt32 Index;
    public Int32[] AnimToRig = Array.Empty<Int32>();
  }

  public static class MPHAnimationReader {
    private const UInt32 MAWB = 0x4257414D;

    public static JBARig FindRigForClip(
      BinaryReader br,
      String clipName
    ) {
      if (br == null) throw new ArgumentNullException(nameof(br));
      if (String.IsNullOrWhiteSpace(clipName)) return null;

      Int64 pos = br.BaseStream.Position;
      Boolean is64 = false;

      if (ReadUInt32At(br, pos) == MAWB) {
        UInt32 version = ReadUInt32At(br, pos + 4);
        if (version != 2) return null;
        is64 = true;
        pos += 8;
      }

      Dictionary<UInt32, MPHRigSection> rigs =
        new Dictionary<UInt32, MPHRigSection>();
      Dictionary<UInt32, MPHMapSection> maps =
        new Dictionary<UInt32, MPHMapSection>();
      List<MPHAnimationList> animationLists =
        new List<MPHAnimationList>();

      while (pos + 16 <= br.BaseStream.Length) {
        UInt32 type = ReadUInt32At(br, pos);
        UInt32 index = ReadUInt32At(br, pos + 4);
        UInt32 length = ReadUInt32At(br, pos + 8);
        Int64 sectionStart = pos + 16;
        Int64 sectionEnd = sectionStart + length;

        if (length == 0 || sectionEnd > br.BaseStream.Length)
          break;

        try {
          if (type == 1)
            animationLists.Add(ReadAnimationList(br, sectionStart, is64));
          else if (type == 2)
            rigs[index] = ReadRig(br, sectionStart, index, is64);
          else if (type == 3)
            maps[index] = ReadMap(br, sectionStart, index, is64);
        }
        catch {
          // A large anim_library can contain sections not needed by this clip.
          // Keep scanning instead of making JBA preview fail completely.
        }

        pos = sectionEnd;
        pos = is64
          ? ((pos - 8 + 15) & ~15L) + 8
          : (pos + 15) & ~15L;
      }

      String wanted = Path.GetFileNameWithoutExtension(clipName);

      foreach (MPHAnimationList list in animationLists) {
        foreach (MPHAnimationSet set in list.Sets) {
          for (Int32 i = 0; i < set.Entries.Count; i++) {
            MPHAnimationEntry entry = set.Entries[i];
            if (entry.AnimIndex >= list.Names.Count) continue;

            String name = list.Names[(Int32)entry.AnimIndex];
            if (!String.Equals(
                  name,
                  wanted,
                  StringComparison.OrdinalIgnoreCase))
              continue;

            if (!rigs.TryGetValue(set.RigSectionIndex, out MPHRigSection rig))
              continue;
            if (!maps.TryGetValue(entry.BoneMappingIndex, out MPHMapSection map))
              continue;

            return new JBARig {
              Source = wanted,
              AnimToRig = map.AnimToRig
            }.WithBones(rig.Bones);
          }
        }
      }

      return null;
    }

    private static MPHAnimationList ReadAnimationList(
      BinaryReader br,
      Int64 start,
      Boolean is64
    ) {
      MPHAnimationList list = new MPHAnimationList();

      UInt32 numSets = ReadUInt32At(br, start);
      UInt32 subsectionListOffset = ReadUInt32At(br, start + (is64 ? 8 : 4));
      UInt32 jbaOffset = ReadUInt32At(br, start + (is64 ? 16 : 8));

      Int64 offsetPos = start + subsectionListOffset;
      List<UInt64> setOffsets = new List<UInt64>();

      for (UInt32 i = 0; i < numSets; i++) {
        UInt64 off = is64
          ? ReadUInt64At(br, offsetPos)
          : ReadUInt32At(br, offsetPos);
        offsetPos += is64 ? 8 : 4;
        setOffsets.Add(off);
      }

      foreach (UInt64 rawOffset in setOffsets) {
        Int64 p = start + (Int64)rawOffset;

        UInt64 unkStart = ReadWide(br, ref p, is64);
        UInt64 rigIndex = ReadWide(br, ref p, is64);
        UInt64 numEntries = ReadWide(br, ref p, is64);
        UInt64 subsectionStart = ReadWide(br, ref p, is64);
        UInt64 subsectionLength = ReadWide(br, ref p, is64);

        MPHAnimationSet set = new MPHAnimationSet {
          RigSectionIndex = (UInt32)rigIndex
        };

        Int32 stride = is64 ? 0x58 : 0x34;

        for (UInt64 i = 0; i < numEntries; i++) {
          Int64 entry = start + (Int64)rawOffset
            + (Int64)subsectionStart
            + (Int64)i * stride;

          UInt32 animIndex;
          UInt32 mappingIndex;

          if (is64) {
            animIndex = ReadUInt32At(br, entry + 0x50);
            mappingIndex = ReadUInt32At(br, entry + 0x54);
          } else {
            animIndex = ReadUInt32At(br, entry + 0x2C);
            mappingIndex = ReadUInt32At(br, entry + 0x30);
          }

          set.Entries.Add(new MPHAnimationEntry {
            AnimIndex = animIndex,
            BoneMappingIndex = mappingIndex
          });
        }

        list.Sets.Add(set);
      }

      list.Names = ReadStringTable(br, start + jbaOffset, is64);
      return list;
    }

    private static MPHRigSection ReadRig(
      BinaryReader br,
      Int64 start,
      UInt32 index,
      Boolean is64
    ) {
      Int64 p = start + 16; // skip compiler rig-header quaternion

      UInt64 parentsOffset = ReadWide(br, ref p, is64);
      UInt32 trajectory = ReadUInt32At(br, p); p += 4;
      UInt32 root = ReadUInt32At(br, p); p += 4;

      UInt64 namesOffset = ReadWide(br, ref p, is64);
      UInt64 rotationsOffset = ReadWide(br, ref p, is64);
      UInt64 translationsOffset = ReadWide(br, ref p, is64);

      Int64 parents = start + (Int64)parentsOffset;
      UInt32 numBones = ReadUInt32At(br, parents);
      Int64 parentData = parents + (is64 ? 16 : 8);

      List<Int32> parentIndices = new List<Int32>();
      for (UInt32 i = 0; i < numBones; i++)
        parentIndices.Add(ReadInt32At(br, parentData + i * 4L));

      List<String> names = ReadStringTable(
        br,
        start + (Int64)namesOffset,
        is64
      );

      MPHRigSection rig = new MPHRigSection { Index = index };

      for (Int32 i = 0; i < numBones; i++) {
        Int64 tp = start + (Int64)translationsOffset + i * 16L;
        Int64 rp = start + (Int64)rotationsOffset + i * 16L;

        Vector3 translation = new Vector3(
          ReadSingleAt(br, tp),
          ReadSingleAt(br, tp + 4),
          ReadSingleAt(br, tp + 8)
        );

        Quaternion rotation = new Quaternion(
          ReadSingleAt(br, rp),
          ReadSingleAt(br, rp + 4),
          ReadSingleAt(br, rp + 8),
          ReadSingleAt(br, rp + 12)
        );

        if (rotation.LengthSquared() > 0.000001F)
          rotation = Quaternion.Normalize(rotation);
        else
          rotation = Quaternion.Identity;

        rig.Bones.Add(new JBARigBone {
          Name = i < names.Count && names[i] != null
            ? names[i]
            : "bone_" + i,
          Parent = parentIndices[i],
          BindTranslation = translation,
          BindRotation = rotation
        });
      }

      return rig;
    }

    private static MPHMapSection ReadMap(
      BinaryReader br,
      Int64 start,
      UInt32 index,
      Boolean is64
    ) {
      Int64 p = start;
      UInt64 count = ReadWide(br, ref p, is64);
      UInt64 entriesOffset = ReadWide(br, ref p, is64);

      List<(UInt16 rig, UInt16 anim)> pairs =
        new List<(UInt16, UInt16)>();
      Int32 maxAnim = -1;

      p = start + (Int64)entriesOffset;

      for (UInt64 i = 0; i < count; i++) {
        UInt16 rig = ReadUInt16At(br, p);
        UInt16 anim = ReadUInt16At(br, p + 2);
        p += 4;

        pairs.Add((rig, anim));
        maxAnim = Math.Max(maxAnim, anim);
      }

      Int32[] map = new Int32[Math.Max(0, maxAnim + 1)];
      for (Int32 i = 0; i < map.Length; i++) map[i] = -1;
      foreach (var pair in pairs)
        map[pair.anim] = pair.rig;

      return new MPHMapSection {
        Index = index,
        AnimToRig = map
      };
    }

    private static List<String> ReadStringTable(
      BinaryReader br,
      Int64 start,
      Boolean is64
    ) {
      UInt32 numStrings = ReadUInt32At(br, start);
      UInt32 stringDataLength = ReadUInt32At(br, start + 4);

      Int64 p = start + 8;
      UInt64 indicesOffset = ReadWide(br, ref p, is64);
      UInt64 lengthsOffset = ReadWide(br, ref p, is64);
      UInt64 stringsOffset = ReadWide(br, ref p, is64);

      Int64 indicesPos = start + (Int64)indicesOffset;
      Int64 offsetsPos = start + (Int64)lengthsOffset;
      Int64 stringsPos = start + (Int64)stringsOffset;

      List<String> result = new List<String>();
      for (Int32 i = 0; i < numStrings; i++) result.Add(null);

      for (Int32 i = 0; i < numStrings; i++) {
        Int32 index = ReadInt32At(br, indicesPos + i * 4L);
        UInt32 off = ReadUInt32At(br, offsetsPos + i * 4L);

        if (index < 0 || index >= result.Count || off >= stringDataLength)
          continue;

        result[index] = ReadCString(br, stringsPos + off);
      }

      return result;
    }

    private static UInt64 ReadWide(
      BinaryReader br,
      ref Int64 pos,
      Boolean is64
    ) {
      UInt64 value = is64
        ? ReadUInt64At(br, pos)
        : ReadUInt32At(br, pos);
      pos += is64 ? 8 : 4;
      return value;
    }

    private static UInt16 ReadUInt16At(BinaryReader br, Int64 pos) {
      Int64 old = br.BaseStream.Position;
      br.BaseStream.Position = pos;
      UInt16 value = br.ReadUInt16();
      br.BaseStream.Position = old;
      return value;
    }

    private static UInt32 ReadUInt32At(BinaryReader br, Int64 pos) {
      Int64 old = br.BaseStream.Position;
      br.BaseStream.Position = pos;
      UInt32 value = br.ReadUInt32();
      br.BaseStream.Position = old;
      return value;
    }

    private static UInt64 ReadUInt64At(BinaryReader br, Int64 pos) {
      Int64 old = br.BaseStream.Position;
      br.BaseStream.Position = pos;
      UInt64 value = br.ReadUInt64();
      br.BaseStream.Position = old;
      return value;
    }

    private static Int32 ReadInt32At(BinaryReader br, Int64 pos) {
      Int64 old = br.BaseStream.Position;
      br.BaseStream.Position = pos;
      Int32 value = br.ReadInt32();
      br.BaseStream.Position = old;
      return value;
    }

    private static Single ReadSingleAt(BinaryReader br, Int64 pos) {
      Int64 old = br.BaseStream.Position;
      br.BaseStream.Position = pos;
      Single value = br.ReadSingle();
      br.BaseStream.Position = old;
      return value;
    }

    private static String ReadCString(BinaryReader br, Int64 pos) {
      Int64 old = br.BaseStream.Position;
      br.BaseStream.Position = pos;

      List<Byte> bytes = new List<Byte>();
      while (br.BaseStream.Position < br.BaseStream.Length) {
        Byte b = br.ReadByte();
        if (b == 0) break;
        bytes.Add(b);
      }

      br.BaseStream.Position = old;
      return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
    }

    private static JBARig WithBones(
      this JBARig rig,
      IEnumerable<JBARigBone> bones
    ) {
      rig.Bones.AddRange(bones);
      return rig;
    }
  }
}
