using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GomLib.Models;

namespace GomLib.Tables {
  /// <summary>GomLib.Tables.ArmorPerLevel.TableData[ArmorSpec][quality][ilvl][ArmorSlot]</summary>
  public class ItemModifierPackageTablePrototype {
    [Newtonsoft.Json.JsonIgnore]
    private readonly DataObjectModel _dom;

    public ItemModifierPackageTablePrototype(DataObjectModel dom) {
      _dom = dom;
      LoadData();
    }

    private Dictionary<long, Dictionary<string, object>> item_modpkgprototype_data;
    readonly string itmModifierPackageTablePrototypePath = "itmModifierPackageTablePrototype";
    private bool disposed = false;

    public void Dispose() {
      Dispose(true);
      // GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
      if (disposed)
        return;
      if (disposing && item_modpkgprototype_data != null) {
        foreach (var dict in item_modpkgprototype_data) {
          dict.Value.Clear();
        }
        item_modpkgprototype_data.Clear();
        item_modpkgprototype_data = null;
      }
      disposed = true;
    }

    ~ItemModifierPackageTablePrototype() {
      Dispose(false);
    }

    public Dictionary<long, Dictionary<string, object>> TableData {
      get {
        if (item_modpkgprototype_data == null) { LoadData(); }
        return item_modpkgprototype_data;
      }
    }

    public long GetModPkgNameId(long id) {
      if (item_modpkgprototype_data == null) { LoadData(); }
      if (item_modpkgprototype_data == null ||
          !item_modpkgprototype_data.TryGetValue(id, out Dictionary<string, object> row) ||
          !row.TryGetValue("itmModPkgNameId", out object value) || value == null) {
        return 0;
      }

      if (value is long l) return l;
      if (value is int i) return i;
      if (value is ulong ul && ul <= long.MaxValue) return (long)ul;
      if (value is uint ui) return ui;

      return Convert.ToInt64(value);
    }

    public Dictionary<object, object> GetModPkgStatValues(long id) {
      if (item_modpkgprototype_data == null) { LoadData(); }
      if (item_modpkgprototype_data == null ||
          !item_modpkgprototype_data.TryGetValue(id, out Dictionary<string, object> row) ||
          !row.TryGetValue("itmModPkgAttributePercentages", out object value) || value == null) {
        return new Dictionary<object, object>();
      }

      if (value is Dictionary<object, object> objectMap)
        return objectMap;

      if (value is Dictionary<string, object> stringMap)
        return stringMap.ToDictionary(kvp => (object)kvp.Key, kvp => kvp.Value);

      if (value is GomObjectData gomData)
        return gomData.Dictionary.ToDictionary(kvp => (object)kvp.Key, kvp => kvp.Value);

      return new Dictionary<object, object>();
    }

    private static Dictionary<string, object> TryGetRowDictionary(object value) {
      if (value == null)
        return null;

      if (value is GomObjectData gomData)
        return new Dictionary<string, object>(gomData.Dictionary);

      if (value is Dictionary<string, object> stringMap)
        return new Dictionary<string, object>(stringMap);

      if (value is IDictionary<string, object> stringDictionary)
        return stringDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

      if (value is Dictionary<object, object> objectMap) {
        Dictionary<string, object> result = new Dictionary<string, object>();
        foreach (KeyValuePair<object, object> kvp in objectMap) {
          if (kvp.Key != null)
            result[kvp.Key.ToString()] = kvp.Value;
        }
        return result;
      }

      // Newer GOM data can wrap a struct in a list/array.  Older PugTools
      // assumed every map value was directly a GomObjectData and crashed here.
      if (value is IEnumerable<object> values) {
        Dictionary<string, object> merged = null;

        foreach (object entry in values) {
          Dictionary<string, object> entryMap = TryGetRowDictionary(entry);
          if (entryMap == null || entryMap.Count == 0)
            continue;

          if (merged == null)
            merged = new Dictionary<string, object>();

          foreach (KeyValuePair<string, object> kvp in entryMap)
            merged[kvp.Key] = kvp.Value;
        }

        return merged;
      }

      return null;
    }

    private static bool TryGetInt64(object value, out long result) {
      switch (value) {
        case long l:
          result = l;
          return true;
        case int i:
          result = i;
          return true;
        case uint ui:
          result = ui;
          return true;
        case ulong ul when ul <= long.MaxValue:
          result = (long)ul;
          return true;
        case short s:
          result = s;
          return true;
        case ushort us:
          result = us;
          return true;
        case byte b:
          result = b;
          return true;
        case sbyte sb:
          result = sb;
          return true;
        default:
          try {
            result = Convert.ToInt64(value);
            return true;
          }
          catch {
            result = 0;
            return false;
          }
      }
    }

    private void LoadData() {
      GomObject table = _dom.GetObject(itmModifierPackageTablePrototypePath);
      if (table == null || table.Data == null) {
        item_modpkgprototype_data = new Dictionary<long, Dictionary<string, object>>();
        return;
      }

      Dictionary<object, object> tableData =
        table.Data.ValueOrDefault<Dictionary<object, object>>("itmModifierPackagesList", null);

      item_modpkgprototype_data = new Dictionary<long, Dictionary<string, object>>();
      if (tableData == null)
        return;

      foreach (KeyValuePair<object, object> kvp in tableData) {
        if (!TryGetInt64(kvp.Key, out long modId))
          continue;

        Dictionary<string, object> map = TryGetRowDictionary(kvp.Value);
        if (map == null)
          continue;

        item_modpkgprototype_data[modId] = map;
      }
    }
  }
}
