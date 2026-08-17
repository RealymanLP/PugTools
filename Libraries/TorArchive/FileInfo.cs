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

    /// <summary>CRC32 checksum</summary>
    public UInt32 Checksum { get; set; }
    public UInt32 CompressedSize { get; set; }
    public UInt16 CompressionMethod { get; set; }
    public Int32 CRC { get; set; }
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
