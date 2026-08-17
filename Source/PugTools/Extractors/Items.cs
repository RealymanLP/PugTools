using System;
// using System.Collections.Generic;
using System.Data;
using System.Linq;
// using System.Text;
using System.Xml.Linq;
// using GomLib;

namespace PugTools {
  internal partial class Tools {
    /*
    private void AddItemToSQL(GomLib.Models.Item itm) {
      String value = itm.ToSQL(PatchVersion);
      SqlAddTransactionValue(value);
    }
    */
    /*
    public void GetItemApps() {
      Clearlist2();
      ClearProgress();
      LoadData();

      List<GomObject> itmList = CurrentDom.GetObjectsStartingWith("ipp.");

      if (chkBuildCompare.Checked)
        ProcessGameObjects("ipp.", "ItemAppearances");
      else {
        Int32 i = 0;
        Int32 count = itmList.Count;

        ClearProgress();

        foreach (GomObject itm in itmList) {
          i++;

          ProgressUpdate(i, count);
          Addtolist2(itm.Name);

          WriteFile(
            new XDocument(new GomLib.Models.GameObject().ToXElement(itm)),
            itm.Name.Replace(".", "\\") + ".ipp",
            false
          );
        }
      }

      EnableButtons();
    }
    */
    /*
    public String ItemDataFromFqnListToSQL(IEnumerable<GomObject> itmList) {
      StringBuilder txtFile = new StringBuilder();

      SqlTransactionsInitialize(InitTable["Items"].InitBegin, InitTable["Items"].InitEnd);

      Int32 i = 0;
      Int32 count = itmList.Count();

      foreach (GomObject gomItm in itmList) {
        ProgressUpdate(i, count);

        GomLib.Models.Item itm = new GomLib.Models.Item();

        CurrentDom.itemLoader.Load(itm, gomItm);
        // These GomObjects are staying loaded in memory and cause our massive memory issues.
        gomItm.Unload();

        Addtolist2("ItemName: " + itm.Name);
        AddItemToSQL(itm);

        i++;
      }
      SqlTransactionsFlush();

      Addtolist("the item mysql table has been generated there were " + i + " items parsed.");
      ClearProgress();
      return txtFile.ToString();
    }
    */
    private static XElement SortItems(XElement items) {
      //addtolist("Sorting Items");
      items.ReplaceNodes(
        items.Elements("Item").OrderBy(
          x => (String)x.Attribute("Status")
        ).ThenBy(
          x => (String)x.Element("Fqn")
        )
      );
      //.ThenBy(x => (string)x.Element("Name")));

      return items;
    }
  }
}
