using System;
using System.Collections.Generic;
using System.Linq;
using GomLib.Models;

namespace GomLib.ModelLoader {
  public class AchievementCatData {
    public AchievementCatData(AchievementCategory cat,
                              AchievementCategory subcat,
                              AchievementCategory tertcat,
                              Int32 r,
                              Int32 p,
                              Boolean da) {
      Category = cat;
      SubCategory = subcat;
      TertiaryCategory = tertcat;
      Row = r;
      Position = p;
      DrawArrow = da;
    }
    public AchievementCategory Category { get; set; }
    public AchievementCategory SubCategory { get; set; }
    public AchievementCategory TertiaryCategory { get; set; }
    public Int32 Row { get; set; }
    public Int32 Position { get; set; }
    public Boolean DrawArrow { get; set; }
    //public bool RequiresPrevious { get; set; }
  }

  public class AchievementLoader : IModelLoader {
    private readonly DataObjectModel _dom;
    Dictionary<UInt64, Achievement> idMap;
    Dictionary<String, Achievement> nameMap;
    // Dictionary<String, Achievement> unknownMap;

    Dictionary<UInt64, AchievementCatData> CategoryMap { get; set; }
    public String ClassName { get => "achAchievement"; }

    const Int64 DescLookupKey = 2806211896052149513;
    const Int64 NameLookupKey = -2761358831308646330;
    const Int64 UnknownLookupKey = 814171245593979527;

    public AchievementLoader(DataObjectModel dom) {
      _dom = dom;
      if (nameMap == null) {
        Flush();
      }
    }

    public GameObject CreateObject() => new Achievement();

