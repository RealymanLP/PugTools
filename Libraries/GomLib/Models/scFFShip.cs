using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Newtonsoft.Json;
using TorArchive;

namespace GomLib.Models {
  public class ScFFShip : PseudoGameObject, IEquatable<ScFFShip> {

    #region Constructors
    #endregion Constructors

    #region Fields
    #endregion Fields

    #region IEquatable

    public Boolean Equals(ScFFShip shp) {
      if (shp == null) return false;

      if (ReferenceEquals(this, shp)) return true;

      if (AbilityPackage != null) {
        if (shp.AbilityPackage != null) {
          if (!AbilityPackage.Equals(shp.AbilityPackage))
            return false;
        } else
          return false;
      } else if (shp.AbilityPackage != null)
        return false;
      if (Category != shp.Category)
        return false;
      if (CategoryId != shp.CategoryId)
        return false;
      if (ColorOptions != null) {
        if (shp.ColorOptions != null) {
          if (!ColorOptions.Keys.SequenceEqual(shp.ColorOptions.Keys))
            return false;
          foreach (var kvp in ColorOptions) {
            if (!kvp.Value.SequenceEqual(shp.ColorOptions[kvp.Key]))
              return false;
          }
        } else
          return false;
      }
      if (ComponentMap != null) {
        if (shp.ComponentMap != null) {
          if (!ComponentMap.Keys.SequenceEqual(shp.ComponentMap.Keys))
            return false;
          foreach (var kvp in ComponentMap) {
            shp.ComponentMap.TryGetValue(kvp.Key, out List<ScFFComponent> oldSlot);
            if (kvp.Value.Count != oldSlot.Count)
              return false;
            foreach (var comp in kvp.Value) {
              var oldComp = oldSlot.Where(x => x.Id == comp.Id);
              if (oldComp == null)
                return false;
              if (!comp.Equals(oldComp.Single()))
                return false;
            }

          }
        } else
          return false;
      }
      if (Cost != shp.Cost)
        return false;
      if (DamagedPackageNodeId != shp.DamagedPackageNodeId)
        return false;

      var suComp = new DictionaryComparer<String, UInt64>();
      if (!suComp.Equals(DefaultLoadout, shp.DefaultLoadout))
        return false;

      if (Description != shp.Description)
        return false;
      if (DescriptionId != shp.DescriptionId)
        return false;
      if (EngStatsNodeId != shp.EngStatsNodeId)
        return false;
      if (EppDynamicCollectionId != shp.EppDynamicCollectionId)
        return false;
      if (Faction != shp.Faction)
        return false;
      if (Icon != shp.Icon)
        return false;
      if (Id != shp.Id)
        return false;
      if (IsAvailable != shp.IsAvailable)
        return false;
      if (IsDeprecated != shp.IsDeprecated)
        return false;
      if (IsHidden != shp.IsHidden)
        return false;
      if (IsPurchasedWithCC != shp.IsPurchasedWithCC)
        return false;
      if (LookupId != shp.LookupId)
        return false;
      if (MajorComponentsContainerId != shp.MajorComponentsContainerId)
        return false;

      var slComp = new DictionaryComparer<String, Int64>();
      if (!slComp.Equals(MajorComponentSlots, shp.MajorComponentSlots))
        return false;

      if (MajorEquipType != shp.MajorEquipType)
        return false;
      if (MinorComponentsContainerId != shp.MinorComponentsContainerId)
        return false;
      if (!slComp.Equals(MinorComponentSlots, shp.MinorComponentSlots))
        return false;
      if (MinorEquipType != shp.MinorEquipType)
        return false;
      if (Model != shp.Model)
        return false;
      if (Name != shp.Name)
        return false;
      if (NameId != shp.NameId)
        return false;
      if (PatternOptions != null) {
        if (shp.PatternOptions != null) {
          if (PatternOptions.Count != shp.PatternOptions.Count)
            return false;
          for (Int32 i = 0; i < PatternOptions.Count; i++) {
            var pat = shp.PatternOptions.Where(x => x.Id == PatternOptions[i].Id);
            if (!PatternOptions[i].Equals(pat.Single()))
              return false;
          }
        } else
          return false;
      }
      if (ShipIcon != shp.ShipIcon)
        return false;

      var sfComp = new DictionaryComparer<String, Single>();
      if (!sfComp.Equals(Stats, shp.Stats))
        return false;

      if (UnknownStat1 != shp.UnknownStat1)
        return false;
      if (UnknownStat2 != shp.UnknownStat2)
        return false;
      if (UnknownStat3 != shp.UnknownStat3)
        return false;
      if (UnknownStat4 != shp.UnknownStat4)
        return false;
      if (UnknownStat5 != shp.UnknownStat5)
        return false;
      if (UnknownStat6 != shp.UnknownStat6)
        return false;
      if (UnknownStat7 != shp.UnknownStat7)
        return false;
      if (UnknownStat8 != shp.UnknownStat8)
        return false;
      return true;
    }

