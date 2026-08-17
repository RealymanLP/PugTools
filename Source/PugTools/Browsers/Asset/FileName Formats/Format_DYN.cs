using System;
using System.Collections.Generic;
using System.IO;
using GomLib;

namespace PugTools {
  internal class Format_DYN {
    private readonly String _dest;
    private readonly List<String> _errors;
    private readonly String _extension;

    internal HashSet<String> FileNames { get; set; }
    internal HashSet<String> UnknownFileNames { get; set; }

    internal Format_DYN(String dest, String ext) {
      _dest = dest;
      _errors = new List<String>();
      _extension = ext;
      FileNames = new HashSet<String>();
      UnknownFileNames = new HashSet<String>();
    }
    internal void ParseDYN(List<GomObject> dynNodes) {
      foreach (GomObject obj in dynNodes) {
        List<Object> dynVisualList =
          obj.Data.ValueOrDefault<List<Object>>("dynVisualList", null);

        if (dynVisualList != null) {
          foreach (Object dynVisualListItem in dynVisualList) {
            GomObjectData dynVisualListItem2 = (GomObjectData)dynVisualListItem;
            String visual =
              dynVisualListItem2.ValueOrDefault<Object>("dynVisualFqn", "").ToString().ToLower();

            if (visual != "") {
              String output = visual.Replace("\\", "/").Replace("//", "/");

              if (visual.Contains(".gr2") || visual.Contains(".lit") || visual.Contains(".mag")) {
                output = ("/resources/" + output).Replace("//", "/");
              } else if (visual.Contains(".fxspec")) {
                output = ("/resources/art/fx/fxspec/" + output).Replace("//", "/");
              } else if (visual.Contains(".fxp")) {
                output = ("/resources/art/fx/fxspec/" + output).Replace("//", "/");
              } else {
                UnknownFileNames.Add(visual);
              }

              FileNames.Add(output);
            }
          }
        }

        Dictionary<Object, Object> dynLightNameToProperty =
          obj.Data.ValueOrDefault<Dictionary<Object, Object>>("dynLightNameToProperty", null);

        if (dynLightNameToProperty != null) {
          foreach (var dynLightNameToPropertyItem in dynLightNameToProperty) {
            GomObjectData dynLightNameToPropertyItem2 =
              (GomObjectData)dynLightNameToPropertyItem.Value;
            String ramp =
              dynLightNameToPropertyItem2.ValueOrDefault<Object>(
                "dynLightRampMap", ""
              ).ToString().ToLower();
            String illum =
              dynLightNameToPropertyItem2.ValueOrDefault<Object>(
                "dynLightIlluminationMap", ""
              ).ToString().ToLower();
            String fall =
              dynLightNameToPropertyItem2.ValueOrDefault<Object>(
                "dynLightFalloff", ""
              ).ToString().ToLower();

            if (ramp != "")
              FileNames.Add("/resources/" + ramp + ".dds");

            if (illum != "")
              FileNames.Add("/resources/" + illum + ".dds");

            if (fall != "")
              FileNames.Add("/resources/" + fall + ".dds");
          }
        }

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
          if (item != "")
            outputFileNames.WriteLine(item);
        }

        outputFileNames.Close();
        FileNames.Clear();
      }

      if (UnknownFileNames.Count > 0) {
        StreamWriter outputUnknownFileNames =
          new StreamWriter(
            _dest + "\\File_Names\\" + _extension + "_unknown_file_names.txt", false
          );

        foreach (String item in UnknownFileNames) {
          if (item != "")
            outputUnknownFileNames.WriteLine(item);
        }

        outputUnknownFileNames.Close();
        UnknownFileNames.Clear();
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
