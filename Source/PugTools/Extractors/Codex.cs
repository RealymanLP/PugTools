using System;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace PugTools {
  internal partial class Tools {
    private static XElement SortCodices(XElement codices) {
      //addtolist("Sorting Codex Entries");
      codices.ReplaceNodes(
        codices.Elements("Codex").OrderBy(
          x => (String)x.Attribute("Status")
        ).ThenBy(
          x => (String)x.Element("Fqn")
        ).ThenBy(
          x => (String)x.Element("Title")
        ).ThenBy(
          x => (String)x.Element("ID")
        )
      );

      return codices;
    }

    /* code moved to GomLib.Models.Codex.cs */

  }
}
