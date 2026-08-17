using System;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace PugTools {
  internal partial class Tools {
    private static XElement SortTalents(XElement talents) {
      // addtolist("Sorting Talents");

      talents.ReplaceNodes(
        talents.Elements("Talent").OrderBy(
          x => (String)x.Attribute("Status")
        ).ThenBy(
          x => (String)x.Element("Fqn")
        ).ThenBy(
          x => (String)x.Element("Title")
        ).ThenBy(
          x => (String)x.Element("ID")
        )
      );

      return talents;
    }

    /* code moved to GomLib.Models.Talent.cs */
  }
}
