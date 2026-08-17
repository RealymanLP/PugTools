using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using GomLib;
using TorArchive;
using File = System.IO.File;

namespace PugTools {
  internal class BnkIdDict {
    private Boolean _loaded;

    internal Dictionary<UInt32, String> Data { get; set; }
    internal static BnkIdDict Instance { get; }

    private BnkIdDict() {
      Data ??= new Dictionary<UInt32, String>();
      Load();
    }
    static BnkIdDict() {
      Instance = new BnkIdDict();
    }
    internal void Load() {
      if (_loaded) return;

      if (File.Exists(Config.ExtractPath + "bnk_id_dictionary.txt")) {
        String[] lines = File.ReadAllLines(Config.ExtractPath + "bnk_id_dictionary.txt");

        if (lines.Length > 0)
          foreach (String line in lines) {
            if (line.Contains(':')) {
              String[] split = line.Split(':');
              Data.Add(UInt32.Parse(split[0].Trim()), split[1].Trim());
            }
          }
      }

      _loaded = true;
    }
    internal void Unload() {
      Data = new Dictionary<UInt32, String>();
      _loaded = false;

      GC.Collect();
    }
  }

  internal partial class Tools {
    internal void BuildBnkIdDict() {
      LoadData();
      AddToList1("Building BNK ID Dictionary...");
      AddToList2("Getting Data...");

      HashSet<String> eventNames = new HashSet<String>();

      // utlHydraMusicEventsTablePrototype ////////////////////////////////////////////////////////
      GomObject bnk1 = CurrentDom.GetObject("utlHydraMusicEventsTablePrototype");
      Dictionary<Object, Object> dataRows1 = bnk1?.Data.ValueOrDefault<Dictionary<Object, Object>>(
        "4611686309966584003",
        null
      );

      if (dataRows1 != null) {
        foreach (KeyValuePair<Object, Object> kvp in dataRows1) {
          GomObjectData value = (GomObjectData)kvp.Value;
          if (value.ContainsKey("4611686309966584004")) {
            eventNames.Add(value.ValueOrDefault<String>("4611686309966584004", null).ToLower());
          }
        }

        dataRows1.Clear();
      }
      bnk1?.Unload();

      // sndAmbienceRegionsTablePrototype /////////////////////////////////////////////////////////
      GomObject bnk2 = CurrentDom.GetObject("sndAmbienceRegionsTablePrototype");
      Dictionary<Object, Object> dataRows2 = bnk2?.Data.ValueOrDefault<Dictionary<Object, Object>>(
        "4611686359651217004",
        null
      );

      if (dataRows2 != null) {
        foreach (KeyValuePair<Object, Object> kvp in dataRows2) {
          foreach (GomObjectData value in (List<Object>)kvp.Value) {
            if (value.ContainsKey("4611686359651217001")) {
              eventNames.Add(value.ValueOrDefault<String>("4611686359651217001", null).ToLower());
              // eventNames.Add(value.ValueOrDefault<String>("4611686359651217002", null).ToLower());
            }
          }
        }

        dataRows2.Clear();
      }
      bnk2?.Unload();

      // sndAreaSoundBanksTablePrototype //////////////////////////////////////////////////////////
      GomObject bnk3 = CurrentDom.GetObject("sndAreaSoundBanksTablePrototype");
      Dictionary<Object, Object> dataRows3 = bnk3?.Data.ValueOrDefault<Dictionary<Object, Object>>(
        "4611686359651217016",
        null
      );

      if (dataRows3 != null) {
        foreach (KeyValuePair<Object, Object> kvp in dataRows3) {
          GomObjectData value = (GomObjectData)kvp.Value;
          if (value.ContainsKey("4611686359651217011")) {
            eventNames.Add(value.ValueOrDefault<String>("4611686359651217011", null).ToLower());
          }
        }

        dataRows3.Clear();
      }
      bnk3?.Unload();

      // sndAudioRegionsTablePrototype ////////////////////////////////////////////////////////////
      GomObject bnk4 = CurrentDom.GetObject("sndAudioRegionsTablePrototype");
      Dictionary<Object, Object> dataRows4 = bnk4?.Data.ValueOrDefault<Dictionary<Object, Object>>(
        "4611686359797337002",
        null
      );

      if (dataRows4 != null) {
        foreach (KeyValuePair<Object, Object> kvp in dataRows4) {
          foreach (GomObjectData value in (List<Object>)kvp.Value) {
            if (value.ContainsKey("4611686359676197004"))
              eventNames.Add(value.ValueOrDefault<String>("4611686359676197004", null).ToLower());
            if (value.ContainsKey("4611686359676197005"))
              eventNames.Add(value.ValueOrDefault<String>("4611686359676197005", null).ToLower());
          }
        }

        dataRows4.Clear();
      }
      bnk4?.Unload();

      // vehsoundpackage //////////////////////////////////////////////////////////////////////////
      GomObject bnk5 = CurrentDom.GetObject("vehsoundpackage");
      List<Object> dataRows5 = bnk5?.Data.ValueOrDefault<List<Object>>("utlDatatableRows", null);

      if (dataRows5 != null) {
        foreach (List<Object> row in dataRows5) {
          if (row.Count >= 6) {
            eventNames.Add(row[2].ToString().ToLower());
            eventNames.Add(row[5].ToString().ToLower());
          }
        }

        dataRows5.Clear();
      }
      bnk5?.Unload();

      // EPPs /////////////////////////////////////////////////////////////////////////////////////
      foreach (Library lib in CurrentAssets.Libraries) {
        String path = lib.Location;

        foreach (KeyValuePair<Int32, Archive> arch in lib.Archives) {
          foreach (TorArchive.File file in arch.Value.EnumerateFiles()) {
            // This scan can cover very large archives; report progress so it does not look frozen.
            if ((file.FileInfo.PrimaryHash & 0x3FF) == 0) AddToList2("Scanning EPP audio data...");
            HashFileInfo hashInfo =
              new HashFileInfo(file.FileInfo.PrimaryHash, file.FileInfo.SecondaryHash, file);

            if (hashInfo.IsNamed) {
              if (hashInfo.Extension != "epp") continue;

              using Stream assetStream = hashInfo.File.OpenCopyInMemory();

              XmlDocument doc = new XmlDocument();
              doc.Load(assetStream);

              XmlNodeList elemList = doc.GetElementsByTagName("dynamicData");

              foreach (XmlNode node in elemList) {
                if (node.InnerText.Contains("@audio")) {
                  String eventName = node.InnerText.Replace("@audio=", "");
                  // if (eventName.Contains(';')) Debug.WriteLine("pause here");
                  eventNames.Add(eventName);
                }
              }
            }
          }
        }
      }

      // PROCESSING ///////////////////////////////////////////////////////////////////////////////
      Dictionary<UInt32, String> eventNameDict = new Dictionary<UInt32, String>();

      if (eventNames.Count > 0) {
        foreach (String name in eventNames) {
          UInt32 id = FileFormats.FileHelpers.GetFNV1Hash(name);

          if (id != 0 && !eventNameDict.Keys.Contains(id)) {
            eventNameDict.Add(id, name);
          }
        }
      }

      AddToList2("Building Output...");

      StringBuilder outputFile = new StringBuilder();

      if (eventNameDict.Count > 0) {
        foreach (KeyValuePair<UInt32, String> kvp in eventNameDict) {
          outputFile.Append(
            kvp.Key.ToString().PadRight(10) + " : " + kvp.Value.Split(",").First() + Environment.NewLine
          );
        }

        using StreamWriter file2 =
          new StreamWriter(Config.ExtractPath + "bnk_id_dictionary.txt", false);

        file2.Write(outputFile.NullSafeToString());
      }

      AddToList2("Output Complete");
      Debug.WriteLine("Export Complete.");
      EnableButtons();
    }
  }
}
