using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GomLib.Models {
  /// <summary>Loot Tables - trs.*</summary>
  public class LootTable {
    private static Dictionary<string, LootTable> fqnMap;
    private static Dictionary<string, string> failedFqns;

    static LootTable() {
      FqnMap = new Dictionary<string, LootTable>();
      FailedFqns = new Dictionary<string, string>();
    }

    public string Spec { get; set; }
    public static Dictionary<String, LootTable> FqnMap { get => fqnMap; set => fqnMap = value; }
    public static Dictionary<String, String> FailedFqns { get => failedFqns; set => failedFqns = value; }

    //public static LootTable FromFqn(string fqn)
    //{
    //    LootTable trs;
    //    if (failedFqns.ContainsKey(fqn)) { return null; }
    //    if (!fqnMap.TryGetValue(fqn, out trs))
    //    {
    //        var rows = TorLib.Tables.TreasureTable.FindRowsBySpec(fqn);
    //        if ((rows == null) || (rows.Count == 0))
    //        {
    //            failedFqns.Add(fqn, "Row not found");
    //            return null;
    //        }
    //        try
    //        {
    //            trs = FromRows(rows);
    //        }
    //        catch (Exception ex)
    //        {
    //            failedFqns.Add(fqn, ex.Message);
    //            return null;
    //        }
    //    }

    //    return trs;
    //}

    //public static LootTable FromRows(List<TorLib.Tables.TreasureTable.Row> rows)
    //{
    //    if (rows.Count == 0) { return null; }
    //    if (fqnMap.ContainsKey(rows[0].tableSpec)) { return fqnMap[rows[0].tableSpec]; }

    //    LootTable trs = new LootTable();
    //    trs.Spec = rows[0].tableSpec;
    //    fqnMap.Add(rows[0].tableSpec, trs);
    //    return trs;
    //}
  }
}
