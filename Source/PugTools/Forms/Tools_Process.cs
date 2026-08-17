using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using GomLib;
using GomLib.Models;
using TorArchive;

namespace PugTools {
  internal partial class Tools {
    private Boolean _exportGOM = false;

    public Boolean ExportICONS1 { get; set; } = false;
    public Boolean ExportNPP1 { get; set; } = false;

    private XElement CompareElements(XElement previousElement, XElement newElement) {
      List<XElement> elementsToRemove = new List<XElement>();
      Int32 unmodifiedBaseElemCount = 0;

      if (previousElement != null) {
        IEnumerable<XNode> changedElements;
        List<String> removedItemIds = new List<String>();

        // Separate out the removed elements and remove them
        IEnumerable<XNode> removedElements =
          previousElement.Elements().Where(
            x => !x.HasAttributes
            ).Cast<XNode>().Except(
              newElement.Elements().Where(x => !x.HasAttributes).Cast<XNode>(),
              new XNodeEqualityComparer()
            );

        // Separate out the removed elements and tag them with the status attribute
        removedItemIds =
          previousElement.Elements().Where(
            x => x.HasAttributes
          ).Where(
            x => x.Attributes().Any(a => a.Name == "Id")
          ).Select(
            i => i.Attribute("Id").Value
          ).Except(
            newElement.Elements().Where(
              x => x.HasAttributes
            ).Where(
              x => x.Attributes().Any(a => a.Name == "Id")
            ).Select(
              i => i.Attribute("Id").Value
            )
          ).ToList();

        // Have to handle String tables separately, as they can have tens of thousands of 
        // sub-elements with the same name.
        if (newElement.Elements().Count() > 200) {
          changedElements =
            newElement.Elements().Cast<XNode>().Except(
              previousElement.Elements().Cast<XNode>(), new XNodeEqualityComparer()
            );

          if (s_removeUnchanged) {
            newElement.Elements().Cast<XNode>().Except(
              changedElements.Cast<XNode>(),
              new XNodeEqualityComparer()
            ).Remove();
          }

          // Remove the unchanged, and non-removed entries from the previous element to speed up 
          // enumeration of it when loading changed values
          List<String> changedItemIds =
            changedElements.Where(
              x => (x as XElement).HasAttributes
            ).Select(
              i => ((XElement)i).Attribute("Id").Value
            ).ToList();

          _ = previousElement.Elements().Cast<XNode>().Except(
            previousElement.Elements().Where(
              x => x.HasAttributes
            ).Where(
              x => changedItemIds.Contains(x.Attribute("Id").Value)
            )
          ).Except(
            previousElement.Elements().Where(
              x => x.HasAttributes
            ).Where(
              x => x.Attribute("Id") != null
            ).Where(
              x => removedItemIds.Contains(x.Attribute("Id").Value)
            )
          );

        } else {
          changedElements = newElement.Elements();
        }

        for (Int32 n = 0; n < changedElements.Count(); n++) {
          XElement subElement = changedElements.ElementAt(n) as XElement;
          XElement prevSubElement = null;
          IEnumerable<XElement> previousSubElements = previousElement.Elements(subElement.Name);
          Int32 prevSubEleCount = previousSubElements.Count();

          if (prevSubEleCount > 0) {
            if (subElement.Attribute("Id") != null) {
              if (prevSubEleCount > 1) {
                IEnumerable<XElement> potentialElements =
                  previousSubElements.Where(
                    x => x.Attributes("Id").Any()
                  ).Where(
                    x => x.Attribute("Id").Value == subElement.Attribute("Id").Value
                  );

                Int32 potEleCount = potentialElements.Count();

                if (potEleCount == 0) {
                  prevSubElement = null;
                } else {
                  if (potEleCount > 1)
                    AddToList2(
                      "Multiple potential matching elements for " + subElement.Name + " found"
                    );

                  prevSubElement = potentialElements.First();
                }
              } else {
                prevSubElement = previousSubElements.First();

                if (prevSubElement.Attribute("Id") != null
                    && subElement.Attribute("Id") != null) {
                  if (prevSubElement.Attribute("Id").Value != subElement.Attribute("Id").Value) {
                    removedItemIds.Remove(prevSubElement.Attribute("Id").Value);
                  }
                }
              }
            } else {
              prevSubElement = previousSubElements.First();
            }
          }

          if (prevSubElement != null) {
            // Need to normalize the xml elements and compare them again to catch ordering of 
            // subelement changes. 
            if (!XNode.DeepEquals(prevSubElement, subElement)) {
              if (prevSubElement.HasElements) {
                if (subElement.HasElements)
                  subElement.ReplaceWith(CompareElements(prevSubElement, subElement));
                else
                  try {
                    newElement.Add(new XAttribute("OldValue", prevSubElement.Value.ToString()));
                  }
                  catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
              } else {
                if (subElement.HasElements) subElement.Add(new XAttribute("Status", "New"));
                else subElement.Add(new XAttribute("OldValue", prevSubElement.Value));
              }
            } else {
              if (!(subElement.Name == "Name"
                  || subElement.Name == "Description"
                  || subElement.Name == "Fqn"
                  || subElement.Name == "enMale"
                  || subElement.Name == "Speaker"))
                // Element didn't change and it's clogging up the file, saving a shallow copy to 
                // remove it later. Because removing it now fucks with the foreach looping
                elementsToRemove.Add(subElement);
              else if (subElement.Attribute("OldValue") == null)
                // If this is a "base" element that we want to keep but its unchanged then 
                // increment the counter.
                unmodifiedBaseElemCount++;
            }
          } else
            subElement.Add(new XAttribute("Status", "New"));
        }

        for (Int32 i = 0; i < removedItemIds.Count; i++) {
          XElement removedItem =
            previousElement.Elements().Where(
              x => x.HasAttributes
            ).Where(
              x => x.Attributes().Any(a => a.Name == "Id")
            ).First(
              x => x.Attribute("Id").Value == removedItemIds[i]
            );

          if (!removedItem.Attributes().Any(a => a.Name == "Status")) {
            removedItem.Add(new XAttribute("Status", "Removed"));
            newElement.Add(removedItem); // Add removed elements to the return value
          }
        }

        List<XName> newElementNames = newElement.Elements().Select(x => x.Name).ToList();

        foreach (XNode remElement in removedElements) {
          XElement remX = remElement as XElement;

          if (!remX.HasElements && !newElementNames.Contains(remX.Name)) {
            ((XElement)remElement).Add(new XAttribute("Status", "Removed"));
            newElement.Add(remElement);
          }
        }
      } else {
        newElement.Add(new XAttribute("NewElement", "true"));
      }

      // Added this check so we can maintain the old functionality with the aid of a checkbox
      if (s_removeUnchanged) {
        for (Int32 i = elementsToRemove.Count - 1; i >= 0; i--) {
          elementsToRemove[i].Remove(); // Removing elements we saved shallow copies off earlier
        }

        if (newElement.Elements().Count() == unmodifiedBaseElemCount) {
          // If all we have are the "base" elements and none are changed then just delete the whole 
          // thing.
          newElement = null;
        }
      }
      previousElement = null;

      return newElement;
    }
    public static String ConvertToJson(Object itm) { // Obsolete
      // Convert the achievement to XElement
      XElement element = ConvertToXElement(itm);

      // Descendants() grabs all the XElements at every depth, Elements() only grabs the immediate 
      // child XElements, and I threw in Self to get the parent XElement, too
      element.DescendantsAndSelf().Where(
        // Look for only the descendant XElements with XAttributes so we don't try to execute an 
        // Object method of a null Object
        x => x.HasAttributes
      ).Where(
        // Second check to make sure that the XAttribute "Id" is present
        x => x.Attribute("Id") != null
        // Suck up the "Id" in an IEnumerable<XAttribute> so we can use the Remove() method on them 
        // all at once
      ).Attributes("Id").Remove(); // Remove the "Id" XAttributes that are used for comparison 
                                   // purposes as they clog up the resulting Json.

      // XDocument is the newer LINQ to XML format while XmlDocument is the older and harder to 
      // manipulate format. You can build and manipulate XDocuments and the related XNode types 
      // like I did above for the XAttributes with common LINQ constructors
      // 
      // But the Json serializer only works on XmlDocuments So I put a helper method ToXmlDocument 
      // in Tools.cs to handle the conversion.
      String jsonString =
        Newtonsoft.Json.JsonConvert.SerializeXmlNode( // Wrap the achievement in an XDocument
          new XDocument(element).ToXmlDocument(), // Convert to XmlDocument 
          Newtonsoft.Json.Formatting.None, // Set the output formatting option
          true // Omit the root Object
        );

      return jsonString;
    }
    public static String ConvertToText(GomObject gomItm, Boolean overrideVerbose) {
      if (gomItm != null) {
        if (gomItm.Name.Contains("/")) return null;
        else return ConvertToText(LoadGameObject(gomItm.Dom_, gomItm, false), overrideVerbose);
      }

      return null;
    }
    public static String ConvertToText(GomObject gomItm) => ConvertToText(gomItm, false);

