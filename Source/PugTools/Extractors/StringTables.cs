using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using GomLib;

namespace PugTools {
  internal partial class Tools {
    private static HashSet<String> DiscoverStringTables(DataObjectModel dom) {
      List<GomObject> itmList = dom.GetObjectsStartingWith("cnv.");
      XDocument doc =
        XDocument.Load(
          dom.Assets.FindFile("\\resources\\gamedata\\str\\stb.manifest").OpenCopyInMemory()
        );

      dom.StringTable.Flush(); //flushing out any loaded string tables that might have been altered.

      HashSet<String> foundStringTables = new HashSet<String>();

      foreach (XElement element in doc.Element("manifest").Elements("file")) {
        foundStringTables.Add(element.Attribute("val").Value);
        // XElement stringTable = StbToXElement(element.Attribute("val").Value);
        // if (stringTable.Elements().Count() > 0) stringTables.Add(stringTable);
      }

      foreach (GomObject itm in itmList) {
        /*
        Dictionary<Object, Object> dialogNodeMap = 
          itm.Data.ValueOrDefault<Dictionary<Object, Object>>(
            "cnvTreeDialogNodes_Prototype", 
            new Dictionary<Object, Object>()
          );

        foreach (KeyValuePair<Object, Object> dialogKvp in dialogNodeMap) {
          long nodeNumber = ((GomObjectData)dialogKvp.Value).Get<long>("cnvNodeNumber");
          Dictionary<Object, Object> textMap = 
            ((GomObjectData)dialogKvp.Value).Get<Dictionary<Object, Object>>("locTextRetrieverMap");
          if (textMap.ContainsKey(nodeNumber)) {
            string stb = 
              ((GomObjectData)textMap[(Int64)nodeNumber]).Get<String>(
                "strLocalizedTextRetrieverBucket"
              );
            foundStringTables.Add(stb);
          }
        }
        // This is always equal to the conversation node name. We can save a ton of time by just 
        // looking at that.
        */

        String potentialStb = "str." + itm.Name;

        foundStringTables.Add(potentialStb);
        itm.Unload();
      }

      return foundStringTables;
    }
    internal void GetStrings() {
      Clearlist2();
      LoadData();

      // String generatedContent = ConversationStringTables(itmList);

      XElement stringTables;
      HashSet<String> foundStringTables = DiscoverStringTables(CurrentDom);

      /*
      if (chkBuildCompare.Checked) {
        prevfoundStringTables = DiscoverStringTables(previousDom);
        if (foundStringTables.Count != prevfoundStringTables.Count) {
          string pausehere = "";
        }
      }
      */

      stringTables = new XElement("StringTables");

      Clearlist2();
      ClearProgress();
      AddToList2("Loading String Tables.");

      Int32 count = foundStringTables.Count;
      Int32 i = 0;

      foreach (String stb in foundStringTables) {
        if (chkBuildCompare.Checked) {
          StringTable curTbl = CurrentDom.StringTable.Find(stb);

          ProgressUpdate(i, count);

          StringTable prevTbl = PreviousDom.StringTable.Find(stb);

          if (curTbl != null) {
            if (prevTbl != null) {
              if (!curTbl.Equals(prevTbl)) {
                AddToList2(string.Format("Changed: {0}", curTbl.Fqn));

                XElement newElement = CompareElements(
                  StbToXElement(prevTbl),
                  StbToXElement(curTbl)
                );

                if (newElement != null) {
                  newElement.Add(new XAttribute("Status", "Changed"));

                  if (newElement.Elements().Any())
                    stringTables.Add(newElement);
                }
              }
            } else {
              AddToList2(string.Format("New: {0}", curTbl.Fqn));

              XElement newElement = StbToXElement(curTbl);
              newElement.Add(new XAttribute("Status", "New"));

              if (newElement.Elements().Any())
                stringTables.Add(newElement);
            }
          } else if (prevTbl != null) {
            AddToList2(string.Format("Removed: {0}", prevTbl.Fqn));

            XElement remElement = StbToXElement(prevTbl);
            remElement.Add(new XAttribute("Status", "Removed"));

            if (remElement.Elements().Any())
              stringTables.Add(remElement);
          }
        } else {
          AddToList2("String Table: " + stb);

          XElement stringTable = StbToXElement(CurrentDom, stb);
          // if (stringTable.Elements().Count() > 0) {
          stringTables.Add(stringTable);
          // }
        }
        CurrentDom.StringTable.Flush(); //Seeing if this helps memory issues

        if (PreviousDom != null) PreviousDom.StringTable.Flush();

        i++;
      }

      if (chkBuildCompare.Checked) {
        // addtolist("Comparing the Current Abilities to the loaded Patch");
        // XElement addedItems = FindChangedEntries(stringTables, "StringTables", "StringTable");

        XElement addedItems = stringTables;

        AddToList1(
          "The String Tables has been generated there are "
            + addedItems.Elements("StringTable").Count()
            + " new/changed String Tables"
        );
        WriteFile(new XDocument(addedItems), "ChangedStringTables.xml", false);
      } else {
        XDocument stringTablesXDocument = new XDocument(stringTables);
        WriteFile(stringTablesXDocument, "StringTables.xml", false);
      }
      EnableButtons();
    }
    private XElement StbToXElement(DataObjectModel dom, String stb) {
      StringTable stbTable = dom.StringTable.Find(stb);

      //Debug.WriteLine(stb);
      if (stbTable == null) {
        //Debug.WriteLine("Couldn't find " + stb + " string table.");
        return new XElement(
          "StringTable",
          new XAttribute("Id", stb),
          new XAttribute("Notfound", "true")
        );
      }

      return StbToXElement(stbTable);
    }
    private XElement StbToXElement(StringTable stb) {
      XElement stringTable = new XElement("StringTable", new XAttribute("Id", stb.Fqn));
      // String stb = "/resources/en-us/" + .Replace(".", "/") + ".stb";

      try {
        foreach (var entry in stb.data) {
          // If we are doing a compare build then strip the entries without any values.

          if (chkBuildCompare.Checked) {
            String value = entry.Value.LocalizedText["enMale"];

            if (value != null && value.Length > 0) {
              stringTable.Add(new XElement("Entry", new XAttribute("Id", entry.Key), value));
            }
          } else {
            if (!string.IsNullOrEmpty(entry.Value.LocalizedText["enMale"]))
              stringTable.Add(
                new XElement("Entry", new XAttribute("Id", entry.Key),
                new XElement("en", entry.Value.LocalizedText["enMale"]),
                new XElement("fr", entry.Value.LocalizedText["frMale"]),
                new XElement("de", entry.Value.LocalizedText["deMale"])
                )
              );
          }
        }
      }
      catch {
        // Debug.WriteLine("Couldn't find " + stb + " string table.");
      }

      return stringTable;
    }
  }
}
