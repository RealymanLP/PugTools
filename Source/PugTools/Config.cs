using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace PugTools {
  public static class Config {
    private static String _assetsPath = ".";
    private static Boolean _assetsUsePTS;
    private static Configuration _configFile;
    private static Boolean _crossLinkDOM;
    private static String _extractAssetsPath = ".";
    private static String _extractPath = ".";
    private static String _prevAssetsPath = ".";
    private static Boolean _prevAssetsUsePTS;

    public static String AssetsPath {
      get => _assetsPath;
      set => _assetsPath = value;
    }
    public static Boolean AssetsUsePTS {
      get => _assetsUsePTS;
      set => _assetsUsePTS = value;
    }
    public static Configuration ConfigFile {
      get => _configFile;
      set => _configFile = value;
    }
    public static Boolean CrossLinkDOM {
      get => _crossLinkDOM;
      set => _crossLinkDOM = value;
    }
    public static String ExtractAssetsPath {
      get => _extractAssetsPath;
      set => _extractAssetsPath = value;
    }
    public static String ExtractPath {
      get => _extractPath;
      set => _extractPath = value;
    }
    public static String PrevAssetsPath {
      get => _prevAssetsPath;
      set => _prevAssetsPath = value;
    }
    public static Boolean PrevAssetsUsePTS {
      get => _prevAssetsUsePTS;
      set => _prevAssetsUsePTS = value;
    }

    private static readonly List<String> liveAssetsPaths = new List<String> {
      "C:\\Program Files (x86)\\EA\\BioWare\\Star Wars - The Old Republic\\Assets\\",
      "C:\\Program Files (x86)\\Electronic Arts\\BioWare\\Star Wars - The Old Republic\\Assets\\",
      "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Star Wars - The Old Republic\\Assets\\"
    };

    private static readonly List<String> ptsAssetsPaths = new List<String> {
      "C:\\Program Files (x86)\\EA\\BioWare\\Star Wars - The Old Republic\\Assets\\",
      "C:\\Program Files (x86)\\Electronic Arts\\BioWare\\Star Wars - The Old Republic\\Assets\\",
      "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Star Wars - The Old Republic - PTS\\Assets\\"
    };

    public static void Load() {
      // Path to the asset files
      String str = ConfigFile.AppSettings.Settings["AssetsPath"].Value;

      if (str != null)
        // Load from config, if directory exists
        if (Directory.Exists(str)) AssetsPath = str;
        // Otherwise check some default directories
        else if (Directory.Exists(liveAssetsPaths[0])) AssetsPath = liveAssetsPaths[0];
        else if (Directory.Exists(liveAssetsPaths[1])) AssetsPath = liveAssetsPaths[1];
        else if (Directory.Exists(liveAssetsPaths[2])) AssetsPath = liveAssetsPaths[2];
        else AssetsPath = String.Empty;

      // Load PTS assets if checked
      str = ConfigFile.AppSettings.Settings["AssetsUsePTS"].Value;
      if (str != null) AssetsUsePTS = Convert.ToBoolean(str);

      // Path to the previous asset files
      str = ConfigFile.AppSettings.Settings["PrevAssetsPath"].Value;

      if (str != null)
        // Load from config, if directory exists
        if (Directory.Exists(str)) PrevAssetsPath = str;
        //otherwise check some default directories
        else if (Directory.Exists(ptsAssetsPaths[0])) PrevAssetsPath = ptsAssetsPaths[0];
        else if (Directory.Exists(ptsAssetsPaths[1])) PrevAssetsPath = ptsAssetsPaths[1];
        else if (Directory.Exists(ptsAssetsPaths[2])) PrevAssetsPath = ptsAssetsPaths[2];
        else PrevAssetsPath = str;

      // Load PTS assets if checked
      str = ConfigFile.AppSettings.Settings["PrevAssetsUsePTS"].Value;

      if (str != null) PrevAssetsUsePTS = Convert.ToBoolean(str);

      // Path to the extract files
      str = ConfigFile.AppSettings.Settings["ExtractPath"].Value;

      if (str != null) {
        if (!str.EndsWith("\\")) ExtractPath = str + "\\";
        else ExtractPath = str;
      }

      // Path Where to Extract Assets
      str = ConfigFile.AppSettings.Settings["ExtractAssetsPath"].Value;

      if (str != null) {
        if (!str.EndsWith("\\")) ExtractAssetsPath = str + "\\";
        else ExtractAssetsPath = str;
      }

      // Cross Link DOM
      str = ConfigFile.AppSettings.Settings["CrossLinkDOM"].Value;

      if (str != null) CrossLinkDOM = Convert.ToBoolean(str);
    }
    public static void Save() {
      // Path to the asset files
      String str = AssetsPath;
      if (str != null) ConfigFile.AppSettings.Settings["AssetsPath"].Value = str;

      // Load PTS assets if checked
      str = AssetsUsePTS.ToString();
      if (str != null) ConfigFile.AppSettings.Settings["AssetsUsePTS"].Value = str;

      // Path to the previous asset files
      str = PrevAssetsPath;
      if (str != null) ConfigFile.AppSettings.Settings["PrevAssetsPath"].Value = str;

      // Load PTS assets if checked
      str = PrevAssetsUsePTS.ToString();
      if (str != null) ConfigFile.AppSettings.Settings["PrevAssetsUsePTS"].Value = str;

      // Path to the extract files
      str = ExtractPath;
      if (str != null) ConfigFile.AppSettings.Settings["ExtractPath"].Value = str;

      // Path to the extract files
      str = ExtractAssetsPath;
      if (str != null) ConfigFile.AppSettings.Settings["ExtractAssetsPath"].Value = str;

      // Cross Link DOM
      str = CrossLinkDOM.ToString();
      if (str != null) ConfigFile.AppSettings.Settings["CrossLinkDOM"].Value = str;

      ConfigFile.Save(ConfigurationSaveMode.Modified);
      ConfigurationManager.RefreshSection("appSettings");
    }
  }
}
