using System;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace PugTools {
  internal partial class Tools {

    #region XML
    private static XElement SortCompanions(XElement companions) {
      //addtolist("Sorting Companions");
      companions.ReplaceNodes(
        companions.Elements("Companion").OrderBy(
          x => (String)x.Attribute("Status")
        ).ThenBy(
          x => (String)x.Element("Fqn")
        ).ThenBy(
          x => (String)x.Element("Name")
        ).ThenBy(
          x => (String)x.Attribute("Id")
        )
      );

      return companions;
    }

    /* code moved to GomLib.Models.Companion.cs */

    #endregion
  }
}
