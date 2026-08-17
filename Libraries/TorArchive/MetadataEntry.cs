using System;

namespace TorArchive {
  public class MetadataEntry : IDisposable {

    #region IDisposable
    private Boolean m_disposed = false;

    ~MetadataEntry() {
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

    /// <summary>
    /// Which archive this file is located in
    /// </summary>
    public Byte Archive { get; set; }

    /// <summary>
    /// FileId this metadata entry points to
    /// </summary>
    public FileId FileId { get; set; }

    #endregion Properties
  }
}
