using System;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace PugTools {
  internal partial class Tools {
    private static XElement SortAchievements(XElement achievements) {
      //addtolist("Sorting Achievement Entries");
      achievements.ReplaceNodes(
        achievements.Elements("Achievement").OrderBy(
          x => (String)x.Attribute("Status")
        ).ThenBy(
          x => (String)x.Element("Fqn")
        ).ThenBy(
          x => (String)x.Element("Name")
        ).ThenBy(
          x => (String)x.Element("Id")
        )
      );

      return achievements;
    }

    /* code moved to GomLib.Models.Achievement.cs */

  }
}
