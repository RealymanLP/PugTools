using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TorArchive {
  public class Icons {

    #region Constructors
    public Icons(Assets assets) {
      _assets = assets;
      Flush();
    }

    #endregion Constructors

    #region Fields
    private Dictionary<UInt64, HashSet<String>> _areaMaps;
    private readonly Assets _assets;
    private HashSet<String> _codex;
    private HashSet<String> _filenames;
    private HashSet<String> _mtx;
    private HashSet<String> _portraits;

    #endregion Fields

    #region Methods
    public void Add(String filename) {
      if (!String.IsNullOrEmpty(filename)) {
        _filenames.Add(filename.ToLower());
      }
    }
    public void AddCodex(String fileName) {
      if (!String.IsNullOrEmpty(fileName)) {
        _codex.Add(fileName.ToLower());
      }
    }
    public void AddMap(UInt64 areaId, String mapName) {
      if (!_areaMaps.TryGetValue(areaId, out HashSet<String> set)) {
        set = new HashSet<String>();
        _areaMaps[areaId] = set;
      }

      _ = set.Add(mapName.ToLower());
    }
    public void AddMtx(String fileName) {
      if (!String.IsNullOrEmpty(fileName)) {
        //Mtx.Add(fileName.ToLower() + "_120x120");
        //Mtx.Add(fileName.ToLower() + "_260x400");
        //Mtx.Add(fileName.ToLower() + "_260x260");
        //Mtx.Add(fileName.ToLower() + "_328x160"); //really only want the biggest one in most cases.
        _ = _mtx.Add(fileName.ToLower() + "_400x400");
      }
    }
    public void AddPortrait(String path) {
      if (!String.IsNullOrEmpty(path)) {
        _portraits.Add(path.ToLower());
      }
    }
    // private void ConvertDDSToJPG() {
    //   String ExecutableFilePath =
    //     Path.Combine(Directory.GetCurrentDirectory(), @"convertddstojpg.bat");
    //   String Arguments = @"";

    //   if (System.IO.File.Exists(ExecutableFilePath)) {
    //     Debug.WriteLine("Executing convertddstojpg.bat : Before");
    //     Process p = new Process() {
    //       StartInfo = new ProcessStartInfo(ExecutableFilePath, Arguments)
    //     };
    //     p.ErrorDataReceived += P_ErrorDataReceived;
    //     p.OutputDataReceived += P_OutputDataReceived;
    //     p.Exited += P_Exited;
    //     p.Start();
    //     Debug.WriteLine("Executing convertddstojpg.bat : Executed");
    //   }
    // }
    private static void DeleteDDSFiles() {
      String[] fileList =
        Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), @"images\"));

      if (!fileList.Where(i => i.EndsWith(".jpg")).Any()) {
        Debug.WriteLine("Images were not converted to JPG, not deleting original DDS files");
        return;
      }

      for (Int32 i = 0; i < fileList.Length; i++) {
        Int32 attempt = 0;

        while (attempt < 3 && System.IO.File.Exists(fileList[i])) {
          System.IO.File.Delete(fileList[i]);
          attempt++;
        }
      }

      Debug.WriteLine("Images were not converted to JPG, not deleting original DDS files");
    }
    public void Flush() {
      _filenames = new HashSet<String>();
      _portraits = new HashSet<String>();
      _codex = new HashSet<String>();
      _mtx = new HashSet<String>();
      _areaMaps = new Dictionary<UInt64, HashSet<String>>();
    }
    private void P_ErrorDataReceived(Object sender, DataReceivedEventArgs e) {
      Debug.WriteLine(e.Data);
    }
    private void P_Exited(Object sender, EventArgs e) {
      Debug.WriteLine("Executing convertddstojpg.bat : Completed");
      DeleteDDSFiles();
    }
    private void P_OutputDataReceived(Object sender, DataReceivedEventArgs e) {
      Debug.WriteLine(e.Data);
    }
    public void SaveCodexTo(String dir, Boolean overwrite = false) {
      SaveSetTo(dir, _codex, "/resources/gfx/codex/{0}.dds", "codex", overwrite);
    }
    public void SaveMapsTo(String dir, Boolean overwrite = false) {
      foreach (KeyValuePair<UInt64, HashSet<String>> maps in _areaMaps) {
        String outDir = Path.Combine(dir, maps.Key.ToString());

        if (!Directory.Exists(outDir)) {
          Directory.CreateDirectory(outDir);
        }

        SaveSetTo(outDir,
                  maps.Value,
                  $"/resources/world/areas/{maps.Key}/{{0}}_r.dds",
                  "map",
                  overwrite);
      }
    }
    public void SaveMtxTo(String dir, Boolean overwrite = false) {
      SaveSetTo(dir, _mtx, "/resources/gfx/mtxstore/{0}.dds", "mtx", overwrite);
    }
    public void SavePortraitsTo(String dir, Boolean overwrite = false) {
      HashSet<String> existingFiles = new HashSet<String>();
      FileMode fileMode = FileMode.Create;

      if (!overwrite) {
        fileMode = FileMode.CreateNew;
        String[] currentFiles = Directory.GetFiles(dir, "*.dds");

        foreach (String fileName in currentFiles) {
          existingFiles.Add(Path.GetFileNameWithoutExtension(fileName));
        }
      }

      Byte[] copyBuffer = new Byte[4096];
      Int32 filesSaved = 0;

      _portraits.ForEach(iconName => {
        String portraitFileName = Path.GetFileNameWithoutExtension(iconName);

        if (existingFiles.Contains(portraitFileName)) {
          return;
        }

        String iconPath = $"/resources{iconName}";
        File file = _assets.FindFile(iconPath);

        if (file == null) {
          Debug.WriteLine("Unable to find portrait: {0}", iconPath);
          return;
        }

        String outPath = Path.Combine(dir, portraitFileName + ".dds");

        using Stream inFile = file.Open();
        using FileStream outFile = System.IO.File.Open(outPath, fileMode, FileAccess.Write);

        inFile.CopyTo(outFile, copyBuffer);
        filesSaved++;
      });

      Debug.WriteLine("Saving {0} Portraits to {1} [Overwrite = {2}]", filesSaved, dir, overwrite);
    }
    private void SaveSetTo(String dir,
                           HashSet<String> fileNames,
                           String internalPathFormat,
                           String imageType,
                           Boolean overwrite) {

      HashSet<String> existingFiles = new HashSet<String>();
      FileMode fileMode = FileMode.Create;

      if (!overwrite) {
        fileMode = FileMode.CreateNew;
        String[] currentFiles = Directory.GetFiles(dir, "*.dds");

        foreach (String fileName in currentFiles) {
          existingFiles.Add(Path.GetFileNameWithoutExtension(fileName));
        }
      }

      Byte[] copyBuffer = new Byte[4096];
      Int32 filesSaved = 0;

      fileNames.ForEach(iconName => {
        String fileName = iconName;

        if (existingFiles.Contains(iconName)) {
          return;
        }

        String iconPath = String.Format(internalPathFormat, iconName);
        File file = _assets.FindFile(iconPath);

        if (file == null) {
          Debug.WriteLine($"Unable to find {imageType}: {iconPath}");
          return;
        }

        String outPath = Path.Combine(dir, iconName + ".dds");

        using Stream inFile = file.Open();
        using FileStream outFile = System.IO.File.Open(outPath, fileMode, FileAccess.Write);

        inFile.CopyTo(outFile, copyBuffer);
        filesSaved++;
      });

      Debug.WriteLine($"Saving {filesSaved} {imageType} Images to {dir} [Overwrite = {overwrite}]");
    }
    public void SaveTo(String dir, Boolean overwrite = false) {
      SaveSetTo(dir, _filenames, "/resources/gfx/icons/{0}.dds", "icon", overwrite);
      SaveSetTo(dir, _filenames, "/resources/gfx/textures/{0}.dds", "icon", overwrite);
    }

    #endregion Methods
  }
}
