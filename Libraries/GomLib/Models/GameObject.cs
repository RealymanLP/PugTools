using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace GomLib.Models {
  public class GameObject : IDependencies {
    //[JsonConverter(typeof(Newtonsoft.Json.Converters.))]
    [JsonConverter(typeof(ULongConverter))]
    public ulong Id { get; set; }
    public string Base62Id {
      get {
        return Id.ToMaskedBase62();
      }
    }
    public string Fqn { get; set; }
    [JsonIgnore]
    public DataObjectModel Dom_ { get; set; }
    [JsonIgnore]
    public Dictionary<string, SortedSet<ulong>> References { get; set; }
    [JsonIgnore]
    public Dictionary<string, List<string>> B62References_ { get; set; }
    public Dictionary<string, List<string>> B62References {
      get {
        if (B62References_ == null) {
          if (References != null) {
            B62References_ = References.ToDictionary(x => x.Key, x => x.Value.Select(y => y.ToMaskedBase62()).ToList());
          }
        }
        return B62References_;
      }
    }
    public Dictionary<ulong, string> FullReferences { get; set; }

    public virtual string ToJSON() {
      JsonSerializerSettings settings = new JsonSerializerSettings
            {
        NullValueHandling = NullValueHandling.Ignore
      };
      return ToJSON(settings);
    }
    public string ToJSON(JsonSerializerSettings settings) {
      string json = JsonConvert.SerializeObject(this, settings);
      return json;
    }

    public override string ToString() { return ToString(true); }
    public virtual string ToString(bool verbose) { return ""; }

    public string ToSQL(string patchVersion) //rewrote the code to allow creation of new outputs faster.
    {
      if (SQLProperties == null)
        return "Unsupported";
      else
        return SQLHelpers.ToSQL(this, SQLInfo(), patchVersion);
    }

    public virtual XElement ToXElement(GomObject gomItm) { return ToXElement(gomItm, true); }
    public virtual XElement ToXElement(GomObject gomItm, bool verbose) {
      GameObject obj = Load(gomItm);
      return obj.ToXElement(verbose);
    }
    public virtual XElement ToXElement(ulong nodeId, DataObjectModel dom) { return ToXElement(nodeId, dom, false); }
    public virtual XElement ToXElement(ulong nodeId, DataObjectModel dom, bool verbose) {
      GameObject obj = Load(nodeId, dom);
      if (obj != null)
        return obj.ToXElement(verbose);
      else
        return new XElement("NotFound", nodeId);
    }
    public virtual XElement ToXElement(string fqn, DataObjectModel dom) { return ToXElement(fqn, dom, false); }
    public virtual XElement ToXElement(string fqn, DataObjectModel dom, bool verbose) {
      GameObject obj = Load(fqn, dom);
      if (obj != null)
        return obj.ToXElement(verbose);
      else
        return new XElement("NotFound", fqn);
    }
    public virtual XElement ToXElement() { return ToXElement(false); }
    public virtual XElement ToXElement(bool verbose) { return new XElement("NotImplemented", GetType().ToString()); }

    public virtual HashSet<string> GetDependencies() {
      return new HashSet<string>();
    }

    public static GameObject Load(ulong nodeId, DataObjectModel dom) {
      var gomItm = dom.GetObject(nodeId);
      if (gomItm != null) return Load(gomItm);
      return null;
    }
    public static GameObject Load(string fqn, DataObjectModel dom) {
      var gomItm = dom.GetObject(fqn);
      if (gomItm != null) return Load(gomItm);
      return null;
    }
    public static GameObject Load(GomObject gomItm) {
      return Load(gomItm, false);
    }
    public static GameObject Load(GomObject gomItm, bool classOverride) {
      switch (gomItm.Name.Substring(0, 4)) {
        case "ach.": return gomItm.Dom_.AchievementLoader.Load(gomItm);
        case "abl.":
          if (!gomItm.Name.Contains("/"))
            return gomItm.Dom_.AbilityLoader.Load(gomItm);
          return null;
        case "apn.": case "apc.": case "pkg.": return gomItm.Dom_.AbilityPackageLoader.Load(gomItm);
        case "cdx.": return gomItm.Dom_.CodexLoader.Load(gomItm);
        case "cnv.": return gomItm.Dom_.ConversationLoader.Load(gomItm);
        case "dis.": return gomItm.Dom_.NewDisciplineLoader.Load(gomItm);
        case "npc.": return gomItm.Dom_.NpcLoader.Load(gomItm);
        case "qst.": return gomItm.Dom_.QuestLoader.Load(gomItm);
        case "tal.": return gomItm.Dom_.TalentLoader.Load(gomItm);
        case "sche": return gomItm.Dom_.SchematicLoader.Load(gomItm);
        case "dec.": return gomItm.Dom_.DecorationLoader.Load(gomItm);
        case "itm.": return gomItm.Dom_.ItemLoader.Load(gomItm);
        case "apt.": return gomItm.Dom_.StrongholdLoader.Load(gomItm);
        case "clas":
          if (classOverride && gomItm.Name.StartsWith("class.pc.advanced."))
            return gomItm.Dom_.AdvancedClassLoader.Load(gomItm);
          else
            return gomItm.Dom_.ClassSpecLoader.Load(gomItm);
        case "ipp.": return gomItm.Dom_.AppearanceLoader.Load(gomItm);
        case "npp.": return gomItm.Dom_.AppearanceLoader.Load(gomItm);
        case "nco.": return gomItm.Dom_.NewCompanionLoader.Load(gomItm);
        case "spn.": return gomItm.Dom_.SpawnerLoader.Load(gomItm);
        case "plc.": return gomItm.Dom_.PlaceableLoader.Load(gomItm);
        case "mpn.": return gomItm.Dom_.MapNoteLoader.Load(gomItm);
        default:
          return null;
      }
    }

    public XElement ReferencesToXElement() {
      XElement references = new XElement("References");
      if (References != null) {
        foreach (KeyValuePair<string, SortedSet<ulong>> entry in References) {
          XElement tmpEle = new XElement(entry.Key);
          foreach (ulong ele in entry.Value) {
            tmpEle.Add(new XElement("Ref", ele));
          }
          references.Add(tmpEle);
        }
      }
      return references;
    }

    public XElement FullReferencesToXElement() {
      XElement references = new XElement("References");
      if (FullReferences != null) {
        foreach (var entry in FullReferences) {
          XElement tmpEle = new XElement("Ref", entry.Value, new XAttribute("Id", entry.Key));
          references.Add(tmpEle);
        }
      }
      return references;
    }

    public SQLData SQLInfo() {
      return new SQLData(SQLProperties);
    }
    [JsonIgnore]
    public virtual List<SQLProperty> SQLProperties { get; set; }
  }

  public class PseudoGameObject : IDependencies {

    #region Constructors
    #endregion Constructors

    #region Fields
    [JsonIgnore] private Dictionary<String, List<String>> m_B62References;

    #endregion Fields

    #region IDependencies
    public virtual HashSet<String> GetDependencies() {
      return new HashSet<String>();
    }

    #endregion IDependencies

    #region Methods
    public static PseudoGameObject Load(String xmlRoot,
                                        DataObjectModel dom,
                                        Object id,
                                        Object gomObjectData) {
      switch (xmlRoot) {
        case "MtxStoreFronts":
          MtxStorefrontEntry mtx = new MtxStorefrontEntry();
          dom.MtxStorefrontEntryLoader.Load(mtx, (Int64)id, (GomObjectData)gomObjectData);
          return mtx;
        case "Collections":
          Collection col = new Collection();
          dom.CollectionLoader.Load(col, (Int64)id, (GomObjectData)gomObjectData);
          return col;
        case "Companions":
          Companion cmp = new Companion();
          dom.CompanionLoader.Load(cmp, (UInt64)id, (GomObjectData)gomObjectData);
          return cmp;
        case "Ships":
          ScFFShip ship = new ScFFShip();
          dom.SCFFShipLoader.Load(ship, (Int64)id, (GomObjectData)gomObjectData);
          return ship;
        case "Conquests":
          Conquest cnq = new Conquest();
          dom.ConquestLoader.Load(cnq, (Int64)id, (GomObjectData)gomObjectData);
          return cnq;
        case "AchCategories":
          AchievementCategory ach = new AchievementCategory();
          dom.AchievementCategoryLoader.Load(ach, (Int64)id, (GomObjectData)gomObjectData);
          return ach;
        case "Areas":
          Area ara = new Area();
          dom.AreaLoader.Load(ara, (GomObjectData)gomObjectData);
          return ara;
        case "SetBonuses":
          SetBonusEntry setEntry = new SetBonusEntry();
          dom.SetBonusLoader.Load(setEntry, (Int64)id, (GomObjectData)gomObjectData);
          return setEntry;
        case "CodexCategoryTotals":
          CodexCatByFaction cdxCatByFaction = new CodexCatByFaction();
          dom.CdxCatTotalsLoader.Load(cdxCatByFaction,
                                      (Int64)id,
                                      (Dictionary<Object, Object>)gomObjectData);
          return cdxCatByFaction;
        case "SchematicVariations":
          SchematicVariation schemVariation = new SchematicVariation();
          dom.SchemVariationLoader.Load(schemVariation,
                                        (UInt64)id,
                                        (Dictionary<Object, Object>)gomObjectData);
          return schemVariation;
        case "PlayerTitles":
          PlayerTitle playerTitle = new PlayerTitle();
          dom.PlayerTitleLoader.Load(playerTitle, (Int64)id, (GomObjectData)gomObjectData);
          return playerTitle;
        default:
          throw new IndexOutOfRangeException();
      }
    }

    public static PseudoGameObject LoadFromProtoName(String protoName,
                                                     DataObjectModel dom,
                                                     Object id,
                                                     Object gomObjectData) {
      switch (protoName) {
        case "mtxStorefrontInfoPrototype":
          return Load("MtxStoreFronts", dom, id, gomObjectData);
        case "colCollectionItemsPrototype":
          return Load("Collections", dom, id, gomObjectData);
        case "chrCompanionInfo_Prototype":
          return Load("Companions", dom, id, gomObjectData);
        case "scFFShipsDataPrototype":
          return Load("Ships", dom, id, gomObjectData);
        case "wevConquestInfosPrototype":
          return Load("Conquests", dom, id, gomObjectData);
        case "achCategoriesTable_Prototype":
          return Load("AchCategories", dom, id, gomObjectData);
        // case "ablPackagePrototype":
        //  return 
        default:
          throw new IndexOutOfRangeException();
      }
    }

    public SQLData SQLInfo() {
      return new SQLData(SQLProperties);
    }

    public String ToJSON() {
      JsonSerializerSettings settings = new JsonSerializerSettings {
        NullValueHandling = NullValueHandling.Ignore
      };
      return ToJSON(settings);
    }

    public String ToJSON(JsonSerializerSettings settings) {
      return JsonConvert.SerializeObject(this, settings);
    }

    public String ToSQL(String patchVersion) { // Re-wrote the code to allow creation of new outputs faster.
      return SQLProperties == null ? "Unsupported"
                                   : SQLHelpers.ToSQL(this, SQLInfo(), patchVersion);
    }

    public virtual XElement ToXElement() {
      return ToXElement(true);
    }

    public virtual XElement ToXElement(Boolean verbose) {
      return new XElement("NotImplemented", GetType().ToString());
    }

    #endregion Methods

    #region Override Methods

    public override String ToString() {
      return ToString(true);
    }

    public virtual String ToString(Boolean verbose) {
      return String.Empty;
    }

    #endregion Override Methods

    #region Properties
    public Dictionary<String, List<String>> B62References =>
      m_B62References ??= (References?.ToDictionary(x => x.Key,
                                                    x => x.Value.Select(y => y.ToMaskedBase62())
                                                                .ToList())
                           ?? new Dictionary<String, List<String>>());
    public String Base62Id => ((UInt64)Id).ToMaskedBase62();
    [JsonIgnore] internal DataObjectModel Dom { get; set; }
    [JsonConverter(typeof(LongConverter))] public virtual Int64 Id { get; set; }
    public virtual String Name { get; set; }
    [JsonIgnore] internal String ProtoDataTable { get; set; } // Which prototype field contains the object
    [JsonIgnore] internal String Prototype { get; set; } // Which prototype this object is from.
    [JsonIgnore] internal Dictionary<String, SortedSet<UInt64>> References { get; set; }
    [JsonIgnore] public HashSet<String> RequiredFiles { get; set; }
    [JsonIgnore] public virtual List<SQLProperty> SQLProperties { get; set; }

    #endregion Properties

  }

  interface IDependencies {
    HashSet<string> GetDependencies();
  }
}

//public class IdToStringConverter : JsonConverter
//{
//    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//    {
//        JToken jt = JValue.ReadFrom(reader);

//        return jt.Value<long>();
//    }

//    public override bool CanConvert(Type objectType)
//    {
//        return typeof(System.Int64).Equals(objectType);
//    }

//    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//    {
//        serializer.Serialize(writer, value.ToString());
//    }
//}
