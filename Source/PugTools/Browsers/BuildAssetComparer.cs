using System;
using System.Collections.Generic;
using System.Linq;

using TorArchive;

namespace PugTools {
  internal enum BuildFileState {
    None,
    New,
    Changed,
    Removed
  }

  internal sealed class BuildFileRecord {
    internal String Identity { get; set; }
    internal Int32 ArchiveIndex { get; set; }
    internal Library Library { get; set; }
    internal HashFileInfo HashInfo { get; set; }

    internal String DisplayPath {
      get {
        if (HashInfo == null) return String.Empty;

        if (HashInfo.IsNamed)
          return HashInfo.Directory + "/" + HashInfo.FileName;

        return HashInfo.Directory + "/" + HashInfo.Extension + "/"
          + HashInfo.FileName + "." + HashInfo.Extension;
      }
    }
  }

  internal sealed class BuildFileDifference {
    internal BuildFileState State { get; set; }
    internal BuildFileRecord Current { get; set; }
    internal BuildFileRecord Previous { get; set; }

    internal BuildFileRecord DisplayRecord => Current ?? Previous;
  }

  internal static class BuildAssetComparer {
    internal static List<BuildFileDifference> Compare(Assets currentAssets,
                                                       Assets previousAssets) {
      Dictionary<String, BuildFileRecord> current = BuildSnapshot(currentAssets);
      Dictionary<String, BuildFileRecord> previous = BuildSnapshot(previousAssets);
      List<BuildFileDifference> differences = new List<BuildFileDifference>();

      foreach (KeyValuePair<String, BuildFileRecord> pair in current) {
        if (!previous.TryGetValue(pair.Key, out BuildFileRecord oldRecord)) {
          differences.Add(new BuildFileDifference {
            State = BuildFileState.New,
            Current = pair.Value
          });
          continue;
        }

        if (FileContentsDiffer(pair.Value.HashInfo.File.FileInfo,
                               oldRecord.HashInfo.File.FileInfo)) {
          differences.Add(new BuildFileDifference {
            State = BuildFileState.Changed,
            Current = pair.Value,
            Previous = oldRecord
          });
        }
      }

      foreach (KeyValuePair<String, BuildFileRecord> pair in previous) {
        if (!current.ContainsKey(pair.Key)) {
          differences.Add(new BuildFileDifference {
            State = BuildFileState.Removed,
            Previous = pair.Value
          });
        }
      }

      foreach (BuildFileDifference difference in differences) {
        HydrateUnknownExtension(difference.Current);
        HydrateUnknownExtension(difference.Previous);
      }

      return differences
        .OrderBy(x => x.State)
        .ThenBy(x => x.DisplayRecord?.DisplayPath, StringComparer.OrdinalIgnoreCase)
        .ToList();
    }

    private static Dictionary<String, BuildFileRecord> BuildSnapshot(Assets assets) {
      Dictionary<String, BuildFileRecord> raw =
        new Dictionary<String, BuildFileRecord>(StringComparer.OrdinalIgnoreCase);

      if (assets == null) return raw;

      foreach (Library lib in assets.Libraries) {
        if (!lib.Loaded) lib.Load();

        foreach (KeyValuePair<Int32, Archive> archive in lib.Archives) {
          foreach (TorArchive.File file in archive.Value.EnumerateFiles()) {
            UInt32 ph = file.FileInfo.PrimaryHash;
            UInt32 sh = file.FileInfo.SecondaryHash;
            String identity = lib.Name + ":" + ph.ToString("X8") + sh.ToString("X8");

            // The same file hash can exist in more than one TOR in a library.
            // The highest archive number is the newest effective copy.
            if (raw.TryGetValue(identity, out BuildFileRecord existing)
                && existing.ArchiveIndex >= archive.Key) {
              continue;
            }

            raw[identity] = new BuildFileRecord {
              Identity = identity,
              ArchiveIndex = archive.Key,
              Library = lib,
              HashInfo = new HashFileInfo(ph, sh, file, false, false)
            };
          }
        }
      }

      // For named files, ask the Assets metadata table for the effective archive copy.
      // This is more precise than assuming the numerically highest TOR always wins.
      foreach (BuildFileRecord record in raw.Values) {
        if (record.HashInfo == null || !record.HashInfo.IsNamed) continue;

        String path = record.HashInfo.Directory + "/" + record.HashInfo.FileName;
        TorArchive.File effective = record.Library?.FindFile(path);
        if (effective == null || Object.ReferenceEquals(effective, record.HashInfo.File)) continue;

        record.HashInfo = new HashFileInfo(
          effective.FileInfo.PrimaryHash, effective.FileInfo.SecondaryHash, effective, false, false
        );
      }

      // Archive housekeeping files are not useful in a build comparison.
      return raw
        .Where(x => !IsIgnored(x.Value.HashInfo))
        .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
    }

    private static void HydrateUnknownExtension(BuildFileRecord record) {
      if (record?.HashInfo == null || record.HashInfo.IsNamed
          || !String.IsNullOrEmpty(record.HashInfo.Extension)) return;

      TorArchive.File file = record.HashInfo.File;
      record.HashInfo = new HashFileInfo(
        file.FileInfo.PrimaryHash, file.FileInfo.SecondaryHash, file, true, false
      );
    }

    private static Boolean IsIgnored(HashFileInfo info) {
      if (info == null) return true;

      if (!info.IsNamed) return false;

      return info.FileName.Equals("metadata.bin", StringComparison.OrdinalIgnoreCase)
        || info.FileName.Equals("ft.sig", StringComparison.OrdinalIgnoreCase)
        || info.FileName.Equals("groupmanifest.bin", StringComparison.OrdinalIgnoreCase);
    }

    private static Boolean FileContentsDiffer(TorArchive.FileInfo current,
                                               TorArchive.FileInfo previous) {
      if (current == null || previous == null) return true;

      // Checksum is SWTOR's CRC32 of the file data. Size is included as a
      // sanity check for very old archive variants with incomplete metadata.
      return current.Checksum != previous.Checksum
        || current.UncompressedSize != previous.UncompressedSize;
    }
  }
}
