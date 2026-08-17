using System;
using System.Collections.Generic;
using System.IO;
using GomLib;

namespace PugTools {
  internal class Format_PLC {
    private readonly String _dest;
    private readonly List<String> _errors;
    private readonly String _extension;

    internal HashSet<String> FileNames { get; set; }

    internal Format_PLC(String dest, String ext) {
      _dest = dest;
      _errors = new List<String>();
      _extension = ext;
      FileNames = new HashSet<String>();
    }
    internal void ParsePLC(List<GomObject> plcNodes) {
      foreach (GomObject obj in plcNodes) {
        String plcModel = obj.Data.ValueOrDefault<String>("plcModel", null);

        if (plcModel != null)
          if (plcModel.Contains("dyn.")) continue;
          else FileNames.Add(plcModel.Replace("\\", "/").Replace("//", "/"));

        obj.Unload();
      }
    }
    internal void WriteFile(Boolean _ = false) {
      if (!Directory.Exists(_dest + "\\File_Names"))
        Directory.CreateDirectory(_dest + "\\File_Names");

      if (FileNames.Count > 0) {
        StreamWriter outputFileNames =
          new StreamWriter(_dest + "\\File_Names\\" + _extension + "_file_names.txt", false);

        foreach (String item in FileNames) {
          if (item != "") outputFileNames.WriteLine(("/resources/" + item).Replace("//", "/"));
        }

        outputFileNames.Close();
        FileNames.Clear();
      }

      if (_errors.Count > 0) {
        StreamWriter outputErrors =
          new StreamWriter(_dest + "\\File_Names\\" + _extension + "_error_list.txt", false);

        foreach (String error in _errors) {
          outputErrors.Write(error + "\r\n");
        }

        outputErrors.Close();
        _errors.Clear();
      }
    }
  }
}
