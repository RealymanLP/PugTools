using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace TorArchive {
  public class Assets : IDisposable {

    #region Constructors
    public Assets(String assetPath) {
      m_assetPath = assetPath;
      Icons = new Icons(this);
      LoadedFileGroups = new List<String>();
    }

    #endregion Constructors

    #region Fields
    private readonly String m_assetPath;
    private readonly Regex m_fileNameParse = new Regex("swtor_(?:test_)?(.*)_1");

    #endregion Fields

    #region IDisposable
    private Boolean m_disposed = false;

    ~Assets() {
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
        foreach (Library lib in Libraries) {
          lib.Dispose();
        }

        Libraries.Clear();
      }

      m_disposed = true;
    }

    #endregion IDisposable

    #region Methods
    public File FindFile(String path) {
      if (path == null) {
        return null;
      }

      path = path.Replace('\\', '/');
      File result = null;

      foreach (Library lib in Libraries) {
        result = lib.FindFile(path);

        if (result != null) {
          return result;
        }
      }

      return result;
    }

    public Boolean HasFile(String path) {
      return FindFile(path) != null;
    }

    public void Load(Boolean isPtr) {
      Libraries = new List<Library>();

      LoadAssetFiles("main", isPtr);
      LoadAssetFiles("en-us", isPtr);
      LoadAssetFiles("fr-fr", isPtr);
      LoadAssetFiles("de-de", isPtr);

      // Beta
      LoadAssetFiles("locale_en_us", isPtr);
      LoadAssetFiles("system", isPtr);

      if (Libraries.Count == 0) {
        if (isPtr == false) {
          Load(true);
        } else {
          throw new Exception("Could not find asset files!");
        }
      }
    }

    private void LoadAssetFiles(String fileGroup, Boolean isPTS) {
      // LIVE & PTS
      String searchPattern = isPTS ? $"swtor_test_{fileGroup}_*.tor" : $"swtor_{fileGroup}_*.tor";
      String[] assetFilePaths = Directory.GetFiles(m_assetPath, searchPattern, 0);

      if (assetFilePaths.Length > 0) {
        foreach (String assetFilePath in assetFilePaths) {
          String assetFileName = Path.GetFileNameWithoutExtension(assetFilePath);
          Match match = m_fileNameParse.Match(assetFileName);

          if (match.Success) {
            String libName = match.Groups[1].Value;
            Library lib = new Library(libName, m_assetPath, isPTS);
            Libraries.Add(lib);
          }
        }

        LoadedFileGroups.Add(fileGroup);
        return;
      }

      // BETA: RED
      assetFilePaths = Directory.GetFiles(m_assetPath, $"red_{fileGroup}_*.tor", 0);

      if (assetFilePaths.Length > 0) {
        Library lib = new Library(fileGroup, m_assetPath);
        Libraries.Add(lib);
        LoadedFileGroups.Add(fileGroup);
        return;
      }

      // BETA: ASSETS
      assetFilePaths = Directory.GetFiles(m_assetPath, $"assets_{fileGroup}_*.tor", 0);

      if (assetFilePaths.Length > 0) {
        Library lib = new Library(fileGroup, m_assetPath);
        Libraries.Add(lib);
        LoadedFileGroups.Add(fileGroup);
        return;
      }
    }

    #endregion Methods

    #region Properties
    public Icons Icons { get; }
    public List<Library> Libraries { get; private set; }
    public List<String> LoadedFileGroups { get; private set; }

    #endregion Properties
  }
}
