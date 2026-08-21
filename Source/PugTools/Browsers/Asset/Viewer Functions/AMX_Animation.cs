using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileFormats {
  /// <summary>
  /// Minimal reader for SWTOR's *.mph.amx AnimShare metadata. Standalone 64-bit
  /// JBA clips frequently omit their channel names; AMX is the most direct
  /// source for those names and is what Jedipedia probes before parsing MPH.
  /// </summary>
  internal static class AMXAnimationReader {
    private const UInt32 AMX_MAGIC = 0x20584D41; // "AMX "

    private sealed class FileEntry {
      internal String Animation;
      internal Int32 BoneList = -1;
    }

    internal static Boolean TryApplyBoneNames(
      BinaryReader br,
      String clipName,
      JBAAnimation animation
    ) {
      if (br == null || animation == null || animation.BoneCount <= 0)
        return false;

      Stream stream = br.BaseStream;
      if (stream == null || !stream.CanSeek || stream.Length < 6)
        return false;

      stream.Position = 0;
      if (br.ReadUInt32() != AMX_MAGIC)
        return false;

      Byte version = br.ReadByte();
      if (version != 1)
        return false;

      var files = new List<FileEntry>();

      while (stream.Position < stream.Length) {
        Byte flags = br.ReadByte();
        if (flags == 0)
          break;
        if (flags != 2 && flags != 3)
          return false;

        String animationName = String.Empty;
        if ((flags & 2) != 0) {
          animationName = ReadByteString(br);
          ReadByteString(br); // body type / animation folder
        }

        Int32 boneList = -1;
        if ((flags & 1) != 0) {
          EnsureAvailable(stream, 4);
          boneList = checked((Int32)br.ReadUInt32());
        }

        files.Add(new FileEntry {
          Animation = animationName,
          BoneList = boneList
        });
      }

      EnsureAvailable(stream, 4);
      UInt32 numBoneLists = br.ReadUInt32();
      if (numBoneLists > 100000)
        return false;

      String clipBase = Path.GetFileNameWithoutExtension(clipName ?? String.Empty)
        ?? String.Empty;

      Int32 wantedList = -1;
      foreach (FileEntry entry in files) {
        if (entry.BoneList < 0)
          continue;
        if (String.Equals(
              entry.Animation,
              clipBase,
              StringComparison.OrdinalIgnoreCase
            )) {
          wantedList = entry.BoneList;
          break;
        }
      }

      List<String> wantedBones = null;

      for (Int32 listIndex = 0; listIndex < numBoneLists; listIndex++) {
        EnsureAvailable(stream, 4);
        UInt32 numBones = br.ReadUInt32();
        if (numBones > 4096)
          return false;

        Boolean capture = listIndex == wantedList;
        if (capture)
          wantedBones = new List<String>(checked((Int32)numBones));

        for (Int32 boneIndex = 0; boneIndex < numBones; boneIndex++) {
          String name = ReadByteString(br);
          EnsureAvailable(stream, 7 * 4L);

          // Quaternion XYZW + bind/local translation XYZ. We only need the
          // names here; the target GR2 supplies the authored hierarchy/bind.
          for (Int32 i = 0; i < 7; i++)
            br.ReadSingle();

          if (capture)
            wantedBones.Add(name);
        }
      }

      if (wantedBones == null || wantedBones.Count != animation.BoneCount)
        return false;

      while (animation.BoneNames.Count < animation.BoneCount)
        animation.BoneNames.Add("bone_" + animation.BoneNames.Count);

      for (Int32 i = 0; i < animation.BoneCount; i++) {
        if (!String.IsNullOrWhiteSpace(wantedBones[i]))
          animation.BoneNames[i] = wantedBones[i];
      }

      return true;
    }

    private static String ReadByteString(BinaryReader br) {
      EnsureAvailable(br.BaseStream, 1);
      Int32 length = br.ReadByte();
      EnsureAvailable(br.BaseStream, length);
      if (length == 0)
        return String.Empty;
      return Encoding.ASCII.GetString(br.ReadBytes(length));
    }

    private static void EnsureAvailable(Stream stream, Int64 count) {
      if (count < 0 || stream.Position > stream.Length - count)
        throw new EndOfStreamException("AMX record points outside the stream.");
    }
  }
}
