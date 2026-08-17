using System;
// using System.Collections.Generic;
using System.Data;
// using System.IO;
using System.Linq;
// using System.Text;
using System.Xml.Linq;

namespace PugTools {
  internal partial class Tools {

    #region Deprecated
    /*
    private String SchematicDataFromFqnList(IEnumerable<GomLib.GomObject> itmList) {
      Double i = 0;
      String n = Environment.NewLine;

      IOrderedEnumerable<GomLib.GomObject> sortedItmList = itmList.OrderBy(x => x.Name);
      Dictionary<String, List<String>> schematics = new Dictionary<String, List<String>>();
      foreach (GomLib.GomObject gomItm in sortedItmList) {
        GomLib.Models.Schematic itm = new GomLib.Models.Schematic();
        CurrentDom.schematicLoader.Load(itm, gomItm);

        addtolist2("Schematic: " + itm.Name);

        var txt = new StringBuilder();

        // itm.Item.Description.Replace("\r\n", replaceWith).Replace("\n", replaceWith)
        //   .Replace("\r", replaceWith), n));
        txt.Append(String.Format("* {0}{1}", itm.Item.Name, n)); 
        List<String> reqs = new List<String>();
        foreach (var kvp in itm.Materials) {
          var mat = itm._dom.itemLoader.Load(kvp.Key);
          reqs.Add(String.Format("{0}x {1}", kvp.Value, mat.Name)); //, mat.Description));
        }

        if (reqs.Count > 0)
          txt.Append(String.Format(" * {0}{1}", String.Join(n + " * ", reqs), n));

        String crewSkill = itm.CrewSkillId.ToString();

        if (!schematics.ContainsKey(crewSkill)) {
          schematics.Add(crewSkill, new List<String>());
        }

        schematics[crewSkill].Add(txt.ToString());
        i++;
      }

      StringBuilder outputTxt = new StringBuilder();

      foreach (var kvp in schematics) {
        List<String> list = kvp.Value;
        list.Sort();
        outputTxt.Append(
          String.Format(
            "##{0}{1}{2}{3}{4}",
            kvp.Key,
            n,
            n,
            String.Join("", list),
            n
          )
        );
      }

      addtolist("The Schematic list has been generated; there are " + i + " Schematics");

      return outputTxt.ToString();
    }
    */
    #endregion

    private static XElement SortSchematics(XElement schematics) {
      //addtolist("Sorting Talents");
      schematics.ReplaceNodes(
        schematics.Elements("Schematic").OrderBy(
          x => (String)x.Attribute("Status")
        ).ThenBy(
          x => (String)x.Element("Fqn")
        )
      );

      return schematics;
    }
  }
}
