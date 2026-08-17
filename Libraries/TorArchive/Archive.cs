using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TorArchive {
  /// <summary>
  /// Class used to manage a single .tor file
  /// </summary>
  public class Archive : IDisposable {

    #region Constructors
    public Archive(String fileName, Library library) {
      FileName = fileName;
      Library = library;
      Initialize();
    }

    #endregion Constructors

    #region Fields
    // public HashSet<string> directories = new HashSet<string>();
    private readonly Dictionary<UInt64, FileInfo> m_fileLookup = new Dictionary<UInt64, FileInfo>();
    private String m_strippedFileName;

    #endregion Fields

    #region IDisposable
    private Boolean m_disposed = false;

    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(Boolean disposing) {
      if (m_disposed) {
        return;
      }

      if (disposing) {
        foreach (KeyValuePair<UInt64, FileInfo> lookup in m_fileLookup) {
          lookup.Value.Dispose();
        }

        m_fileLookup.Clear();

        // File instances are lightweight wrappers around FileInfo and are now
        // created on demand by EnumerateFiles(). The FileInfo lookup remains
        // the authoritative index.
        // directories.Clear();
        // Library.Dispose();                
      }

      m_disposed = true;
    }

    #endregion IDisposable

    #region Methods
    public File FindFile(FileId fileId) {
      return FindFile(fileId.AsUInt64());
    }

    public File FindFile(UInt64 fileId) {
      if (!Initialized) {
        Initialize();
      }

      if (!m_fileLookup.TryGetValue(fileId, out FileInfo fileInfo)) {
        return null;
      }

      File file = new File(this, fileInfo);
      return file;
    }

    /// <summary>
    /// Load file tables and fill-in fileLookup dictionary
    /// </summary>
    private void Initialize() {
      using (FileStream fs = OpenStreamAt(0)) {
        using (BinaryReader reader = new BinaryReader(fs)) {
          Int32 magicNumber = reader.ReadInt32();

          if (magicNumber != 0x50594D) {
            throw new InvalidOperationException($"Wait a minute! {FileName} isn't a MYP file!");
          }

          fs.Seek(12, SeekOrigin.Begin);
          UInt64 fileTableOffset = reader.ReadUInt64();

          while (fileTableOffset != 0) {
            fs.Seek((Int64)fileTableOffset, SeekOrigin.Begin);

            UInt32 numFiles = reader.ReadUInt32();
            fileTableOffset = reader.ReadUInt64();

            for (Int32 i = 0; i < numFiles; i++) {
              // Read file info blocks
              FileInfo info = new FileInfo {
                Offset = reader.ReadUInt64()
              };

              if (info.Offset == 0) {
                // No file offset, no file -- skip this entry and try the next one
                fs.Seek(26, SeekOrigin.Current);
                continue;
              }

              info.HeaderSize = reader.ReadUInt32();
              info.CompressedSize = reader.ReadUInt32();
              info.UncompressedSize = reader.ReadUInt32();

              Int64 current_position = reader.BaseStream.Position;

              info.SecondaryHash = reader.ReadUInt32();
              info.PrimaryHash = reader.ReadUInt32();

              reader.BaseStream.Seek(current_position, SeekOrigin.Begin);

              info.FileId = reader.ReadUInt64();
              info.Checksum = reader.ReadUInt32();
              info.CompressionMethod = reader.ReadUInt16();
              info.CRC = (Int32)info.Checksum;

              m_fileLookup.Add(info.FileId, info);
            }
          }
        }
      }

      Initialized = true;
    }

    internal FileStream OpenStream(FileInfo fileInfo) {
      UInt64 offset = fileInfo.Offset + fileInfo.HeaderSize;
      return OpenStreamAt((Int64)offset);
    }

    internal FileStream OpenStreamAt(Int64 offset) {
      FileStream fs = System.IO.File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
      fs.Seek(offset, SeekOrigin.Begin);
      return fs;
    }

    public override String ToString() {
      return FileName.Split('\\').Last();
    }

    #endregion Methods

    #region Properties
    public String FileName { get; set; }
    /// <summary>
    /// Enumerates file wrappers without keeping one File object per archive
    /// entry alive for the lifetime of the archive.
    /// </summary>
    public IEnumerable<File> EnumerateFiles() {
      foreach (FileInfo info in m_fileLookup.Values) {
        yield return new File(this, info);
      }
    }

    /// <summary>
    /// Compatibility property. Prefer EnumerateFiles() in long-running views.
    /// </summary>
    public List<File> Files => EnumerateFiles().ToList();

    public Boolean Initialized { get; private set; }
    internal Library Library { get; set; }
    /// <summary>
    /// Gets the archive name, minus the "swtor_" or "swtor_test_" and .tor part of the name.
    /// </summary>
    public String StrippedFileName {
      get {
        if (m_strippedFileName == null && FileName != null) {
          StrippedFileName = FileName;
        }

        return m_strippedFileName;
      }
      set {
        // Remove the directory.
        String fileName = value.Split('/').Last();
        fileName = fileName.Split('\\').Last();

        // Remove swtor_test_
        fileName = fileName.Replace("swtor_", String.Empty);
        fileName = fileName.Replace("test_", String.Empty);

        // Remove .tor
        fileName = fileName.Replace(".tor", String.Empty);

        m_strippedFileName = fileName;
      }
    }

    #endregion Properties
  }
}