    #endregion IEquatable

    #region Methods
    private static XElement ContainerToXElement(Dictionary<String, Int64> containerMap,
                                                Dictionary<String, List<ScFFComponent>> componentMap,
                                                String containerName,
                                                Dictionary<String, UInt64> defaultLoadoutMap,
                                                Boolean verbose) {
      XElement container = new XElement(containerName);
      if (containerMap != null) {
        Int32 c = 1;
        foreach (var containerMapSlot in containerMap) {
          XElement subContainer = new XElement(containerName,
                                               new XAttribute("Name", containerMapSlot.Key),
                                               new XAttribute("Id", c),
                                               new XAttribute("NumSlots",
                                                              containerMapSlot.Value.ToString()));
          if (componentMap != null) {
            if (componentMap.ContainsKey(containerMapSlot.Key)) {
              foreach (ScFFComponent comp in componentMap[containerMapSlot.Key]) {
                Boolean isDefault = defaultLoadoutMap[containerMapSlot.Key] == comp.Id;
                /* code moved to GomLib.Models.scFFComponent.cs */
                subContainer.Add(comp.ToXElement(isDefault, verbose));
              }
            }
          }

          container.Add(subContainer);
          c++;
        }
      }

      return container;
    }

    #endregion Methods

    #region Override Methods

    public override Boolean Equals(Object obj) {
      if (obj == null) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj is not ScFFShip shp) return false;
      return Equals(shp);
    }

    public override HashSet<String> GetDependencies() {
      return base.GetDependencies();
    }

    public override Int32 GetHashCode() {
      // return base.GetHashCode();
      return Name.GetHashCode(); // TODO:
    }

    public override String ToString() {
      return base.ToString();
    }

    public override String ToString(Boolean verbose) {
      return base.ToString(verbose);
    }

    public override XElement ToXElement() {
      return base.ToXElement();
    }

    public override XElement ToXElement(Boolean verbose) {
      XElement shipContainer = new XElement("Ship");
      if (Id != 0) {
        String currency = " Fleet Requisition";
        if (IsPurchasedWithCC) currency = " ???";
        shipContainer.Add(new XElement("Name", Name),
            new XAttribute("Id", Id),
            new XElement("Description", Description),
            new XElement("Faction", Faction),
            new XElement("Category", Category),
            new XElement("IsAvailable_IsDeprecated_IsHidden", IsAvailable + "," + IsDeprecated + "," + IsHidden),
            new XElement("Cost", Cost + currency));
        if (verbose) {
          shipContainer.Add(//new XElement("Fqn", Fqn,
                            //new XAttribute("NodeId", NodeId)),
                            //new XAttribute("Hash", GetHashCode()),
          new XElement("Tag", Icon),
          new XElement("ShipIcon", ShipIcon),
          new XElement("Model", Model));
          foreach (var optionContainer in ColorOptions) {
            XElement colorOptions = new XElement("ColorOption",
                            new XAttribute("Id", optionContainer.Key.ToString().Replace("scFFColorOption", "")));
            foreach (var colorOption in optionContainer.Value) {
              colorOptions.Add(new XElement("Color", new XElement("Name", colorOption.Name,
                  new XAttribute("Id", colorOption.NameId)),
                  new XElement("Icon", colorOption.Icon),
                  new XElement("HashedIcon", colorOption.HashedIcon),
                  new XElement("IsAvailable", colorOption.IsAvailable),
                  //new XElement("Type", colorOption.type), //unneeded
                  new XAttribute("Id", colorOption.ShortId)));
            }
            shipContainer.Add(colorOptions);
          }
          XElement patternOptions = new XElement("Patterns");
          foreach (var pattern in PatternOptions) {
            patternOptions.Add(new XElement("Pattern", new XElement("Name", pattern.Name,
                  new XAttribute("Id", pattern.NameId)),
                  new XElement("Icon", pattern.Icon),
                  new XElement("HashedIcon", pattern.HashedIcon),
                  new XElement("IsAvailable", pattern.IsAvailable),
                  new XElement("Texture", pattern.TextureForCurrentShip),
                  //new XElement("Type", pattern.type), //unneeded
                  new XAttribute("Id", pattern.ShortId)));
          }
          shipContainer.Add(patternOptions);
        }
        XElement stats = new XElement(
                    "Stats"
                );
        if (Stats != null) {
          //stats.Add("[ " + string.Join("; ", Stats.Select(x => currentDom.statD.ToStat(x.Key) + ", " + x.Value).ToArray()) + "; ]");
          stats.Add(Stats.Select(x => new XElement("Stat", new XAttribute("Id", ((Dom.StatData.ToStat(x.Key)) ?? new DetailedStat()).DisplayName ?? x.Key), x.Value)));
          if (!verbose) {
            stats.Elements().Where(x => x.Attribute("Id").Value.Contains("4611") || x.Attribute("Id").Value.Contains("OBSOLETE")).Remove();
          }
        }
        shipContainer.Add(
            stats
        );
        shipContainer.Add(
            ContainerToXElement(MajorComponentSlots, ComponentMap, "MajorSlot", DefaultLoadout, verbose).Elements()
        );
        shipContainer.Add(
            ContainerToXElement(MinorComponentSlots, ComponentMap, "MinorSlot", DefaultLoadout, verbose).Elements()
        );

        if (verbose) {
          shipContainer.Add(new XElement("s1", UnknownStat1),
                            new XElement("s2", UnknownStat2),
                            new XElement("s3", UnknownStat3),
                            new XElement("s4", UnknownStat4),
                            new XElement("s5", UnknownStat5),
                            new XElement("s6", UnknownStat6),
                            new XElement("s7", UnknownStat7),
                            new XElement("s8", UnknownStat8));
        }
      }
      return shipContainer;
    }

