using System;

namespace TorArchive {
  public class FileInfo : IDisposable {

    #region IDisposable
    private Boolean m_disposed = false;

    ~FileInfo() {
      Dispose(false);
    }

    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(Boolean disposing) {
      if (m_disposed) {
        return;
      }

      if (disposing) {

      }

      m_disposed = true;
    }

    #endregion IDisposable

    #region Properties

    private UInt32 _checksum;
    private Archive _archive;

    /// <summary>TOR metadata checksum. Beta placeholder values are reconstructed on first access.</summary>
    public UInt32 Checksum {
      get => _archive == null ? _checksum : _archive.ResolveChecksum(this);
      set => _checksum = value;
    }

    internal UInt32 StoredChecksum { get => _checksum; set => _checksum = value; }
    internal Archive OwnerArchive { get => _archive; set => _archive = value; }
    public UInt32 CompressedSize { get; set; }
    public UInt16 CompressionMethod { get; set; }
    public Int32 CRC {
      get => unchecked((Int32)Checksum);
      set => _checksum = unchecked((UInt32)value);
    }
    public UInt64 FileId { get; set; }
    public UInt32 HeaderSize { get; set; }
    public Boolean IsCompressed => CompressionMethod != 0;
    public UInt64 Offset { get; set; }
    public UInt32 PrimaryHash { get; set; }
    public UInt32 SecondaryHash { get; set; }
    public UInt32 UncompressedSize { get; set; }

    #endregion Properties
  }
}