    // Was tired of writing code that duplicated this functionality
    public static String ConvertToText(Object item, Boolean overrideVerbose) {
      if (item == null) return null;

      String type = item.GetType().ToString();

      switch (type) {
        case "GomLib.Models.Conversation":
        case "GomLib.Models.Decoration":
        case "GomLib.Models.Conquest":
          return ((PseudoGameObject)item).ToString(!overrideVerbose);
        default:
          break;
      }

      return null;
    }
    // Was tired of writing code that duplicated this functionality
    public static String ConvertToText(Object item) => ConvertToText(item, false);
    public static String ConvertToText(String fqn, DataObjectModel dom, Boolean overrideVerbose) {
      GomObject gomItm = dom.GetObject(fqn);
      return ConvertToText(gomItm, overrideVerbose);
    }
    public static String ConvertToText(String fqn, DataObjectModel dom) {
      if (dom == null) throw new ArgumentNullException(nameof(dom));

      return ConvertToText(fqn, false);
    }
    public static String ConvertToText(UInt64 itemId,
                                       DataObjectModel dom,
                                       Boolean overrideVerbose) {

      GomObject gomItm = dom.GetObject(itemId);
      return ConvertToText(gomItm, overrideVerbose);
    }
    public static String ConvertToText(UInt64 itemId, DataObjectModel dom)
      => ConvertToText(itemId, dom, false);

