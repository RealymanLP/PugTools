using System;
using System.Collections.Generic;
using System.Drawing;
using GomLib.Models;

namespace GomLib.ModelLoader {
  public class AppearanceLoader {
    private readonly DataObjectModel _dom;
    internal Dictionary<String, WeaponAppearance> itmAppearanceDatatable;

    public AppearanceLoader(DataObjectModel dom) {
      _dom = dom;
    }
    public void Flush() {
      itmAppearanceDatatable = null;
    }
    public GameObject Load(GameObject obj, GomObject gom) {
      if (obj == null) throw new ArgumentNullException(nameof(obj));
      if (gom == null) return null;

      return Load(gom);
    }
    public GameObject Load(GomObject obj) {
      if (obj == null) return null;

      switch (obj.Name.Substring(0, 3)) {
        case "ipp":
          return LoadIpp(obj);
        case "npp":
          return LoadNpp(obj);
        default:
          throw new IndexOutOfRangeException();
      }
    }

    public GameObject Load(String fqn) => Load(_dom.GetObject(fqn));

    public GameObject Load(UInt64 nodeId) => Load(_dom.GetObject(nodeId));

    public AppSlot LoadAppSlot(GomObjectData obj, String btOverride) {
      if (obj == null) return new AppSlot(_dom);

      AppSlot app = new AppSlot(_dom) {
        Dom = _dom,
        BodyType = btOverride
      };
      ScriptEnum typ = (ScriptEnum)obj.ValueOrDefault<Object>("appAppearanceSlotType", null);

      if (typ == null) app.Type = "appSlotAge";
      else app.Type = typ.ToString();

      app.ModelID =
        obj.ValueOrDefault<Int64>("appAppearanceSlotModelID", 0);
      app.MaterialIndex =
        obj.ValueOrDefault<Int64>("appAppearanceSlotMaterialIndex", 0);
      app.Attachments =
        obj.ValueOrDefault(
          "appAppearanceSlotAttachments",
          new List<Object>()
        ).ConvertAll(x => (Int64)x);
      app.RandomWeight =
        obj.Get<Int64>("appAppearanceSlotRandomWeight");
      app.PrimaryHueId =
        obj.ValueOrDefault<Int64>("appAppearanceSlotHuePrimary", 0);
      app.SecondaryHueId =
        obj.ValueOrDefault<Int64>("appAppearanceSlotHueSecondary", 0);

      return app;
    }
    public ItemAppearance LoadIpp(GomObject obj) {

      ItemAppearance pkg = new ItemAppearance(_dom) {
        Fqn = obj.Name,
        Id = obj.Id,
        Dom_ = _dom,
        References = obj.References,
        ColorScheme = obj.Data.ValueOrDefault<Int64>("ippColorScheme", 0),
        VOSoundTypeOverride = obj.Data.ValueOrDefault("ippVOSoundTypeOverride", ""),
        IPP = LoadAppSlot(obj.Data, "")
      };

      return pkg;
    }
    public NpcAppearance LoadNpp(GomObject obj) {
      NpcAppearance pkg = new NpcAppearance(_dom) {
        Fqn = obj.Name,
        //Debug.WriteLine(obj.Name);
        Id = obj.Id,
        Dom_ = _dom,
        References = obj.References,
        BodyType = obj.Data.ValueOrDefault<String>("nppBodyType")
      };

      Dictionary<Object, Object> slotMap =
        obj.Data.ValueOrDefault<Dictionary<Object, Object>>(
          "nppAppearanceSlotMap_ForPrototype",
          null
        );

      pkg.AppearanceSlotMap = new Dictionary<String, List<AppSlot>>();

      if (slotMap != null) {
        foreach (var kvp in slotMap) {
          String key = ((ScriptEnum)kvp.Key).ToString();
          List<AppSlot> appList = new List<AppSlot>();

          for (Int32 i = 0; i < ((List<Object>)kvp.Value).Count; i++) {
            // if (((List<Object>)kvp.Value).Count > 1) throw new IndexOutOfRangeException();
            AppSlot value =
              LoadAppSlot((GomObjectData)((List<Object>)kvp.Value)[0], pkg.BodyType);
            appList.Add(value);
          }

          pkg.AppearanceSlotMap.Add(key, appList);
        }
      }

      pkg.NppType =
        ((ScriptEnum)obj.Data.ValueOrDefault<Object>("nppNppType")
          ?? new ScriptEnum()).ToString();

      pkg.SoundPackage = obj.Data.ValueOrDefault("nppSoundPackage", "");
      pkg.ArmorSoundsetOverride = obj.Data.ValueOrDefault("nppArmorSoundsetOverride", "");

      Dictionary<Object, Object> vocalOverrides =
        obj.Data.ValueOrDefault("nppVocalSoundsetOverride", new Dictionary<Object, Object>());
      pkg.VocalSoundsetOverride = new Dictionary<Int64, String>();

      foreach (var kvp in vocalOverrides) {
        pkg.VocalSoundsetOverride.Add((Int64)kvp.Key, (String)kvp.Value);
      }

      return pkg;
    }
    public WeaponAppearance LoadWeaponAppearance(String name, GomObjectData obj) {
      WeaponAppearance pkg = new WeaponAppearance(_dom) {
        Prototype = "itmAppearanceDatatable",
        ProtoDataTable = "itmAppearances",
        Name = name,
        BoneName = obj.ValueOrDefault<String>("itmBoneName", null),
        CombatStance = obj.ValueOrDefault<String>("itmCombatStance", null),
        DrawnOffset = obj.ValueOrDefault<List<Single>>("itmDrawnOffset", null),
        DrawnRotation = obj.ValueOrDefault<List<Single>>("itmDrawnRotation", null),
        DrawnScale = obj.ValueOrDefault<List<Single>>("itmDrawnScale", null),
        DynamicData = obj.ValueOrDefault<String>("itmDynamicData", null),
        FxSpec = obj.ValueOrDefault<String>("itmFxSpec", null),
        Model = obj.ValueOrDefault<String>("itmModel", null),
        StowedOffset = obj.ValueOrDefault<List<Single>>("itmStowedOffset", null),
        StowedRotation = obj.ValueOrDefault<List<Single>>("itmStowedRotation", null),
        StowedScale = obj.ValueOrDefault<List<Single>>("itmStowedScale", null),
        WeaponType = obj.ValueOrDefault("itmWeaponType", new ScriptEnum()).ToString()
      };

      return pkg;
    }
    public WeaponAppearance LoadWeaponAppearance(String name) {
      if (itmAppearanceDatatable == null) {
        GomObject dataTable = _dom.GetObject("itmAppearanceDatatable");
        Dictionary<Object, Object> tempDict =
          dataTable.Data.Get<Dictionary<Object, Object>>("itmAppearances");
        dataTable.Unload();
        itmAppearanceDatatable = new Dictionary<String, WeaponAppearance>();

        foreach (var kvp in tempDict) {
          itmAppearanceDatatable.Add(
            (String)kvp.Key,
            LoadWeaponAppearance((String)kvp.Key, (GomObjectData)kvp.Value)
          );
        }
      }

      itmAppearanceDatatable.TryGetValue(name, out WeaponAppearance output);
      return output;
    }
  }
  public class DetailedAppearanceColorLoader {
    public DataObjectModel _dom;
    private Dictionary<Int64, DetailedAppearanceColor> idMap;

