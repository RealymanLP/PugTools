using System;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace PugTools {
  internal partial class Tools {
    /* code moved to GomLib.Models.Npc.cs */

    // sqlexec("
    //   INSERT INTO `npc` (
    //     `npc_name`,
    //     `npc_nodeid`, 
    //     `npc_id`, 
    //     `ClassSpec`, 
    //     `Codex`, 
    //     `CompanionOverride`, 
    //     `Conversation`, 
    //     `ConversationFqn`, 
    //     `DifficultyFlags`, 
    //     `Faction`, 
    //     `Fqn`, 
    //     `IsClassTrainer`, 
    //     `IsVendor`, 
    //     `LootTableId`, 
    //     `MaxLevel`, 
    //     `MinLevel`, 
    //     `ProfessionTrained`, 
    //     `Title`, 
    //     `Toughness`, 
    //     `VendorPackages`
    //   ) VALUES (
    //     '" + insert_name + "', 
    //     '" + itm.NodeId + "', 
    //     '" + itm.Id + "', 
    //     '" + itm.ClassSpec + "', 
    //     '" + itm.Codex + "', 
    //     '" + itm.CompanionOverride + "', 
    //     '" + itm.Conversation + "', 
    //     '" + itm.ConversationFqn + "', 
    //     '" + itm.DifficultyFlags + "', 
    //     '" + itm.Faction + "', 
    //     '" + itm.Fqn + "', 
    //     '" + itm.IsClassTrainer + "', 
    //     '" + itm.IsVendor + "', 
    //     '" + itm.LootTableId + "', 
    //     '" + itm.MaxLevel + "', 
    //     '" + itm.MinLevel + "', 
    //     '" + itm.ProfessionTrained + "', 
    //     '" + insert_title + "', 
    //     '" + itm.Toughness + "', 
    //     '" + itm.VendorPackages + "'
    //   );
    // ");

    /* code moved to GomLib.Models.Npc.cs */
    /* code moved to GomLib.Models.Appearances.cs */

    private static XElement SortNpcs(XElement npcs) {
      //addtolist("Sorting Npcs");
      npcs.ReplaceNodes(
        npcs.Elements("Npc").OrderBy(
          x => (String)x.Attribute("Status")
        ).ThenBy(
          x => (String)x.Element("Fqn")
        ).ThenBy(
          x => (String)x.Element("Name")
        ).ThenBy(
          x => (String)x.Attribute("Id")
        )
      );

      return npcs;
    }
  }
}