    public static XElement ConvertToXElement(GomObject gomItm, Boolean overrideVerbose) {
      if (gomItm != null) {
        if (gomItm.Name.Contains("/")) return null;

        switch (gomItm.Name.Substring(0, 3)) {
          case "abl":
            if (!gomItm.Name.Contains("/"))
              return new GameObject().ToXElement(gomItm, !overrideVerbose);
            else break;
          case "itm":
          case "npc":
          case "qst":
          case "cdx":
          case "cnv":
          case "ach":
          case "tal":
          case "sch":
          case "dec":
            return new GameObject().ToXElement(gomItm, !overrideVerbose);
          default:
            break;
        }
      }

      return null;
    }
    public static XElement ConvertToXElement(GomObject gomItm) => ConvertToXElement(gomItm, false);
    // Was tired of writing code that duplicated this functionality
    public static XElement ConvertToXElement(Object item, Boolean overrideVerbose) {
      if (item == null) return null;

      String type = item.GetType().ToString();

      switch (type) {
        case "GomLib.Models.Item":
        case "GomLib.Models.Npc":
        case "GomLib.Models.Ability":
        case "GomLib.Models.Quest":
        case "GomLib.Models.QuestItem":
        case "GomLib.Models.Codex":
        case "GomLib.Models.Conversation":
        case "GomLib.Models.Achievement":
        case "GomLib.Models.Talent":
        case "GomLib.Models.Schematic":
        case "GomLib.Models.Decoration":
        case "GomLib.Models.ItemAppearance":
        case "GomLib.Models.Stronghold":
        case "GomLib.Models.Room":
        case "GomLib.Models.Planet":
        case "GomLib.Models.AdvancedClass":
        case "GomLib.Models.Discipline":
        // Broken
        // case "GomLib.Models.AbilityPackage":
        //   return ((GomLib.Models.GameObject)item).ToXElement(!overrideVerbose);
        case "GomLib.Models.Conquest":
        case "GomLib.Models.scFFShip":
        case "GomLib.Models.Companion":
        case "GomLib.Models.Collection":
        // Broken
        // case "GomLib.Models.MtxStorefrontEntry":
        //   return ((GomLib.Models.PseudoGameObject)item).ToXElement(!overrideVerbose);
        // Broken
        // case "GomLib.Models.ConquestObjective":
        //   return ((GomLib.Models.ConquestObjective)item).ToXElement(!overrideVerbose);
        // Broken
        // case "GomLib.Models.ConquestData":
        //   return ((GomLib.Models.ConquestData)item).ToXElement(!overrideVerbose);
        default:
          if (item is GameObject @Object)
            return @Object.ToXElement(!overrideVerbose);
          else if (item is PseudoGameObject object1)
            return object1.ToXElement(!overrideVerbose);
          break;
      }

      return null;
    }
    // Was tired of writing code that duplicated this functionality
    public static XElement ConvertToXElement(Object item) => ConvertToXElement(item, false);
    public static XElement ConvertToXElement(String fqn,
                                             DataObjectModel dom,
                                             Boolean overrideVerbose) {

      GomObject gomItm = dom.GetObject(fqn);
      return ConvertToXElement(gomItm, overrideVerbose);
    }
    public static XElement ConvertToXElement(String fqn, DataObjectModel dom) {
      if (dom == null) throw new ArgumentNullException(nameof(dom));

      return ConvertToXElement(fqn, false);
    }
    public static XElement ConvertToXElement(UInt64 itemId,
                                             DataObjectModel dom,
                                             Boolean overrideVerbose) {

      GomObject gomItm = dom.GetObject(itemId);
      return ConvertToXElement(gomItm, overrideVerbose);
    }
    public static XElement ConvertToXElement(UInt64 itemId, DataObjectModel dom)
      => ConvertToXElement(itemId, dom, false);
    public void ExportIconFromPath(String path, String name, String exportPath) {
      Library lib =
        CurrentAssets.Libraries.Where(x => x.Name.Contains("main_gfx_assets")).Single();

      if (!lib.Loaded) lib.Load();

      TorArchive.File iconFile = lib.FindFile(path);

      if (iconFile != null) {
        HashDictionaryInstance hashData = HashDictionaryInstance.Instance;

        if (!hashData.Loaded) hashData.Load();

        hashData.Dictionary.CreateHelpers();
        HashFileInfo hashInfo =
          new HashFileInfo(
            iconFile.FileInfo.PrimaryHash,
            iconFile.FileInfo.SecondaryHash,
            iconFile
          );
        String stateName = hashInfo.FileState.ToString();
        DevIL.ImageImporter imp = new DevIL.ImageImporter();
        DevIL.Image dds;

        using (MemoryStream iconStream = (MemoryStream)iconFile.OpenCopyInMemory()) {
          dds = imp.LoadImageFromStream(DevIL.ImageType.Dds, iconStream);
        }

        using MemoryStream outputStream = new MemoryStream();
        DevIL.ImageExporter exp = new DevIL.ImageExporter();
        // Save DDS to stream in PNG format
        exp.SaveImageToStream(dds, DevIL.ImageType.Png, outputStream);

        name += "_" + stateName;
        foreach (Char character in Path.GetInvalidFileNameChars()) {
          // Make sure the name doesn't contain invalid characters.
          name = name.Replace(character, '-');
        }

        WriteFile(outputStream, String.Format(exportPath, name));
      }
    }
    private XElement FindChangedEntries(XElement items,
                                        XDocument previousPatch,
                                        String containerName,
                                        String subContainerName) {

      AddToList1("Comparing to Previous Version.");
      Clearlist2();
      AddToList2("No Output will appear here.");
      AddToList2("Is this really necessary?");

      String filename = s_prefix + containerName + ".xml";
      XElement addedChangedItems =
        new XElement(
          containerName,
          items.Elements(subContainerName).Cast<XNode>().Except(
            previousPatch.Element(containerName).Elements(subContainerName).Cast<XNode>(),
            new XNodeEqualityComparer()
          )
        );

      // This section should add a OldVersion element to every changed item which contains the old 
      // version of it.
      if (containerName == "GOM_Items") {
        if (items.Descendants("References") != null) { // OldValue doesn't apply to Gom_Items
          List<String> changedItemIds =
            addedChangedItems.Elements(
              subContainerName
            ).Select(
              i => i.Attribute("Id").Value
            ).Intersect(
              previousPatch.Element(
                containerName
              ).Elements(
                subContainerName
              ).Select(
                i => i.Attribute("Id").Value
              )
            ).ToList();

          Int32 count = changedItemIds.Count;

          foreach (String changedItemId in changedItemIds) {
            XElement previousElement =
              previousPatch.Element(
                containerName
              ).Elements(
                subContainerName
              ).First(
                p => p.Attribute("Id").Value == changedItemId
              );
            XElement newElement =
              addedChangedItems.Elements(
                subContainerName
              ).Where(
                x => x.Attribute("Id").Value == changedItemId
              ).First();
            XElement changedItem =
              addedChangedItems.Elements().First(
                x => x.Attribute("Id").Value == changedItemId
              );
            List<String> toRemove =
              previousElement.Element(
                "References"
              ).Elements().Select(
                i => i.Attribute("Id").Value
              ).Intersect(
                newElement.Element("References").Elements().Select(i => i.Attribute("Id").Value)
              ).ToList();

            foreach (var removedItemId in toRemove) {
              XElement itemToRemove =
                changedItem.Element("References").Elements().First(
                  x => x.Attribute("Id").Value == removedItemId
                );

              if (itemToRemove != null) itemToRemove.Remove();
            }

            if (changedItem.Attribute("Status") == null)
              changedItem.Add(new XAttribute("Status", "Changed"));
          }
        }
      } else if (containerName != "GOM_Fields") {
        List<String> changedItemIds =
          addedChangedItems.Elements(subContainerName).Select(
            i => i.Attribute("Id").Value
          ).Intersect(
            previousPatch.Element(containerName).Elements(subContainerName).Select(
              i => i.Attribute("Id").Value
            )
          ).ToList();
        Int32 count = changedItemIds.Count;

        foreach (String changedItemId in changedItemIds) {
          XElement previousElement =
            previousPatch.Element(containerName).Elements(subContainerName).First(
              p => p.Attribute("Id").Value == changedItemId
            );
          XElement newElement =
            addedChangedItems.Elements(subContainerName).Where(
              x => x.Attribute("Id").Value == changedItemId
            ).First();
          XElement changedItem =
            addedChangedItems.Elements().First(x => x.Attribute("Id").Value == changedItemId);

          changedItem.ReplaceWith(CompareElements(previousElement, newElement));

          if (changedItem != null && changedItem.Attribute("Status") == null)
            changedItem.Add(new XAttribute("Status", "Changed"));
        }
      }

      // Separate out the new elements and tag them with the status attribute
      List<String> newItemIds =
        addedChangedItems.Elements(subContainerName).Select(
          i => i.Attribute("Id").Value
        ).Except(
          previousPatch.Element(containerName).Elements(subContainerName).Select(
            i => i.Attribute("Id").Value
          )
        ).ToList();

      foreach (String newItemId in newItemIds) {
        XElement newItem =
          addedChangedItems.Elements().First(x => x.Attribute("Id").Value == newItemId);
        newItem.Add(new XAttribute("Status", "New"));
      }

      // Separate out the removed elements and tag them with the status attribute
      List<String> removedItemIds =
        previousPatch.Element(containerName).Elements(subContainerName).Select(
          i => i.Attribute("Id").Value
        ).Except(
          items.Elements(subContainerName).Select(i => i.Attribute("Id").Value)
        ).ToList();

      foreach (String removedItemId in removedItemIds) {
        XElement removedItem =
          previousPatch.Element(containerName).Elements().First(
            x => x.Attribute("Id").Value == removedItemId
          );
        removedItem.Add(new XAttribute("Status", "Removed"));
        addedChangedItems.Add(removedItem); // Add removed elements to the return value
      }

      previousPatch = null; // Trashing this
      return addedChangedItems;
    }
    private void GameObjectListAsJSON(String prefix, List<GameObject> itmList) {
      Int32 i = 0;
      Int16 e = 0;
      String n = Environment.NewLine;
      StringBuilder txtFile = new StringBuilder();
      String filename = String.Format("\\json\\{0}{1}", prefix, ".json");

      WriteFile(String.Format("{0}{1}", PatchVersion, n), filename, false);

      Int32 count = itmList.Count;
      HashTableHashing.MurmurHash2Unsafe jsonHasher = new HashTableHashing.MurmurHash2Unsafe();

      for (Int32 b = itmList.Count - 1; b >= 0; b--) { // Go backwards so we can delete values
        ProgressUpdate(i, count);

        if (e % 1000 == 1) {
          WriteFile(txtFile.ToString(), filename, true);
          txtFile.Clear();
          e = 0;
        }

        AddToList2(String.Format("{0}: {1}", prefix, itmList[b].Fqn));

        String jsonString = itmList[b].ToJSON();
        UInt32 hash = jsonHasher.Hash(Encoding.ASCII.GetBytes(jsonString));

        txtFile.Append(
          String.Format(
            "{0},{1},{2}{3}",
            itmList[b].Base62Id,
            hash,
            jsonString,
            Environment.NewLine
          )
        ); // Append it with a newline to the output.

        itmList[b] = null;
        i++;
        e++;
      }

      AddToList1(
        String.Format("The {0} json file has been generated; there were {1} {0}", prefix, i)
      );
      WriteFile(txtFile.ToString(), filename, true);
      DeleteEmptyFile(filename, i);
      GC.Collect();
      CreateGzip(filename);
      ClearProgress();
    }
    private void GameObjectListAsText(String prefix, IEnumerable<GameObject> itmList) {
      Int32 i = 0;
      Int16 e = 0;
      // _ = Environment.NewLine;
      StringBuilder txtFile = new StringBuilder();
      String filename = String.Join("", prefix, ".txt");

      if (!itmList.Any()) return;

      String headerRow =
        GetHeaderRow(
          prefix.Replace(
            "Changed", ""
          ).Replace(
            "Full", ""
          ).Replace(
            "New", ""
          ).Replace(
            "Removed", ""
          ),
          itmList.First().Dom_
        );

      WriteFile(headerRow, filename, false);

      Int32 count = itmList.Count();

      foreach (GameObject itm in itmList) {
        ProgressUpdate(i, count);

        if (e % 1000 == 1) {
          WriteFile(txtFile.ToString(), filename, true);
          txtFile.Clear();
          e = 0;
        }

        AddToList2(String.Format("{0}: {1}", prefix, itm.Fqn));

        String textString = ConvertToText(itm);
        txtFile.Append(textString + Environment.NewLine); // Append it with a newline to the output.

        i++;
        e++;
      }

      AddToList1(
        String.Join(
          "",
          "The ",
          prefix,
          " text file has been generated; there are ",
          i,
          " ",
          prefix
        )
      );
      WriteFile(txtFile.ToString(), filename, true);
      DeleteEmptyFile(filename, i);
      ClearProgress();
    }
    private static String GetHeaderRow(String prefix, DataObjectModel dom) {
      switch (prefix) {
        case "Decorations":
          List<String> hookNameList =
            dom.DecorationLoader.HookList.Select(x => x.Value.Name).ToList();
          hookNameList.Sort();
          String hookString = String.Join(";", hookNameList);
          return String.Format(
            "Name;Sources;Binding;Category;SubCategory;Purchase for Guild Cost;Stub Type;{0}{1}",
            hookString,
            Environment.NewLine
          );
        default: // Don't have a predefined header row for this type
          break;
      }

      return "";
    }
    // This function is meant to handle unique cases of loading a list of objects from the GOM
    private static IEnumerable<GomObject> GetMatchingGomObjects(DataObjectModel dom, String gomPrefix) {
      IEnumerable<GomObject> itmList;

      switch (gomPrefix) {
        case "abl.":
          // Abilities with a / in the name are Effects.
          itmList =
            dom.GetObjectsStartingWith(gomPrefix).Where(x => !x.Name.Contains("/"));
          break;
        case "apn.":
          // Union APC/APN
          itmList =
            dom.GetObjectsStartingWith(gomPrefix).Union(dom.GetObjectsStartingWith("apc."));
          break;
        case "eff.":
          itmList =
            dom.GetObjectsStartingWith("abl.").Where(x => x.Name.Contains("/"));
          break;
        default:
          // No need for the extra Linq statement for non-unique cases
          itmList = dom.GetObjectsStartingWith(gomPrefix);
          break;
      }

      return itmList;
    }
    public void GetObjects(String prefix, String elementName) {
      Clearlist2();
      ClearProgress();
      LoadData();
      AddToList1(String.Format("Getting {0}", elementName));

      switch (elementName) {
        case "Abilities": // This section is for exploring Ability Effects
          ProcessGameObjects(prefix, elementName);
          CurrentDom.AbilityLoader.effKeys.Sort();
          String effKeyList = String.Join(Environment.NewLine, CurrentDom.AbilityLoader.effKeys);
          WriteFile(effKeyList, "effKeys.txt", false);

          CurrentDom.AbilityLoader.effWithUnknowns =
            CurrentDom.AbilityLoader.effWithUnknowns.Distinct().OrderBy(o => o).ToList();
          String effUnknowns =
            String.Join(Environment.NewLine, CurrentDom.AbilityLoader.effWithUnknowns);

          WriteFile(effUnknowns, "effUnknowns.txt", false);

          CurrentDom.AbilityLoader.effWithUnknowns = new List<String>();
          CurrentDom.AbilityLoader.effKeys = new List<String>();

          break;
        default:
          ProcessGameObjects(prefix, elementName);

          break;
      }

      FlushTempTables();
      EnableButtons();
    }
    private static String GetObjectText(Object obj) {
      if (obj is GameObject @Object) return @Object.Fqn;
      else if (obj is PseudoGameObject object1) return object1.Name;
      else return "";
    }
    public void GetPrototypeObjects(String xmlRoot, String prototype, String dataTable) {
      Clearlist2();
      ClearProgress();
      LoadData();
      AddToList1(String.Format("Getting {0}", xmlRoot));
      ProcessProtoData(xmlRoot, prototype, dataTable);
      FlushTempTables();
      EnableButtons();
    }
    private void ObjectListAsSql(String prefix, String xmlRoot, IEnumerable<Object> itmList) {
      if (prefix == "Removed") return; //not supported as of yet.
      if (!itmList.Any()) return;

      Int32 i = 0;
      Int16 e = 0;
      Int32 f = 1;
      String n = Environment.NewLine;
      StringBuilder txtFile = new StringBuilder();
      String filename = String.Format("\\sql\\{0}{1}", prefix, xmlRoot);
      String frs = "{0}.sql";

      WriteFile("", String.Format(frs, filename, f), false);

      Int32 count = itmList.Count();
      String transQuery;

      // Verify that there is an SQL Transaction Query for this Object type
      if (InitTable.TryGetValue(xmlRoot, out SQLInitStore transInit)) {
        transQuery = transInit.InitBegin + n;

        if (_sql)
          // Initialize the SQL tranaction if direct SQL output is enabled.
          SqlTransactionsInitialize(transInit.InitBegin, transInit.InitEnd);
      } else {
        AddToList2(String.Format("Output type not supported for: {0}", xmlRoot));
        return;
      }

      String joiner = ",";

      foreach (Object itm in itmList) {
        if (i == count - 1) joiner = "";

        ProgressUpdate(i, count);
        AddToList2(String.Format("{0}: {1}", prefix, GetObjectText(itm)));

        String sqlString = ToSQL(itm);

        if (_sql)
          // Add to current SQL Transaction if direct SQL output is enabled.
          SqlAddTransactionValue(sqlString);

        // Append it with a newline to the output.
        txtFile.Append(String.Join(joiner, sqlString, n));

        i++;
        e++;
      }

      txtFile.Append(transInit.InitEnd);

      AddToList1(String.Format("The {0} sql file has been generated; there were {1} {0}", prefix, i));
      WriteFile(transQuery, String.Format(frs, filename, f), false);
      WriteFile(txtFile.ToString(), String.Format(frs, filename, f), true);
      InitTable[xmlRoot].OutputCreationSQL(); //output the creation sql file for this table
      SqlTransactionsFlush(); //flush the transaction queue
      DeleteEmptyFile(String.Format(frs, filename, f), i);
      GC.Collect();

      for (Int32 j = 1; j <= f; j++) {
        CreateGzip(String.Format(frs, filename, j)); //compresses output for upload
      }

      ClearProgress();
    }
    private static Boolean OutputCompatible(String xmlRoot) {
      switch (s_outputTypeName) {
        case "Text":
        case "JSON":
        case "XML":
          return true;
        case "SQL":
          if (InitTable.ContainsKey(xmlRoot)) return true;
          else return false;
        default:
          return false;
      }
    }
    private void ProcessEffectChanges() {
      IEnumerable<GomObject> currentAblObjects =
        CurrentDom.GetObjectsStartingWith("abl.").Where(x => x.GetType() == typeof(GomObject));
      IEnumerable<GomObject> previousAblObjects =
        PreviousDom.GetObjectsStartingWith("abl.").Where(x => x.GetType() == typeof(GomObject));

      // Build a dictionary of effects so we can quickly look them up.
      Dictionary<String, GomObject> currentEffectByNameID = new Dictionary<String, GomObject>();

      foreach (GomObject obj in currentAblObjects) {
        if (!obj.Name.Contains("/")) {
          // Only care about effect nodes.
          continue;
        }

        String[] nameArray = obj.Name.Split('/');
        String name = nameArray[0] + '/' + nameArray[2];
        currentEffectByNameID.Add(name, obj);
      }

      Dictionary<String, GomObject> previousEffectByNameID = new Dictionary<String, GomObject>();

      foreach (GomObject obj in previousAblObjects) {
        if (!obj.Name.Contains("/")) {
          // Only care about effect nodes.
          continue;
        }

        String[] nameArray = obj.Name.Split('/');
        String name = nameArray[0] + '/' + nameArray[2];
        previousEffectByNameID.Add(name, obj);
      }

      foreach (KeyValuePair<String, GomObject> currentPair in currentEffectByNameID) {
        if (previousEffectByNameID.TryGetValue(currentPair.Key, out GomObject prevObj)) {
          if (!prevObj.Equals(currentPair.Value)) {
            // Effect node not equal!
            XElement oldElement = prevObj.Print();
            prevObj.Unload();
            XElement newElement = currentPair.Value.Print();
            currentPair.Value.Unload();

            newElement = CompareElements(oldElement, newElement);
            oldElement = null;

            if (newElement != null) {
              Regex regex = new Regex(Regex.Escape("."));
              String newText = regex.Replace(currentPair.Key, "\\", 1);

              WriteFile(
                new XDocument(newElement),
                String.Format("\\GOM\\ChangedEffects\\{0}.xml", newText),
                false
              );
            }
          }
        }
      }
    }
    public void ProcessGameObjects(String gomPrefix, String xmlRoot) {
      Boolean classOverride = xmlRoot == "AdvancedClasses";

      if (!OutputCompatible(xmlRoot)) {
        ClearProgress();
        FlushTempTables();
        return;
      }

      IEnumerable<GomObject> curItmList = GetMatchingGomObjects(CurrentDom, gomPrefix);
      List<String> curItmNames = curItmList.Select(x => x.Name).ToList();
      Dictionary<String, List<GameObject>> ObjectLists = new Dictionary<String, List<GameObject>>();
      Dictionary<GameObject, GameObject> chaItems = new Dictionary<GameObject, GameObject>();
      List<GameObject> newItems = new List<GameObject>();
      List<GameObject> remItems = new List<GameObject>();
      Int32 i = 0;
      Int32 count = 0;

      if (chkBuildCompare.Checked) {
        List<String> prevItmNames =
          GetMatchingGomObjects(PreviousDom, gomPrefix).Select(x => x.Name).ToList();
        // Couldn't find a more elegant way to do this with linq.
        List<String> removedNames =
          prevItmNames.Except(curItmNames).ToList();

        ClearProgress();

        i = 0;
        count = curItmList.Count() + removedNames.Count;

        foreach (var curObject in curItmList) {
          ProgressUpdate(i, count);
          try {
            GameObject curItm = LoadGameObject(CurrentDom, curObject, classOverride);
            if (curItm == null) {
              AddToList2(String.Join("", "Skipped: ", curObject.Name, " (loader returned null)"));
            } else {
              GomObject prevObject = PreviousDom.GetObject(curObject.Name);
              if (prevObject != null) {
                GameObject prevItm = LoadGameObject(PreviousDom, prevObject, classOverride);
                if (prevItm != null && !prevItm.Equals(curItm)) {
                  AddToList2(String.Join("", "Changed: ", curItm.Fqn));
                  chaItems[prevItm] = curItm;
                }
              } else {
                AddToList2(String.Join("", "New: ", curItm.Fqn));
                newItems.Add(curItm);
              }
            }
          } catch (Exception ex) {
            AddToList2(String.Join("", "Skipped: ", curObject.Name, " - ", ex.GetType().Name, ": ", ex.Message));
          } finally {
            curObject.Unload();
          }
          i++;
        }

        foreach (String removedName in removedNames) {
          ProgressUpdate(i, count);
          AddToList2(String.Join("", "Removed: ", removedName));
          GameObject prevItm =
            LoadGameObject(PreviousDom, PreviousDom.GetObject(removedName), classOverride);

          remItems.Add(prevItm);
          i++;
        }

        ClearProgress();

        ObjectLists.Add("New", newItems);
        ObjectLists.Add("Changed", chaItems.Values.ToList());
        ObjectLists.Add("Removed", remItems);

      } else {
        i = 0;
        count = curItmList.Count();

        foreach (GomObject curObject in curItmList) {
          ProgressUpdate(i, count);
          try {
            GameObject obj = LoadGameObject(curObject.Dom_, curObject, classOverride);
            if (obj != null && obj.Id != 0) newItems.Add(obj);
          } catch (Exception ex) {
            AddToList2(String.Join("", "Skipped: ", curObject.Name, " - ", ex.GetType().Name, ": ", ex.Message));
          } finally {
            curObject.Unload();
          }

          i++;
        }

        ObjectLists.Add("Full", newItems);
      }

      Clearlist2();
      AddToList2(String.Format("Generating {0} Output", s_outputTypeName));

      XDocument xmlDoc = new XDocument();
      XElement elements = new XElement(xmlRoot);

      count = 0;
      i = 0;

      foreach (var itmList in ObjectLists) {
        count += itmList.Value.Count;
      }

      foreach (var itmList in ObjectLists) {
        if (s_outputTypeName == "JSON")
          GameObjectListAsJSON(String.Join("", itmList.Key, xmlRoot), itmList.Value);
        else if (s_outputTypeName == "Text")
          GameObjectListAsText(String.Join("", itmList.Key, xmlRoot), itmList.Value);
        else if (s_outputTypeName == "SQL")
          ObjectListAsSql(itmList.Key, xmlRoot, itmList.Value);
        else {
          if (itmList.Key == "Changed") {
            foreach (var changedPair in chaItems) {
              ProgressUpdate(i, count);

              XElement oldElement = ConvertToXElement(changedPair.Key);
              XElement newElement = ConvertToXElement(changedPair.Value);

              newElement = CompareElements(oldElement, newElement);
              oldElement = null;

              if (newElement != null) {
                newElement.Add(new XAttribute("Status", itmList.Key));
                elements.Add(newElement);
              }

              i++;
            }

          } else {
            if ((itmList.Key == "New" || itmList.Key == "Full") && gomPrefix == "ach.") {
              WriteFile("", "brokenAchieves.txt", false);
            }

            foreach (GameObject itm in itmList.Value) {
              ProgressUpdate(i, count);

              XElement itemElement = ConvertToXElement(itm);

              if ((itmList.Key == "New" || itmList.Key == "Full") && gomPrefix == "ach.") {
                if (((Achievement)itm).AchId == 0) {
                  WriteFile(
                    itm.Fqn
                      + " : "
                      + ((Achievement)itm).Name
                      + Environment.NewLine,
                    "brokenAchieves.txt",
                    true
                  );
                }
              }

              if (itmList.Key != "Full") itemElement.Add(new XAttribute("Status", itmList.Key));

              itemElement.Add(ReferencesToXElement(itm.References));
              elements.Add(itemElement);

              i++;
            }
          }
        }
      }

      if (s_outputTypeName == "XML") {
        String filename = String.Join("", xmlRoot, ".xml");
        String outputComment = "total";

        if (chkBuildCompare.Checked) {
          filename = String.Join("", "Changed", filename);
          outputComment = "new/changed/removed ";
        }

        elements = Sort(elements);

        AddToList1(
          String.Format("{0} - {1} {2}", xmlRoot, elements.Elements().Count(), outputComment)
        );
        xmlDoc.Add(elements);
        WriteFile(xmlDoc, filename, false, false);
      }

      ClearProgress();
      FlushTempTables();
    }
    private void ProcessGom() {
      IEnumerable<GomObject> currentObjects =
        CurrentDom.GetObjectsStartingWith("").Where(x => x.GetType() == typeof(GomObject));

      Dictionary<String, List<GomObject>> ObjectLists = new Dictionary<String, List<GomObject>>();
      Dictionary<GomObject, GomObject> chaObjects = new Dictionary<GomObject, GomObject>();
      List<GomObject> newObjects = new List<GomObject>();
      List<GomObject> remObjects = new List<GomObject>();
      List<String> currentNames = currentObjects.Select(x => x.Name).ToList();
      Int32 i = 0;
      Int32 count = 0;

      if (chkBuildCompare.Checked) {
        IEnumerable<GomObject> previousObjects =
          PreviousDom.GetObjectsStartingWith("").Where(x => x.GetType() == typeof(GomObject));
        List<String> previousNames = previousObjects.Select(x => x.Name).ToList();
        // Couldn't find a more elegant way to do this with LINQ.
        List<String> removedNames = previousNames.Except(currentNames).ToList();

        ClearProgress();

        i = 0;
        count = currentObjects.Count() + removedNames.Count;

        foreach (GomObject curObject in currentObjects) {
          ProgressUpdate(i, count);

          GomObject prevObject = null;
          try {
            prevObject = PreviousDom.GetObject(curObject.Name);
          }
          catch (Exception ex) {
            // A malformed/unsupported legacy object must not terminate the
            // whole extraction thread. Treat it as new and keep processing.
            Debug.WriteLine(
              $"Unable to load previous GOM object '{curObject.Name}': {ex}");
            AddToList1(
              $"Warning: unable to compare '{curObject.Name}': {ex.Message}"
            );
          }

          if (prevObject != null) {
            if (!prevObject.Equals(curObject)) {
              chaObjects.Add(prevObject, curObject);
            }

            prevObject.Unload();
          } else {
            newObjects.Add(curObject);
          }

          curObject.Unload();
          i++;
        }

        foreach (String removedName in removedNames) {
          ProgressUpdate(i, count);
          remObjects.Add(PreviousDom.GetObject(removedName));
          i++;
        }

        ObjectLists.Add("New", newObjects);
        ObjectLists.Add("Changed", chaObjects.Values.ToList());
        ObjectLists.Add("Removed", remObjects);
      } else {
        ObjectLists.Add("", currentObjects.ToList());
      }

      AddToList1(String.Format("Generating Output {0}", s_outputTypeName));
      ClearProgress();

      //-----------------------------------------------------------------------------------------//

      XDocument xmlDoc = new XDocument();
      XElement gomItems = new XElement("GOM_Items");
      XElement z_effects = new XElement("Z_Effects");

      i = 0;
      count = 0;

      foreach (var kvp in ObjectLists) {
        count += kvp.Value.Count;
      }

      foreach (var itmList in ObjectLists) {
        ProgressUpdate(i, count);

        foreach (GomObject gomItm in itmList.Value) {
          XElement gomElement = new XElement("GOM_Item", new XAttribute("Id", gomItm));

          if (itmList.Key != "")
            gomElement.Add(new XAttribute("Status", itmList.Key));

          if (gomItm.References != null)
            gomElement.Add(ReferencesToXElement(gomItm.References));

          if (gomItm.FullReferences != null)
            gomElement.Add(ReferencesToXElement(gomItm.FullReferences));

          if (gomItm.Name.Contains("/"))
            z_effects.Add(gomElement);
          else
            gomItems.Add(gomElement);

          gomItm.Unload();
        }

        i++;
      }

      String filename = "GOM_Items.xml";

      if (chkBuildCompare.Checked) {
        filename = String.Format("{0}{1}", "Changed", filename);
        gomItems.ReplaceNodes(
          gomItems.Elements("GOM_Item").OrderBy(
            x => (String)x.Attribute("Status")
          ).ThenBy(x => (String)x.Attribute("Id")));
        z_effects.ReplaceNodes(
          z_effects.Elements("GOM_Item").OrderBy
          (x => (String)x.Attribute("Status")
        ).ThenBy(x => (String)x.Attribute("Id")));
      } else {
        gomItems.ReplaceNodes(
          gomItems.Elements("GOM_Item").OrderBy(x => (String)x.Attribute("Id"))
        );
        z_effects.ReplaceNodes(
          z_effects.Elements("GOM_Item").OrderBy(x => (String)x.Attribute("Id"))
        );
      }

      AddToList1(
        "GOM Items - "
        + (gomItems.Elements("GOM_Item").Count()
        + z_effects.Elements("GOM_Item").Count())
      );
      xmlDoc.Add(new XElement("Container", gomItems, z_effects));
      WriteFile(xmlDoc, filename, false, false);

      if (chkBuildCompare.Checked) {
        if (_exportGOM) {
          foreach (var objList in ObjectLists) {
            if (objList.Key == "Changed") {
              foreach (var changedPair in chaObjects) {
                XElement oldElement = changedPair.Key.Print();
                changedPair.Key.Unload();
                XElement newElement = changedPair.Value.Print();
                changedPair.Value.Unload();

                newElement = CompareElements(oldElement, newElement);
                oldElement = null;

                if (newElement != null) {
                  Regex regex = new Regex(Regex.Escape("."));
                  String newText = regex.Replace(changedPair.Value.Name, "\\", 1);

                  WriteFile(
                    new XDocument(newElement),
                    String.Format("\\GOM\\{0}\\{1}.xml", objList.Key, newText),
                    false
                  );
                }
              }
            } else {
              foreach (GomObject obj in objList.Value) {
                XElement newElement = obj.Print();
                obj.Unload();

                Regex regex = new Regex(Regex.Escape("."));
                String newText = regex.Replace(obj.Name.Replace('/', '.'), "\\", 1);

                WriteFile(
                  new XDocument(newElement),
                  String.Format("\\GOM\\{0}\\{1}.xml", objList.Key, newText),
                  false
                );
              }
            }
          }
        }
      }

      ClearProgress();
    }

