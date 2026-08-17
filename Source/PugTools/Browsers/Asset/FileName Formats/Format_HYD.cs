using System;
using System.Collections.Generic;
using System.IO;
using GomLib;

namespace PugTools {
  internal class Format_HYD {
    private readonly String _dest;
    private readonly List<String> _errors;
    private readonly String _extension;

    internal HashSet<String> AnimFileNames { get; set; }
    internal HashSet<String> VfxFileNames { get; set; }

    internal Format_HYD(String dest, String ext) {
      _dest = dest;
      _errors = new List<String>();
      _extension = ext;
      AnimFileNames = new HashSet<String>();
      VfxFileNames = new HashSet<String>();
    }
    internal void ParseHYD(List<GomObject> hydNodes) {
      foreach (GomObject obj in hydNodes) {
        Dictionary<Object, Object> hydScriptMap =
          obj.Data.ValueOrDefault<Dictionary<Object, Object>>("hydScriptMap", null);

        if (hydScriptMap != null) {
          foreach (KeyValuePair<Object, Object> scriptMapItem in hydScriptMap) {
            GomObjectData scriptMapItem2 = (GomObjectData)scriptMapItem.Value;
            List<Object> hydScriptBlocks =
              scriptMapItem2.ValueOrDefault<List<Object>>("hydScriptBlocks", null);

            if (hydScriptBlocks != null) {
              foreach (Object hydScriptBlocksItem in hydScriptBlocks) {
                GomObjectData hydScriptBlocksItem2 = (GomObjectData)hydScriptBlocksItem;
                List<Object> hydActionBlocks =
                  hydScriptBlocksItem2.ValueOrDefault<List<Object>>("hydActionBlocks", null);

                if (hydActionBlocks != null) {
                  foreach (Object hydActionBlocksItem in hydActionBlocks) {
                    GomObjectData hydActionBlocksItem2 = (GomObjectData)hydActionBlocksItem;
                    List<Object> hydActions =
                      hydActionBlocksItem2.ValueOrDefault<List<Object>>("hydActions", null);

                    if (hydActions != null) {
                      foreach (Object hydActionsItem in hydActions) {
                        GomObjectData hydActionsItem2 = (GomObjectData)hydActionsItem;
                        String action =
                          hydActionsItem2.ValueOrDefault<Object>("hydAction", "").ToString();
                        String value =
                          hydActionsItem2.ValueOrDefault<Object>(
                            "hydValue", ""
                          ).ToString().ToLower();

                        if (action.Contains("Animation"))
                          AnimFileNames.Add(value);
                        else if (action.Contains("VFX"))
                          VfxFileNames.Add(value);
                      }
                    }
                  }
                }
              }
            }
          }
        }

        obj.Unload();
      }
    }
    internal void WriteFile(Boolean _ = false) {
      if (!Directory.Exists(_dest + "\\File_Names"))
        Directory.CreateDirectory(_dest + "\\File_Names");

      if (AnimFileNames.Count > 0) {
        StreamWriter outputAnimFileNames =
          new StreamWriter(_dest + "\\File_Names\\" + _extension + "_anim_file_names.txt", false);

        foreach (String item in AnimFileNames) {
          if (item != "")
            outputAnimFileNames.WriteLine(item);
        }

        outputAnimFileNames.Close();
        AnimFileNames.Clear();
      }

      if (VfxFileNames.Count > 0) {
        StreamWriter outputVfxFileNames =
          new StreamWriter(_dest + "\\File_Names\\" + _extension + "_fxspec_file_names.txt", false);

        foreach (String item in VfxFileNames) {
          if (item != "") {
            if (item.Contains("art/")) {
              String output = "/resources/" + item + ".fxspec";
              outputVfxFileNames.WriteLine(
                output.Replace("//", "/").Replace(".fxspec.fxspec", ".fxspec")
              );
            } else {
              String output = "/resources/art/fx/fxspec/" + item + ".fxspec";
              outputVfxFileNames.WriteLine(
                output.Replace("//", "/").Replace(".fxspec.fxspec", ".fxspec")
              );
            }
          }
        }

        outputVfxFileNames.Close();
        VfxFileNames.Clear();
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