    #endregion Override Methods

    #region Properties
    internal AbilityPackage AbilityPackage { get; set; }
    internal String AbilityPackageB62Id => AbilityPackage?.Id.ToMaskedBase62();
    internal String Category { get; set; }
    internal Int32 CategoryId { get; set; }
    internal Dictionary<String, List<ScFFColorOption>> ColorOptions { get; set; }
    internal Dictionary<String, List<ScFFComponent>> ComponentMap { get; set; }
    internal Int64 Cost { get; set; }
    internal UInt64 DamagedPackageNodeId { get; set; }
    internal Dictionary<String, UInt64> DefaultLoadout { get; set; }
    internal String Description { get; set; }
    [JsonConverter(typeof(LongConverter))] internal Int64 DescriptionId { get; set; }
    internal UInt64 EngStatsNodeId { get; set; }
    [JsonConverter(typeof(ULongConverter))] internal UInt64 EppDynamicCollectionId { get; set; }
    internal String Faction { get; set; }
    internal String HashedIcon {
      get {
        FileId fileId = FileId.FromFilePath($"/resources/gfx/icons/{Icon}.dds");
        return $"{fileId.Ph}_{fileId.Sh}";
      }
    }
    // public Int64 Id { get; set; }
    internal String Icon { get; set; }
    internal Boolean IsAvailable { get; set; }
    internal Boolean IsDeprecated { get; set; }
    internal Boolean IsHidden { get; set; }
    internal Boolean IsPurchasedWithCC { get; set; }
    internal Dictionary<String, String> LocalizedDescription { get; set; }
    internal Dictionary<String, String> LocalizedName { get; set; }
    [JsonConverter(typeof(ULongConverter))] internal Int64 LookupId { get; set; }
    internal Dictionary<String, Int64> MajorComponentSlots { get; set; }
    internal String Model { get; set; }
    // public String Name { get; set; }
    [JsonConverter(typeof(LongConverter))] internal Int64 NameId { get; set; }
    internal List<ScFFPattern> PatternOptions { get; set; }
    internal String ShipIcon { get; set; }
    internal Dictionary<String, Single> Stats { get; set; }
    internal UInt64 MajorComponentsContainerId { get; set; }
    internal String MajorEquipType { get; set; }
    internal UInt64 MinorComponentsContainerId { get; set; }
    internal Dictionary<String, Int64> MinorComponentSlots { get; set; }
    internal String MinorEquipType { get; set; }
    internal Single UnknownStat1 { get; set; }
    internal Single UnknownStat2 { get; set; }
    internal Single UnknownStat3 { get; set; }
    internal Single UnknownStat4 { get; set; }
    internal Single UnknownStat5 { get; set; }
    internal Single UnknownStat6 { get; set; }
    internal Single UnknownStat7 { get; set; }
    internal Single UnknownStat8 { get; set; }

    #endregion Properties

  }
}
