using System;
using System.Linq;

using nsHashDictionary;

namespace TorArchive {
  public class HashFileInfo {

    public enum State {
      New,
      Modified,
      Unchanged
    }

    #region Constructors
    public HashFileInfo(UInt32 ph, UInt32 sh, File file)
      : this(ph, sh, file, true) {
    }

    /// <summary>
    /// Creates hash metadata without probing file contents when detectUnknownExtension is false.
    /// Browser indexes use this overload to avoid opening/decompressing unknown TOR entries at startup.
    /// </summary>
    public HashFileInfo(UInt32 ph, UInt32 sh, File file, Boolean detectUnknownExtension) {
      if (ph == 0 && sh == 0 && file == null) {
        return;
      }

      FileInfo info = file.FileInfo;
      HashData data =
        HashDictionaryInstance.Instance.Dictionary.SearchHashList(ph,
                                                                  sh,
                                                                  file.Archive.StrippedFileName);

      _FileRef = file;
      Source = file.Archive.FileName.Split('\\').Last();

      if (data != null && data.FileName.Length > 0) {
        IsNamed = true;
        FileName = data.FileName;
        Extension = FileName.Split('.').Last();

        String[] temp = FileName.Split('/');

        Directory = String.Join("/", temp.Take(temp.Length - 1));
        FileName = temp.Last();

        if (info.CRC != data.Crc) {
          FileState = State.Modified;
          HashDictionaryInstance.Instance.Dictionary.UpdateCRC(info.PrimaryHash,
                                                               info.SecondaryHash,
                                                               info.CRC,
                                                               file.Archive.StrippedFileName);
        } else if (info.CRC == data.Crc) {
          FileState = State.Unchanged;
        }
      } else {
        IsNamed = false;
        Directory = "/" + Source;
        Extension = detectUnknownExtension ? FileExtension.Instance.GuessExtension(file) : "";

        if (data == null) {
          FileState = State.New;
          FileName = $"{info.Checksum:X8}_{info.FileId:X16}";
          HashDictionaryInstance.Instance.Dictionary.AddHash(info.PrimaryHash,
                                                             info.SecondaryHash,
                                                             "",
                                                             info.CRC,
                                                             file.Archive.StrippedFileName);
        } else if (info.CRC != data.Crc) {
          FileState = State.Modified;
          HashDictionaryInstance.Instance.Dictionary.UpdateCRC(info.PrimaryHash,
                                                               info.SecondaryHash,
                                                               info.CRC,
                                                               file.Archive.StrippedFileName);
        } else if (info.CRC == data.Crc) {
          FileState = State.Unchanged;
        }

        if (FileName == null) {
          FileName = $"{info.Checksum:X8}_{info.FileId:X16}";
        }
      }
    }

    #endregion Constructors

    #region Fields
    private File _FileRef;

    #endregion Fields

    #region Properties
    public String Directory { get; set; }
    public String Extension { get; private set; }
    public File File {
      get => _FileRef;
      set => _FileRef = value;
    }
    public String FileName { get; private set; }
    public State FileState { get; private set; }
    public Boolean IsNamed { get; private set; }
    public String Source { get; private set; }

    #endregion Properties
  }
}
