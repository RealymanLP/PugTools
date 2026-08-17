using System;
using System.Collections.Generic;
using System.Linq;
using GomLib.Models;

namespace GomLib.ModelLoader {
  public class DisciplineLoader {
    readonly DataObjectModel _dom;

    public DisciplineLoader(DataObjectModel dom) {
      _dom = dom;
      Flush();
    }

    public static void Flush() {

    }

    public Discipline Load(Discipline model, GomObjectData gom) {
      if (gom == null) { return model; }
      if (model == null) { return null; }

      model.Dom = _dom;
      model.Icon = gom.ValueOrDefault<string>("classIcon");
      model.SortIdx = gom.ValueOrDefault<long>("disSortIdx");
      model.ClassId = gom.ValueOrDefault<ulong>("disClassId");
      model.PathApcId = gom.ValueOrDefault<ulong>("disApcId");
      model.Id = (long)model.PathApcId;
      model.PathAbilities = _dom.AbilityPackageLoader.Load(model.PathApcId);
      model.NameId = gom.ValueOrDefault<long>("disName") + 2031339142381568;
      model.ClassNameId = gom.ValueOrDefault<long>("className") + 2031339142381568;

      var nameTable = _dom.StringTable.Find("str.gui.abl.player.skill_trees");

      model.Name = nameTable.GetText(model.NameId, "str.gui.abl.player.skill_trees");
      model.LocalizedName = nameTable.GetLocalizedText(model.NameId, "str.gui.abl.player.skill_trees");

      model.ClassName = nameTable.GetText(model.ClassNameId, "str.gui.abl.player.skill_trees");
      model.LocalizedClassName = nameTable.GetLocalizedText(model.ClassNameId, "str.gui.abl.player.skill_trees");

      model.Role = gom.ValueOrDefault("disRole", new ScriptEnum()).ToString().Replace("chrRole", "");

      var disDisciplnePreviews = _dom.GetObject("disDisicplinePreviews"); // Pre-7.0
      object disPrevObj = null;
      if (disDisciplnePreviews != null) {
        var disDescBaseTable = disDisciplnePreviews.Data.ValueOrDefault<Dictionary<object, object>>("disDescBaseTable");
        disDisciplnePreviews.Unload();
        disDescBaseTable.TryGetValue(model.PathApcId, out disPrevObj);
      }

      if (disPrevObj == null) return model;

      model.DescriptionId = ((GomObjectData)disPrevObj).ValueOrDefault<long>("disPreviewDesc");
      var descTable = _dom.StringTable.Find("str.gui.disciplines");
      model.Description = descTable.GetText(model.DescriptionId, "str.gui.disciplines");
      model.LocalizedDescription = descTable.GetLocalizedText(model.DescriptionId, "str.gui.disciplines");

      model.BaseAbilityIds = new Dictionary<ulong, int>();
      for (int i = 1; i < 5; i++) {
        ulong baseId = ((GomObjectData)disPrevObj).ValueOrDefault<ulong>(string.Format("disBaseAbl{0}", i));
        int lvl = 0;
        try {
          lvl = model.PathAbilities.PackageAbilities.Where(x => x.AbilityId == baseId).Select(y => y.Level).First();
        }
        catch (Exception) {
          continue;
        }
        model.BaseAbilityIds.Add(baseId, lvl);
      }

      return model;
    }
  }

  public class NewDisciplineLoader {
    readonly DataObjectModel _dom;
    StringTable strTable;

    public NewDisciplineLoader(DataObjectModel dom) {
      _dom = dom;
      Flush();
    }

    public void Flush() {
      strTable = null;
    }

    public NewDiscipline Load(GomObject obj) {
      NewDiscipline dis = new NewDiscipline();
      return Load(dis, obj);
    }

    public NewDiscipline Load(GameObject obj, GomObject gom) {
      if (gom == null) {
        return (NewDiscipline)obj;
      }

      return Load(obj as NewDiscipline, gom);
    }

    public NewDiscipline Load(NewDiscipline model, GomObject obj) {
      if (obj == null) { return model; }
      if (model == null) { return null; }

      model.Dom_ = _dom;

      return model;
    }

    public NewDiscipline Load(NewDiscipline model, Int64 Id, GomObjectData obj) {
      if (obj == null) { return model; }
      if (model == null) { return null; }

      model.Id = (ulong)Id;
      model.Dom_ = _dom;

      //The base id to use for finding the real id with an offset.
      //First id in the file take 1.
      Int64 baseId = strTable.data.Keys.First() - 1;

      return model;
    }

  }
}
