using System;

namespace TorArchive {
  public class AssetHandler : IDisposable {

    #region Fields
    private static readonly AssetHandler s_instance = new AssetHandler();
    private readonly Object m_currentLock = new Object();
    private readonly Object m_previousLock = new Object();
    private Assets m_currentData;
    private Assets m_previousData;
    private Boolean m_currentLoaded;
    private Boolean m_previousLoaded;

    // public Dictionary<string, Assets> loadedData = new Dictionary<string, Assets>();

    #endregion Fields

    #region IDisposable
    private Boolean m_disposed = false;

    ~AssetHandler() {
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
    public Assets GetCurrentAssets(String path = null, Boolean isPTR = false) {
      // Multiple browser windows can call this concurrently (e.g. two browsers opened close
      // together, both racing to trigger the first load). Without a lock, both could start
      // constructing/loading Assets at once, wasting work at best and corrupting shared state
      // at worst. Double-checked locking: cheap re-check outside the lock for the common case
      // (already loaded), real check + load happens only once, serialized, inside the lock.
      if (m_currentLoaded) {
        return m_currentData;
      }

      lock (m_currentLock) {
        if (m_currentLoaded) {
          return m_currentData;
        }

        if (path != null) {
          m_currentData = new Assets(path);
          m_currentData.Load(isPTR);
          m_currentLoaded = true;

          return m_currentData;
        } else {
          throw new ArgumentException("No path to the assests was provided");
        }
      }
    }
    public Assets GetPreviousAssets(String path = null, Boolean isPTR = false) {
      if (m_previousLoaded) {
        return m_previousData;
      }

      lock (m_previousLock) {
        if (m_previousLoaded) {
          return m_previousData;
        } else {
          if (path != null) {
            m_previousData = new Assets(path);
            m_previousData.Load(isPTR);
            m_previousLoaded = true;
            HashDictionaryInstance.Instance.Unload();
            return m_previousData;
          } else {
            throw new ArgumentException("No path to the assests were provided");
          }
        }
      }
    }

    #endregion Methods

    #region Properties
    public static AssetHandler Instance => s_instance;
    public Boolean CurrentLoaded => m_currentLoaded;
    public Boolean PreviousLoaded => m_previousLoaded;

    #endregion Properties

    #region Unload Data
    public void UnloadAllAssets() {
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
    public void UnloadCurrentAssets() {
      if (m_currentLoaded) {
        m_currentData.Dispose();
        m_currentData = null;
        m_currentLoaded = false;
      }
    }
    public void UnloadPreviousAssets() {
      if (m_previousLoaded) {
        m_previousData.Dispose();
        m_previousData = null;
        m_previousLoaded = false;
      }
    }

    #endregion Unload Data

  }
}