    public void Flush() {
      idMap = new Dictionary<UInt64, Achievement>();
      nameMap = new Dictionary<String, Achievement>();
      // unknownMap = new Dictionary<String, Achievement>();
      CategoryMap = new Dictionary<UInt64, AchievementCatData>();
    }
    private Dictionary<String, String> LegacyTitleLookup(Int64 legacyTitleField) {
      GomObject titleTable = _dom.GetObject("lgcLegacyTitlesTablePrototype");
      Dictionary<Object, Object> titleLookupList =
        titleTable.Data.Get<Dictionary<Object, Object>>("lgcLegacyTitlesData");
      titleLookupList.TryGetValue(legacyTitleField, out Object titleTextLookup);

      if (titleTextLookup != null) {
        Int64 titleId =
          ((GomObjectData)titleTextLookup).ValueOrDefault<Int64>("lgcLegacyTitleString");
        return _dom.StringTable.TryGetLocalizedStrings("str.pc.legacytitle", titleId);
      } else {
        return new Dictionary<String, String> {
          { "enMale", "" },
          //{ "enFemale", "" },
          { "frMale", "" },
          { "frFemale", "" },
          { "deMale", "" },
          { "deFemale", "" },
        };
      }
    }
    public Achievement Load(GameObject obj, GomObject gom) {
      if (gom == null) return (Achievement)obj;
      if (obj == null) return null;

      Achievement ach = obj as Achievement;

      if (CategoryMap.Count == 0) {
        AchievementCategory rootCat = _dom.AchievementCategoryLoader.Load(0);

        if (rootCat != null) {
          foreach (Int64 mainCatId in rootCat.SubCategories) { // things like location, events, etc
            AchievementCategory mainCat = _dom.AchievementCategoryLoader.Load(mainCatId);

            if (mainCat.Rows.Count > 0) {
              // String soinon = ""; //main cats shouldn't have these
            }

            foreach (Int64 subCatId in mainCat.SubCategories) { // planets, areas, etc
              AchievementCategory subCat = _dom.AchievementCategoryLoader.Load(subCatId);

              if (subCat.Rows.Count > 0) {
                // String soinon = ""; //sub cats shouldn't have these
              }

              foreach (Int64 tertCatId in subCat.SubCategories) { //final category
                AchievementCategory tertCat = _dom.AchievementCategoryLoader.Load(tertCatId);
                if (tertCat.SubCategories.Count != 0) {
                  // String nested = ""; //nested cats?
                }

                if (tertCat.Rows.Count != 0) {
                  for (Int32 r = 0; r < tertCat.Rows.Count; r++) {
                    for (Int32 c = 0; c < tertCat.Rows[r].Count; c++) {
                      Boolean da = tertCat.Rows[r][c].DrawArrow;
                      AchievementCatData dat =
                        new AchievementCatData(mainCat, subCat, tertCat, r, c, da);

                      if (!CategoryMap.ContainsKey(tertCat.Rows[r][c].Id)) {
                        CategoryMap.Add(tertCat.Rows[r][c].Id, dat);
                      }

                      //if (!CategoryMap.ContainsKey(tertCat.Rows[r][c].DrawArrow))
                      //{
                      //    CategoryMap.Add(tertCat.Rows[r][c].DrawArrow, dat);
                      //}
                    }
                  }
                } else {
                  // String hmmm = ""; //empty final category?
                }
              }
            }
          }
        }
      }

      CategoryMap.TryGetValue(gom.Id, out AchievementCatData blah);
      ach.CategoryData = blah;

      ach.Fqn = gom.Name;
      ach.NodeId = gom.Id;
      ach.Dom_ = _dom;
      ach.References = gom.References;

      // Achievement Info
      ach.Icon = gom.Data.ValueOrDefault<String>("achIcon", null);
      _dom.Assets.Icons.Add(ach.Icon);

      // Wrong Way - When you read a value with Get or ValueOrDefault, you have to use the actual 
      // <type> the value is stored in
      // ach.Visibility = 
      //   obj.Data.ValueOrDefault<AchievementVisibility>(
      //     "achVisibility", 
      //     AchievementVisibility.Always
      //   ); // 4611686344448990000

      // Right Way - Then once read you can cast to your own type like so:
      ach.Visibility = //4611686344448990000
        (AchievementVisibility)gom.Data.ValueOrDefault("achVisibility", new ScriptEnum()).Value;
      // Or you could store the ScriptEnum in ach.Visibility and cast the value to your custom enum
      // at output time.

      ach.AchId = gom.Data.ValueOrDefault<Int64>("achId", 0);
      ach.Rewards = null;

      if (gom.Data.ContainsKey("achRewardId")) {
        ach.RewardsId = gom.Data.Get<Int64>("achRewardId");
        GomObject rewardsTable = _dom.GetObject("achRewardsTable_Prototype");
        // Fix this to be a reference to somehwere so we don't have to load it each time to read
        // one value.
        Dictionary<Object, Object> rewardsLookupList =
          rewardsTable.Data.Get<Dictionary<Object, Object>>("achRewardsData");

        if (rewardsLookupList.TryGetValue(ach.RewardsId, out Object rawRewardsObj)) {
          GomObjectData rawRewards = rawRewardsObj as GomObjectData;
          ach.Rewards = new Rewards();

          var achievementPoints = rawRewards.ValueOrDefault<Int64>("achRewardPoints", 0);
          ach.Rewards.AchievementPoints = achievementPoints;
          var cartelCoins = rawRewards.ValueOrDefault<Int64>("achRewardCartelCoins", 0);
          ach.Rewards.CartelCoins = cartelCoins;

          var legacyTitleField = rawRewards.ValueOrDefault<Int64>("achRewardLegacyTitleId", 0);
          ach.Rewards.LocalizedLegacyTitle = new Dictionary<String, String>();
          if (legacyTitleField != 0) {
            ach.Rewards.LocalizedLegacyTitle = LegacyTitleLookup(legacyTitleField);
            if (ach.Rewards.LocalizedLegacyTitle != null)
              ach.Rewards.LegacyTitle = ach.Rewards.LocalizedLegacyTitle[GomLib.StringTable.SelectedLocalization];
          }


          Int64 requisition = rawRewards.ValueOrDefault<Int64>("achRewardFleetRequisition", 0);
          ach.Rewards.Requisition = requisition;
          /*String title = "";
          String codexFqn = "";
          bool prefix = false;
          if (titleField > 0 && titleField < 1000)
          {
              GomObject titleTable = _dom.GetObject("chrPlayerTitlesTablePrototype");
              var titleLookupList = titleTable.Data.Get<List<Object>>("chrPlayerTitlesMapping");
              var titleTextLookup = (GomObjectData)titleLookupList[Convert.ToInt32(titleField) - 1];
              var titleId = titleTextLookup.ValueOrDefault<Int64>("titleDetailStringID", -1);
              title = _dom.stringTable.TryGetString("str.pc.title", titleId);
              var titleCodexId = titleTextLookup.ValueOrDefault<UInt64>("titleCodex", 0);
              GomObject codex = _dom.GetObject(titleCodexId);
              prefix = titleTextLookup.ValueOrDefault<bool>("titleDetailLegacyPrefix", false);
          }
          else if (titleField > 1000)
          {
              GomObject titleTable = _dom.GetObject("chrPlayerTitlesTablePrototype");
              var titleLookupList = titleTable.Data.Get<List<Object>>("chrPlayerTitlesMapping");
              var titleTextLookup = (GomObjectData)titleLookupList[Convert.ToInt32(titleField)];
              var titleId = titleTextLookup.ValueOrDefault<Int64>("titleDetailStringID", -1);
              title = _dom.stringTable.TryGetString("str.pc.title", titleId);
              var titleCodexId = titleTextLookup.ValueOrDefault<UInt64>("titleCodex", 0);
              GomObject codex = _dom.GetObject(titleCodexId);
              prefix = titleTextLookup.ValueOrDefault<bool>("titleDetailLegacyPrefix", false);
          }*/

          // TODO: This is not working, no items are being read.
          List<Object> itemRew = rawRewards.Get<List<Object>>("achRewardItems");
          ach.Rewards.ItemRewardList = new Dictionary<UInt64, Int64>();

          foreach (var gomDat in itemRew) {
            Int64 quant = ((GomObjectData)gomDat).Get<Int64>("achRewardItemQty");
            UInt64 itemId = ((GomObjectData)gomDat).Get<UInt64>("achRewardItemId");
            GomObject rew = _dom.GetObject(itemId);

            /*if (rew.Name.Contains("itm.stronghold.") && !rew.Name.Contains(".trophy.") && !rew.Name.Contains("datacron_master_display")) //obsolete debugging code
            {
                String paushere = "";
            }*/

            ach.Rewards.ItemRewardList.Add(itemId, quant);
          }
        }

        rewardsTable.Unload();
      }

      Dictionary<Object, Object> textLookup =
        gom.Data.Get<Dictionary<Object, Object>>("locTextRetrieverMap");

      // Load Achievement Name
      GomObjectData nameLookupData = (GomObjectData)textLookup[NameLookupKey];
      ach.NameId = nameLookupData.Get<Int64>("strLocalizedTextRetrieverStringID");
      ach.LocalizedName = _dom.StringTable.TryGetLocalizedStrings(ach.Fqn, nameLookupData);
      Normalize.Dictionary(ach.LocalizedName, ach.Fqn);
      ach.Name = _dom.StringTable.TryGetString(ach.Fqn, nameLookupData);

      // Load Achievement Description
      GomObjectData descLookupData = (GomObjectData)textLookup[DescLookupKey];
      ach.DescriptionId = descLookupData.Get<Int64>("strLocalizedTextRetrieverStringID");
      ach.LocalizedDescription = _dom.StringTable.TryGetLocalizedStrings(ach.Fqn, descLookupData);
      ach.Description = _dom.StringTable.TryGetString(ach.Fqn, descLookupData);

      GomObjectData nonSpoilerData = (GomObjectData)textLookup[UnknownLookupKey];
      ach.NonSpoilerId = nonSpoilerData.Get<Int64>("strLocalizedTextRetrieverStringID");
      ach.LocalizedNonSpoilerDesc =
        _dom.StringTable.TryGetLocalizedStrings(ach.Fqn, nonSpoilerData);
      ach.NonSpoilerDesc = _dom.StringTable.TryGetString(ach.Fqn, nonSpoilerData);
      ach.Id = gom.Id; // (UInt64)(ach.NameId >> 32);

      // Conditions
      List<Object> conditionLookup = gom.Data.ValueOrDefault<List<Object>>("achConditions", null);
      ach.Conditions = new List<AchCondition>();

      if (conditionLookup != null) {
        foreach (Object cond in conditionLookup) {
          GomObjectData condLookupData = (GomObjectData)cond;
          AchCondition tmpCondition = new AchCondition {
            UnknownBoolean = condLookupData.ValueOrDefault("4611686294605190001", false), 
            // is only set true on kill achievements
            // All Unknown13 type achievements (except a test one) has this set true
            // Player Faction restricted kill achievements have this set false
            Type =
              (AchConditionType)condLookupData.ValueOrDefault(
                "achConditionType",
                new ScriptEnum()
              ).Value,
            // 13 - player/non-faction npc kills
            // 
            Target =
              (AchConditionTarget)condLookupData.ValueOrDefault(
                "achConditionTarget",
                new ScriptEnum()
              ).Value
          };

          // if (tmpCondition.Type == AchConditionType.Unknown13 
          //     || tmpCondition.Type == AchConditionType.Faction 
          //     || tmpCondition.UnknownBoolean)
          // break;
          // TODO: read all the other condition fields

          ach.Conditions.Add(tmpCondition);
          //Debug.WriteLine(ach.Fqn);
        }
      }

      // Initialize the Tasks
      Dictionary<Object, Object> tasksLookup =
        gom.Data.Get<Dictionary<Object, Object>>("achTasks"); //add a task loader
      ach.Tasks = new List<AchTask>();

      foreach (Object task in tasksLookup.Keys) {
        GomObjectData taskLookupData = (GomObjectData)tasksLookup[task];
        Dictionary<Object, Object> subtasks =
          taskLookupData.Get<Dictionary<Object, Object>>("achTaskSubtasks");
        Dictionary<Object, Object> events =
          taskLookupData.Get<Dictionary<Object, Object>>("achTaskEvents");

        String taskName = "";
        Dictionary<String, String> localizedTaskName = new Dictionary<String, String>();

        if (textLookup.ContainsKey(task)) {
          GomObjectData taskNameObj = (GomObjectData)textLookup[task];
          localizedTaskName = _dom.StringTable.TryGetLocalizedStrings(ach.Fqn, taskNameObj);
          taskName = _dom.StringTable.TryGetString(ach.Fqn, taskNameObj);
        }

        // When achTaskSubtasks is empty:
        // * This task consists of only one task
        // * This task needs to be completed as many times as given in achTaskTotal
        // * Completing any of the events in achTaskEvents will count toward this task
        if (subtasks.Count == 0) {
          AchTask tmpTask = new AchTask {
            Index = (Int64)task,
            Count = taskLookupData.Get<Int64>("achTaskTotal"),
            Events = new List<AchEvent>()
          };

          foreach (var curEvent in events) {
            AchEvent tmpEvent = new AchEvent {
              Id = (UInt64)(Int64)curEvent.Key
            };
            tmpEvent.CheckNodeRef(_dom);
            tmpEvent.Value = (Int64)curEvent.Value;
            tmpTask.Events.Add(tmpEvent);
          }
          tmpTask.Name = taskName;
          tmpTask.LocalizedNames = localizedTaskName;
          ach.Tasks.Add(tmpTask);
        }
        // When achTaskSubtasks is not empty:
        // * This task consists of multiple subtasks
        // * Each entry in achTaskSubtasks stands for one subtask
        // * Each subtask also has an entry in achTaskEvents
        // * The values of achTaskSubtasks indicate the order of the subtasks
        // * Each entry in achTaskSubtasks needs to be completed exactly once
        // * achTaskTotal is a bitflag used by the game to check whether the achievement is completed
        // * achTaskObjectives may have more information to describe the subtask
        else {
          foreach (var curSubtask in subtasks) {
            AchTask tmpTask = new AchTask {
              Index = (Int64)task,
              Index2 = (Int64)curSubtask.Value,
              Count = 1L,
              Id = (UInt64)(Int64)curSubtask.Key,
              Events = new List<AchEvent>()
            };
            AchEvent tmpEvent = new AchEvent {
              Id = (UInt64)(Int64)curSubtask.Key
            };
            tmpEvent.CheckNodeRef(_dom);

            foreach (var curEvent in events) {
              if (curEvent.Key == curSubtask.Key) {
                tmpEvent.Value = (Int64)curEvent.Value;
                break;
              }
            }

            tmpTask.Events.Add(tmpEvent);
            tmpTask.Name = taskName;
            tmpTask.LocalizedNames = localizedTaskName;
            ach.Tasks.Add(tmpTask);
          }
        }
      }

      ach.Tasks = ach.Tasks.OrderBy(x => x.Index).ThenBy(x => x.Index2).ToList();
      gom.Unload();
      return ach;
    }
    public Achievement Load(GomObject obj) => Load(new Achievement(), obj);

    public Achievement Load(String fqn) {
      if (nameMap.TryGetValue(fqn, out Achievement result)) return result;
      else return Load(new Achievement(), _dom.GetObject(fqn));
    }
    public Achievement Load(UInt64 nodeId) {
      if (idMap.TryGetValue(nodeId, out Achievement result)) return result;
      else return Load(new Achievement(), _dom.GetObject(nodeId));
    }

    public void LoadObject(GameObject loadMe, GomObject obj) => Load((Achievement)loadMe, obj);

    public void LoadReferences(GameObject obj, GomObject gom) {
      // No references to load
    }
  }
}
