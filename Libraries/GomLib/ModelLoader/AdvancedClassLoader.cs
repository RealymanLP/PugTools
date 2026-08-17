using System;
using System.Collections.Generic;
using System.Linq;
using GomLib.Models;

namespace GomLib.ModelLoader {
  public class AdvancedClassLoader {
    private StringTable classNames;
    private StringTable classDescriptions;

    private readonly DataObjectModel _dom;

    public AdvancedClassLoader(DataObjectModel dom) {
      _dom = dom;
    }

    public void Flush() {
      classNames = null;
    }

    public AdvancedClass Load(GomObject obj) {
      if (classNames == null) {
        classNames = _dom.StringTable.Find("str.gui.classnames");
        classDescriptions = _dom.StringTable.Find("str.gui.classdescriptions");
      }

      AdvancedClass ac = new AdvancedClass
      {
        Dom_ = _dom,
        Id = obj.Id,
        Fqn = obj.Name,
        NameId = obj.Data.ValueOrDefault<long>("chrAdvancedClassDataNameId", 0)
      };
      //ac.AcId = (int)ac.NameId;
      ac.Name = classNames.GetText(ac.NameId, obj.Name);
      ac.LocalizedName = classNames.GetLocalizedText(ac.NameId, obj.Name);
      ac.DescriptionId = Convert.ToInt64(obj.Data.ValueOrDefault<string>("chrAdvancedClassDescription"));
      ac.Description = classDescriptions.GetText(ac.DescriptionId, obj.Name);
      ac.LocalizedDescription = classDescriptions.GetLocalizedText(ac.DescriptionId, obj.Description);
      ac.ClassSpecId = obj.Data.ValueOrDefault<ulong>("chrAdvancedClassDataClassSpec", 0);
      ac.ClassSpec = _dom.ClassSpecLoader.Load(ac.ClassSpecId);
      ac.ClassBackground = obj.Data.ValueOrDefault<string>("chrAdvancedClassBackground");

      var ablPackagePrototype = _dom.GetObject("ablPackagePrototype");
      if (ablPackagePrototype == null) {
        return ac;
      }
      var classDisciplinesTable = ablPackagePrototype.Data.ValueOrDefault<Dictionary<object, object>>("classDisciplinesTable");
      var disUtilityTable = ablPackagePrototype.Data.ValueOrDefault<Dictionary<object, object>>("disUtilityTable");
      var disDiscTable = ablPackagePrototype.Data.ValueOrDefault<Dictionary<object, object>>("disDiscTable");
      var classBaseTable = ablPackagePrototype.Data.ValueOrDefault<Dictionary<object, object>>("classBaseTable");
      var classPcTable = ablPackagePrototype.Data.ValueOrDefault<Dictionary<object, object>>("classBaseTable");
      ablPackagePrototype.Unload();

      // Class Disciplines Table, has all the Combat Styles (formerly Advanced Classes)
      if (classDisciplinesTable.ContainsKey(ac.Id)) {
        var discData = ((List<object>)classDisciplinesTable[ac.Id]).ConvertAll(x => (GomObjectData)x);

        ac.Disciplines = new List<Discipline>();
        foreach (var disc in discData) {
          Discipline dis = new Discipline();
          _dom.DisciplineLoader.Load(dis, disc);
          ac.Disciplines.Add(dis);
        }
        ac.Disciplines = ac.Disciplines.OrderBy(x => x.SortIdx).ToList();

        // Discipline Utility Table
        if (disUtilityTable != null) { // pre-7.0
          if (disUtilityTable.ContainsKey(ac.Id)) {
            var entry = (GomObjectData)disUtilityTable[ac.Id];
            ac.UtiltyPkgId = entry.ValueOrDefault<ulong>("disApcId");
            ac.UtilPkgIsActive = entry.ValueOrDefault<bool>("disUtilPkgActive");

            var backupNameId = entry.ValueOrDefault<long>("className") + 2031339142381568;
            var unusedStringId = entry.ValueOrDefault<long>("disName") + 2031339142381568;
            var nameTable = _dom.StringTable.Find("str.gui.abl.player.skill_trees");
            string backupName = nameTable.GetText(backupNameId, "str.gui.abl.player.skill_trees");
            string unusedString = nameTable.GetText(unusedStringId, "str.gui.abl.player.skill_trees");
          }
        }
        // Class Base Table
        if (classBaseTable.ContainsKey(ac.Id)) {
          ac.AdvancedClassPkgIds = new List<ulong>();
          var entries = ((List<object>)classBaseTable[ac.Id]).ConvertAll(x => (GomObjectData)x);
          foreach (var entry in entries) {
            ac.AdvancedClassPkgIds.Add(entry.ValueOrDefault<ulong>("disApcId"));
          }

          ac.BaseClassPkgIds = new List<ulong>();
          if (classBaseTable.ContainsKey(ac.ClassSpecId)) {
            entries = ((List<object>)classBaseTable[ac.ClassSpecId]).ConvertAll(x => (GomObjectData)x);
            foreach (var entry in entries) {
              ac.BaseClassPkgIds.Add(entry.ValueOrDefault<ulong>("disApcId"));
            }
          } else {
            // Post 7.0, class.pc.sith_warrior, class.pc.smuggler, and class.pc.sith_sorcerer are missing from the classBaseTable
            // so hardcode apc.pc_default into ac.BaseClassPkdIds for those
            if (ac.BaseClassPkgIds.Count == 0) {
              ac.BaseClassPkgIds.Add(16140935400633136849);
            }
          }
          // For post 7.0, if missing the base class package, add it from class node
          if (ac.BaseClassPkgIds.Count < 2) {
            ac.BaseClassPkgIds.Add(ac.ClassSpec.AbilityPackageId);
          }
        }
      }

      return ac;
    }
  }
}
