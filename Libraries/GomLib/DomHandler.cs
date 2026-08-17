using System;
using TorArchive;

namespace GomLib {
  public class DomHandler : IDisposable {

    #region Fields
    private static readonly DomHandler s_instance = new DomHandler();
    private readonly Object m_currentLock = new Object();
    private readonly Object m_previousLock = new Object();
    private DataObjectModel m_currentData;
    private DataObjectModel m_previousData;
    private Boolean m_currentLoaded;
    private Boolean m_previousLoaded;

    #endregion Fields

    #region IDisposable
    private Boolean m_disposed = false;

    ~DomHandler() {
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
        m_currentData.Dispose();
        m_previousData.Dispose();
      }
      m_disposed = true;
    }

    #endregion IDisposable

    #region Methods
    public DataObjectModel GetCurrentDOM(Assets assets = null) {
      // See AssetHandler.GetCurrentAssets for why this needs locking: multiple browser windows
      // can trigger the first load concurrently.
      if (m_currentLoaded) {
        return m_currentData;
      }

      lock (m_currentLock) {
        if (m_currentLoaded) {
          return m_currentData;
        } else {
          if (assets != null) {
            m_currentData = new DataObjectModel(assets);
            m_currentData.Load();
            m_currentLoaded = true;
            return m_currentData;
          } else {
            throw new ArgumentException("No list of assests were provided");
          }
        }
      }
    }

    public DataObjectModel GetPreviousDOM(Assets assets = null) {
      if (m_previousLoaded) {
        return m_previousData;
      }

      lock (m_previousLock) {
        if (m_previousLoaded) {
          return m_previousData;
        } else {
          if (assets != null) {
            m_previousData = new DataObjectModel(assets);
            m_previousData.Load();
            m_previousLoaded = true;
            return m_previousData;
          } else {
            throw new ArgumentException("No list of assests were provided");
          }
        }
      }
    }

    #endregion Methods

    #region Properties
    public static DomHandler Instance => s_instance;
    public Boolean CurrentLoaded => m_currentLoaded;
    public Boolean PreviousLoaded => m_previousLoaded;

    #endregion Properties

    #region Unload Data
    public void UnloadAllDOM() {
      if (m_currentLoaded) {
        m_currentData.Dispose();
        m_currentData = null;
        m_currentLoaded = false;
      }

      if (m_previousLoaded) {
        m_previousData.Dispose();
        m_previousData = null;
        m_previousLoaded = false;
      }
      GC.Collect();
    }

    public void UnloadCurrentDOM() {
      if (m_currentLoaded) {
        m_currentData.Dispose();
        m_currentData = null;
        m_currentLoaded = false;
        GC.Collect();
      }

    }

    public void UnloadPreviousDOM() {
      if (m_previousLoaded) {
        m_previousData.Dispose();
        m_previousData = null;
        m_previousLoaded = false;
        GC.Collect();
      }
    }

    #endregion Unload Data
  }
}
