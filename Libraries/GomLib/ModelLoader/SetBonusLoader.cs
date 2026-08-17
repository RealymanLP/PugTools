using System;
using System.Collections.Generic;
using System.Linq;
using GomLib.Models;

namespace GomLib.ModelLoader {
  public class SetBonusLoader {
    readonly DataObjectModel _dom;
    public Dictionary<Object, Object> SetBonusEntryData;
    StringTable strTable;

    public SetBonusLoader(DataObjectModel dom) {
      _dom = dom;
      Flush();
    }
    public void Flush() {
      strTable = null;
      SetBonusEntryData = new Dictionary<Object, Object>();
    }
    public SetBonusEntry Load(Int64 id) {
      if (SetBonusEntryData.Count == 0) {
        SetBonusEntryData =
          _dom.GetObject(
            "itmSetBonusesPrototype"
          ).Data.Get<Dictionary<Object, Object>>("itmSetBonuses");
      }

      SetBonusEntryData.TryGetValue(id, out Object setData);
      SetBonusEntry set = new SetBonusEntry();

      return Load(set, id, (GomObjectData)setData);
    }

    public SetBonusEntry Load(SetBonusEntry setEntry, Int64 Id, GomObjectData objData) {
      if (objData == null) return setEntry;
      if (setEntry == null) return null;

      setEntry.Id = Id;
      setEntry.Dom = _dom;
      setEntry.Prototype = "itmSetBonusesPrototype";
      setEntry.ProtoDataTable = "itmSetBonuses";

      if (strTable == null) strTable = _dom.StringTable.Find("str.gui.itm.setbonuses");
      if (strTable == null) return null;

      //The base id to use for finding the real id with an offset.
      //First id in the file take 1.
      Int64 baseId = strTable.data.Keys.First() - 1;
      Int64 nameOffset = objData.ValueOrDefault<Int64>("itmSetBonusDisplayName", 0);

      //What is the second value supposed to be for?
      setEntry.Name = strTable.GetText(baseId + nameOffset, string.Empty);
      setEntry.LocalizedName = strTable.GetLocalizedText(baseId + nameOffset, string.Empty);

      if (setEntry.LocalizedName == null) {
        setEntry.LocalizedName = new Dictionary<String, String> {
          { "enMale", "Unnamed Space Combat Set Bonus" },
          { "frMale", "Bonus d'ensemble de combat spatial sans nom" },
          { "frFemale", "Bonus d'ensemble de combat spatial sans nom" },
          { "deMale", "Unbenannter Space Combat Set Bonus" },
          { "deFemale", "Unbenannter Space Combat Set Bonus" }
        };
      }

      setEntry.MaxItemCount = objData.ValueOrDefault<Int64>("itmSetBonusItemCount", 0);
      // Dictionary<Int64, Models.Ability> setAblsByNum = 
      //   new Dictionary<Int64, Models.Ability>();
      Dictionary<Int64, UInt64> setAblsByNum =
        new Dictionary<Int64, UInt64>();
      Dictionary<Object, Object> setAblData =
        objData.ValueOrDefault("itmSetBonusBonuses", new Dictionary<Object, Object>());

      foreach (KeyValuePair<Object, Object> kvp in setAblData) {
        Int64 setNum = (Int64)kvp.Key;
        UInt64 abilityNodeId = (UInt64)kvp.Value;
        //Models.Ability abl = _dom.abilityLoader.Load(abilityNodeId);
        setAblsByNum.Add(setNum, abilityNodeId);  //setNum, abl);
      }

      setEntry.BonusAbilityIdsByNum = setAblsByNum;
      //List<Models.Item> setSourceItmList = new List<Models.Item>();
      List<UInt64> setSourceItmList = new List<UInt64>();
      Dictionary<Object, Object> setSources =
        objData.ValueOrDefault("itmSetBonusSetItems", new Dictionary<Object, Object>());

      foreach (KeyValuePair<Object, Object> kvp in setSources) {
        UInt64 itmNodeId = (UInt64)kvp.Key;
        setSourceItmList.Add(itmNodeId); //_dom.itemLoader.Load(itmNodeId));
      }

      setEntry.SourcesIds = setSourceItmList;

      return setEntry;
    }
  }
}
