using System;
using System.Collections.Generic;
using System.Linq;
using GomLib.Models;

namespace GomLib.ModelLoader {
  public class SCFFShipLoader {
    readonly DataObjectModel _dom;
    Dictionary<Object, Object> colorMap;
    Dictionary<Object, Object> ComponentColorUIData;
    Dictionary<Object, Object> defaultLoadoutData;
    Dictionary<Object, Object> patternMap;
    Dictionary<Object, Object> patternUIData;
    Dictionary<Object, Object> shipMap;

    public SCFFShipLoader(DataObjectModel dom) {
      _dom = dom;
      Flush();
    }
    private static void CheckAvailability(ScFFShip shp, ScriptEnum availability) {
      Boolean available = false;
      Boolean deprecated = false;
      Boolean hidden = false;

      switch (availability.ToString()) {
        case "scFFDeprecated":
          deprecated = true;
          break;
        case "scFFAvailable": //default is scFFUnavailable
          available = true;
          break;
        case "scFFHidden":
          hidden = true;
          break;
      }

      shp.IsAvailable = available;
      shp.IsDeprecated = deprecated;
      shp.IsHidden = hidden;
    }
    public void Flush() {
      shipMap = new Dictionary<Object, Object>();
      colorMap = new Dictionary<Object, Object>();
      ComponentColorUIData = new Dictionary<Object, Object>();
      patternMap = new Dictionary<Object, Object>();
      patternUIData = new Dictionary<Object, Object>();
      defaultLoadoutData = new Dictionary<Object, Object>();
    }
    public ScFFShip Load(ScFFShip shp, Int64 Id, GomObjectData obj) {
      if (obj == null) { return shp; }
      if (shp == null) { return null; }

      if (shipMap != null && shipMap.Count == 0) {
        GomObject masterMap = _dom.GetObject("MasterComponentMap");

        if (masterMap != null) {
          shipMap =
            masterMap.Data.ValueOrDefault<Dictionary<Object, Object>>(
              "scFFShipMasterComponentMap",
              null
            );
          masterMap.Unload();
        }

        GomObject masterColorMap = _dom.GetObject("scFFColorOptionMasterPrototype");

        if (masterColorMap != null) {
          colorMap =
            masterColorMap.Data.ValueOrDefault<Dictionary<Object, Object>>(
              "scffFactionColorMap",
              null
            );
          ComponentColorUIData =
            masterColorMap.Data.ValueOrDefault<Dictionary<Object, Object>>(
              "scFFComponentColorUIData",
              null
            );
          masterColorMap.Unload();
        }

        // Protoype ?!?
        GomObject masterPatternData = _dom.GetObject("scFFPatternsDefinitionProtoype");

        if (masterPatternData != null) {
          patternMap =
            masterPatternData.Data.ValueOrDefault<Dictionary<Object, Object>>(
              "scFFShipPatternMap",
              null
            );
          patternUIData =
            masterPatternData.Data.ValueOrDefault<Dictionary<Object, Object>>(
              "scFFShipPatternUIData",
              null
            );
          masterPatternData.Unload();
        }

        GomObject masterLoadoutData = _dom.GetObject("scFFShipDefaultLoadoutsPrototype");

        if (masterLoadoutData != null) {
          defaultLoadoutData =
            masterLoadoutData.Data.ValueOrDefault<Dictionary<Object, Object>>(
              "scFFDefaultLoadoutData",
              null
            );
          masterLoadoutData.Unload();
        }
      }

      shp.Dom = _dom;
      shp.Prototype = "scFFShipsDataPrototype";
      shp.ProtoDataTable = "scFFShipsData";

      //scFFShortIdtoShipIdMap has a lookup list for these in the current prototype ex. 1682
      Int64 shortId = obj.ValueOrDefault<Int64>("scFFShortId", 0);
      Object id = new Object();
      Dictionary<Object, Object> shipIdLookup = new Dictionary<Object, Object>();
      _dom.GetObject(
        "scFFShipsDataPrototype"
      ).Data.ValueOrDefault(
        "scFFShortIdtoShipIdMap",
        shipIdLookup
      ).TryGetValue(shortId, out id);
      shp.LookupId = 0;

      if (id != null) shp.LookupId = (Int64)((List<Object>)id)[0];

      shp.Id = Id;
      /*if (shp.Id != shp.LookupId)
      {
          return shp;
      }*/ //not sure what this was originally intended to do.

      // ex. "/art/dynamic/space_pvp/ships/imp_striker/imp_striker_c.gr2"
      shp.Model = obj.Get<String>("scFFShipModel");

      //ex. 16140923285529548575 (Node conSpec_scff_equip_min_ACRS) 
      shp.MinorComponentsContainerId = obj.Get<UInt64>("scFFMinorComponentsPackage");
      shp.MinorComponentSlots = new Dictionary<String, Int64>();

      GomObject minorComponentData =
        _dom.GetObject(shp.MinorComponentsContainerId);
      Dictionary<Object, Object> minorContainerSlots =
        minorComponentData.Data.Get<Dictionary<Object, Object>>("conContainerDataSlots");
      String minorEquipType =
        minorComponentData.Data.Get<String>("conContainerEventType");

      shp.MinorEquipType = minorEquipType.Replace("conSpec_scff_equip_min_", "");
      minorComponentData.Unload();

      foreach (var slot in minorContainerSlots) {
        shp.MinorComponentSlots.Add(
          slot.Key.ToString().Replace(
            "conSlotEquipSCFF",
            ""
          ).Replace("AuxSystem", "Systems"), (Int64)slot.Value);
      }

      //ex. 16141040006346149027 (Node conSpec_scff_equip_maj_PSYHE)
      shp.MajorComponentsContainerId = obj.Get<UInt64>("scFFMajorComponentsPackage");

      shp.MajorComponentSlots =
        new Dictionary<String, Int64>();
      GomObject majorEquipData =
        _dom.GetObject(shp.MajorComponentsContainerId);
      Dictionary<Object, Object> majorContainerSlots =
        majorEquipData.Data.Get<Dictionary<Object, Object>>("conContainerDataSlots");
      String majorEquipType =
        majorEquipData.Data.Get<String>("conContainerEventType");

      shp.MajorEquipType = majorEquipType.Replace("conSpec_scff_equip_maj_", "");
      majorEquipData.Unload();

      foreach (var slot in majorContainerSlots) {
        shp.MajorComponentSlots.Add(
          slot.Key.ToString().Replace(
            "conSlotEquipSCFF",
            ""
          ).Replace("AuxSystem", "Systems"), (Int64)slot.Value);
      }

      Object shipComponentMap = new Object();

      if (shipMap != null) {
        shipMap.TryGetValue(Id, out shipComponentMap);
        shp.ComponentMap = new Dictionary<String, List<ScFFComponent>>();

        foreach (var componentSlot in (Dictionary<Object, Object>)shipComponentMap) {
          List<ScFFComponent> compNames = new List<ScFFComponent>();
          String compName =
            componentSlot.Key.ToString().Replace(
              "conSlotEquipSCFF",
              ""
            ).Replace("AuxSystem", "Systems");

          Int32 c = 0;

          foreach (var cNodeLookup in (List<Object>)componentSlot.Value) {
            ScFFComponent cmp = _dom.SCFFComponentLoader.Load((UInt64)cNodeLookup);

            if (cmp.ComponentId == 0) cmp.ComponentId = c;

            compNames.Add(cmp);
            c++;
          }

          shp.ComponentMap.Add(compName, compNames);
        }
      }

      // ex. 16141171526721883186 (Node imp_striker_c_damage_package)
      shp.DamagedPackageNodeId = obj.Get<UInt64>("scFFDamagePackage");

      // ex. scFFUnavailable
      ScriptEnum availability = obj.Get<ScriptEnum>("scFFAvailability");
      CheckAvailability(shp, availability);

      // lgcAchievementEventsPrototype - Legacy Achievement Events Lookup
      Int64 lgcAchEvt = obj.ValueOrDefault<Int64>("lgcAchievementEvents", 0);
      // scFFCrewPackagesPrototype - Crew Package Lookup
      Int64 crewPkg = obj.ValueOrDefault<Int64>("scFFCrewPackage", 0);
      // scFFColorOptionMasterPrototype - Color Option Master Lookup
      Int64 shipColorId = obj.ValueOrDefault<Int64>("scffFactionId", 0);
      Object shipColorOptionsMap = new Object();
      shp.ColorOptions = new Dictionary<String, List<ScFFColorOption>>();

      if (colorMap != null) {
        colorMap.TryGetValue(shipColorId, out shipColorOptionsMap);
        if (shipColorOptionsMap != null) {
          foreach (var colorList in (Dictionary<Object, Object>)shipColorOptionsMap) {
            List<ScFFColorOption> colorNames = new List<ScFFColorOption>();
            String colorListName = colorList.Key.ToString();
            foreach (Object colorId in (List<Object>)colorList.Value) {
              //Console.WriteLine(colorId.ToString());
              Object colorData = new Object();
              ComponentColorUIData.TryGetValue(colorId, out colorData);
              ScFFColorOption col = new ScFFColorOption();
              _dom.SCFFColorOptionLoader.Load(col, (GomObjectData)colorData);
              colorNames.Add(col);
            }
            shp.ColorOptions.Add(colorListName, colorNames);
          }
        }
      }

      // ex. 16140982269598876670 (Node imp_striker_c_eppdynamicdata_collection)
      shp.EppDynamicCollectionId = obj.ValueOrDefault("scFFEppDynamicCollection", (UInt64)0);

      // str.spvp.ships.stb ex. 3282759468450093
      shp.DescriptionId =
        obj.ValueOrDefault<Int64>("scFFShipDescription", 0);
      shp.Description =
        _dom.StringTable.TryGetString("str.spvp.ships", shp.DescriptionId);
      shp.LocalizedDescription =
        _dom.StringTable.TryGetLocalizedStrings("str.spvp.ships", shp.DescriptionId);

      // scFFPatternsDefinitionPrototype - Pattern Definition Lookup
      Int64 patternDefinition = obj.ValueOrDefault<Int64>("scFFPatternId", 0);
      Object shipPatMap = new Object();
      shp.PatternOptions = new List<ScFFPattern>();

      if (patternMap != null) {
        patternMap.TryGetValue(patternDefinition, out shipPatMap);

        if (shipPatMap != null) {
          foreach (var patternId in (List<Object>)shipPatMap) {
            Object patData = new Object();
            patternUIData.TryGetValue(patternId, out patData);
            ScFFPattern pat = new ScFFPattern();
            _dom.SCFFPatternLoader.Load(pat, (GomObjectData)patData);
            pat.TextureForCurrentShip = pat.TexturesByShipId[shp.Id];
            shp.PatternOptions.Add(pat);
          }
        }
      }

      shp.DefaultLoadout = new Dictionary<String, UInt64>();
      if (defaultLoadoutData != null) {
        defaultLoadoutData.TryGetValue((UInt64)shp.Id, out Object shipLoadoutMap);
        if (shipLoadoutMap != null) {
          foreach (var slotList in (Dictionary<Object, Object>)shipLoadoutMap) {
            String slotName =
              slotList.Key.ToString().Replace(
                "conSlotEquipSCFF",
                ""
              ).Replace("AuxSystem", "Systems");
            // Don't need to load the full component. This was for testing
            // scFFComponent lCmp = 
            //   scFFComponentLoader.Load((UInt64)((List<Object>)slotList.Value)[0]);
            shp.DefaultLoadout.Add(slotName, (UInt64)((List<Object>)slotList.Value)[0]);

            if (((List<Object>)slotList.Value).Count > 1) {
              // For testing not needed.
              // lCmp = scFFComponentLoader.Load((UInt64)((List<Object>)slotList.Value)[1]);
              shp.DefaultLoadout.Add(slotName + 2, (UInt64)((List<Object>)slotList.Value)[1]);
            }
          }
        }
      }

      shp.NameId = obj.Get<Int64>("scFFShipName"); //str.spvp.ships.stb ex. 3282759468450057
      shp.Name = _dom.StringTable.TryGetString("str.spvp.ships", shp.NameId);
      shp.LocalizedName = _dom.StringTable.TryGetLocalizedStrings("str.spvp.ships", shp.NameId);

      // ex. 16140995633020982770 (Node epp.space_combat.freeflight.imperial.fighter_afterburner)
      UInt64 interdictionDriveEppNodeId = obj.ValueOrDefault<UInt64>("scFFAfterBurnerEpp", 0);

      // scale ?? ex. 0.15 - Override Power_Shield_Regen_Rate Modifier?
      shp.UnknownStat1 = obj.ValueOrDefault<Single>("4611686298643904002", 0);
      // ?? ex. 0.0025 
      shp.UnknownStat2 = obj.ValueOrDefault<Single>("4611686298656584000", 0);
      // ex. 2
      shp.UnknownStat3 = obj.ValueOrDefault<Single>("4611686348394117006", 0);
      // ex. 2
      shp.UnknownStat4 = obj.ValueOrDefault<Single>("4611686348594427000", 0);
      // ex. 1.25
      shp.UnknownStat5 = obj.ValueOrDefault<Single>("4611686348594427001", 0);
      // ex. 0.032
      shp.UnknownStat6 = obj.ValueOrDefault<Single>("4611686348976567004", 0);
      // ex. 0.3 
      shp.UnknownStat7 = obj.ValueOrDefault<Single>("4611686348976567005", 0);
      // ex. 0.6
      shp.UnknownStat8 = obj.ValueOrDefault<Single>("4611686348976567007", 0);
      // ex. 29
      Int64 unknown9 = obj.ValueOrDefault<Int64>("4611686349455207001", 0);

      // only accessible ships have this.
      ScriptEnum shipCategory =
        obj.ValueOrDefault("scFFShipCategory", new ScriptEnum());
      shp.Category =
        shipCategory.ToString().Replace("0x00", "Strike Fighter").Replace("scFFShip", "");
      shp.CategoryId =
        (Int32)obj.ValueOrDefault<Int64>("scFFShipCategoryId", 0);

      // ex. 16141013442936013929 pkg.pvp.striker
      UInt64 shipStatPackageId = obj.Get<UInt64>("scFFShipStatsPackage");

      GomObject statPackage = _dom.GetObject(shipStatPackageId);
      shp.Stats = new Dictionary<String, Single>();

      if (statPackage != null) {
        Dictionary<Object, Object> statsObject =
          statPackage.Data.ValueOrDefault("scFFShipStatData", new Dictionary<Object, Object>());
        statPackage.Unload();

        foreach (var stat in statsObject) {
          shp.Stats.Add(stat.Key.ToString(), (Single)stat.Value);
        }
      }

      // ex. 16140967054983465763 (Node spvp.eng.striker)
      shp.EngStatsNodeId = obj.Get<UInt64>("scFFEngineStatsPackage");
      GomObject engStatPackage = _dom.GetObject(shp.EngStatsNodeId);

      if (engStatPackage != null) {
        Dictionary<String, Single> tmpDict = engStatPackage.Data.Dictionary.Skip(3).ToDictionary(
                                               x => x.Key.ToString(),
                                               x => (Single)x.Value
                                             );

        foreach (var stat in tmpDict) {
          /*if (statNames.ContainsKey(stat.Key))
          {
              shp.Stats.Add(statNames[stat.Key], stat.Value);
          }
          else
          {*/
          shp.Stats.Add(stat.Key, stat.Value);
          //}
        }

        engStatPackage.Unload();
      }

      // ex. 16140941624160915343 (Node spvp.camera.package.imp_striker_c)
      UInt64 cameraPackageLookupNodeId = obj.Get<UInt64>("scFFCameraPackage");

      // ex. "shipicon_imp_sniper" //not sure what this does, but it's not the icon.
      shp.ShipIcon = obj.Get<String>("scFFShipHullIcon");
      // ex. "spvp_imp_striker_3" 
      shp.Icon = obj.ValueOrDefault("scFFShipIcon", "");
      _dom.Assets.Icons.Add(shp.Icon);
      _dom.Assets.Icons.Add(shp.ShipIcon);
      shp.Faction = "";

      Int64 f = obj.Get<Int64>("scffFactionId");
      if (f == -1855280666668608219) { // Imperial
        shp.Faction = "Imperial";
      } else if (f == 1086966210362573345) { //Republic
        shp.Faction = "Republic";
      } else {
        shp.Faction = "???";
      }

      // ex. "Play_playership_engine_med_imp"
      String engineSound = obj.ValueOrDefault<String>("scFFEngineSound", null);
      // ex. "Play_playership_engine_med_imp_remote"
      String engineSoundRemote = obj.ValueOrDefault<String>("scFFEngineSoundRemote", null);
      // FFDeathPackagePrototype - Death Package Near Lookup 
      UInt64 deathPkgNear = obj.Get<UInt64>("scFFDeathPackageNear");
      // FFDeathPackagePrototype - Death Package Far Lookup 
      UInt64 deathPkgFar = obj.Get<UInt64>("scFFDeathPackageFar");

      GomObject masterCostData = _dom.GetObject("scFFShipCostPrototype");

      if (masterCostData != null) {
        Dictionary<Object, Object> costLookup =
          masterCostData.Data.Get<Dictionary<Object, Object>>("scFFShipCostMap");

        if (costLookup.ContainsKey(shp.Id)) {
          GomObjectData costData = (GomObjectData)costLookup[shp.Id];
          shp.Cost = costData.ValueOrDefault<Int64>("scFFShipCost", 0);
          shp.IsPurchasedWithCC = costData.ValueOrDefault("scFFIsPurchasedWithCC", false);
        } else {
          shp.Cost = -1;
        }
      }

      UInt64 apcLookup = obj.ValueOrDefault("scFFShipAblPackage", new UInt64());

      if (apcLookup != 0) {
        // ex. 16141125605689795242 (Node apc.spvp.gunship.weapon_swap_secondary)
        shp.AbilityPackage = _dom.AbilityPackageLoader.Load(apcLookup);
      }

      return shp;
    }
  }
}
