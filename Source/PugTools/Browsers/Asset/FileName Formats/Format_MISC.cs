using System;
using System.Collections.Generic;
using System.IO;
using GomLib;

namespace PugTools {
  internal class Format_MISC {
    private readonly String _dest;
    private readonly List<String> _errors;
    private readonly String _extension;
    private readonly HashSet<String> _fileNames;
    private readonly Dictionary<String, HashSet<String>> _mapNames;
    private readonly HashSet<String> _worldFileNames;

    internal Int32 Found { get; set; }
    internal Int32 Searched { get; set; }

    internal Format_MISC(String dest, String ext) {
      _dest = dest;
      _errors = new List<String>();
      _extension = ext;
      _fileNames = new HashSet<String>();
      _mapNames = new Dictionary<String, HashSet<String>>();
      _worldFileNames = new HashSet<String>();
    }
    internal void ParseMISC_CDX(List<GomObject> cdxNodes) {
      foreach (GomObject obj in cdxNodes) {
        Searched++;
        String full = obj.Name.ToLower().ToString();
        _fileNames.Add("/resources/gfx/codex/" + full + ".dds");
        obj.Unload();
      }
    }
    internal void ParseMISC_IPP(List<GomObject> ippNodes) {
      foreach (GomObject obj in ippNodes) {
        Searched++;
        String full = obj.Name.ToLower().ToString();
        String partial = obj.Name.ToLower().ToString().Replace("ipp.", "");

        _fileNames.Add("/resources/gfx/icons/" + full + ".dds");
        _fileNames.Add("/resources/gfx/icons/" + partial + ".dds");

        _fileNames.Add("/resources/gfx/mtxstore/" + full + "_120x120.dds");
        _fileNames.Add("/resources/gfx/mtxstore/" + full + "_260x260.dds");
        _fileNames.Add("/resources/gfx/mtxstore/" + full + "_260x400.dds");
        _fileNames.Add("/resources/gfx/mtxstore/" + full + "_328x160.dds");
        _fileNames.Add("/resources/gfx/mtxstore/" + full + "_400x400.dds");

        _fileNames.Add("/resources/gfx/mtxstore/" + partial + "_120x120.dds");
        _fileNames.Add("/resources/gfx/mtxstore/" + partial + "_260x260.dds");
        _fileNames.Add("/resources/gfx/mtxstore/" + partial + "_260x400.dds");
        _fileNames.Add("/resources/gfx/mtxstore/" + partial + "_328x160.dds");
        _fileNames.Add("/resources/gfx/mtxstore/" + partial + "_400x400.dds");

        obj.Unload();
      }
    }
    internal void ParseMISC_ITEM(Dictionary<Object, Object> itemApperances) {
      foreach (KeyValuePair<Object, Object> kvp in itemApperances) {
        Searched++;
        GomObjectData itemAppearance = (GomObjectData)kvp.Value;
        String itmModel = itemAppearance.ValueOrDefault<String>("itmModel", null);

        if (itmModel != null)
          _fileNames.Add(("/resources/" + (itmModel.Replace("\\", "/"))).Replace("//", "/"));

        String itmFxSpec = itemAppearance.ValueOrDefault<String>("itmFxSpec", null);

        if (itmFxSpec != null)
          _fileNames.Add(
            ("/resources/art/fx/fxspec/" + itmFxSpec + ".fxspec").Replace("//", "/").Replace(
              ".fxspec.fxspec",
              ".fxspec"
            )
          );
      }
    }
    internal void ParseMISC_LdnScn(GomObject ldnScreenNode) {
      Dictionary<Object, Object> ldgLookup =
        ldnScreenNode.Data.Get<Dictionary<Object, Object>>("ldgAreaNameToLoadScreen");

      foreach (KeyValuePair<Object, Object> kvpLdgClass in ldgLookup) {
        Searched++;
        GomObjectData areaLdgInfo = (GomObjectData)kvpLdgClass.Value;
        String loadingScreen = areaLdgInfo.ValueOrDefault("ldgScreenName", String.Empty);

        if (loadingScreen.Length > 0) {
          _fileNames.Add("/resources/gfx/loadingscreen/" + loadingScreen + ".dds");
        }

        String loadingOverlay = areaLdgInfo.ValueOrDefault("ldgOverlayName", String.Empty);

        if (loadingOverlay.Length > 0) {
          _fileNames.Add("/resources/gfx/gfx_productions/" + loadingOverlay + ".gfx");
        }
      }
    }
    internal void ParseMISC_NODE(Dictionary<String, DomType> nodeDict) {
      foreach (KeyValuePair<String, DomType> obj in nodeDict) {
        Searched++;
        GomObject node = (GomObject)obj.Value;
        _fileNames.Add("/resources/systemgenerated/prototypes/" + node.Id.ToString() + ".node");
        node.Unload();
      }
    }
    internal void ParseMISC_TUTORIAL(DataObjectModel currentDom) {
      StringTable tutorialTable = currentDom.StringTable.Find("str.gui.tutorials");

      if (tutorialTable != null && tutorialTable.data != null) {
        foreach (KeyValuePair<Int64, StringTableEntry> item in tutorialTable.data) {
          if (item.Value.LocalizedText.ContainsKey("enMale")) {
            String text = item.Value.LocalizedText["enMale"];

            if (text.Contains(".dds")) {
              Int32 start = 0;

              while ((start = text.IndexOf("img://", start)) != -1) {
                Int32 end = text.IndexOf(".dds", start);

                if (end != -1) {
                  String temp = text.Substring(start, end - start + 4).ToLower();
                  temp =
                    temp.Replace("img://", "/resources/").Replace("//", "/").Replace(
                      "<<grammar::locpath>>",
                      "en-us"
                    );
                  _fileNames.Add(temp);
                  start++;
                }
              }

            } else if (text.Contains("img://")) {
              Int32 start = 0;

              while ((start = text.IndexOf("img://", start)) != -1) {
                Int32 end = text.IndexOf("'", start);

                if (end != -1) {
                  String temp = text.Substring(start, (end - start) + 1).ToLower();
                  temp =
                    temp.Replace("img://", "/resources/").Replace("//", "/").Replace(
                      "<<grammar::locpath>>",
                      "en-us"
                    );
                  _fileNames.Add(temp + ".dds");
                  start++;
                }
              }
            }
          }
        }
      }
    }
    internal void ParseMISC_WORLD(List<GomObject> worldAreas,
                                Dictionary<Object, Object> worldAreasProto,
                                DataObjectModel currentDom) {

      foreach (GomObject obj in worldAreas) {
        Searched++;
        UInt64 areaId = obj.Data.ValueOrDefault<UInt64>("mapDataContainerAreaID", 0);

        if (areaId > 0) {
          _worldFileNames.Add(
            String.Format("/resources/world/areas/{0}/area.dat", areaId.ToString()));
          _worldFileNames.Add(
            String.Format("/resources/world/areas/{0}/mapnotes.not", areaId.ToString()));

          List<Object> mapPages =
            obj.Data.ValueOrDefault<List<Object>>("mapDataContainerMapDataList", null);

          if (mapPages != null) {
            foreach (GomObjectData mapPage in mapPages) {
              String mapName = mapPage.ValueOrDefault<String>("mapName", null);

              if (!_mapNames.ContainsKey(areaId.ToString()))
                _mapNames.Add(areaId.ToString(), new HashSet<String>());
              _mapNames[areaId.ToString()].Add(mapName.ToString());
            }

            mapPages.Clear();
          }
        }

        obj.Unload();
      }

      worldAreas.Clear();

      foreach (KeyValuePair<Object, Object> gomItm in worldAreasProto) {
        GomLib.Models.Area area = new GomLib.Models.Area();
        currentDom.AreaLoader.Load(area, (GomObjectData)gomItm.Value);

        if (area.Id == 0 && area.AreaId == 0) continue;

        Searched++;

        if (area.MapPages != null) {
          Int32 ii = 0;

          foreach (GomLib.Models.MapPage map_page in area.MapPages) {
            ii++;

            if (map_page.HasImage) {
              if (!_mapNames.ContainsKey(area.AreaId.ToString()))
                _mapNames.Add(area.AreaId.ToString(), new HashSet<String>());

              _mapNames[area.AreaId.ToString()].Add(map_page.MapName);
            }
          }

          area.MapPages.Clear();
        }

        if (area.Assets != null) area.Assets.Clear();
      }

      worldAreasProto.Clear();
    }
    internal void WriteFile() {
      if (!Directory.Exists(_dest + "\\File_Names"))
        Directory.CreateDirectory(_dest + "\\File_Names");

      Found = _fileNames.Count;
      if (_fileNames.Count > 0) {
        StreamWriter outputNames =
          new StreamWriter(_dest + "\\File_Names\\" + _extension + "_file_names.txt", false);

        foreach (String file in _fileNames) {
          outputNames.Write(file.Replace("\\", "/") + "\r\n");
        }

        outputNames.Close();
        _fileNames.Clear();
      }

      Found += _worldFileNames.Count;

      if (_worldFileNames.Count > 0) {
        StreamWriter outputNames = new StreamWriter(
          _dest + "\\File_Names\\" + _extension + "_world_file_names_1.txt", false
        );
        Int32 fileCount = 1;
        Int32 lineCount = 1;

        foreach (String file in _worldFileNames) {
          if (lineCount >= 750000) {
            outputNames.Close();
            fileCount++;
            outputNames =
              new StreamWriter(
                _dest + "\\File_Names\\" + _extension + "_world_file_names_" + fileCount + ".txt",
                false
              );
            lineCount = 0;
          }

          outputNames.WriteLine(file.Replace("\\", "/"));
          lineCount++;
        }

        outputNames.Close();
        _worldFileNames.Clear();
      }

      Found += _mapNames.Count;

      if (_mapNames.Count > 0) {
        StreamWriter outputMapNames =
          new StreamWriter(
            _dest + "\\File_Names\\" + _extension + "_world_map_file_names_1.txt",
            false
          );
        Int32 fileCount = 1;
        Int32 lineCount = 1;

        foreach (KeyValuePair<String, HashSet<String>> kvp in _mapNames) {
          foreach (String line in kvp.Value) {
            if (lineCount >= 500000) {
              outputMapNames.Close();
              fileCount++;
              outputMapNames =
                new StreamWriter(
                  _dest + "\\File_Names\\"
                    + _extension + "_world_map_file_names_"
                    + fileCount + ".txt",
                  false
                );
              lineCount = 0;
            }

            outputMapNames.WriteLine(
              String.Format(
                "/resources/world/areas/{0}/{1}_r.dds",
                kvp.Key,
                line
              ).Replace("\\", "/").Replace("//", "/")
            );
            lineCount++;

            for (Int32 m = 0; m <= 50; m++) {
              for (Int32 mm = 0; mm <= 50; mm++) {
                outputMapNames.WriteLine(
                  String.Format(
                    "/resources/world/areas/{0}/minimaps/{1}_{2:00}_{3:00}_r.dds",
                    kvp.Key,
                    line,
                    m,
                    mm
                  ).Replace("\\", "/").Replace("//", "/")
                );
                lineCount++;
              }
            }
          }

          kvp.Value.Clear();
        }

        outputMapNames.Close();
        _mapNames.Clear();
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