    public DetailedAppearanceColorLoader(DataObjectModel dom) {
      _dom = dom;
      Flush();
    }
    public void Flush() => idMap = new Dictionary<Int64, DetailedAppearanceColor>();
    private void Initialize() {
      GomObject itmAppearanceColorsPrototype =
        _dom.GetObject("itmAppearanceColorsPrototype");
      List<Object> itmAppColorTable =
        itmAppearanceColorsPrototype.Data.ValueOrDefault("itmAppColorTable", new List<Object>());
      Dictionary<Object, Object> itmAppColorIdLookup =
        itmAppearanceColorsPrototype.Data.ValueOrDefault(
          "itmAppColorIdLookup",
          new Dictionary<Object, Object>()
        );
      itmAppearanceColorsPrototype.Unload();
      StringTable stringTable = _dom.StringTable.Find("str.gui.colornames");

      foreach (GomObjectData gom in itmAppColorTable.ConvertAll(x => (GomObjectData)x)) {
        DetailedAppearanceColor det = new DetailedAppearanceColor {
          ColorId = gom.ValueOrDefault<Int64>("itmAppColorId", 0)
        };

        if (itmAppColorIdLookup.ContainsKey(det.ColorId)) {
          det.ShortId = (Int64)itmAppColorIdLookup[det.ColorId];
        }

        det.ColorNameId =
          gom.ValueOrDefault<Int64>("itmAppColorName", 0);
        det.ColorName =
          stringTable.GetText(det.ColorNameId, "str.gui.colornames");
        det.LocalizedColorName =
          stringTable.GetLocalizedText(det.ColorNameId, "str.gui.colornames");
        det.ColorSchemeId =
          gom.ValueOrDefault<Int64>("itmAppColorSchemeId", 0);
        det.HueName =
          gom.ValueOrDefault("itmAppColorHueName", "");
        det.UnknownBool1 =
          gom.ValueOrDefault("4611686298195974006", false);
        det.UnknownBool2 =
          gom.ValueOrDefault("4611686298195974007", false);

        GomObjectData pal1 =
          (GomObjectData)gom.ValueOrDefault<Object>("itmAppColorPalette1Rep", null);

        if (pal1 != null) {
          Byte a = Convert.ToByte(255f * pal1.ValueOrDefault("a", 0f));
          Byte r = Convert.ToByte(255f * pal1.ValueOrDefault("r", 0f));
          Byte g = Convert.ToByte(255f * pal1.ValueOrDefault("g", 0f));
          Byte b = Convert.ToByte(255f * pal1.ValueOrDefault("b", 0f));
          det.Palette1Rep = Color.FromArgb(a, r, g, b);
        }

        GomObjectData pal2 =
          (GomObjectData)gom.ValueOrDefault<Object>("itmAppColorPalette2Rep", null);

        if (pal2 != null) {
          Byte a = Convert.ToByte(255f * pal2.ValueOrDefault("a", 0f));
          Byte r = Convert.ToByte(255f * pal2.ValueOrDefault("r", 0f));
          Byte g = Convert.ToByte(255f * pal2.ValueOrDefault("g", 0f));
          Byte b = Convert.ToByte(255f * pal2.ValueOrDefault("b", 0f));
          det.Palette2Rep = Color.FromArgb(a, r, g, b);
        }

        idMap.Add(det.ShortId, det);
      }
    }
    public DetailedAppearanceColor Load(Int64 id) {
      if (idMap.Count == 0) {
        Initialize();
      }
      idMap.TryGetValue(id, out DetailedAppearanceColor ret);
      return ret;
    }
  }
}
