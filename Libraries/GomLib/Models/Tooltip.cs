using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace GomLib.Models {
  public class AttributeComparer : IComparer<String> {
    private static List<String> _orderedAttributes =
      new List<String> {
        "Mastery",
        "Endurance",
        "Accuracy Rating",
        "Power",
        "Critical Rating",
        "Force Power",
        "Tech Power",
        "Alacrity Rating",
        "Absorption Rating",
        "Defense Rating"
      };
    public static List<String> OrderedAttributes {
      get => _orderedAttributes;
      set => _orderedAttributes = value;
    }

    public Int32 Compare(String x, String y) {
      Int32 xi = OrderedAttributes.IndexOf(x);
      Int32 yi = OrderedAttributes.IndexOf(y);

      if (xi > yi) return 1;

      if (xi < yi) return -1;

      return 0;
    }
  }

  public class Tooltip : GameObject {

    private static String _language = "enMale";
    [Newtonsoft.Json.JsonIgnore]
    private GameObject _obj;
    private static Dictionary<Int64, Dictionary<String, String>> _tooltipNameMap =
      new Dictionary<Int64, Dictionary<String, String>>();
    [Newtonsoft.Json.JsonIgnore]
    private String _type;

    public String HTML {
      get => GetHTML();
    }
    public static String Language {
      get => _language;
      set => _language = value;
    }
    public static String LinkLocal {
      get {
        switch (Language) {
          case "frMale": return "/fr";
          case "deMale": return "/de";
        }
        return "";
      }
    }
    public GameObject Obj {
      get {
        if (_obj == null && PObj == null) _obj = Load(Id, Dom_);
        return _obj;
      }
      set { _obj = value; }
    }
    public PseudoGameObject PObj { get; set; }
    public override List<SQLProperty> SQLProperties {
      get => new List<SQLProperty> { 
        // (SQL Column Name, C# Property Name, SQL Column type statement, isUnique/PrimaryKey, 
        // Serialize value to json)
        new SQLProperty("NodeId", "Id", "bigint(20) unsigned NOT NULL", true),
        new SQLProperty("Base62Id", "Base62Id", "varchar(7) COLLATE utf8_unicode_ci NOT NULL"),
        new SQLProperty("Fqn", "Fqn", "varchar(255) COLLATE utf8_unicode_ci NOT NULL"),
        new SQLProperty("Tooltip", "HTML", "varchar(10000) COLLATE utf8_unicode_ci NOT NULL")
        };
    }
    public static Dictionary<Int64, Dictionary<String, String>> TooltipNameMap {
      get => _tooltipNameMap;
      set => _tooltipNameMap = value;
    }
    public String Type {
      get => _type ?? Obj.GetType().ToString();
      set => _type = value;
    }

    public Tooltip() { }

    public Tooltip(UInt64 id, DataObjectModel dom) {
      Dom_ = dom;
      Id = id;
      Fqn = Dom_.GetStoredTypeName(id);
    }

    public Tooltip(String fqn, DataObjectModel dom) {
      Dom_ = dom;
      Fqn = fqn;
      Id = Dom_.GetStoredTypeId(Fqn);
    }

    public Tooltip(GameObject gObj) {
      Obj = gObj;
      Id = Obj.Id;
      Fqn = Obj.Fqn;
    }

    public Tooltip(PseudoGameObject gObj) {
      PObj = gObj;
      Id = (UInt64)gObj.Id;
      Fqn = gObj.Prototype;
    }

    public static void Flush() => TooltipNameMap =
      new Dictionary<Int64, Dictionary<String, String>>();

    private String GetHTML() {
      if (Obj != null) {
        switch (Obj.GetType().ToString()) {
          case "GomLib.Models.Item":
            return ((Item)Obj).GetHTML().ToString(SaveOptions.DisableFormatting);
          case "GomLib.Models.Schematic":
            return ((Schematic)Obj).GetHTML().ToString(SaveOptions.DisableFormatting);
          case "GomLib.Models.Ability":
            return ((Ability)Obj).GetHTML().ToString(SaveOptions.DisableFormatting);
          case "GomLib.Models.Effect":
            return ((Effect)Obj).GetHTML().ToString(SaveOptions.DisableFormatting);
          case "GomLib.Models.Quest":
            return ((Quest)Obj).GetHTML().ToString(SaveOptions.DisableFormatting);
          case "GomLib.Models.Talent":
            return ((Talent)Obj).GetHTML().ToString(SaveOptions.DisableFormatting);
          case "GomLib.Models.Achievement":
            return ((Achievement)Obj).GetHTML().ToString(SaveOptions.DisableFormatting);
          case "GomLib.Models.Codex":
            return ((Codex)Obj).GetHTML().ToString(SaveOptions.DisableFormatting);
          case "GomLib.Models.Npc":
            return ((Npc)Obj).GetHTML().ToString(SaveOptions.DisableFormatting);
          case "GomLib.Models.NewCompanion":
            return ((NewCompanion)Obj).GetHTML().ToString(SaveOptions.DisableFormatting);
        }
        return "<div>Not implemented</div>";
      }

      if (PObj != null) {
        switch (PObj.GetType().ToString()) {
          case "GomLib.Models.Collection":
            return ((Collection)PObj).GetHTML().ToString(SaveOptions.DisableFormatting);
          case "GomLib.Models.MtxStorefrontEntry":
            return ((MtxStorefrontEntry)PObj).GetHTML().ToString(SaveOptions.DisableFormatting);
          case "GomLib.Models.SetBonusEntry":
            return ((SetBonusEntry)PObj).GetHTML().ToString(SaveOptions.DisableFormatting);
          case "GomLib.Models.Area":
            return ((Area)PObj).GetHTML().ToString(SaveOptions.DisableFormatting);
          default:
            break;
        }
        //if (obj.GetType() == typeof(Discipline))
        //{
        //    return ((Discipline)obj).GetHTML().ToString(SaveOptions.DisableFormatting);
        //}
        return "<div>Not implemented</div>";
      }

      return null;
    }

    public Boolean IsImplemented() {
      if (Obj != null) {
        switch (Obj.GetType().ToString()) {
          case "GomLib.Models.Item":
          case "GomLib.Models.Schematic":
          case "GomLib.Models.Ability":
          case "GomLib.Models.Quest":
          case "GomLib.Models.Talent":
          case "GomLib.Models.Achievement":
          case "GomLib.Models.Codex":
          case "GomLib.Models.Npc":
          case "GomLib.Models.NewCompanion":
            return true;
        }
      }

      if (PObj != null) {
        switch (PObj.GetType().ToString()) {
          case "GomLib.Models.Collection":
          case "GomLib.Models.MtxStorefrontEntry":
          case "GomLib.Models.SetBonusEntry":
            return true;
        }
      }

      return false;
    }
  }

  public static class TooltipHelpers {

    internal static List<UInt64> ImpClasses = new List<UInt64> {
            16140902893827567561, //Sith Warrior
            16141024490216983174, //Sith Marauder
            16141180228828243745, //Sith Juggernaut
            16140943676484767978, //Imperial Agent
            16141046347418927959, //Sniper
            16140905232405801950, //Operative
            16141010271067846579, //Sith Inquisitor
            16141163438392504574, //Sith Assassin
            16141067119934185414, //Sith Sorcerer
            16141170711935532310, //Bounty Hunter
            16141007401395916385, //Powertech
            16141111589108060476, //Mercenary
        };

    internal static void AddBaseClassIds(HashSet<UInt64> clsIds) {
      if (clsIds.Contains(16140902893827567561)) { //Sith Warrior 
        clsIds.Add(16141024490216983174); //Sith Marauder
        clsIds.Add(16141180228828243745); //Sith Juggernaut
      }

      if (clsIds.Contains(16140943676484767978)) { //Imperial Agent
        clsIds.Add(16141046347418927959); //Sniper
        clsIds.Add(16140905232405801950); //Operative
      }

      if (clsIds.Contains(16141010271067846579)) { //Sith Inquisitor
        clsIds.Add(16141163438392504574); //Sith Assassin
        clsIds.Add(16141067119934185414);  //Sith Sorcerer
      }

      if (clsIds.Contains(16141170711935532310)) { //Bounty Hunter
        clsIds.Add(16141007401395916385); //Powertech
        clsIds.Add(16141111589108060476); //Mercenary
      }

      if (clsIds.Contains(16141179471541245792)) { //Jedi Consular
        clsIds.Add(16140939761890536394); //Jedi Sage
        clsIds.Add(16141082698337403481); //Jedi Shadow
      }

      if (clsIds.Contains(16140912704077491401)) { //Smuggler
        clsIds.Add(16141041084185282043); //Gunslinger
        clsIds.Add(16141067128654459200); //Scoundrel
      }

      if (clsIds.Contains(16140973599688231714)) { //Trooper
        clsIds.Add(16141067504602942620); //Commando
        clsIds.Add(16141087184558207941); //Vanguard
      }

      if (clsIds.Contains(16141119516274073244)) { //Jedi Knight
        clsIds.Add(16140975849784542883); //Jedi Guardian
        clsIds.Add(16141180228828243745); //Jedi Sentinel
      }
    }
    private static void AddStringWithBreaks(ref XElement element, String desc) {
      if (desc.Contains('\n')) {
        String[] splits = desc.Split('\n');
        Int32 count = splits.Length;
        for (Int32 i = 0; i < count; i++) {
          element.Add(splits[i]);
          if (i != count) element.Add(new XElement("br"));
        }
      } else {
        element.Add(desc);
      }
    }
    private static void AddTableToMap(StringTable table) {
      if (table == null) return;

      foreach (var entry in table.data) {
        Dictionary<String, String> tempDict =
          entry.Value.LocalizedText.ToDictionary(
            x => x.Key,
            x => x.Value.Replace("<<1>>", "{0}").Replace("<<2>>", "{1}"));
        Tooltip.TooltipNameMap.Add(entry.Value.Id, tempDict);
      }
    }
    public static String ConvertToString(this Profession crewSkillId)
      => GetLocalizedText((Int32)crewSkillId + 836161413054464);

    public static String ConvertToString(this SlotType slot, String language) {
      switch (slot) {
        case SlotType.EquipHumanMainHand:
          return GetLocalizedText(2073124879204376, language);
        case SlotType.EquipHumanOffHand:
          return GetLocalizedText(2073124879204354, language);
        case SlotType.EquipHumanWrist:
          return GetLocalizedText(2073124879204357, language);
        case SlotType.EquipHumanBelt:
          return GetLocalizedText(2073124879204358, language);
        case SlotType.EquipHumanChest:
          return GetLocalizedText(2073124879204355, language);
        case SlotType.EquipHumanEar:
          return GetLocalizedText(2073124879204362, language);
        case SlotType.EquipHumanFace:
          return GetLocalizedText(2073124879204361, language);
        case SlotType.EquipHumanFoot:
          return GetLocalizedText(2073124879204360, language);
        case SlotType.EquipHumanGlove:
          return GetLocalizedText(2073124879204359, language);
        case SlotType.EquipHumanImplant:
          return GetLocalizedText(2073124879204363, language);
        case SlotType.EquipHumanLeg:
          return GetLocalizedText(2073124879204356, language);
        case SlotType.EquipDroidUpper:
          return GetLocalizedText(2073124879204390, language);
        case SlotType.EquipDroidLower:
          return GetLocalizedText(2073124879204392, language);
        case SlotType.EquipDroidUtility:
          return GetLocalizedText(2073124879204394, language);
        case SlotType.EquipDroidSensor:
          return GetLocalizedText(2073124879204388, language);
        case SlotType.EquipHumanHeirloom:
          return GetLocalizedText(2073124879204364, language);
        case SlotType.EquipHumanRangedPrimary:
          return GetLocalizedText(2073124879204365, language);
        case SlotType.EquipHumanRangedSecondary:
          return GetLocalizedText(2073124879204375, language);
        case SlotType.EquipHumanCustomRanged:
          return GetLocalizedText(2073124879204366, language);
        case SlotType.EquipHumanCustomMelee:
          return GetLocalizedText(2073124879204367, language);
        case SlotType.EquipHumanShield:
          return GetLocalizedText(2073124879204368, language);
        case SlotType.EquipHumanOutfit:
          return GetLocalizedText(2073124879204369, language);
        case SlotType.EquipDroidLeg:
          return GetLocalizedText(2073124879204370, language);
        case SlotType.EquipDroidFeet:
          return GetLocalizedText(2073124879204371, language);
        case SlotType.EquipDroidOutfit:
          return GetLocalizedText(2073124879204389, language);
        case SlotType.EquipDroidChest:
          return GetLocalizedText(2073124879204373, language);
        case SlotType.EquipDroidHand:
          return GetLocalizedText(2073124879204374, language);
        case SlotType.EquipHumanRelic:
          return GetLocalizedText(2073124879204377, language);
        case SlotType.EquipHumanFocus:
          return GetLocalizedText(2073124879204368, language);
        case SlotType.EquipSpaceShipArmor:
          return GetLocalizedText(2073124879204381, language);
        case SlotType.EquipSpaceBeamGenerator:
          return GetLocalizedText(2073124879204382, language);
        case SlotType.EquipSpaceBeamCharger:
          return GetLocalizedText(2073124879204383, language);
        case SlotType.EquipSpaceEnergyShield:
          return GetLocalizedText(2073124879204384, language);
        case SlotType.EquipSpaceShieldRegenerator:
          return GetLocalizedText(2073124879204385, language);
        case SlotType.EquipSpaceMissileMagazine:
          return GetLocalizedText(2073124879204386, language);
        case SlotType.EquipSpaceProtonTorpedoes:
          return GetLocalizedText(2073124879204387, language);
        case SlotType.EquipSpaceAbilityDefense:
          return GetLocalizedText(2073124879204651, language);
        case SlotType.EquipSpaceAbilityOffense:
          return GetLocalizedText(2073124879204652, language);
        case SlotType.EquipSpaceAbilitySystems:
          return GetLocalizedText(2073124879204653, language);
        case SlotType.Any:
          return null;
        case SlotType.EquipDroidShield:
        case SlotType.EquipDroidGyro:
        case SlotType.EquipDroidSpecial:
        case SlotType.EquipDroidWeapon1:
        case SlotType.EquipDroidWeapon2:
        case SlotType.Upgrade:
        case SlotType.EquipHumanRanged:
        case SlotType.EquipHumanRangedTertiary:
        case SlotType.EquipHumanLightSide:
        case SlotType.EquipHumanDarkSide:
        case SlotType.EquipSpaceShipAbilityDefense:
        case SlotType.EquipSpaceShipAbilityOffense:
        case SlotType.EquipSpaceShipAbilitySystems:
          return slot.ToString();
        default:
          return "";
      }
    }
    private static XElement GetDyeBlock(System.Drawing.Color color) {
      if (color.Name != "0") {
        String color2 =
          String.Format(
            "background-color: rgba({0}, {1}, {2}, {3}); box-shadow: 0px 0px 2px;",
            (Int32)color.R,
            (Int32)color.G,
            (Int32)color.B,
            (Int32)color.A
          );

        return new XElement(
          "div",
          XClass("torctip_col_block"),
          new XAttribute("style", color2), " ");
      } else
        return new XElement(
          "div",
          XClass("torctip_col_block"),
          GetLocalizedText(836131348283742)); //"No Color"
    }
    public static String GetFaction(this ClassSpec cls) {
      return ImpClasses.Contains(cls.Id) ? "Imperial" : "Republic";
    }
    #region Ability
    public static XElement GetHTML(this Ability itm) {
      if (Tooltip.TooltipNameMap.Count == 0) LoadNameMap(itm.Dom_);
      if (itm.Id == 0) return new XElement("div", "Not Found");

      String stringQual = "ability";
      String icon = itm.Icon;

      XElement tooltip = new XElement("div", new XAttribute("class", "torctip_wrapper"));
      TorArchive.FileId fileId =
        TorArchive.FileId.FromFilePath(String.Format("/resources/gfx/icons/{0}.dds", icon));

      if (itm != null) {
        XElement tooltip_header = new XElement("div", new XAttribute("class", "torctip_header"));
        XElement imgelement = new XElement("div", XClass("torctip_image_wrapper"), String.Empty);
        imgelement.Add(
          new XElement(
            "div",
            XClass(
              String.Format("torctip_image torctip_image_{0}",
              stringQual)
            ),
            new XElement(
              "img",
              new XAttribute(
                "src",
                String.Format("https://torcommunity.com/db/icons/{0}_{1}.jpg", fileId.Ph, fileId.Sh)
              ),
              new XAttribute(
                "alt",
                String.Empty
              )
            )
          )
        );
        XElement cast = new XElement("div", String.Empty);

        if (itm.IsPassive) {
          cast.Add(
            new XElement(
              "span",
              XClass("torctip_white"),
              GetLocalizedText(836131348283424) // "Passive"
            )
          );
        } else if (itm.CastingTime > 0) {
          cast.Add(
            XStat(
              GetLocalizedText(836131348283425), // "Activation: "
              String.Format("{0}s", itm.CastingTime)
            )
          );
        } else if (itm.ChannelingTime > 0) {
          cast.Add(
            XStat(
              GetLocalizedText(836131348283426), // "Channeled: "
              String.Format("{0}s", itm.ChannelingTime)
            )
          );
        } else {
          cast.Add(
            new XElement(
              "span",
              XClass("torctip_white"),
              GetLocalizedText(836131348283428) // "Instant"
            )
          );
        }

        XElement tooltip_header_text =
          new XElement(
            "div",
            new XAttribute("class", "torctip_header_text"),
            new XElement("span", itm.LocalizedName[Tooltip.Language]),
            cast
          );

        tooltip_header.Add(imgelement, tooltip_header_text);
        tooltip.Add(tooltip_header);
        XElement inner = new XElement("div", XClass("torctip_tooltip"));

        XElement playerblock = new XElement("div", XClass("torctip_section"));
        String costType = "";
        Single cost = 0;

        if (itm.ApCost > 0) {
          switch (itm.ApType) {
            case ApType.Ammo:
              costType = GetLocalizedText(836131348283422); // "Heat/Ammo: ";
              cost = itm.ApCost;
              break;
            case ApType.Heat:
              costType = GetLocalizedText(836131348283423); // "Heat/Ammo: ";
              cost = itm.ApCost;
              break;
            case ApType.Focus:
              costType = GetLocalizedText(836131348283420); // "Force: ";
              cost = itm.ApCost;
              break;
            case ApType.Rage:
              costType = GetLocalizedText(836131348283421); // "Rage: ";
              cost = itm.ApCost;
              break;
            default:
              // if (itm.Fqn.Contains("test")
              //     || itm.Fqn.Contains("npc")
              //     || itm.Fqn.Contains("creature")
              //     || itm.Fqn.Contains("qtr")) {
              //   break;
              // } else {
              break;
              // }
          }
        } else if (itm.ForceCost > 0) {
          switch (itm.ApType) {
            case ApType.Focus:
              costType = GetLocalizedText(836131348283420); // "Force: ";
              cost = itm.ForceCost;
              break;
            case ApType.Rage:
              costType = GetLocalizedText(836131348283421); // "Rage: ";
              cost = itm.ForceCost;
              break;
            default:
              costType = GetLocalizedText(836131348283420); // "Force: ";
              cost = itm.ForceCost;
              break;
          }
        } else if (itm.EnergyCost > 0) {
          costType = GetLocalizedText(836131348283419); // "Energy: ";
          cost = itm.EnergyCost;
        }

        if (costType != "" && cost != 0) playerblock.Add(XStat(costType, cost.ToString()));

        if (itm.Cooldown > 0) {
          //playerblock.Add(XStat("Cooldown: ", String.Format("{0}s", itm.Cooldown.ToString())));
          playerblock.Add(
              XStat(
                  GetLocalizedText(836131348283427),
                  String.Format("{0}s", itm.Cooldown.ToString())
              )
          );
        }

        if (itm.MaxRange > 0) {
          // playerblock.Add(
          //   XStat(
          //     "Range: ", 
          //     String.Format("{0}m", Math.Round(itm.MaxRange * 10).ToString())
          //   )
          // );
          playerblock.Add(
            XStat(
              GetLocalizedText(836131348283429),
              String.Format("{0}m", Math.Round(itm.MaxRange * 10).ToString())
            )
          );
        }

        if (playerblock.HasElements) inner.Add(playerblock);

        inner.Add(
          new XElement(
            "div",
            XClass("torctip_section"),
            new XElement(
              "div",
              XClass("torctip_white"),
              Ability.ParseDescription(itm, itm.LocalizedDescription[Tooltip.Language])
            )
          )
        );
        tooltip.Add(inner);
      }

      return tooltip;
    }
    #endregion
    #region Achievements
    public static XElement GetHTML(this Achievement itm) {
      if (Tooltip.TooltipNameMap.Count == 0) LoadNameMap(itm.Dom_);

      if (itm.Id == 0) return new XElement("div", "Not Found");

      String icon = "none";
      icon = itm.Icon;
      String stringQual = "achievement";
      XElement tooltip = new XElement("div", XClass("torctip_wrapper"), String.Empty);
      TorArchive.FileId fileId =
        TorArchive.FileId.FromFilePath(String.Format("/resources/gfx/icons/{0}.dds", icon));

      if (itm != null) {
        Int64 points = 0;

        if (itm.Rewards != null) points = itm.Rewards.AchievementPoints;

        XElement tooltip_header = new XElement("div", XClass("torctip_header"), String.Empty);
        XElement imgelement = new XElement(
          "div",
          XClass(
            String.Format("torctip_image_wrapper {0}", stringQual)),
            new XElement(
              "div",
              XClass(
                String.Format("torctip_image torctip_image_{0}", stringQual)
              ),
              new XElement(
                "img",
                new XAttribute(
                  "src",
                  String.Format(
                    "https://torcommunity.com/db/icons/{0}_{1}.jpg",
                    fileId.Ph,
                    fileId.Sh
                  )
                ),
                new XAttribute("alt", "")
              ),
              new XElement(
                "div",
                XClass("torctip_icon_points"),
                new XElement("span", XClass("torctip_ach_star"), " "),
                points
              )
            )
          );
        XElement inner = new XElement(
          "div",
          XClass("torctip_tooltip"),
          new XElement(
            "span",
            XClass(String.Format("torctip_{0}", stringQual)),
            itm.LocalizedName[Tooltip.Language]
          )
        );
        Boolean addRewards = false;

        if (itm.Rewards != null)
          inner.Add(
            new XElement(
              "span",
              XClass("torctip_ach_points"),
              " [ ",
              new XElement("span", XClass("torctip_ach_star"), " "),
              String.Format(" {0} ]", itm.Rewards.AchievementPoints)
            )
          );

        XElement desc =
          new XElement("div", XClass("torctip_blue"), itm.LocalizedDescription[Tooltip.Language]);
        inner.Add(desc);
        XElement taskText = new XElement("span", XClass("torctip_ach_tsks"));

        foreach (AchTask tsk in itm.Tasks) {
          String tskName = tsk.Name;

          if (tsk.LocalizedNames.Count != 0) tskName = tsk.LocalizedNames[Tooltip.Language];

          if (tskName == "") {
            GomObject tskSub = itm.Dom_.GetObject(tsk.Id);

            if (tskSub != null) {
              GameObject obj = GameObject.Load(tskSub);

              switch (tskSub.Name.Substring(0, 4)) {
                case "ach.":
                  taskText.Add(
                    new XElement(
                      "div",
                      XClass("torctip_ach_tsk"),
                      new XElement("span", String.Format("0/{0} ", tsk.Count)),
                      new XElement(
                        "a",
                        XClass(String.Format("torctip_{0}", "achievement")),
                        new XAttribute(
                          "href",
                          String.Format(
                            "https://torcommunity.com{2}/database/achievement/{0}/{1}/",
                            obj.Base62Id,
                            ((Achievement)obj).LocalizedName[Tooltip.Language].LinkString(),
                            Tooltip.LinkLocal)
                        ),
                        new XAttribute("data-torc", "norestyle"),
                        ((Achievement)obj).LocalizedName[Tooltip.Language]
                      )
                    )
                  );
                  break;
                case "abl.":
                  taskText.Add(
                    new XElement(
                      "div",
                      XClass("torctip_ach_tsk"),
                      new XElement("span", String.Format("0/{0} ", tsk.Count)),
                      new XElement(
                        "a",
                        XClass(String.Format("torctip_{0}", "ability")),
                        new XAttribute(
                          "href",
                          String.Format(
                            "https://torcommunity.com{2}/database/ability/{0}/{1}/",
                            obj.Base62Id,
                            ((Ability)obj).LocalizedName[Tooltip.Language].LinkString(),
                            Tooltip.LinkLocal)
                        ),
                        new XAttribute("data-torc", "norestyle"),
                        ((Ability)obj).LocalizedName[Tooltip.Language]
                      )
                    )
                  );
                  break;
                case "cdx.":
                  taskText.Add(
                    new XElement(
                      "div",
                      XClass("torctip_ach_tsk"),
                      new XElement(
                        "span",
                        String.Format("0/{0} ", tsk.Count)
                      ),
                      new XElement(
                        "a",
                        XClass(String.Format("torctip_{0}", "codex")),
                        new XAttribute(
                          "href",
                          String.Format(
                            "https://torcommunity.com{2}/database/codex/{0}/{1}/",
                            obj.Base62Id,
                            ((Codex)obj).LocalizedName[Tooltip.Language].LinkString(),
                            Tooltip.LinkLocal)
                        ),
                        new XAttribute("data-torc", "norestyle"),
                        ((Codex)obj).LocalizedName[Tooltip.Language]
                      )
                    )
                  );
                  break;
                case "npc.":
                  taskText.Add(
                    new XElement(
                      "div",
                      XClass("torctip_ach_tsk"),
                      new XElement(
                        "span",
                        String.Format("0/{0} ", tsk.Count)
                      ),
                      new XElement(
                        "a",
                        XClass(String.Format("torctip_{0}", "npc")),
                        new XAttribute(
                          "href",
                          String.Format(
                            "https://torcommunity.com{2}/database/npc/{0}/{1}/",
                            obj.Base62Id,
                            ((Npc)obj).LocalizedName[Tooltip.Language].LinkString(),
                            Tooltip.LinkLocal)
                        ),
                        new XAttribute("data-torc", "norestyle"),
                        ((Npc)obj).LocalizedName[Tooltip.Language]
                      )
                    )
                  );
                  break;
                case "nco.":
                  taskText.Add(
                    new XElement(
                      "div",
                      XClass("torctip_ach_tsk"),
                      new XElement(
                        "span",
                        String.Format("0/{0} ", tsk.Count)
                      ),
                      new XElement(
                        "a",
                        XClass(String.Format("torctip_{0}", "nco")),
                        new XAttribute(
                          "href",
                          String.Format(
                            "https://torcommunity.com{2}/database/npc/{0}/{1}/",
                            obj.Base62Id,
                            ((NewCompanion)obj)
                              .Companion
                              .LocalizedName[Tooltip.Language]
                              .LinkString(),
                            Tooltip.LinkLocal)
                        ),
                        new XAttribute("data-torc", "norestyle"),
                        ((NewCompanion)obj).Companion.LocalizedName[Tooltip.Language]
                      )
                    )
                  );
                  break;
                case "qst.":
                  taskText.Add(
                    new XElement(
                      "div",
                      XClass("torctip_ach_tsk"),
                      new XElement(
                        "span",
                        String.Format("0/{0} ", tsk.Count)
                      ),
                      new XElement(
                        "a",
                        XClass(String.Format("torctip_{0}", "mission")),
                        new XAttribute(
                          "href",
                          String.Format(
                            "https://torcommunity.com{2}/database/mission/{0}/{1}/",
                            obj.Base62Id,
                            ((Quest)obj).LocalizedName[Tooltip.Language].LinkString(),
                            Tooltip.LinkLocal)
                        ),
                        new XAttribute("data-torc", "norestyle"),
                        ((Quest)obj).LocalizedName[Tooltip.Language]
                      )
                    )
                  );
                  break;
                case "tal.":
                  tskName = ((Talent)obj).LocalizedName[Tooltip.Language];
                  break;
                case "sche":
                  tskName = ((Schematic)obj).LocalizedName[Tooltip.Language];
                  break;
                case "dec.":
                  tskName = ((Decoration)obj).LocalizedName[Tooltip.Language];
                  break;
                case "itm.":
                  taskText.Add(
                    new XElement(
                      "div",
                      XClass("torctip_ach_tsk"),
                      new XElement("span", String.Format("0/{0} ", tsk.Count)),
                      new XElement(
                        "a",
                        XClass(String.Format("torctip_{0}", ((Item)obj).Quality.ToString())),
                        new XAttribute(
                          "href",
                          String.Format(
                            "https://torcommunity.com{2}/database/item/{0}/{1}/",
                            obj.Base62Id,
                            ((Item)obj).LocalizedName[Tooltip.Language].LinkString(),
                            Tooltip.LinkLocal
                          )
                        ),
                        new XAttribute("data-torc", "norestyle"),
                        ((Item)obj).LocalizedName[Tooltip.Language]
                      )
                    )
                  );
                  break;
                default:
                  break;
              }
            } else {
              Area area = itm.Dom_.AreaLoader.Load(tsk.Id);

              if (area.Id != 0) {
                //area.SortMaps();
                if (area.FowGroupStringIds.Count > 0) {
                  Boolean splitfound = false;
                  List<KeyValuePair<String, String>> ReverseLookup =
                    new List<KeyValuePair<String, String>>();

                  foreach (var kvp in area.FowGroupLocalizedStrings) {
                    if (kvp.Value != null) {
                      kvp.Value.TryGetValue(Tooltip.Language, out String fowName);

                      if (fowName.Contains("<br>")) {
                        splitfound = true;
                        Int32 index = fowName.IndexOf("<br>");
                        String start = fowName.Substring(0, index);
                        String end = fowName[(index + 4)..];
                        ReverseLookup.Add(new KeyValuePair<String, String>(end, start));
                      } else {
                        taskText.Add(
                          new XElement(
                            "div",
                            XClass("torctip_white"),
                            String.Format("0/1 {0}", fowName)
                          )
                        );
                      }
                    }
                  }

                  if (splitfound) {
                    IEnumerable<String> distinct = ReverseLookup.Select(x => x.Value).Distinct();

                    if (distinct.Count() == 1) {
                      IOrderedEnumerable<String> ordered =
                        ReverseLookup.Select(x => x.Key).OrderBy(x => x);

                      foreach (String key in ordered) {
                        taskText.Add(
                          new XElement(
                            "div",
                            XClass("torctip_white"),
                            String.Format("0/1 {0}", key)
                          )
                        );
                      }
                    } else {
                      distinct = distinct.OrderBy(x => x).ToList();

                      foreach (var val in distinct) {
                        IEnumerable<String> distinceSubs =
                          ReverseLookup
                            .Where(x => x.Value == val)
                            .OrderBy(x => x.Key)
                            .Select(x => x.Key);
                        taskText.Add(
                          new XElement(
                            "div",
                            XClass("torctip_white torctip_sub_parent"),
                            new XElement(
                              "span",
                              String.Format("0/{0} {1}", distinceSubs.Count(), val)
                            ),
                            new XElement(
                              "div",
                              XClass("torctip_sub_tasks"),
                              distinceSubs.Select(
                                x => new XElement(
                                  "div",
                                  XClass("torctip_white"),
                                  String.Format("0/1 {0}", x)
                                )
                              )
                            )
                          )
                        );
                      }
                    }
                  }
                }
              }
            }
          } else {
            taskText.Add(
              new XElement(
                "div",
                XClass("torctip_white"),
                String.Format("0/{0} {1}", tsk.Count, tskName)
              )
            );
          }
        }

        inner.Add(taskText);
        XElement rewards =
          new XElement(
            "div",
            XClass("torctip_rewards"),
            new XElement(
              "span",
              GetLocalizedText(3146450091376903) // "Rewards"
            )
          );
        XElement rewardContainer = new XElement("div", XClass("torctip_rwd_inner"));

        if (itm.Rewards != null) {
          if (itm.Rewards.CartelCoins > 0) {
            addRewards = true;
            rewardContainer.Add(
              new XElement(
                "div",
                XClass("torctip_ach_items"),
                new XElement(
                  "div",
                  XClass("torctip_blue"),
                  GetLocalizedText(3146450091376915) // "Cartel Coins:"
                ),
                new XElement(
                  "div",
                  XClass("torctip_mtx_coins"),
                  itm.Rewards.CartelCoins
                )
              )
            );
          }

          if (itm.Rewards.Requisition > 0) {
            addRewards = true;
            rewardContainer.Add(
              new XElement(
                "div",
                XClass("torctip_ach_items"),
                new XElement(
                  "div",
                  XClass("torctip_blue"),
                  GetLocalizedText(3146450091376916) // "Fleet Requisition:"
                ),
                new XElement(
                  "div",
                  XClass("torctip_gsf_cur"),
                  itm.Rewards.Requisition
                )
              )
            );
          }

          if (itm.Rewards.LegacyTitle != null) {
            addRewards = true;
            rewardContainer.Add(
              new XElement(
                "div",
                XClass("torctip_ach_items"),
                new XElement(
                  "div",
                  XClass("torctip_blue"),
                  GetLocalizedText(3146450091376914) // "Legacy Title:"
                ),
                new XElement(
                  "div",
                  XClass("torctip_lgy_tit"),
                  itm.Rewards.LocalizedLegacyTitle[Tooltip.Language]
                )
              )
            );
          }

          if (itm.Rewards.ItemRewardList != null) {
            XElement providedRewards =
              new XElement(
                "div",
                XClass("torctip_ach_items"),
                new XElement(
                  "div",
                  XClass("torctip_blue"),
                  GetLocalizedText(3146450091376919) // "Item Rewards:"
                )
              );

            foreach (var rew in itm.Rewards.ItemRewardList) {
              addRewards = true;
              Item mat = itm.Dom_.ItemLoader.Load(rew.Key);

              if (mat == null) continue;

              String matstringQual =
                (mat.TypeBitFlags.IsModdable && (mat.Quality == ItemQuality.Prototype))
                ? "moddable"
                : mat.Quality.ToString().ToLower();
              TorArchive.FileId matfileId =
                TorArchive.FileId.FromFilePath(
                  String.Format("/resources/gfx/icons/{0}.dds", mat.Icon)
                );
              XElement matElement =
                new XElement(
                  "div",
                  XClass("torctip_rwd"),
                  new XAttribute("style", "display: inline;"),
                  new XElement(
                    "a",
                    new XAttribute(
                      "href",
                      String.Format(
                        "https://torcommunity.com{2}/database/item/{0}/{1}/",
                        mat.Base62Id,
                        LinkString(mat.Name),
                        Tooltip.LinkLocal
                      )
                    ),
                    new XAttribute("data-torc", "norestyle"),
                    XClass(String.Format("torctip_image torctip_image_{0}", matstringQual)),
                    new XElement(
                      "img",
                      new XAttribute(
                        "src",
                        String.Format(
                          "https://torcommunity.com/db/icons/{0}_{1}.jpg",
                          matfileId.Ph,
                          matfileId.Sh
                        )
                      ),
                      new XAttribute("alt", mat.Name)
                    ),
                    new XElement(
                      "span",
                      XClass("torctip_rwd_overlay"),
                      rew.Value
                    )
                  )
                );
              providedRewards.Add(matElement);
            }

            if (providedRewards.Elements().Count() > 1) rewardContainer.Add(providedRewards);
          }
        }

        imgelement.Add(inner);
        tooltip_header.Add(imgelement);
        tooltip.Add(tooltip_header);

        if (addRewards) {
          rewards.Add(rewardContainer);
          tooltip.Add(rewards);
        }
      }

      return tooltip;
    }
    #endregion
    #region Area
    public static XElement GetHTML(this Area itm) {
      if (Tooltip.TooltipNameMap.Count == 0) LoadNameMap(itm.Dom);
      if (itm.Id == 0) return new XElement("div", "Not Found");

      XElement tooltip = new XElement("div", new XAttribute("class", "torctip_wrapper"));

      if (itm != null) {
        String name = "";

        if (itm.LocalizedName != null) itm.LocalizedName.TryGetValue(Tooltip.Language, out name);
        XElement inner =
          new XElement(
            "div",
            XClass("torctip_tooltip torctip_area"),
            new XElement("H2", XClass("torctip_white torctip_2l_elipsis"), name)
          );
        tooltip.Add(inner);
      }

      return tooltip;
    }
    #endregion
    #region Codex
    public static XElement GetHTML(this Codex itm) {
      if (Tooltip.TooltipNameMap.Count == 0) LoadNameMap(itm.Dom_);
      if (itm.Id == 0) return new XElement("div", "Not Found");

      String stringQual = "codex";
      XElement tooltip = new XElement("div", new XAttribute("class", "torctip_wrapper"));

      if (itm != null) {
        tooltip.Add(new XElement("div", XClass("torctip_image"), " "));
        XElement inner =
          new XElement(
            "div",
            XClass("torctip_tooltip"),
            new XElement(
              "span",
              XClass(String.Format("torctip_{0}", stringQual)),
              itm.LocalizedName[Tooltip.Language]
            )
          );

        inner.Add(
          new XElement(
            "div",
            XClass("torctip_cdx_image"),
            new XElement(
              "img",
              new XAttribute(
                "src",
                String.Format("https://torcommunity.com/db/codex/{0}_thumb.jpg", itm.Icon)
              ),
              new XAttribute("alt", "")
            )
          )
        );

        XElement desc = new XElement("div", XClass("torctip_codex_text")/*, itm.Description*/);
        AddStringWithBreaks(ref desc, itm.LocalizedDescription[Tooltip.Language]);
        inner.Add(desc);
        tooltip.Add(inner);
      }

      return tooltip;
    }
    #endregion
    #region Collections
    public static XElement GetHTML(this Collection itm) {
      if (Tooltip.TooltipNameMap.Count == 0) LoadNameMap(itm.Dom);
      if (itm.Id == 0) return new XElement("div", "Not Found");

      String icon = "none";

      if (!String.IsNullOrEmpty(itm.Icon)) icon = itm.Icon.ToLower();
      if (!itm.Dom.Assets.HasFile(
        String.Format("/resources/gfx/mtxstore/{0}_260x260.dds", icon)
      )) icon = "titles_sticker";

      XElement tooltip =
        new XElement(
          "div",
          new XAttribute(
            "class",
            "torctip_wrapper"
          )
        );
      //var fileId = TorArchive.FileId.FromFilePath(String.Format("/resources/gfx/portraits/{0}.dds", icon));

      if (itm != null) {
        itm.LocalizedName = Normalize.Dictionary(itm.LocalizedName, itm.Icon);
        XElement inner =
          new XElement(
            "div",
            XClass("torctip_tooltip torctip_collection"),
            new XAttribute(
              "style",
              String.Format(
                "background-image:url(https://www.torcommunity.com/db/mtxstore/{0}_260x260.jpg);",
                icon)
            ),
            new XElement(
              "div",
              XClass("torctip_header"),
              new XElement(
                "h2",
                XClass("torctip_white torctip_2l_elipsis"),
                itm.LocalizedName[Tooltip.Language]
              ),
              new XElement(
                "div",
                XClass("torctip_image"),
                String.Empty
              )
            )
          );
        XElement torc_relative =
          new XElement(
            "div",
            XClass("torctip_rela")
          );

        if (itm.RequiredLevel > 0) {
          torc_relative.Add(
            new XElement(
              "div",
              XClass("torctip_col_req"),
              new XElement(
                "span",
                XClass("torc_req_parent"),
                itm.RequiredLevel,
                new XElement(
                  "div",
                  XClass("torc_hover"),
                  new XElement(
                    "div",
                    XClass("torc_col_req_wrapper"),
                    new XElement(
                      "div",
                      XClass("torctip_req_head"),
                      String.Format(GetLocalizedText(836131348283393), itm.RequiredLevel)
                    )
                  )
                )
              )
            )
          );
        }

        Boolean female_only = false;

        foreach (String bullet in itm.BulletPoints) {
          String bulletText = bullet;

          if (bullet == "This armor set requires a female character.") female_only = true;
        }

        if (female_only == true) {
          XElement torc_female =
            new XElement(
              "div",
              XClass("torctip_col_req_gender")
            );
          torc_female.Add(
            new XElement(
              "span",
              XClass("torc_req_parent"),
              new XElement(
                "span",
                XClass("icon_female"),
                String.Empty
              ),
              new XElement(
                "div",
                XClass("torc_hover"),
                new XElement(
                  "div",
                  XClass("torc_col_req_wrapper"),
                  new XElement(
                    "div",
                    XClass("torctip_req_head"),
                    GetLocalizedText(836131348283502)
                  )
                )
              )
            )
          );
          torc_relative.Add(torc_female);
        }

        if (itm.ItemIdsList.Count > 0) {
          XElement torc_req_block = new XElement("div", XClass("torctip_col_req_itms"));
          XElement torc_req_anchor =
            new XElement(
              "span",
              XClass("torc_req_parent"),
              String.Format("0/{0}", itm.ItemIdsList.Count)
            );
          XElement torc_req_sub =
            new XElement("div", XClass("torc_hover"), String.Empty);
          XElement torc_req_itm_wrapper =
            new XElement(
              "div",
              XClass("torc_col_req_wrapper"),
              new XElement(
                "div",
                XClass("torctip_req_head"),
                String.Format(
                  "{0} 0/{1}",
                  itm.LocalizedName[Tooltip.Language],
                  itm.ItemIdsList.Count
                )
              )
            );

          foreach (Item item in itm.ItemList) {
            torc_req_itm_wrapper.Add(
              new XElement(
                "div",
                XClass("torctip_col_req_itm"),
                new XElement(
                  "a",
                  XClass(String.Format("torctip_{0}", item.Quality.ToString().ToLower())),
                  new XAttribute(
                    "href",
                    String.Format(
                      "https://torcommunity.com{2}/database/item/{0}/{1}/",
                      item.Base62Id,
                      item.LocalizedName[Tooltip.Language].LinkString(),
                      Tooltip.LinkLocal
                    )
                  ),
                  new XAttribute("data-torc", "norestyle"),
                  new XElement(
                    "div",
                    XClass(
                      String.Format(
                        "torctip_image torctip_image_{0} small_border",
                        item.Quality.ToString().ToLower()
                      )
                    ),
                    new XElement(
                      "img",
                      new XAttribute(
                        "src",
                        String.Format(
                          "https://torcommunity.com/db/icons/{0}.jpg",
                          item.HashedIcon
                        )
                      ),
                      new XAttribute("alt", String.Empty), XClass("small_image")
                    )
                  ),
                  new XElement(
                    "span",
                    XClass("torctip_name"),
                    item.LocalizedName[Tooltip.Language]
                  )
                )
              )
            );
          }

          torc_req_sub.Add(torc_req_itm_wrapper);
          torc_req_anchor.Add(torc_req_sub);
          torc_req_block.Add(torc_req_anchor);
          torc_relative.Add(torc_req_block);
        }

        inner.Add(torc_relative);
        tooltip.Add(inner);
      }

      return tooltip;
    }
    #endregion
    #region Effect
    public static XElement GetHTML(this Effect itm) {
      if (Tooltip.TooltipNameMap.Count == 0) LoadNameMap(itm.Dom_);
      if (itm.Id == 0) return new XElement("div", "Not Found");

      String stringQual = "effect";
      String icon = itm.Icon;

      if (itm.Icon == null) {
        if (itm.Ability != null) // Test for empty item
          icon = itm.Ability.Icon;
      }

      XElement tooltip =
        new XElement("div", new XAttribute("class", "torctip_wrapper"));
      TorArchive.FileId fileId =
        TorArchive.FileId.FromFilePath(String.Format("/resources/gfx/icons/{0}.dds", icon));

      if (itm != null) {
        XElement tooltip_header =
          new XElement("div", new XAttribute("class", "torctip_header"));

        XElement imgelement =
          new XElement("div", XClass("torctip_image_wrapper"), String.Empty);

        imgelement.Add(
          new XElement(
            "div",
            XClass(String.Format("torctip_image torctip_image_{0}", stringQual)),
            new XElement(
              "img",
              new XAttribute(
                "src",
                String.Format(
                  "https://torcommunity.com/db/icons/{0}_{1}.jpg",
                  fileId.Ph,
                  fileId.Sh
                )
              ),
              new XAttribute("alt", String.Empty)
            )
          )
        );

        XElement tooltip_header_text =
          new XElement(
            "div",
            new XAttribute(
              "class",
              "torctip_header_text"
            ),
            new XElement("span", itm.Name/*, itm.LocalizedName[Tooltip.language]*/)
          );
        tooltip_header.Add(imgelement, tooltip_header_text);
        tooltip.Add(tooltip_header);
        XElement inner = new XElement("div", XClass("torctip_tooltip"));
        XElement stats = new XElement("div", XClass("torctip_section"), String.Empty);

        // Interval
        if (itm.Interval > 0)
          stats.Add(XStat("Interval: ", String.Format("{0}s", itm.Interval)));

        if (itm.Passive) {
          stats.Add(
            new XElement(
              "span",
              XClass("torctip_white"),
              GetLocalizedText(836131348283424) // "Passive"
            )
          );
        }

        if (itm.IsInstant)
          stats.Add(
            new XElement(
              "span",
              XClass("torctip_white"),
              GetLocalizedText(836131348283428) // "Instant"
            )
          );

        if (itm.IsDebuff)
          stats.Add(
            new XElement(
              "span",
              XClass("torctip_white"),
              GetLocalizedText(962682559820424) // "Debuff"
            )
          );


        if (itm.GCD > 0)
          stats.Add(XStat("GCD", String.Format("{0}ms", itm.GCD.ToString())));

        if (stats.HasElements)
          inner.Add(stats);

        if (itm.LocalizedDescription != null) {
          inner.Add(
            new XElement(
              "div",
              XClass("torctip_section"),
              new XElement(
                "div",
                XClass("torctip_white"),
                itm.LocalizedDescription[Tooltip.Language]
              )
            )
          );
        }

        tooltip.Add(inner);
      }

      return tooltip;
    }
    #endregion
    public static XElement GetHTML(this Item itm) { // Behold LINQ!
      if (Tooltip.TooltipNameMap.Count == 0) LoadNameMap(itm.Dom_);
      if (itm.Id == 0) return new XElement("div", "Not Found");

      XElement tooltip = new XElement("div", new XAttribute("class", "torctip_wrapper"));

      if (itm != null) tooltip.Add(itm.ItemHeaderHTML(), itm.ItemInnerHTML());

      return tooltip;
    }
    #region MTXStorefront
    public static XElement GetHTML(this MtxStorefrontEntry itm) {
      if (Tooltip.TooltipNameMap.Count == 0) LoadNameMap(itm.Dom);
      if (itm.Id == 0) return new XElement("div", "Not Found");

      String icon = !String.IsNullOrEmpty(itm.Icon) ? itm.Icon.ToLower() : "none";
      XElement tooltip = new XElement("div", new XAttribute("class", "torctip_wrapper"));
      // TorArchive.FileId fileId = TorArchive.FileId.FromFilePath(String.Format("/resources/gfx/portraits/{0}.dds", icon));

      if (itm != null) {
        String name = "";

        if (itm.LocalizedName != null) itm.LocalizedName.TryGetValue(Tooltip.Language, out name);

        XElement inner =
          new XElement(
            "div",
            XClass("torctip_tooltip torctip_collection"),
            new XAttribute(
              "style",
              String.Format(
                "background-image:url(https://www.torcommunity.com/db/mtxstore/{0}_260x260.jpg);",
                icon
              )
            ),
            new XElement(
              "H2",
              XClass("torctip_white torctip_2l_elipsis"),
              name
            )
          );
        tooltip.Add(inner);
      }

      return tooltip;
    }
    #endregion
    #region Companion
    public static XElement GetHTML(this NewCompanion itm) {
      if (Tooltip.TooltipNameMap.Count == 0) LoadNameMap(itm.Dom_);
      if (itm.Id == 0) return new XElement("div", "Not Found");

      String icon = !String.IsNullOrEmpty(itm.Icon) ? itm.Icon : "none";
      String stringQual = "none";

      if (itm.Companion != null) {
        if (itm.Companion.Npc.DetFaction != null) {
          if (itm.Companion.Npc.DetFaction.LocalizedName != null)
            stringQual = itm.Companion.Npc.DetFaction.LocalizedName[GomLib.StringTable.SelectedLocalization].ToLower();
          else
            stringQual = itm.Companion.Npc.DetFaction.FactionString.ToLower();
        }
      }

      XElement tooltip =
        new XElement("div", new XAttribute("class", "torctip_wrapper"));
      TorArchive.FileId fileId =
        TorArchive.FileId.FromFilePath(String.Format("/resources/gfx/portraits/{0}.dds", icon));

      tooltip.Add(
        new XElement(
          "div",
          new XAttribute("class", String.Format("torctip_image torctip_image_{0}", stringQual)),
          new XElement(
            "img",
            new XAttribute(
              "src",
              String.Format(
                "https://torcommunity.com/db/portraits/{0}_{1}_thumb.png",
                fileId.Ph,
                fileId.Sh
              )
            ),
            new XAttribute("alt", "")
          )
        )
      );

      if (itm != null) {
        String toughness = "standard";

        if (itm.Companion != null)
          if (itm.Companion.Npc.LocalizedToughness != null)
            toughness = itm.Companion.Npc.LocalizedToughness["enMale"].Replace(" ", "_").ToLower();
          else
            toughness = "";

        XElement inner =
          new XElement(
            "div",
            XClass("torctip_tooltip"),
            new XElement(
              "span",
              XClass(String.Format("torctip_npc_{0}", stringQual)),
              itm.LocalizedName[Tooltip.Language],
              new XElement(
                "span",
                XClass(String.Format("torctip_toughness_{0}", toughness)),
                " "
              )
            )
          );

        if (itm.LocalizedTitle != null)
          inner.Add(
            new XElement("div", XClass("torctip_npc_title"), itm.LocalizedTitle[Tooltip.Language])
          );

        if (itm.Companion != null)
          inner.Add( // new XElement("br"),
            new XElement(
              "div",
              XClass("torctip_white"),
              String.Format(
                GetLocalizedText(848771437035837), //"Level {0} {1}"
                itm.Companion.Npc.MinLevel == itm.Companion.Npc.MaxLevel
                  ? itm.Companion.Npc.MinLevel.ToString()
                  : String.Join("-", itm.Companion.Npc.MinLevel, itm.Companion.Npc.MaxLevel),
                itm.Companion.Npc.ClassSpec.LocalizedName != null
                  ? itm.Companion.Npc.ClassSpec.LocalizedName[Tooltip.Language]
                  : "Unknown"
              )
            )
          );

        tooltip.Add(inner);
      }

      return tooltip;
    }
    #endregion
    #region NPC
    public static XElement GetHTML(this Npc itm) {
      if (Tooltip.TooltipNameMap.Count == 0) LoadNameMap(itm.Dom_);
      if (itm.Id == 0) return new XElement("div", "Not Found");

      // String icon = "none";
      String stringQual = "none";

      if (itm.DetFaction != null) {
        if (itm.DetFaction.LocalizedName != null)
          stringQual = itm.DetFaction.LocalizedName[GomLib.StringTable.SelectedLocalization].ToLower();
        else
          stringQual = itm.DetFaction.FactionString.ToLower();
      }

      XElement tooltip = new XElement("div", new XAttribute("class", "torctip_wrapper"));
      // _ = TorArchive.FileId.FromFilePath(String.Format("/resources/gfx/icons/{0}.dds", icon));

      if (itm != null) {
        XElement inner =
          new XElement(
            "div",
            XClass("torctip_tooltip"),
            new XElement(
              "span",
              XClass(String.Format("torctip_npc_{0}", stringQual)),
              itm.LocalizedName[Tooltip.Language],
              new XElement(
                "span",
                XClass(
                  String.Format(
                    "torctip_toughness_{0}",
                    (itm.LocalizedToughness != null
                      ? itm.LocalizedToughness["enMale"]
                      : ""
                    ).Replace(" ", "_").ToLower()
                  )
                ),
                " "
              )
            )
          );

        if (itm.LocalizedTitle != null)
          inner.Add(
            new XElement(
              "div",
              XClass("torctip_npc_title"),
              itm.LocalizedTitle[Tooltip.Language]
            )
          );

        inner.Add(//new XElement("br"),
          new XElement(
            "div",
            XClass("torctip_white"),
            String.Format(
              GetLocalizedText(848771437035837), // "Level {0} {1}"
              (itm.MinLevel == itm.MaxLevel)
                ? itm.MinLevel.ToString()
                : String.Join("-", itm.MinLevel, itm.MaxLevel),
              (itm.ClassSpec.LocalizedName != null)
                ? itm.ClassSpec.LocalizedName[Tooltip.Language]
                : "Unknown"
            )
          )
        );

        tooltip.Add(inner);
      }

      return tooltip;
    }
    #endregion
    public static XElement GetHTML(this Quest itm) {
      if (Tooltip.TooltipNameMap.Count == 0) LoadNameMap(itm.Dom_);
      if (itm.Id == 0) return new XElement("div", "Not Found");

      XElement tooltip = new XElement("div", new XAttribute("class", "torctip_wrapper"));

      if (itm != null) tooltip.Add(itm.MissionInnerHTML());

      return tooltip;
    }
    public static XElement GetHTML(this Schematic itm) {
      if (Tooltip.TooltipNameMap.Count == 0) LoadNameMap(itm.Dom_);
      if (itm.Id == 0) return new XElement("div", "Not Found");

      XElement tooltip = new XElement("div", new XAttribute("class", "torctip_wrapper"));

      if (itm != null) tooltip.Add(itm.SchematicHeaderHTML(), itm.SchematicInnerHTML());

      return tooltip;
    }
    public static XElement GetHTML(this SetBonusEntry itm) // Behold linq!
        {
      if (Tooltip.TooltipNameMap.Count == 0) LoadNameMap(itm.Dom);
      if (itm.Id == 0) return new XElement("div", "Not Found");

      XElement tooltip = new XElement("div", new XAttribute("class", "torctip_wrapper setbonus"));

      if (itm != null) tooltip.Add(itm.SetBonusHeaderHTML(), itm.SetBonusInnerHTML());

      return tooltip;
    }
    #region Talents
    public static XElement GetHTML(this Talent itm) {
      if (Tooltip.TooltipNameMap.Count == 0) LoadNameMap(itm.Dom_);
      if (itm.Id == 0) return new XElement("div", "Not Found");

      String stringQual = "talent";
      String icon = itm.Icon;

      XElement tooltip = new XElement("div", new XAttribute("class", "torctip_wrapper"));
      TorArchive.FileId fileId =
        TorArchive.FileId.FromFilePath(String.Format("/resources/gfx/icons/{0}.dds", icon));

      if (itm != null) {
        tooltip.Add(
          new XElement(
            "div",
            new XAttribute(
              "class",
              String.Format("torctip_image torctip_image_{0}", stringQual)
            ),
            new XElement(
              "img",
              new XAttribute(
                "src",
                String.Format(
                  "https://torcommunity.com/db/icons/{0}_{1}.jpg",
                  fileId.Ph,
                  fileId.Sh
                )
              ),
              new XAttribute("alt", "")
            )
          ) //,
            // new XElement(
            //   "div",
            //   new XAttribute("class", "torctip_name"),
            //   new XElement(
            //     "a",
            //     new XAttribute(
            //       "href", 
            //       String.Format(
            //         "https://torcommunity.com{2}/database/talent/{0}/{1}/", 
            //         itm.Base62Id, 
            //         itm.LocalizedName[Tooltip.language].LinkString(), 
            //         Tooltip.linkLocal
            //       )
            //     ),
            //     new XAttribute("data-torc", "norestyle"),
            //     itm.LocalizedName[Tooltip.language]
            //   )
            // )
        );
        XElement inner =
          new XElement(
            "div",
            XClass("torctip_tooltip"),
            new XElement(
              "span",
              XClass(String.Format("torctip_{0}", stringQual)),
              itm.Name
            ),
            new XElement(
              "div",
              XClass("torctip_white"),
              GetLocalizedText(836131348283424) //"Passive"
            ),
            new XElement("br"),
            new XElement("div",
            XClass("torctip_white"),
            Talent.ParseDescription(itm, itm.LocalizedDescription[Tooltip.Language]))
          );


        inner.Add();
        tooltip.Add(inner);
      }

      return tooltip;
    }
    #endregion
    public static String GetLocalizedText(Int64 id, String language, String defaultVal) {
      String returnVal = GetLocalizedText(id, language);
      return returnVal ?? defaultVal;
    }
    public static String GetLocalizedText(Int64 id, String language) {
      if (Tooltip.TooltipNameMap.ContainsKey(id)) return Tooltip.TooltipNameMap[id][language];
      else return null;
    }
    private static String GetLocalizedText(Int64 id) {
      return GetLocalizedText(id, Tooltip.Language);
    }
    public static XElement ItemHeaderHTML(this Item itm) {
      XElement imgelement = new XElement("div", XClass("torctip_image_wrapper"), String.Empty);

      TorArchive.FileId fileId =
        TorArchive.FileId.FromFilePath(String.Format("/resources/gfx/icons/{0}.dds", itm.Icon));

      // String stringQual = ((itm.TypeBitFlags.IsModdable && (itm.Quality == ItemQuality.Prototype)) ? "moddable" : itm.Quality.ToString().ToLower());
      String stringQual = itm.Quality.ToString().ToLower();
      String cartelMarket = itm.TypeBitFlags.IsMtxItem ? " torctip_image_mtx" : "";
      String cartelMarketRarity =
        itm.MTXRarity != null ? String.Format(" torctip_mtx_{0}", itm.MTXRarity.ToLower()) : "";

      if (itm.AppearanceImperial == itm.AppearanceRepublic) {
        imgelement.Add(
          new XElement(
            "div",
            XClass(
              String.Format(
                "torctip_image torctip_image_{0}{1}{2}",
                stringQual,
                cartelMarket,
                cartelMarketRarity
              )
            ),
            new XElement(
              "img",
              new XAttribute(
                "src",
                String.Format(
                  "https://torcommunity.com/db/icons/{0}_{1}.jpg",
                  fileId.Ph,
                  fileId.Sh
                )
              ),
              new XAttribute("alt", "")
            )
          )
        );
      } else {
        TorArchive.FileId repfileId =
          TorArchive.FileId.FromFilePath(
            String.Format("/resources/gfx/icons/{0}.dds", itm.RepublicIcon)
          );
        TorArchive.FileId impfileId =
          TorArchive.FileId.FromFilePath(
            String.Format("/resources/gfx/icons/{0}.dds", itm.ImperialIcon)
          );
        imgelement.Add(
          new XElement(
            "div",
            XClass("torctip_image_faction"),
            new XElement(
              "span",
              XClass("torctip_app_faction_imp torctip_lc"),
              GetLocalizedText(1173582633762817, Tooltip.Language)
            ),
            new XElement(
              "div",
              XClass(String.Format("torctip_image torctip_image_{0} torctip_lc", stringQual)),
              new XElement(
                "img",
                new XAttribute(
                  "src",
                  String.Format(
                    "https://torcommunity.com/db/icons/{0}_{1}.jpg",
                    impfileId.Ph,
                    impfileId.Sh
                  )
                ),
                new XAttribute("alt", "")
              )
            )
          ),
          new XElement(
            "div",
            XClass("torctip_image_faction"),
            new XElement(
              "span",
              XClass("torctip_app_faction_rep torctip_lc"),
              GetLocalizedText(1173582633762818, Tooltip.Language)
            ),
            new XElement(
              "div",
              XClass(String.Format("torctip_image torctip_image_{0} torctip_lc", stringQual)),
              new XElement(
                "img",
                new XAttribute(
                  "src",
                  String.Format(
                    "https://torcommunity.com/db/icons/{0}_{1}.jpg",
                    repfileId.Ph,
                    repfileId.Sh
                  )
                ),
                new XAttribute("alt", "")
              )
            )
          )
        );
      }

      XElement tooltip_header =
        new XElement(
          "div",
          XClass(String.Format("torctip_header torctip_header_{0}", stringQual)),
          imgelement,
          new XElement(
            "div",
            new XAttribute("class", "torctip_header_text"),
            new XElement(
              "span",
              // XClass(String.Format("torctip_{0}", stringQual)),
              itm.LocalizedName[Tooltip.Language]
            ),
            new XElement(
              "span",
              XClass("torctip_header_rating"),
              !itm.TypeBitFlags.IsRepTrophy && itm.CombinedRating != 0
                ? String.Format("{0} {1}", GetLocalizedText(836131348284091), itm.CombinedRating)
                : "" // "Rating {0}
            )
          )
        );

      return tooltip_header;
    }
    public static XElement ItemInnerHTML(this Item itm) {
      // Create Wrapper
      XElement tooltip = new XElement("div", XClass("torctip_tooltip"), String.Empty);

      // Section Variables
      Boolean has0 = false;
      Boolean has1 = false;
      Boolean has2 = false;
      Boolean has3 = false;
      Boolean has4 = false;
      Boolean has5 = false;
      Boolean has6 = false;
      Boolean has7 = false;
      Boolean has8 = false;
      Boolean has9 = false;

      // Section 0: Cooldown
      XElement tooltip_section0 = new XElement("div", XClass("torctip_section"), String.Empty);

      // Add Section 0 to Tooltip
      if (has0) tooltip.Add(tooltip_section0);

      // Section 1: Binding, Durability, Slot, Category
      XElement tooltip_section1 =
        new XElement("div", XClass("torctip_section torctip_sidebyside"), String.Empty);

      // Create Left and Right Side
      XElement tooltip_left =
        new XElement("div", XClass("torctip_left"), String.Empty);
      XElement tooltip_right =
        new XElement("div", XClass("torctip_right"), String.Empty);

      // Binding
      if (itm.Binding != 0) {
        has1 = true;
        String binding = itm.Binding.ToString(); // String.Format("Binds on {0}", itm.Binding.ToString());

        switch (itm.Binding.ToString()) {
          case "Equip":
            binding = GetLocalizedText(946314439294988);
            break;
          case "Pickup":
            binding = GetLocalizedText(946314439294989);
            break;
          case "Legacy":
            binding = GetLocalizedText(946314439295234);
            binding =
              binding.Replace(
                "&lt;img src=\'img://gfx/symbols/legacy_bound.dds\' width=\'10\' height=\'10\'/&gt;",
                ""
              );
            binding =
              binding.Replace(
                "<img src=\'img://gfx/symbols/legacy_bound.dds\' width=\'10\' height=\'10\'/>",
                ""
              );
            break;
          case "LegacyOnEquip":
            binding = GetLocalizedText(946314439295251);
            break;
          case "Use":
            binding = GetLocalizedText(946314439295248);
            break;
          default:
            break;
        }

        // Regular Binding
        if (itm.Binding.ToString() == "Legacy" || itm.Binding.ToString() == "LegacyOnEquip") {
          tooltip_left.Add(
            new XElement(
              "div",
              new XElement("span", XClass("bindstolegacy"), String.Empty),
              binding
            )
          );
        } else {
          tooltip_left.Add(new XElement("div", binding));
        }

        // Binds to Slot
        if (itm.BindsToSlot) {
          tooltip_left.Add(new XElement("div", "Binds to Slot"));
        }
      }

      // Reputation Trophy
      if (itm.TypeBitFlags.IsRepTrophy && itm.LocalizedRepFactionDictionary.Count != 0) {
        has1 = true;
        String repName = "";

        if (itm.LocalizedRepFactionDictionary["Imperial"][Tooltip.Language] !=
            itm.LocalizedRepFactionDictionary["Republic"][Tooltip.Language]) {
          repName =
            String.Format(
              "{0} / {1}",
              itm.LocalizedRepFactionDictionary["Imperial"][Tooltip.Language],
              itm.LocalizedRepFactionDictionary["Republic"][Tooltip.Language]
            );
        } else {
          repName = itm.LocalizedRepFactionDictionary["Imperial"][Tooltip.Language];
        }
        tooltip_left.Add(
            new XElement("div", XClass("torctip_rep"), GetLocalizedText(836131348283741)), // "Reputation Trophy"
            new XElement("div", XClass("torctip_rep"), repName)
        );
      }

      // Companion Gift
      if (itm.TypeBitFlags.IsGift) {
        has1 = true;

        if (itm.GiftType != GiftType.None) {
          tooltip_left.Add(
            new XElement(
              "div",
              XClass("torctip_compgift"),
              GetLocalizedText(836131348283395 + (Int32)itm.GiftType) // Gift Type (Varies)
            ),
            new XElement(
              "div",
              XClass("torctip_compgift"),
              String.Format(
                "{0} {1}",
                GetLocalizedText(836131348283411),
                itm.GiftRankNum
              ) // "Rank {0}"
            ),
            new XElement(
              "div",
              XClass("torctip_compgift"),
              "{Companion}'s Reaction: {0} Influence Gain"
            )
          );
        }
      }

      // Unique
      if (itm.TypeBitFlags.HasUniqueLimit) {
        has1 = true;
        tooltip_left.Add(
            new XElement("div", GetLocalizedText(836131348283436) /* "Unique" */)
        );
      }

      // Dyes
      if (itm.TypeBitFlags.IsMod && itm.DyeId != 0) {
        has1 = true;
        String name = "";

        if (itm.DyeColor.LocalizedColorName != null)
          name = itm.DyeColor.LocalizedColorName[Tooltip.Language];

        String blks = String.Format("{0} {{0}} ({1})", name, GetLocalizedText(836131348283461));

        switch (itm.EnhancementType) {
          case EnhancementType.Dye:
            blks = String.Format(blks, GetLocalizedText(1173453784744196));
            break;
          case EnhancementType.ColorCrystal:
            blks = String.Format(blks, GetLocalizedText(1173453784743941));
            break;
        }

        tooltip_left.Add(
          new XElement("div", blks),
          new XElement(
            "div",
            new XElement(
              "span",
              XClass("torctip_val"),
              GetLocalizedText(836131348284082) // "Primary"
            ),
            GetDyeBlock(itm.DyeColor.Palette1Rep)
          ),
          new XElement(
            "div",
            new XElement(
              "span",
              XClass("torctip_val"),
              GetLocalizedText(836131348284083) // "Secondary"
            ),
            GetDyeBlock(itm.DyeColor.Palette2Rep)
          )
        );
      }

      // Slot
      Boolean isEquipable = false;
      if (itm.Slots.Count > 1) // the Any slot was removed from the item by the itemloader
      {
        has1 = true;
        List<String> slot_list =
          itm.Slots.Select(
            x => x.ConvertToString(Tooltip.Language)
          ).Where(x => x != null).ToList();

        foreach (var slot in slot_list) {
          tooltip_left.Add(new XElement("div", slot));
        }

        //tooltip_left.Add(
        //    new XElement(
        //        "div",
        //        String.Join("\n", itm.Slots.Select(x => x.ConvertToString(Tooltip.language)).Where(x => x != null).ToList())
        //    )
        //);

        isEquipable = true;
      }

      // Durability
      if (itm.Durability > 0) {
        has1 = true;
        tooltip_right.Add(
          new XElement(
              "div",
              String.Format("100% {0}", GetLocalizedText(836131348283458)) // 100% "Durability:"
          )
        );
        tooltip_right.Add(
          new XElement(
            "div",
            String.Format(
              "{0}/{1}",
              itm.Durability,
              itm.MaxDurability > 0
                ? itm.MaxDurability
                : itm.Durability
            ) // "{0}/{0}"
          )
        );
      }

      // Add left and right sides to main section
      tooltip_section1.Add(tooltip_left, tooltip_right);

      // Add Section 1 to Tooltip
      if (itm.SchematicId == 0 && has1 == true) tooltip.Add(tooltip_section1);

      // Section 2: Key Stats, Total Stats

      XElement tooltip_section2 =
        new XElement(
          "div",
          XClass("torctip_section"),
          String.Empty
        );

      // Key Stats
      Single techpower = 0;
      Single forcepower = 0;
      Single absorbchance = 0;
      Single shieldchance = 0;
      Int32 level = itm.ItemLevel;

      // Weapon
      if (itm.WeaponSpec != null) {
        has2 = true;
        XElement key_stats =
          new XElement(
            "div",
            XClass("torctip_stats"),
            new XElement(
              "span",
              XClass("torctip_beige"),
              GetLocalizedText(836131348284151) // "Key Stats"
            )
          );

        List<Int32> mainSlots = new List<Int32> { 1, 3, 9 };
        ItemEnhancement mainMod = null;

        if (itm.EnhancementSlots != null) {
          IEnumerable<ItemEnhancement> potentials =
            itm.EnhancementSlots.Where(x => x.Slot.IsBaseMod());

          if (potentials.Any())
            mainMod = itm.EnhancementSlots.Where(x => x.Slot.IsBaseMod()).Single();
        }
        ItemQuality qual = ItemQuality.Premium;

        if (itm.EnhancementSlots != null && itm.EnhancementSlots.Count != 0) {
          if (mainMod != null) {
            if (mainMod.ModificationId != 0) {
              level = mainMod.Modification.ItemLevel;
              qual = mainMod.Modification.Quality;
            } else {
              level = 1;
            }
          }
        } else {
          level = itm.ItemLevel;
          qual = itm.Quality;
        }

        Single min = 0f;
        Single max = 0f;

        try {
          min = itm.Dom_.Data.weaponPerLevel.GetStat(
            itm.WeaponSpec.Id,
            level,
            qual,
            Stat.MinWeaponDamage
          );
          max = itm.Dom_.Data.weaponPerLevel.GetStat(
            itm.WeaponSpec.Id,
            level,
            qual,
            Stat.MaxWeaponDamage
          ); // Change this so items without barrels use level 1 premium numbers
          techpower = itm.Dom_.Data.weaponPerLevel.GetStat(
            itm.WeaponSpec.Id,
            level,
            qual,
            Stat.TechPowerRating
          );
          forcepower = itm.Dom_.Data.weaponPerLevel.GetStat(
            itm.WeaponSpec.Id,
            level,
            qual,
            Stat.ForcePowerRating
          );
        }
        catch (Exception) {
          // String dosomething = ""; // Suppress for now, break here to debug
        }

        String dType = itm.WeaponSpec.DamageType;

        switch (dType) {
          case "Kinetic":
            dType = GetLocalizedText(946314439294990);
            break;
          case "Energy":
            dType = GetLocalizedText(946314439294991);
            break;
          case "Elemental":
            dType = GetLocalizedText(946314439294992);
            break;
          case "Internal":
            dType = GetLocalizedText(946314439294993);
            break;
          default:
            break;
        }

        key_stats.Add(
          new XElement(
            "div",
            XClass("torctip_stat torctip_beige"),
            new XElement(
              "span",
              XClass("torctip_minDam"),
              String.Format("{0:N0}", min)
            ),
            " - ",
            new XElement(
              "span",
              XClass("torctip_maxDam"),
              String.Format("{0:N0}", max)
            ),
            String.Format(" {0} {1}", dType, GetLocalizedText(836131348283440)) // " {0} Damage"
          )
        );
        tooltip_section2.Add(key_stats);
      }
      // Not a Weapon
      else if (isEquipable) {
        has2 = true;
        XElement key_stats =
          new XElement(
            "div",
            XClass("torctip_stats"),
            new XElement(
              "span",
              XClass("torctip_beige"),
              GetLocalizedText(836131348284151) // "Key Stats"
            )
          );

        // Shield
        ArmorSpec shield = itm.ShieldSpec;

        if (shield != null) {
          List<Int32> mainSlots = new List<Int32> { 1, 3, 9 };
          ItemEnhancement mainMod = null;

          if (itm.EnhancementSlots != null) {
            IEnumerable<ItemEnhancement> potentials =
              itm.EnhancementSlots.Where(x => x.Slot.IsBaseMod());

            if (potentials.Any())
              mainMod = itm.EnhancementSlots.Where(x => x.Slot.IsBaseMod()).Single();
          }

          ItemQuality qual = ItemQuality.Premium;

          if (itm.EnhancementSlots != null && itm.EnhancementSlots.Count != 0) {
            if (mainMod != null) {
              if (mainMod.ModificationId != 0) {
                level = mainMod.Modification.ItemLevel;
                qual = mainMod.Modification.Quality;
              }
            }
          } else {
            level = itm.ItemLevel;
            qual = itm.Quality;
          }
          try {
            techpower = itm.Dom_.Data.shieldPerLevel.GetShield(
              itm.ArmorSpec,
              qual,
              level,
              Stat.TechPowerRating
            );
            forcepower = itm.Dom_.Data.shieldPerLevel.GetShield(
              itm.ArmorSpec,
              qual,
              level,
              Stat.ForcePowerRating
            );
            absorbchance = itm.Dom_.Data.shieldPerLevel.GetShield(
              itm.ArmorSpec,
              qual,
              level,
              Stat.MeleeShieldAbsorb
            );
            shieldchance = itm.Dom_.Data.shieldPerLevel.GetShield(
              itm.ArmorSpec,
              qual,
              level,
              Stat.MeleeShieldChance
            );
          }
          catch (Exception) {
            // String dosomething = ""; //suppress for now, break here to debug
          }

          if (absorbchance > 0) {
            key_stats.Add(
              new XElement(
                "div",
                XClass("torctip_stat torctip_beige"),
                String.Format(
                  GetLocalizedText(836131348283463),
                  (absorbchance * 100).ToString("n1")
                ) // "Shield Absorb: {0}%"
              )
            );
          }

          if (shieldchance > 0) {
            key_stats.Add(
              new XElement(
                "div",
                XClass("torctip_stat torctip_beige"),
                String.Format(
                  GetLocalizedText(836131348283464),
                  (shieldchance * 100).ToString("n1")
                ) // "Shield Chance: {0}%"
              )
            );
          }
        }

        // Armor
        ArmorSpec arm = itm.ArmorSpec;

        if (arm != null) {
          IEnumerable<ItemEnhancement> temp;

          if (itm.EnhancementSlots != null) {
            temp = itm.EnhancementSlots.Where(x => x.Slot == EnhancementType.Harness);
          } else {
            temp = new ItemEnhancementList();
          }

          if (temp.Any()) {
            if (temp.First().ModificationId != 0) {
              level = temp.First().Modification.ItemLevel;
            }
          }

          try {
            ItemQuality qual = itm.Quality;

            if (qual == ItemQuality.Moddable) {
              qual = ItemQuality.Prototype;
            }

            Int32 armor =
              itm.Dom_.Data.armorPerLevel.GetArmor(
                arm,
                level,
                qual,
                itm.Slots.Where(x => x != SlotType.Any).First()
              );

            if (armor > 0) {
              key_stats.Add(
                new XElement(
                  "div",
                  XClass("torctip_stat torctip_beige"),
                  String.Format(
                    " {0} {1}",
                    armor,
                    GetLocalizedText(836131348283506)
                  ) // "{0} Armor"
                )
              );
            }
          }
          catch (Exception) {
            // String sdfkljn = "";
          }
        }

        tooltip_section2.Add(key_stats);
      }

      // Total Stats
      if ((itm.CombinedStatModifiers != null && itm.CombinedStatModifiers.Count != 0)
          || itm.WeaponSpec != null) {
        has2 = true;
        XElement stats =
          new XElement(
            "div",
            XClass("torctip_stats"),
            new XElement(
              "span",
              XClass("torctip_white"),
              GetLocalizedText(836131348283465) // "Total Stats"
            )
          );

        // if (!isEquipable) {
        //   switch (itm.EnhancementType) {
        //     case EnhancementType.Harness:
        //       stats.Add(
        //         new XElement(
        //           "div",
        //           XClass("torctip_stat"),
        //           String.Format(
        //             "{0} {1}", 
        //             GetLocalizedText(836131348283474), 
        //             itm.CombinedRating
        //           ) // "Armor Rating {0}"
        //         )
        //       );
        //       break;
        //     case EnhancementType.Barrel:
        //     case EnhancementType.Hilt:
        //     case EnhancementType.PowerCrystal:
        //       stats.Add(
        //         new XElement(
        //           "div",
        //           XClass("torctip_stat"),
        //           String.Format(
        //             "{0} {1}", 
        //             GetLocalizedText(836131348283475), 
        //             itm.CombinedRating
        //           ) //"Weapon Damage/Item Rating {0}"
        //         )
        //       );
        //       break;
        //     default:
        //       stats.Add(
        //         new XElement(
        //           "div",
        //           XClass("torctip_stat"),
        //           String.Format(
        //             "{0} {1}", 
        //             GetLocalizedText(836131348284091), 
        //             itm.CombinedRating
        //           ) //"Item Rating {0}"
        //         )
        //       );
        //       break;
        //    }
        // }

        if (itm.CombinedStatModifiers != null) {
          List<String> temp_modifiers = new List<String>();

          for (Int32 i = 0; i < itm.CombinedStatModifiers.Count; i++) {
            temp_modifiers.Add(
              itm.CombinedStatModifiers[i].DetailedStat
                .LocalizedDisplayName[Tooltip.Language]
            );
          }

          if (techpower > 0) {
            temp_modifiers.Add(
              itm.Dom_.StatData.ToStat("STAT_rtg_tech_power")
                .LocalizedDisplayName[Tooltip.Language]
            );
          }

          if (forcepower > 0) {
            temp_modifiers.Add(
              itm.Dom_.StatData.ToStat("STAT_rtg_force_power")
                .LocalizedDisplayName[Tooltip.Language]
            );
          }

          String[] combined_stat_modifiers = temp_modifiers.ToArray();
          Array.Sort(combined_stat_modifiers, new AttributeComparer());

          for (Int32 i = 0; i < combined_stat_modifiers.Length; i++) {
            String current_element = combined_stat_modifiers.ElementAt(i);

            for (Int32 j = 0; j < itm.CombinedStatModifiers.Count; j++) {
              if (current_element == itm.CombinedStatModifiers[j].DetailedStat
                .LocalizedDisplayName[Tooltip.Language]) {
                stats.Add(
                  new XElement(
                    "div",
                    XClass("torctip_stat"),
                    String.Format(
                      "+{0} {1}",
                      itm.CombinedStatModifiers[j].Modifier,
                      itm.CombinedStatModifiers[j].DetailedStat.
                        LocalizedDisplayName[Tooltip.Language]
                    )
                  )
                );
              }
            }

            if (current_element == itm.Dom_.StatData.ToStat("STAT_rtg_tech_power")
              .LocalizedDisplayName[Tooltip.Language]) {
              stats.Add(
                new XElement(
                  "div",
                  XClass("torctip_stat"),
                  String.Format(
                    "+{0} {1}",
                    techpower,
                    itm.Dom_.StatData.ToStat("STAT_rtg_tech_power")
                      .LocalizedDisplayName[Tooltip.Language]
                  )
                )
              );
            }

            if (current_element == itm.Dom_.StatData.ToStat("STAT_rtg_force_power")
              .LocalizedDisplayName[Tooltip.Language]) {
              stats.Add(
                new XElement(
                  "div",
                  XClass("torctip_stat"),
                  String.Format(
                    "+{0} {1}",
                    forcepower,
                    itm.Dom_.StatData.ToStat("STAT_rtg_force_power")
                      .LocalizedDisplayName[Tooltip.Language]
                  )
                )
              );
            }
          }

          // for (Int32 i = 0; i < itm.CombinedStatModifiers.Count; i++) {
          //   stats.Add(
          //     new XElement(
          //       "div",
          //       XClass("torctip_stat"),
          //       String.Format(
          //         "+{0} {1}", 
          //         itm.CombinedStatModifiers[i].Modifier, 
          //         itm.CombinedStatModifiers[i].DetailedStat.LocalizedDisplayName[Tooltip.language]
          //       )
          //     )
          //   );
          // }

        } else {
          if (techpower > 0) {
            stats.Add(
              new XElement(
                "div",
                XClass("torctip_stat"),
                String.Format(
                  "+{0} {1}",
                  techpower,
                  itm.Dom_.StatData.ToStat("STAT_rtg_tech_power")
                    .LocalizedDisplayName[Tooltip.Language]
                )
              )
            );
          }

          if (forcepower > 0) {
            stats.Add(
              new XElement(
                "div",
                XClass("torctip_stat"),
                String.Format(
                  "+{0} {1}",
                  forcepower,
                  itm.Dom_.StatData.ToStat("STAT_rtg_force_power")
                    .LocalizedDisplayName[Tooltip.Language]
                )
              )
            );
          }
        }

        tooltip_section2.Add(stats);
      }

      // Add Section 2 to Tooltip
      if (has2) tooltip.Add(tooltip_section2);

      // Section 3: Item Modifications

      XElement tooltip_section3 = new XElement("div", XClass("torctip_section"), String.Empty);

      // Modifications
      if (itm.EnhancementSlots != null && itm.EnhancementSlots.Count != 0) {
        has3 = true;
        XElement enhance =
          new XElement(
            "div",
            XClass("torctip_mods"),
            new XElement(
              "span",
              XClass("torctip_white"),
              GetLocalizedText(836131348283461) // "Item Modifications"
            )
          );

        Dictionary<String, XElement> enhancements = new Dictionary<String, XElement>();

        for (Int32 i = 0; i < itm.EnhancementSlots.Count; i++) {
          String enhName = "";

          if (itm.EnhancementSlots[i].DetailedSlot.LocalizedDisplayName != null) {
            enhName = itm.EnhancementSlots[i].DetailedSlot.LocalizedDisplayName[Tooltip.Language];
          } else if (itm.EnhancementSlots[i].Modification != null
                     && itm.EnhancementSlots[i].Modification.AuctionSubCategory != null) {
            enhName = itm.EnhancementSlots[i].Modification.AuctionSubCategory
                        .LocalizedName[Tooltip.Language];
          }

          if (String.IsNullOrWhiteSpace(enhName))
            enhName = itm.EnhancementSlots[i].Slot.ToString();

          enhancements.Add(enhName, itm.EnhancementSlots[i].ToHTML());
        }

        List<String> sortOrder = new List<String> {
          "Armoring",
          "Barrel",
          "Hilt",
          "Mod",
          "Enhancement",
          "Color Crystal",
          "Dye Module"
        };

        foreach (String key in sortOrder) {
          if (enhancements.ContainsKey(key)) {
            enhance.Add(enhancements[key]);
            enhancements.Remove(key);
          }
        }

        if (enhancements.Count > 0) { // Some new kind of slot?
          foreach (var kvp in enhancements) {
            enhance.Add(kvp.Value); // Append them for compatibility.
          }
        }

        String repString = GetLocalizedText(836131348283476); // "{0}: Open"
        /*enhance.Add(new XElement("div",
            XClass("torctip_mod"),
            new XElement("div",
                XClass("torctip_mslot"),
                String.Format(repString, GetLocalizedText(1173453784743948))) // "Augment: Open"
            ));*/
        tooltip_section3.Add(enhance);
      }

      // Add Section 3 to Tooltip
      if (has3) tooltip.Add(tooltip_section3);

      // Section 4: Armor Set, Set Bonus

      XElement tooltip_section4 =  new XElement("div", XClass("torctip_section"), String.Empty);

      // Armor Set and Set Bonus
      if (itm.SetBonusId != 0) {
        has4 = true;
        tooltip_section4.Add(new XElement("div", XClass("torctip_set"), ""));
        tooltip_section4.Add(itm.SetBonus.ToHTML());
      }

      // Add Section 4 to Tooltip
      if (has4) tooltip.Add(tooltip_section4);

      // Section 5: Decorations
      XElement tooltip_section5 = new XElement("div", XClass("torctip_section"), String.Empty);

      if (itm.TeachesType == "Decoration" && itm.Decoration != null) {
        has5 = true;
        tooltip_section5.Add(
          new XElement(
            "div",
            new XElement(
              "span",
              GetLocalizedText(836131348284096)
            ), // "Stronghold Decoration: "
            new XElement(
              "span",
              XClass("torctip_val"),
              String.Format(
                "{0} - {1}",
                itm.Decoration.CategoryName,
                itm.Decoration.SubCategoryName
              )
            )
          ),
          new XElement(
            "div",
            new XElement(
              "span",
              GetLocalizedText(836131348284097)
            ), // "Hook Type: "
            new XElement(
              "span",
              XClass("torctip_val"),
              String.Join(", ", itm.Decoration.AvailableHooks)
            )
          ),
          new XElement(
            "div",
            new XElement(
              "span",
              GetLocalizedText(836131348284098)
            ), // "You own: "
            new XElement(
              "span",
              XClass("torctip_val"),
              String.Format("0/{0}", itm.Decoration.MaxUnlockLimit)
            )
          )
        );
      }

      // Decoration Source
      if (itm.TeachesType == "Decoration"
          || (itm.StrongholdSourceList != null && itm.StrongholdSourceList.Count > 0)) {
        if (itm.LocalizedStrongholdSourceNameDict != null) {
          has5 = true;

          foreach (var kvp in itm.LocalizedStrongholdSourceNameDict) {
            tooltip_section5.Add(
              new XElement(
                "div",
                XClass("torctip_val"),
                String.Format(
                  GetLocalizedText(946314439295249),
                  (kvp.Value != null) ? kvp.Value[Tooltip.Language] : "unknown"
                ) // "Source: {0}"
              )
            );
          }
        }
      }

      // Add Section 5 to Tooltip
      if (has5) tooltip.Add(tooltip_section5);

      // Section 6: Ability Use/Equip, Description
      XElement tooltip_section6 = new XElement("div", XClass("torctip_section"), String.Empty);

      String reqParanString = GetLocalizedText(836131348283395); // "Requires {0} ({1})"
      String reqString = GetLocalizedText(836131348283394); // "Requires {0}"
      Regex regex_newline = new Regex("(\r\n|\r|\n)");
      if (itm.EquipAbilityId != 0) {
        if (itm.EquipAbility != null) {
          String ablDesc = itm.EquipAbility.ParsedDescription ?? "";

          if (ablDesc != "") {
            has6 = true;
            XElement desc =
              new XElement(
                "div",
                XClass("torctip_use"),
                String.Format("{0} ", GetLocalizedText(836131348283442)) // "Equip: "
              );
            XElement link =
              new XElement(
                "a",
                new XAttribute(
                  "href",
                  String.Format(
                    "https://torcommunity.com{2}/database/item/{0}/{1}/",
                    itm.EquipAbility.Base62Id,
                    itm.EquipAbility.LocalizedName[Tooltip.Language].LinkString(),
                    Tooltip.LinkLocal
                  )
                ),
                new XAttribute("data-torc", "norestyle")
              );
            AddStringWithBreaks(ref link, ablDesc);
            desc.Add(link);
            tooltip_section6.Add(desc);
          }
        }
      }

      if (itm.UseAbilityId != 0) {
        if (itm.UseAbility != null) {
          has6 = true;

          if (itm.UseAbility.Fqn != "abl.player.mount.grant_mount") {
            // if (itm.UseAbility.Fqn != null) {
            //    if (itm.UseAbility.Fqn.StartsWith("abl.player.") 
            //        && String.IsNullOrEmpty(itm.UseAbility.LocalizedDescription[Tooltip.language])) {
            //        // String pausehere = "";
            //    }
            // }

            String ablDesc = "";

            if (itm.UseAbility.ParsedLocalizedDescription != null)
              ablDesc = itm.UseAbility.ParsedLocalizedDescription[Tooltip.Language];

            // if (ablDesc == "") break;
            // ablDesc = System.Text.RegularExpressions.Regex.Replace(ablDesc, @"\r\n?|\n", "<br />");
            // tooltip.Add(new XElement("div",
            //    XClass("torctip_use"),
            //    String.Format("Use: {0}", ablDesc)
            //    ));

            if (ablDesc != "") {
              XElement useAbil =
                new XElement(
                  "div",
                  XClass("torctip_use"),
                  String.Format("{0} ", GetLocalizedText(836131348283443)) // "Use: ");
                );
              XElement link =
                new XElement(
                  "a",
                  new XAttribute(
                    "href",
                    String.Format(
                      "https://torcommunity.com{2}/database/ability/{0}/{1}/",
                      itm.UseAbility.Base62Id,
                      itm.UseAbility.LocalizedName[Tooltip.Language].LinkString(),
                      Tooltip.LinkLocal
                    )
                  ),
                  new XAttribute("data-torc", "norestyle")
                );
              AddStringWithBreaks(ref link, ablDesc);
              useAbil.Add(link);
              tooltip_section6.Add(useAbil);
            }
          }
        }
      }

      // Description
      if (!String.IsNullOrWhiteSpace(itm.Description)) {
        has6 = true;
        String itmDesc = itm.LocalizedDescription[Tooltip.Language];
        itmDesc = Regex.Replace(itmDesc, @"\r\n?|\n", "\n");

        if (itm.TeachesType == "Mount") {
          // Replace <<1>> with Speeder Piloting %
          itmDesc = itmDesc.Replace("&lt;&lt;1&gt;&gt;", GetLocalizedText(946288669491503));
          itmDesc = itmDesc.Replace("<<1>>", GetLocalizedText(946288669491503));
        }

        XElement desc = new XElement("div", XClass("torctip_desc"));
        AddStringWithBreaks(ref desc, itmDesc);
        tooltip_section6.Add(desc);
      }

      // Add Section 6 to Tooltip
      if (has6) tooltip.Add(tooltip_section6);

      // Section 7: Cartel Market Item
      XElement tooltip_section7 = new XElement("div", XClass("torctip_section"), String.Empty);

      // Cartel Market Item
      if (itm.TypeBitFlags.IsMtxItem) {
        has7 = true;
        tooltip_section7.Add(
          new XElement(
            "div",
            XClass("torctip_val"),
            GetLocalizedText(836131348283729) // "Cartel Market Item"
          )
        );
      }

      // Add Section 7 to Tooltip
      if (has7) tooltip.Add(tooltip_section7);

      // Section 8: Requirements
      XElement tooltip_section8 = new XElement("div", XClass("torctip_section"), String.Empty);
      XElement tooltip_right2 = new XElement("div", XClass("torctip_right"), String.Empty);

      // Requirements
      if (itm.RequiredLevel != 0) {
        has8 = true;
        tooltip_right2.Add(
          new XElement(
            "div",
            String.Format(
              GetLocalizedText(836131348283393),
              itm.RequiredLevel
            ) // "Requires Level {0}"
          )
        );
      }

      if (itm.RequiredClasses != null && itm.RequiredClasses.Count != 0) {
        has8 = true;
        tooltip_right2.Add(
          new XElement(
            "div",
            String.Format(
              reqString,
              String.Join(
                ",",
                itm.RequiredClasses.Select(x => x.Name)
              )
            ) // "Requires {0}"
          )
        );
      }

      if (itm.ArmorSpec != null) {
        has8 = true;
        tooltip_right2.Add(
          new XElement(
            "div",
            String.Format(
              reqString,
              itm.ArmorSpec.LocalizedName[Tooltip.Language]
            ) // "Requires {0}"
          )
        );
      }

      if (itm.WeaponSpec != null && itm.WeaponSpec.LocalizedName != null) {
        has8 = true;
        tooltip_right2.Add(
          new XElement(
            "div",
            String.Format(
              reqString,
              itm.WeaponSpec.LocalizedName[Tooltip.Language]
            ) // "Requires {0}"
          )
        );
      }

      if (itm.RequiredGender != Gender.None) {
        has8 = true;
        String genderString = "";

        switch (itm.RequiredGender.ToString()) {
          case "Male":
            genderString = GetLocalizedText(836131348283503); // "Male Clothing"
            break;
          case "Female":
            genderString = GetLocalizedText(836131348283502); // "Female Clothing"
            break;
        }

        tooltip_right2.Add(new XElement("div", genderString));
      }

      if (itm.RequiredProfession != Profession.None) {
        has8 = true;
        tooltip_right2.Add(
          new XElement(
            "div",
            String.Format(
              reqParanString,
              itm.RequiredProfession.ConvertToString(),
              itm.RequiredProfessionLevel
            ) // "Requires {0} ({1})"
          )
        );
      }

      if (itm.RequiredValorRank > 0) {
        has8 = true;
        tooltip_right2.Add(
          new XElement(
            "div",
            String.Format(
              reqParanString,
              GetLocalizedText(836131348283505),
              itm.RequiredValorRank
            ) // "Requires Valor Rank ({0})"
          )
        );
      }

      if (itm.RequiresAlignment) {
        has8 = true;
        String alignment =
          GetLocalizedText(836131348283656 - Convert.ToInt32(itm.RequiredAlignmentInverted));
        AlignmentTier tier = itm.Dom_.AlignmentData.ToTier(itm.RequiredAlignmentTier);

        if (tier != null) {
          tooltip_right2.Add(
            new XElement(
              "div",
              String.Format(
                alignment,
                tier.LocalizedName[Tooltip.Language]
              ) // "Requires {0} {1}{2}"
            )
          );
        } else {
          tooltip_right2.Add(
            new XElement(
              "div",
              String.Format(
                alignment,
                itm.RequiredAlignmentTier
              ) // "Requires {0} {1}{2}"
            )
          );
        }
      }

      if (itm.RequiresSocial) {
        has8 = true;
        SocialTier soc = itm.Dom_.SocialTierData.ToTier(itm.RequiredSocialTier);
        tooltip_right2.Add(
          new XElement(
            "div",
            String.Format(
              GetLocalizedText(836131348283656),
              soc.LocalizedName[Tooltip.Language]
            ) // "Requires Social {0} or above"
          )
        );
      }

      if (itm.RequiredReputationId != 0) {
        has8 = true;
        String repString = GetLocalizedText(836131348283738, Tooltip.Language, "Requires {1} standing with {0}");
        tooltip_right2.Add(
          new XElement(
            "div",
            String.Format(
              repString,
              (itm.LocalizedRequiredReputationLevelName != null)
                ? itm.LocalizedRequiredReputationLevelName[Tooltip.Language]
                : "unknown",
              itm.LocalizedRepFactionName[Tooltip.Language]
            ) // "Requires {1} standing with {0}"
          )
        );
      }

      // Add right side to main section
      tooltip_section8.Add(tooltip_right2);

      // Hide Requires in this section if it's a Mission Discovery item
      if (itm.SchematicId != 0 && itm.Schematic.MissionDescription != "") has8 = false;

      // Add Section 8 to Tooltip
      if (has8) tooltip.Add(tooltip_section8);

      // Section 9: Schematics

      XElement tooltip_section9 = new XElement("div", XClass("torctip_item"), String.Empty);

      if (itm.SchematicId != 0) {
        has9 = true;

        if (itm.Schematic.Item != null) { // Test for empty item
          tooltip_section9.Add(
            ItemHeaderHTML(itm.Schematic.Item),
            ItemInnerHTML(itm.Schematic.Item)
          );
        } else if (itm.Schematic.MissionDescription != "") {

          XElement mission_discovery =
            new XElement(
              "div",
              XClass("torctip_section"),
              String.Empty
            );

          XElement tooltip_left3 =
            new XElement(
              "div",
              XClass("torctip_left"),
              String.Empty
            );

          XElement tooltip_right3 =
            new XElement(
              "div",
              XClass("torctip_right torctip_sidebyside"),
              String.Empty
            );

          tooltip_left3.Add(
            new XElement(
              "div",
              XClass("torctip_mission"),
              new XElement(
                "a",
                new XAttribute(
                  "href",
                  String.Format(
                    "{0}/{1}/{2}",
                    "https://torcommunity.com/database/schematic",
                    itm.SchematicB62Id,
                    LinkString(itm.Schematic.LocalizedName[Tooltip.Language])
                  )
                ),
                itm.Schematic.LocalizedName[Tooltip.Language]
              )
            )
          );

          if (itm.Schematic.CraftingTime > 0) {
            Int32 mins = itm.Schematic.CraftingTime / 60;
            Int32 secs = itm.Schematic.CraftingTime % 60;
            tooltip_right3.Add(
              new XElement(
                "div",
                XClass("torctip_time"),
                String.Format("{0}m {1}s", mins, secs)
              )
            );
          }

          tooltip_right3.Add(
            new XElement(
              "span",
              XClass("creditsymbol"),
              String.Empty
            ),
            new XElement(
              "div",
              XClass("torctip_cost"),
              itm.Schematic.MissionCost
            )
          );

          mission_discovery.Add(
            new XElement(
              "div",
              XClass("torctip_sidebyside"),
              tooltip_left3,
              tooltip_right3
            ),
            new XElement(
              "div",
              XClass("torctip_mission_yield"),
              itm.Schematic.LocalizedMissionYieldDescription[Tooltip.Language]
            ),
            new XElement(
              "div",
              XClass("torctip_right"),
              new XElement(
                "div",
                String.Format(
                  reqParanString,
                  itm.Schematic.LocalizedCrewSkillName[Tooltip.Language],
                  itm.Schematic.SkillOrange
                ) // "Requires {0} ({1})"
              )
            )
          );

          tooltip_section9.Add(mission_discovery);
        } else {
          tooltip_section9.Add(
            new XElement(
              "div",
              XClass("torctip_use"),
              "Unknown Schematic"
            )
          );
        }
      }

      // Add Section 9 to Tooltip
      if (has9) tooltip.Add(tooltip_section9);

      return tooltip;
    }
    private static String LinkString(this String name) {
      String cleaned = name;
      cleaned = cleaned.Replace(".", "")
          .Replace(",", "")
          .Replace("'", "")
          .Replace("\"", "")
          .Replace("[", "")
          .Replace("]", "")
          .Replace("(", "")
          .Replace(")", "")
          .Replace(" ", "+");
      return cleaned.ToLower();
    }
    public static void LoadNameMap(DataObjectModel dom) {
      AddTableToMap(dom.StringTable.Find("str.gui.equipslot"));
      AddTableToMap(dom.StringTable.Find("str.gui.tooltips"));
      AddTableToMap(dom.StringTable.Find("str.gui.items"));
      AddTableToMap(dom.StringTable.Find("str.gui.itm.enhancement.types"));
      AddTableToMap(dom.StringTable.Find("str.prf.professions"));
      AddTableToMap(dom.StringTable.Find("str.gui.crafting"));
      AddTableToMap(dom.StringTable.Find("str.gui.missionlog"));
      AddTableToMap(dom.StringTable.Find("str.gui.missionreward"));
      AddTableToMap(dom.StringTable.Find("str.gui.achievementwindow"));
      AddTableToMap(dom.StringTable.Find("str.gui.system"));
      AddTableToMap(dom.StringTable.Find("str.sys.factions"));
      AddTableToMap(dom.StringTable.Find("str.gui.abilities"));
      AddTableToMap(dom.StringTable.Find("str.gui.itm.setbonuses"));
    }
    public static XElement MissionInnerHTML(this Quest itm) {
      String icon = itm.Icon ?? "none";
      String tooltipType = "mission";
      TorArchive.FileId fileId =
        TorArchive.FileId.FromFilePath(String.Format("/resources/gfx/icons/{0}.dds", icon));

      // Create Wrapper
      XElement tooltip = new XElement("div", XClass("torctip_tooltip"), String.Empty);

      if (itm != null) {
        // Mission Header
        XElement mission_header = new XElement("div", XClass("torctip_header"), String.Empty);

        // Section: Mission Name
        XElement section_mission_name =
          new XElement(
            "div",
            XClass("torctip_section"),
            new XElement(
              "span",
              XClass(String.Format("torctip_{0}", tooltipType)),
              itm.LocalizedName[Tooltip.Language],
              new XElement(
                "span",
                XClass("torctip_image"),
                String.Empty
              )
            )
          );

        // Add Section: Mission Name to Tooltip
        mission_header.Add(section_mission_name);

        // Add Mission Header to Tooltip
        tooltip.Add(mission_header);

        // Section: Repeatable
        Boolean has_repeatable = false;
        XElement section_repeatable = new XElement("div", XClass("torctip_section"), String.Empty);

        // Repeatable
        String isRepeatable = "";

        if (itm.IsRepeatable) {
          has_repeatable = true;
          isRepeatable = GetLocalizedText(836096988545282); // "Repeatable - This mission can be repeated in the future."
        }

        // Add Section: Repeatable to Tooltip
        if (has_repeatable) tooltip.Add(section_repeatable);

        // Section: Journal Text
        Boolean has_journal_text = false;
        XElement section_journal_text =
          new XElement("div", XClass("torctip_section"), String.Empty);

        // Journal Text
        XElement journal_text =
          new XElement("div", XClass("torctip_white"), String.Empty);

        String journalText =
          itm.Branches.Select(
            x => x.Steps.Select(
              y => (y.LocalizedJournalText.Count > 0)
                ? y.LocalizedJournalText[Tooltip.Language]
                : "").FirstOrDefault(
                  z => !String.IsNullOrEmpty(z))).FirstOrDefault(z => !String.IsNullOrEmpty(z));

        if (!String.IsNullOrEmpty(journalText)) {
          has_journal_text = true;
          AddStringWithBreaks(ref journal_text, journalText);
          section_journal_text.Add(journal_text);
        }

        // Add Section: Journal Text to Tooltip
        if (has_journal_text) tooltip.Add(section_journal_text);

        // Section: Tasks
        Boolean has_tasks = false;
        XElement section_tasks = new XElement("div", XClass("torctip_section"), String.Empty);

        XElement taskText =
          new XElement("div", XClass("torctip_tsk_txt"), GetLocalizedText(836096988545049)); // "Tasks:"

        Boolean taskAdded = false;
        foreach (QuestBranch branch in itm.Branches) {
          Int32 stepNum = 1;
          XElement branchElement = new XElement("div", XClass("torctip_branch"));
          Boolean addElement = false;

          foreach (QuestStep step in branch.Steps) {
            Boolean addTask = false;

            if (step.Tasks.Count > 0) {
              XElement taskCont =
                new XElement(
                  "div",
                  XClass("torc_task_cont"),
                  new XElement(
                    "span",
                    XClass("torc_brnch_id"),
                    String.Format("{0}) ", stepNum)
                  )
                );
              XElement taskInner = new XElement("div", XClass("torc_mis_tasks"));

              foreach (QuestTask task in step.Tasks) {
                if (String.IsNullOrEmpty(task.Text)) continue;

                if (itm.Fqn == "qst.location.coruscant.bonus.staged.crisis_control_stage_2") {
                  break;
                }

                if (task.ShowCount) {
                  taskInner.Add(
                    new XElement(
                      "div",
                      XClass("torc_task"),
                      String.Format("{0}: ", task.LocalizedString[Tooltip.Language]),
                      new XElement(
                        "span",
                        XClass("torctip_val"),
                        String.Format("0/{0}", task.CountMax)
                      )
                    )
                  );
                } else {
                  taskInner.Add(
                    new XElement(
                      "div",
                      XClass("torc_task"),
                      task.LocalizedString[Tooltip.Language]
                    )
                  );
                }

                addTask = true;
                addElement = true;
              }

              if (addTask) {
                taskCont.Add(taskInner);
                branchElement.Add(taskCont);
              }
            }

            if (addTask) stepNum++;
          }

          if (addElement) {
            has_tasks = true;

            if (!taskAdded) section_tasks.Add(taskText);

            section_tasks.Add(branchElement);
          }
        }

        // Add Section: Tasks to Tooltip
        if (has_tasks) tooltip.Add(section_tasks);

        // Section: Required Classes
        Boolean has_classes = false;
        XElement section_classes =
          new XElement(
            "div",
            XClass("torctip_section"),
            String.Empty
          );

        // Required Classes
        if (itm.Classes.Count > 0) {
          has_classes = true;
          XElement requiredClasses =
            new XElement(
              "div",
              XClass("torctip_rqd_cls"),
              new XElement(
                "span",
                GetLocalizedText(836058333839384) // "Requires: "
              )
            );
          List<ClassSpec> classes =
            itm.Classes.OrderBy(x => x.GetFaction()).ThenBy(x => x.Name).ToList();

          for (Int32 i = 0; i < classes.Count; i++) {
            String joiner = ",";

            if (i == classes.Count - 1) joiner = "";

            requiredClasses.Add(
              new XElement(
                "span",
                XClass(String.Format("torc_cls_{0}", classes[i].GetFaction())),
                String.Format(
                  "{0}{1} ",
                  classes[i].LocalizedName == null
                    ? classes[i].Name
                    : classes[i].LocalizedName[Tooltip.Language],
                  joiner
                )
              )
            );
          }

          section_classes.Add(requiredClasses);
        }

        // Add Section: Tasks to Tooltip
        if (has_classes) tooltip.Add(section_classes);

        // Section: Rewards
        Boolean has_rewards = false;

        // Rewards
        XElement rewards =
          new XElement(
            "div",
            XClass("torctip_rewards"),
            new XElement(
              "span",
              GetLocalizedText(963081991618571) // "Mission Rewards"
            )
          );
        XElement rewardContainer =
          new XElement(
            "div",
            XClass("torctip_rwd_inner")
          );
        Int32 rewardCount = 0;

        if (itm.XP != 0 && itm.Difficulty != "qstDifficultyNoExp") {
          rewardContainer.Add(
            new XElement(
              "div",
              XClass("torctip_rwd_info"),
              new XElement(
                "span",
                String.Format("{0}:", GetLocalizedText(963081991618566)) // "Experience: "
              ),
              new XElement(
                "span",
                new XElement(
                  "span",
                  XClass("torctip_exp"),
                  String.Format("{0} (Sub)", itm.SubXP.ToString("n0"))
                ),
                " / ",
                new XElement(
                  "span",
                  XClass("torctip_exp_f2p"),
                  String.Format("{0} (F2P)", itm.F2PXP.ToString("n0"))
                )
              )
            )
          );
          rewardCount++;
        }

        if (itm.CreditsRewarded != 0) {
          rewardContainer.Add(
            new XElement(
              "div",
              XClass("torctip_rwd_info"),
              new XElement(
                "span",
                String.Format("{0}:", GetLocalizedText(963081991618567))
              ),
              new XElement(
                "span",
                XClass("torctip_credits"),
                itm.CreditsRewarded
              )
            )
          );
          rewardCount++;
        }

        if (itm.Rewards != null) {
          XElement items = new XElement("div", XClass("torctip_rwd_itms"));
          XElement providedRewards =
            new XElement(
              "div",
              XClass("torctip_rwd_items"),
              new XElement(
                "div",
                String.Format("{0}:", GetLocalizedText(963081991618562)) // "Provided Rewards:"
              )
            );
          Dictionary<String, XElement> providedClassRewards = new Dictionary<String, XElement>();
          XElement selectOneRewards =
            new XElement(
              "div",
              XClass("torctip_rwd_items"),
              new XElement(
                "div",
                String.Format("{0}:", GetLocalizedText(963081991618561)) // "Select One Reward:"
              )
            );
          Dictionary<String, XElement> selectOneClassRewards = new Dictionary<String, XElement>();
          HashSet<UInt64> clsIds = new HashSet<UInt64>();
          clsIds.UnionWith(itm.Classes.Select(x => x.Id).ToList());
          AddBaseClassIds(clsIds);

          foreach (QuestReward rew in itm.Rewards) {
            Item mat = rew.RewardItem;
            if (mat == null) continue;
            String matstringQual =
              (mat.TypeBitFlags.IsModdable && (mat.Quality == ItemQuality.Prototype))
                ? "moddable"
                : mat.Quality.ToString().ToLower();
            TorArchive.FileId matfileId =
              TorArchive.FileId.FromFilePath(
                String.Format("/resources/gfx/icons/{0}.dds", mat.Icon)
              );
            XElement matElement =
              new XElement(
                "div",
                XClass("torctip_rwd"),
                new XAttribute("style", "display:inline;"),
                new XElement(
                  "a",
                  new XAttribute(
                    "href",
                    String.Format(
                      "https://torcommunity.com{2}/database/item/{0}/{1}/",
                      mat.Base62Id,
                      LinkString(mat.Name),
                      Tooltip.LinkLocal
                    )
                  ),
                  new XAttribute("data-torc", "norestyle"),
                  new XAttribute(
                    "class",
                    String.Format(
                      "torctip_image torctip_image_{0}",
                      matstringQual
                    )
                  ),
                  new XElement(
                    "img",
                    new XAttribute(
                      "src",
                      String.Format(
                        "https://torcommunity.com/db/icons/{0}_{1}.jpg",
                        matfileId.Ph,
                        matfileId.Sh
                      )
                    ),
                    new XAttribute("alt", mat.Name),
                    XClass("image")
                  )
                )
              );

            if (rew.RewardItem.MaxStack > 1) {
              matElement.Element("a").Add(
                new XElement("span", XClass("torctip_rwd_overlay"), rew.NumberOfItem)
              );
            }

            if (rew.IsAlwaysProvided) {
              if (rew.Classes.Count > 0 && itm.Classes.Count > 1) {
                foreach (var cls in rew.Classes) {
                  if (clsIds.Count > 0 && !clsIds.Contains(cls.Id)) continue;

                  String restrictName = cls.Name;

                  if (rew.MinLevel != 1 || rew.MaxLevel != 65)
                    restrictName = String.Format(
                      "{0} {1}-{2}",
                      restrictName,
                      rew.MinLevel,
                      rew.MaxLevel
                    );

                  if (!providedClassRewards.ContainsKey(restrictName)) {
                    providedClassRewards.Add(
                      restrictName,
                      new XElement(
                        "div",
                        XClass(
                          String.Format(
                            "torctip_class_restrict torc_cls_{0}",
                            cls.GetFaction()
                          )
                        ),
                        new XElement("span", restrictName),
                        new XAttribute("data-min-lvl", rew.MinLevel),
                        new XAttribute("data-max-lvl", rew.MaxLevel)
                      )
                    );
                  }

                  providedClassRewards[restrictName].Add(matElement);
                }
              } else if (rew.MinLevel != 1 || rew.MaxLevel != 65) {
                String levelRestrict = String.Format("Level {0}-{1}", rew.MinLevel, rew.MaxLevel);

                if (!providedClassRewards.ContainsKey(levelRestrict)) {
                  providedClassRewards.Add(
                    levelRestrict,
                    new XElement(
                      "div",
                      XClass("torctip_class_restrict torc_cls_level"),
                      new XElement(
                        "span",
                        levelRestrict
                      ),
                      new XAttribute("data-min-lvl", rew.MinLevel),
                      new XAttribute("data-max-lvl", rew.MaxLevel)
                    )
                  );
                }

                providedClassRewards[levelRestrict].Add(matElement);
              } else {
                providedRewards.Add(matElement);
              }
            } else {
              if (rew.Classes.Count > 0 && itm.Classes.Count > 1) {
                foreach (var cls in rew.Classes) {
                  if (clsIds.Count > 0 && !clsIds.Contains(cls.Id)) continue;

                  String restrictName = cls.Name;

                  if (rew.MinLevel != 1 || rew.MaxLevel != 65) {
                    restrictName = String.Format(
                      "{0} {1}-{2}",
                      restrictName,
                      rew.MinLevel,
                      rew.MaxLevel
                    );
                  }

                  if (!selectOneClassRewards.ContainsKey(restrictName)) {
                    selectOneClassRewards.Add(
                      restrictName,
                      new XElement(
                        "div",
                        XClass(
                          String.Format(
                            "torctip_class_restrict torc_cls_{0}",
                            cls.GetFaction()
                          )
                        ),
                        new XElement(
                          "span",
                          restrictName
                        ),
                        new XAttribute("data-min-lvl", rew.MinLevel),
                        new XAttribute("data-max-lvl", rew.MaxLevel)
                      )
                    );
                  }

                  selectOneClassRewards[restrictName].Add(matElement);
                }
              } else if (rew.MinLevel != 1 || rew.MaxLevel != 65) {
                String levelRestrict = String.Format("Level {0}-{1}", rew.MinLevel, rew.MaxLevel);

                if (!selectOneClassRewards.ContainsKey(levelRestrict)) {
                  selectOneClassRewards.Add(
                    levelRestrict,
                    new XElement(
                      "div",
                      XClass("torctip_class_restrict torc_cls_level"),
                      new XElement(
                        "span",
                        levelRestrict
                      ),
                      new XAttribute("data-min-lvl", rew.MinLevel),
                      new XAttribute("data-max-lvl", rew.MaxLevel)
                    )
                  );
                }

                selectOneClassRewards[levelRestrict].Add(matElement);
              } else {
                selectOneRewards.Add(matElement);
              }
            }

            rewardCount++;
          }

          if (providedClassRewards.Count > 0)
            providedRewards.Add(
              providedClassRewards.Values.OrderBy(
                x => x.Attribute("class").Value).ThenBy(x => x.Element("span").Value));

          if (selectOneClassRewards.Count > 0)
            selectOneRewards.Add(
              selectOneClassRewards.Values.OrderBy(
                x => x.Attribute("class").Value).ThenBy(x => x.Element("span").Value));

          if (providedRewards.Elements().Count() > 1)
            rewardContainer.Add(providedRewards);

          if (selectOneRewards.Elements().Count() > 1)
            rewardContainer.Add(selectOneRewards);
        }

        if (rewardCount > 0) {
          has_rewards = true;
          rewards.Add(rewardContainer);
        }

        // Add Section: Rewards to Tooltip
        if (has_rewards) tooltip.Add(rewards);
      }

      if (itm.Base62Id == "xxScuD3") tooltip.Descendants().Where(x => x.Value == null).Remove();

      return tooltip;
    }
    public static XElement SchematicHeaderHTML(this Schematic itm) {
      XElement imgelement = new XElement("div", XClass("torctip_image_wrapper"));

      String icon = "none";
      String stringQual = "none";

      if (itm.Item != null) {
        // stringQual = 
        //   (itm.Item.TypeBitFlags.IsModdable && (itm.Item.Quality == ItemQuality.Prototype)) 
        //     ? "moddable" 
        //     : itm.Item.Quality.ToString().ToLower();
        stringQual = itm.Item.Quality.ToString().ToLower();
        icon = itm.Item.Icon;
        if (String.IsNullOrEmpty(itm.Name)) itm.Name = itm.Item.Name;
        if (itm.LocalizedName == null) itm.LocalizedName = itm.Item.LocalizedName;
      }

      TorArchive.FileId fileId =
        TorArchive.FileId.FromFilePath(String.Format("/resources/gfx/icons/{0}.dds", icon));

      imgelement.Add(
        new XElement(
          "div",
          XClass(String.Format("torctip_image torctip_image_{0}", stringQual)),
          new XElement(
            "img",
            new XAttribute(
              "src",
              String.Format(
                "https://torcommunity.com/db/icons/{0}_{1}.jpg",
                fileId.Ph,
                fileId.Sh)
            ),
            new XAttribute("alt", "")
          )
        )
      );

      XElement tooltip_header =
        new XElement(
          "div",
          XClass(String.Format("torctip_header torctip_header_{0}", stringQual)),
          imgelement,
          new XElement(
            "div",
            new XAttribute(
              "class",
              "torctip_header_text"
            ),
            new XElement(
              "span",
              // XClass(String.Format("torctip_{0}", stringQual)),
              (itm.LocalizedName != null)
                ? itm.MissionDescriptionId == 0
                  ? String.Format("Schematic: {0}", itm.LocalizedName[Tooltip.Language])
                  : String.Format("Mission: {0}", itm.LocalizedName[Tooltip.Language])
                : ""
            )
          )
        );

      return tooltip_header;
    }
    public static XElement SchematicInnerHTML(this Schematic itm) {
      // String stringQual = (itm.Item == null) ? "none" : itm.Item.Quality.ToString().ToLower();

      XElement tooltip_inner = new XElement("div", XClass("torctip_tooltip"));
      XElement skill =
        new XElement(
          "div",
          XClass("torctip_section"),
          new XElement(
            "span",
            XClass("torctip_white"),
            String.Format("{0}:", GetLocalizedText(836058333839618)) // "Difficulty: "),
          ),
          new XElement(
            "div",
            XClass(""),
            new XElement(
              "span",
              XClass("torctip_hard"),
              String.Format("{0} ", itm.SkillOrange)
            ),
            new XElement(
              "span",
              XClass("torctip_medium"),
              String.Format("{0} ", itm.SkillYellow)
            ),
            new XElement(
              "span",
              XClass("torctip_easy"),
              String.Format("{0} ", itm.SkillGreen)
            ),
            new XElement(
              "span",
              XClass("torctip_trivial"),
              String.Format("{0} ", itm.SkillGrey)
            )
          )
        );
      String reqParanString = GetLocalizedText(836131348283395);  // "Requires {0} ({1})"
                                                                  // _ = GetLocalizedText(836131348283394);  // "Requires {0}"

      if (itm.MissionDescriptionId == 0) {
        XElement components =
          new XElement(
            "div",
            XClass("torctip_section"),
            new XElement(
              "span",
              XClass("torctip_white"),
              GetLocalizedText(836058333839387) // "Components:"
            )
          );

        if (itm.Materials != null) {
          foreach (var kvp in itm.Materials) {
            Item mat = (Item)GameObject.Load(kvp.Key, itm.Dom_);

            if (mat != null) {
              String matstringQual =
                (mat.TypeBitFlags.IsModdable && (mat.Quality == ItemQuality.Prototype))
                  ? "moddable"
                  : mat.Quality.ToString().ToLower();
              TorArchive.FileId matfileId =
                TorArchive.FileId.FromFilePath(
                  String.Format("/resources/gfx/icons/{0}.dds", mat.Icon)
                );
              components.Add(
                new XElement(
                  "div",
                  XClass("torctip_mat"),
                  new XElement(
                    "span",
                    String.Format("{0}x ", kvp.Value)
                ),
                new XElement(
                  "div",
                  XClass(String.Format("torctip_image torctip_image_{0} small_border", matstringQual)),
                  new XElement(
                    "img",
                    new XAttribute(
                      "src",
                      String.Format(
                        "https://torcommunity.com/db/icons/{0}_{1}.jpg",
                        matfileId.Ph,
                        matfileId.Sh
                      )
                    ),
                    new XAttribute(
                      "alt",
                      mat.LocalizedName[Tooltip.Language]
                    ),
                    XClass("small_image")
                  )
                ),
                new XElement(
                  "div",
                  XClass("torctip_mat_name"),
                  new XElement(
                    "a",
                    XClass(String.Format("torctip_{0}", matstringQual)),
                    new XAttribute(
                      "href",
                      String.Format(
                        "https://torcommunity.com{2}/database/item/{0}/{1}/",
                        mat.Base62Id,
                        LinkString(mat.LocalizedName[Tooltip.Language]),
                        Tooltip.LinkLocal
                      )
                    ),
                    new XAttribute(
                      "data-torc",
                      "norestyle"
                    ),
                    mat.LocalizedName[Tooltip.Language]
                  )
                )
                )
              );
            }
          }
        } else {
          components.Add(new XElement("div", "Components List Empty!"));
        }

        // Section: Requires
        XElement schematic_requires =
          new XElement(
            "div",
            XClass("torctip_section"),
            new XElement(
              "div",
              XClass("torctip_right"),
              String.Format(
                reqParanString,
                itm.LocalizedCrewSkillName[Tooltip.Language],
                itm.SkillOrange
              ) // "Requires {0} ({1})"
            )
          );

        if (itm.Item != null) {
          tooltip_inner.Add(
            skill,
            components,
            schematic_requires,
            new XElement(
              "div",
              XClass("torctip_item"),
              itm.Item.ItemHeaderHTML(),
              itm.Item.ItemInnerHTML()
            )
          );
        } else {
          tooltip_inner.Add(new XElement("div", "Crafted Item Missing!"));
        }
      } else {
        XElement mission_stuff = new XElement("div", XClass("torctip_section"), String.Empty);

        XElement tooltip_left = new XElement("div", XClass("torctip_left"), String.Empty);

        XElement tooltip_right =
          new XElement("div", XClass("torctip_right torctip_sidebyside"), String.Empty);

        tooltip_left.Add(
          new XElement(
            "div",
            XClass(
              String.Format(
                "torctip_mission_faction {0}",
                itm.MissionFaction.ToString().ToLower()
              )
            ),
            String.Empty
          ),
          new XElement(
            "div",
            XClass("torctip_mission"),
            new XElement(
              "div",
              XClass("torctip_mission_name"),
              (itm.LocalizedName != null) ? itm.LocalizedName[Tooltip.Language] : ""
            )
          )
        );

        if (itm.CraftingTime > 0) {
          Int32 mins = itm.CraftingTime / 60;
          Int32 secs = itm.CraftingTime % 60;
          tooltip_right.Add(
            new XElement("div", XClass("torctip_time"), String.Format("{0}m {1}s", mins, secs))
          );
        }

        tooltip_right.Add(
          new XElement("span", XClass("creditsymbol"), String.Empty),
          new XElement("div", XClass("torctip_cost"), itm.MissionCost)
        );

        mission_stuff.Add(
          new XElement("div", XClass("torctip_sidebyside"), tooltip_left, tooltip_right),
          new XElement(
            "div",
            XClass("torctip_mission_desc"),
            (itm.LocalizedMissionDescription != null)
              ? itm.LocalizedMissionDescription[Tooltip.Language]
              : ""
          ),
          new XElement(
            "div",
            XClass("torctip_mission_desc"),
            (itm.MissionResultDescription != null)
              ? itm.LocalizedMissionResultDescription[Tooltip.Language].Replace(
                "&lt;&lt;1&gt;&gt;", "Your Companion").Replace("<<1>>", "Your Companion")
              : ""
          ),
          new XElement(
            "div",
            XClass("torctip_mission_yield"),
            (itm.LocalizedMissionYieldDescription != null)
              ? itm.LocalizedMissionYieldDescription[Tooltip.Language]
              : ""
          ),
          new XElement(
            "div",
            XClass("torctip_right"),
            new XElement(
              "div",
              String.Format(
                reqParanString,
                itm.LocalizedCrewSkillName[Tooltip.Language],
                itm.SkillOrange
              ) // "Requires {0} ({1})"
            )
          )
        );

        tooltip_inner.Add(skill, mission_stuff);
      }

      return tooltip_inner;
    }
    public static XElement SetBonusHeaderHTML(this SetBonusEntry itm) {
      // Create Left and Right Side
      XElement tooltip_left =
        new XElement(
          "div",
          XClass("torctip_left"),
          new XElement(
            "div",
            new XAttribute("class", "torctip_header_text"),
            new XElement("span", itm.LocalizedName[Tooltip.Language])
          )
        );

      XElement tooltip_right =
        new XElement(
          "div",
          XClass("torctip_right"),
          new XElement(
            "div",
            new XAttribute("class", "torctip_header_text"),
            new XElement("span", itm.MaxItemCount)
          )
        );

      XElement tooltip_header =
        new XElement(
          "div",
          XClass(String.Format("torctip_header")),
          tooltip_left,
          tooltip_right
        );

      return tooltip_header;
    }
    public static XElement SetBonusInnerHTML(this SetBonusEntry itm) {
      // Create Wrapper
      XElement tooltip = new XElement("div", XClass("torctip_tooltip"), String.Empty);

      // Section Variables
      Boolean has1 = true;
      Boolean has2 = false;
      // Boolean has3 = false;

      // Section 1: Set Bonus Images
      XElement tooltip_section1 =
        new XElement(
          "div",
          XClass("torctip_section slot_images"),
          new XElement("div", new XAttribute("class", "slot_head"), String.Empty),
          new XElement("div", new XAttribute("class", "slot_chest"), String.Empty),
          new XElement("div", new XAttribute("class", "slot_wrists"), String.Empty),
          new XElement("div", new XAttribute("class", "slot_hands"), String.Empty),
          new XElement("div", new XAttribute("class", "slot_waist"), String.Empty),
          new XElement("div", new XAttribute("class", "slot_legs"), String.Empty),
          new XElement("div", new XAttribute("class", "slot_feet"), String.Empty)
        );

      // Add Section 1 to Tooltip
      if (has1) tooltip.Add(tooltip_section1);

      // Section 2: Set Bonuses
      XElement tooltip_section2 = new XElement("div", XClass("torctip_section"), String.Empty);

      if (itm.LocalizedBonusDescriptions.Count != 0) has2 = true;

      foreach (var prop in itm.LocalizedBonusDescriptions) {
        tooltip_section2.Add(
          new XElement(
            "div",
            XClass("torctip_desc"),
            String.Format("({0}) {1}", prop.Key, prop.Value[Tooltip.Language])
          )
        );
      }

      // Add Section 2 to Tooltip
      if (has2) tooltip.Add(tooltip_section2);

      // Add Section 3 to Tooltip
      // if (has3) tooltip.Add(tooltip_section3);

      return tooltip;
    }
    private static XElement ToHTML(this ItemEnhancement itm) {
      //String slot = itm.Slot.ConvertToString();
      String slot = "unknown";

      if (itm.DetailedSlot.LocalizedDisplayName != null)
        slot = itm.DetailedSlot.LocalizedDisplayName[Tooltip.Language];

      XElement enhancement = new XElement("div", XClass("torctip_mod"));

      if (itm.ModificationId != 0) {
        slot = itm.Modification.LocalizedName[Tooltip.Language];

        if ((itm.Slot == EnhancementType.ColorCrystal || itm.Slot == EnhancementType.Dye)
            && itm.Modification.DyeColor != null) {
          XElement colors =
            new XElement(
              "div",
              "[",
              GetDyeBlock(itm.Modification.DyeColor.Palette1Rep),
              "|",
              GetDyeBlock(itm.Modification.DyeColor.Palette2Rep),
              "]"
            );
          enhancement.Add(
            new XElement(
              "div",
              XClass("torctip_mslot"),
              new XElement(
                "a",
                XClass(
                  String.Format(
                    "torctip_{0}",
                    itm.Modification.Quality.ToString()
                  )
                ),
                new XAttribute(
                  "href",
                  String.Format(
                    "https://torcommunity.com{2}/database/item/{0}/{1}/",
                    itm.Modification.Base62Id,
                    itm.Modification.LocalizedName[Tooltip.Language].LinkString(),
                    Tooltip.LinkLocal
                  )
                ),
                new XAttribute("data-torc", "norestyle"),
                String.Format("{0} ({1})", slot, itm.Modification.Rating.ToString()),
                colors
              )
            )
          );
        } else {
          enhancement.Add(
            new XElement(
              "div",
              XClass("torctip_mslot"),
              new XElement(
                "a",
                XClass(String.Format("torctip_{0}", itm.Modification.Quality.ToString())),
                new XAttribute(
                  "href",
                  String.Format(
                    "https://torcommunity.com{2}/database/item/{0}/{1}/",
                    itm.Modification.Base62Id,
                    itm.Modification.LocalizedName[Tooltip.Language].LinkString(),
                    Tooltip.LinkLocal
                  )
                ),
                new XAttribute("data-torc", "norestyle"),
                String.Format("{0} ({1})", slot, itm.Modification.Rating.ToString())
              )
            )
          );
        }

        for (Int32 e = 0; e < itm.Modification.CombinedStatModifiers.Count; e++) {
          enhancement.Add(
            new XElement(
              "div",
              XClass("torctip_mstat"),
              String.Format(
                "+{0} {1}",
                itm.Modification.CombinedStatModifiers[e].Modifier,
                itm.Modification.CombinedStatModifiers[e]
                  .DetailedStat.LocalizedDisplayName[Tooltip.Language]
              )
            )
          );
        }
      } else {
        //empty mod
        String repString = GetLocalizedText(836131348283476); // "{0}: Open"
        enhancement.Add(
          new XElement(
            "div",
            XClass("torctip_mslot"),
            String.Format(repString, slot)
          )
        );
      }

      return enhancement;
    }
    private static XElement ToHTML(this SetBonusEntry itm) {
      String name = null;

      if (itm == null) return new XElement("div", "set bonus not found");
      if (itm.LocalizedName != null) itm.LocalizedName.TryGetValue(Tooltip.Language, out name);
      if (!String.IsNullOrEmpty(name)) name = "Unnamed Set Bonus";

      XElement enhancement =
        new XElement(
          "div",
          XClass("torctip_set_wrapper"),
          new XElement(
            "div",
            XClass("torctip_set_name"),
            new XElement("span", String.Format("{0} (", name)),
            new XElement("span", new XAttribute("id", "set_count"), 1),
            new XElement("span", String.Format("/{0})", itm.MaxItemCount))
          )
        );

      //add item list here eventually
      foreach (var kvp in itm.BonusAbilityByNum) {
        enhancement.Add(
          new XElement(
            "div",
            XClass("torctip_set_bonus"),
            String.Format(
              "({0}) {1}",
              kvp.Key,
              kvp.Value.ParsedLocalizedDescription[Tooltip.Language].Replace("\'", "'")
            )
          )
        );
      }

      return enhancement;
    }
    public static String ToRoman(this Int32 number) {
      if ((number < 0) || (number > 3999)) return number.ToString();
      if (number < 1) return String.Empty;
      if (number >= 1000) return "M" + ToRoman(number - 1000);
      if (number >= 900) return "CM" + ToRoman(number - 900);
      if (number >= 500) return "D" + ToRoman(number - 500);
      if (number >= 400) return "CD" + ToRoman(number - 400);
      if (number >= 100) return "C" + ToRoman(number - 100);
      if (number >= 90) return "XC" + ToRoman(number - 90);
      if (number >= 50) return "L" + ToRoman(number - 50);
      if (number >= 40) return "XL" + ToRoman(number - 40);
      if (number >= 10) return "X" + ToRoman(number - 10);
      if (number >= 9) return "IX" + ToRoman(number - 9);
      if (number >= 5) return "V" + ToRoman(number - 5);
      if (number >= 4) return "IV" + ToRoman(number - 4);
      if (number >= 1) return "I" + ToRoman(number - 1);
      throw new Exception("something bad happened");
    }
    /// <summary>
    /// Returns the XML String of the <paramref name="xElement"/> WITHOUT CHARACTER CHECKING.
    /// </summary>
    /// <param name="xElement"></param>
    /// <returns></returns>
    public static String ToStringWithoutCharacterChecking(this XElement xElement) {
      using System.IO.StringWriter stringWriter = new System.IO.StringWriter();
      XmlWriterSettings s =
        new XmlWriterSettings {
          CheckCharacters = false,
          Indent = false,
          Encoding = Encoding.Default,
          OmitXmlDeclaration = true
        };

      using (XmlWriter xmlTextWriter = XmlWriter.Create(stringWriter, s)) {
        xElement.WriteTo(xmlTextWriter);
      }

      return stringWriter.ToString();
    }

    public static XAttribute XClass(String classname) => new XAttribute("class", classname);

    private static XElement XStat(String text, String value) {
      return new XElement(
        "div",
        new XElement("span", XClass("torctip_val"), text),
        new XElement("span", XClass("torctip_white"), value)
      );
    }
  }
}
