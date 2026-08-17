using System;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace PugTools {
  internal partial class Tools {
    #region XML

    private static XElement SortAbilities(XElement abilities) {
      //addtolist("Sorting Abilities");
      abilities.ReplaceNodes(
        abilities.Elements("Ability").OrderBy(
          x => (String)x.Attribute("Status")
        ).ThenBy(
          x => (String)x.Element("Fqn")
        ).ThenBy(
          x => (String)x.Element("Name")
        ).ThenBy(
          x => (String)x.Attribute("Id")
        )
      );

      return abilities;
    }

    /* code moved to GomLib.Models.AbilityPackage.cs */

    /* code moved to GomLib.Models.Ability.cs */

    #endregion
  }
}
