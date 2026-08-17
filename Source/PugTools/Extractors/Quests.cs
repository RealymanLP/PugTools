using System;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace PugTools {
  internal partial class Tools {
    /* code moved to GomLib.Models.Quest.cs */
    /* code moved to GomLib.Models.QuestItem.cs */
    /* code moved to GomLib.Models.QuestBranch.cs */
    /* code moved to GomLib.Models.QuestStep.cs */
    /* code moved to GomLib.Models.QuestTask.cs */

    private static XElement SortQuests(XElement quests) {
      //addtolist("Sorting Quests");
      quests.ReplaceNodes(
        quests.Elements("Quest").OrderBy(
          x => (String)x.Attribute("Status")
        ).ThenBy(
          x => (String)x.Element("Fqn")
        ).ThenBy(
          x => (String)x.Attribute("Id")
        )
      );

      return quests;
    }
  }
}
