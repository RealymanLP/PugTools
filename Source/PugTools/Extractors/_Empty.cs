using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace PugTools {
  internal partial class Tools {
    #region JSON
    private static void EmptyFromPrototypeAsJSON(ref List<Object> _) {
      // Double i = 0;
      // String n = Environment.NewLine;

      // StringBuilder txtFile = new StringBuilder();
      // StringWriter txtWriter = new StringWriter(txtFile);
      // JsonWriter jsonWriter = new JsonTextWriter(txtWriter);
      // jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;

      // foreach (var entry in proto) {
      //   GomLib.Models.ReputationRank model = new GomLib.Models.ReputationRank();
      //   currentDom.reputationRankLoader.Load(model, (GomObjectData)entry);
      //   addtolist2("Rank Id: " + model.RankId);

      //   jsonWriter.WriteStartObject();

      //   jsonWriter.WritePropertyName("Rank Id");
      //   jsonWriter.WriteValue(model.RankId);

      //   jsonWriter.WritePropertyName("Rank Title Id");
      //   jsonWriter.WriteValue(model.RankTitleId);

      //   jsonWriter.WritePropertyName("Rank Title");
      //   jsonWriter.WriteValue(model.RankTitle);
      //   jsonWriter.WritePropertyName("Rank Points");
      //   jsonWriter.WriteValue(model.RankPoints);

      //   jsonWriter.WriteEndObject();

      //   i++;
      // }
      // String path = "\\JSON\\";
      // if (!System.IO.Directory.Exists(Config.ExtractPath + prefix + path)) { System.IO.Directory.CreateDirectory(Config.ExtractPath + prefix + path); }
      // WriteFile(txtFile.ToString(), path + "Empty.json", false);
      // addtolist("the Empty list has been generated there are " + i + " Ranks");
    }
    #endregion
    #region Txt
    private static void EmptyFromPrototypeAsTxt(List<Object> _) {
      // Double i = 0;
      // Double e = 0;
      // String n = Environment.NewLine;
      // StringBuilder txtFile = new StringBuilder();

      // foreach (var rankEntry in repRankData) {
      //   GomLib.Models.Empty rank = new GomLib.Models.Empty();
      //   currentDom.reputationRankLoader.Load(rank, (GomObjectData)rankEntry);

      //   addtolist2("Rank Title: " + rank.RankTitle);
      //   String t = "  ";
      //   txtFile.Append("Rank Title: " + rank.RankTitle + n);
      //   txtFile.Append(t + "Rank Id: " + rank.RankId + n);
      //   txtFile.Append(t + "Rank Title Id: " + rank.RankTitleId + n);
      //   txtFile.Append(t + "Rank Points: " + rank.RankPoints + n);
      //   i++;

      // }
      // WriteFile(txtFile.ToString(), "Reputation_Ranks.txt", false);
      // addtolist("the Reputation Rank list has been generated there are " + i + " Reputation Ranks");
    }
    #endregion
    #region XML
    private XElement EmptyFromPrototypeAsXElement(List<Object> rankProto, Boolean _) {
      if (rankProto == null) throw new ArgumentNullException(nameof(rankProto));

      XElement ranks = new XElement("Ranks");
      //...

      AddToList1("The Empty list has been generated there are " + 0 + " Empty");

      return ranks;
    }
    #endregion
    internal void GetEmpty() {
      Clearlist2();
      LoadData();

      List<Object> data = CurrentDom.GetObject("...").Data.Get<List<Object>>("...");

      Boolean addedChanged = false;

      if (chkBuildCompare.Checked) addedChanged = true;

      if (s_outputTypeName == "Text") {
        EmptyFromPrototypeAsTxt(data);
      } else if (s_outputTypeName == "JSON") {
        EmptyFromPrototypeAsJSON(ref data);
      } else {
        EmptyFromPrototypeAsXElement(data, addedChanged);
      }

      UnloadEmpty();
      EnableButtons();
    }

    private static void UnloadEmpty() {
      //currentDom.reputationRankLoader.Flush();
    }
  }
}
