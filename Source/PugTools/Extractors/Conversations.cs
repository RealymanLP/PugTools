using System;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace PugTools {
  internal partial class Tools {

    /* code moved to GomLib.Models.Conversation.cs */

    private static XElement SortConversations(XElement items) {
      //addtolist("Sorting Items");
      items.ReplaceNodes(
        items.Elements("Conversation").OrderBy(x => (String)x.Attribute("Fqn"))
      );

      return items;
    }
  }
}