    private void ProcessGomFields() {
      XDocument currentFields = CurrentDom.ReturnTypeNames();

      if (chkBuildCompare.Checked) {
        XDocument previousFields = PreviousDom.ReturnTypeNames();

        WriteFile(
          new XDocument(currentFields.Root.Element("FieldUseInDomClass")),
          "CurrentGom_Fields.xml",
          false
        );
        WriteFile(
          new XDocument(previousFields.Root.Element("FieldUseInDomClass")),
          "PreviousGom_Fields.xml",
          false
        );

        XElement wrapper = new XElement("Wrapper");
        XElement compared =
          FindChangedEntries(
            currentFields.Root.Element("Gom_Fields"),
            new XDocument(previousFields.Root.Element("Gom_Fields")),
            "Gom_Fields",
            "Gom_Field"
          );

        wrapper.Add(compared);

        XElement compared2 =
          FindChangedEntries(
            currentFields.Root.Element("FieldUseInDomClass"),
            new XDocument(previousFields.Root.Element("FieldUseInDomClass")),
            "FieldUseInDomClass",
            "DomClass"
          );

        wrapper.Add(compared2);

        XDocument xmlDoc = new XDocument(wrapper);

        if (!xmlDoc.Root.IsEmpty) WriteFile(xmlDoc, "ChangedGom_Fields.xml", false);
      } else {
        WriteFile(currentFields, "Gom_Fields.xml", false);
      }
    }
    public void ProcessProtoData(String xmlRoot, String prototype, String dataTable) {
      if (!OutputCompatible(xmlRoot)) {
        ClearProgress();
        FlushTempTables();
        return;
      }

      Dictionary<Object, Object> currentDataProto = new Dictionary<Object, Object>();
      GomObject currentDataObject = CurrentDom.GetObject(prototype);

      if (currentDataObject != null) { // Fix to ensure old game assets don't throw exceptions.
        currentDataProto = currentDataObject.Data.Get<Dictionary<Object, Object>>(dataTable);
        currentDataObject.Unload();
      } else { // Check replaced prototype
        Dictionary<String, KeyValuePair<String, String>> replacedProtos =
          new Dictionary<String, KeyValuePair<String, String>> {
            { "wevConquestInfosPrototype",
              new KeyValuePair<String, String>("cnqConquestInfoPrototype","cnqConquestTable") }
          };

        if (replacedProtos.TryGetValue(prototype, out KeyValuePair<String, String> protkey)) {
          prototype = protkey.Key;
          dataTable = protkey.Value;
          currentDataObject = CurrentDom.GetObject(prototype);

          // Fix to ensure old game assets don't throw exceptions.
          if (currentDataObject != null) {
            currentDataProto = currentDataObject.Data.Get<Dictionary<Object, Object>>(dataTable);
            currentDataObject.Unload();
          }
        }
      }

      Dictionary<Object, Object>.KeyCollection curIds = currentDataProto.Keys;
      Dictionary<String, List<PseudoGameObject>> ObjectLists =
        new Dictionary<String, List<PseudoGameObject>>();
      Dictionary<PseudoGameObject, PseudoGameObject> chaItems =
        new Dictionary<PseudoGameObject, PseudoGameObject>();
      List<PseudoGameObject> newItems = new List<PseudoGameObject>();

      Int32 i;
      Int32 count;

      if (chkBuildCompare.Checked) {
        Dictionary<Object, Object> previousDataProto = new Dictionary<Object, Object>();
        GomObject previousDataObject = PreviousDom.GetObject(prototype);

        // Fix to ensure old game assets don't throw exceptions.
        if (previousDataObject != null) {
          previousDataProto = previousDataObject.Data.Get<Dictionary<Object, Object>>(dataTable);
          previousDataObject.Unload();
        }

        Dictionary<Object, Object>.KeyCollection prevIds = previousDataProto.Keys;
        List<PseudoGameObject> remItems = new List<PseudoGameObject>();
        List<Object> removedIds = prevIds.Except(curIds).ToList();

        i = 0;
        count = curIds.Count + removedIds.Count;

        ClearProgress();

        foreach (Object id in curIds) {
          ProgressUpdate(i, count);

          try {
            currentDataProto.TryGetValue(id, out Object curData);
            PseudoGameObject curObj = PseudoGameObject.Load(xmlRoot, CurrentDom, id, curData);
            previousDataProto.TryGetValue(id, out Object prevData);
            PseudoGameObject prevObj = PseudoGameObject.Load(xmlRoot, PreviousDom, id, prevData);

            if (curObj == null) {
              AddToList2(String.Join("", "Skipped: ", id, " (loader returned null)"));
            } else if (prevObj != null && prevObj.Id != 0) {
              if (!prevObj.Equals(curObj)) {
                AddToList2(String.Join("", "Changed: ", curObj.Name));
                chaItems[prevObj] = curObj;
              }
            } else {
              AddToList2(String.Join("", "New: ", curObj.Name));
              newItems.Add(curObj);
            }
          } catch (Exception ex) {
            AddToList2(String.Join("", "Skipped: ", id, " - ", ex.GetType().Name, ": ", ex.Message));
          }

          i++;
        }

        foreach (Object removedId in removedIds) {
          ProgressUpdate(i, count);

          previousDataProto.TryGetValue(removedId, out Object prevData);
          PseudoGameObject prevObj =
            PseudoGameObject.Load(xmlRoot, PreviousDom, removedId, prevData);

          AddToList2(String.Join("", "Removed: ", prevObj.Name));
          remItems.Add(prevObj);

          i++;
        }

        ClearProgress();

        ObjectLists.Add("New", newItems);
        ObjectLists.Add("Changed", chaItems.Values.ToList());
        ObjectLists.Add("Removed", remItems);

      } else {
        i = 0;
        count = curIds.Count;

        foreach (Object id in curIds) {
          ProgressUpdate(i, count);

          try {
            currentDataProto.TryGetValue(id, out Object curData);
            PseudoGameObject curObj = PseudoGameObject.Load(xmlRoot, CurrentDom, id, curData);
            if (curObj != null) newItems.Add(curObj);
          } catch (Exception ex) {
            AddToList2(String.Join("", "Skipped: ", id, " - ", ex.GetType().Name, ": ", ex.Message));
          }

          i++;
        }

        ObjectLists.Add("Full", newItems);
      }

      Clearlist2();
      AddToList2(String.Format("Generating {0} Output", s_outputTypeName));

      XDocument xmlDoc = new XDocument();
      XElement elements = new XElement(xmlRoot);

      count = 0;
      i = 0;

      foreach (var itmList in ObjectLists)
        count += itmList.Value.Count;

      foreach (var itmList in ObjectLists) {
        if (s_outputTypeName == "JSON")
          PseudoGameObjectListAsJSON(String.Join("", itmList.Key, xmlRoot), itmList.Value);
        else if (s_outputTypeName == "Text")
          PseudoGameObjectListAsText(String.Join("", itmList.Key, xmlRoot), itmList.Value);
        else if (s_outputTypeName == "SQL")
          ObjectListAsSql(itmList.Key, xmlRoot, itmList.Value);
        else {
          if (itmList.Key == "Changed") {
            foreach (var changedPair in chaItems) {
              ProgressUpdate(i, count);
              XElement oldElement = ConvertToXElement(changedPair.Key);
              XElement newElement = ConvertToXElement(changedPair.Value);

              newElement = CompareElements(oldElement, newElement);
              oldElement = null;

              if (newElement != null) {
                newElement.Add(new XAttribute("Status", itmList.Key));
                elements.Add(newElement);
              }

              i++;
            }
          } else {
            foreach (PseudoGameObject itm in itmList.Value) {
              ProgressUpdate(i, count);

              XElement itemElement = itm.ToXElement();

              if (itmList.Key != "Full") itemElement.Add(new XAttribute("Status", itmList.Key));

              if (itmList.Key == "New") {
                if (xmlRoot == "Collections") {
                  // Named icon export.
                  // Bleh item specific code here.
                  if (itm is Collection colItm) {
                    ExportIconFromPath(
                      "/resources/gfx/mtxstore/"
                        + colItm.Icon
                        + "_400x400.dds",
                      colItm.Name,
                      "/MtxImages/Named/Collections/{0}.png"
                    );
                  }
                } else if (xmlRoot == "MtxStoreFronts") {
                  if (itm is MtxStorefrontEntry mtxItm) {
                    ExportIconFromPath(
                      "/resources/gfx/mtxstore/"
                        + mtxItm.Icon
                        + "_400x400.dds",
                      mtxItm.Name,
                      "/MtxImages/Named/MtxStore/{0}.png"
                    );
                  }
                }
              }

              elements.Add(itemElement);
              i++;
            }
          }
        }
      }

      if (s_outputTypeName == "XML") {
        String filename = String.Join("", xmlRoot, ".xml");
        String outputComment = "";

        if (chkBuildCompare.Checked) {
          filename = String.Join("", "Changed", filename);
          outputComment = "new/changed/removed ";
        }

        elements = Sort(elements);

        AddToList1(
          String.Format("{0} - {1} {2}", xmlRoot, elements.Elements().Count(), outputComment)
        );
        xmlDoc.Add(elements);
        WriteFile(xmlDoc, filename, false, false);
      }

      ClearProgress();
      FlushTempTables();
    }
    private void PseudoGameObjectListAsJSON(String prefix, List<PseudoGameObject> itmList) {
      Int32 i = 0;
      Int16 e = 0;
      String n = Environment.NewLine;
      StringBuilder txtFile = new StringBuilder();
      String filename = String.Format("\\json\\{0}{1}", prefix, ".json");

      WriteFile(String.Format("{0}{1}", PatchVersion, n), filename, false);

      Int32 count = itmList.Count;
      HashTableHashing.MurmurHash2Unsafe jsonHasher = new HashTableHashing.MurmurHash2Unsafe();

      for (Int32 c = 0; c < count; c++) {
        ProgressUpdate(i, count);

        if (e % 1000 == 1) {
          WriteFile(txtFile.ToString(), filename, true);
          txtFile.Clear();

          e = 0;
        }

        AddToList2(String.Format("{0}: {1}", prefix, itmList[c].Name));

        String jsonString = itmList[c].ToJSON();
        UInt32 hash = jsonHasher.Hash(Encoding.ASCII.GetBytes(jsonString));
        txtFile.Append(
          String.Format(
            "{0},{1},{2}{3}",
            itmList[c].Base62Id,
            hash,
            jsonString,
            Environment.NewLine
          )
        ); //Append it with a newline to the output.

        itmList[c] = null;
        i++;
        e++;
      }

      AddToList1(
        String.Format("The {0} json file has been generated; there were {1} {0}", prefix, i)
      );
      WriteFile(txtFile.ToString(), filename, true);
      DeleteEmptyFile(filename, i);
      GC.Collect();
      CreateGzip(filename);
      ClearProgress();
    }
    private void PseudoGameObjectListAsText(String prefix, IEnumerable<PseudoGameObject> itmList) {
      Int32 i = 0;
      Int16 e = 0;
      String n = Environment.NewLine;
      StringBuilder txtFile = new StringBuilder();
      String filename = String.Format("{0}{1}", prefix, ".txt");

      WriteFile("", filename, false);

      Int32 count = itmList.Count();
      Dictionary<Int64, String> conqOrder = null;

      if (prefix.Contains("Conquest")) {
        WriteFile("", "ConquestSCSV.txt", false);
        WriteFile("", "ConquestOrder.txt", false);
      }

      foreach (PseudoGameObject itm in itmList) {
        ProgressUpdate(i, count);

        if (e % 1000 == 1) {
          WriteFile(txtFile.ToString(), filename, true);
          txtFile.Clear();
          e = 0;
        }

        AddToList2(String.Format("{0}: {1}", prefix, itm.Name));

        String jsonString = ConvertToText(itm);
        txtFile.Append(jsonString + Environment.NewLine); // Append it with a newline to the output.

        i++;
        e++;

        if (prefix.Contains("Conquest")) {
          if (conqOrder == null) conqOrder = new Dictionary<Int64, String>();

          if (((Conquest)itm).ActiveData != null)
            foreach (var actDat in ((Conquest)itm).ActiveData) {
              conqOrder.Add(
                actDat.ActualOrderNum,
                String.Format(
                  "{0}: {1} EST - {2}",
                  actDat.ActualOrderNum,
                  actDat.StartTime.ToString(),
                  ((Conquest)itm).Name
                )
              );
            }
          else if (((Conquest)itm).NewActiveData != null)
            foreach (var actDat in ((Conquest)itm).NewActiveData) {
              conqOrder.Add(
                actDat.Ticks,
                String.Format("{0} EST - {1}  ", actDat.ToString(), ((Conquest)itm).Name)
              );
            }

          WriteFile(((Conquest)itm).ConquestToSCSV(), "ConquestSCSV.txt", true);
        }
      }

      if (prefix.Contains("Conquest"))
        WriteFile(
          String.Join(
            Environment.NewLine,
            conqOrder.OrderBy(x => x.Key).Select(x => x.Value)
          ),
          "ConquestOrder.txt",
          true
        );

      AddToList1(
        String.Join(
          "",
          "The ",
          prefix,
          " text file has been generated; there are ",
          i,
          " ",
          prefix
        )
      );
      WriteFile(txtFile.ToString(), filename, true);
      DeleteEmptyFile(filename, i);
      ClearProgress();
    }
    private static XElement ReferencesToXElement(Dictionary<String, SortedSet<UInt64>> refs) {
      XElement references = new XElement("References");

      if (refs != null) {
        foreach (var entry in refs) {
          XElement tmpEle = new XElement(entry.Key);

          foreach (UInt64 ele in entry.Value) {
            tmpEle.Add(new XElement("Ref", ele));
          }

          references.Add(tmpEle);
        }
      }

      return references;
    }
    private static XElement ReferencesToXElement(Dictionary<UInt64, String> refs) {
      XElement references = new XElement("References");

      if (refs != null) {
        foreach (var entry in refs) {
          XElement tmpEle = new XElement("Ref", entry.Value, new XAttribute("Id", entry.Key));
          references.Add(tmpEle);
        }
      }

      return references;
    }
  }
}
