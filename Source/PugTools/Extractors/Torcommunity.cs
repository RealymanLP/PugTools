using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
// using System.Net;
using System.Text;
using System.Text.RegularExpressions;
// using System.Threading;
using System.Windows.Forms;
using GomLib;
using GomLib.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PugTools {
  internal partial class Tools : Form {
    private JProperty AbilityPackageToMinifiedJSON(String name,
                                                   AbilityPackage ablPkg,
                                                   Boolean removeEmptyValues) {

      return new JProperty(
        name,
        new JArray(
          ablPkg.PackageAbilities/*.Where(x => !x.Ability.IsHidden)*/.Select(
            x => AbilityToMinifiedJSON(
              x.Ability,
              x,
              removeEmptyValues
            )
          ).Where(
            x => x != null
          ).Concat(
            ablPkg.PackageTalents.Select(
              x => TalentToMinifiedJSON(
                x.Talent,
                (Int32)x.Level
              )
            )
          ).OrderBy(
            x => x.Value<Int32>("Level")
          ).ThenBy(
            x => x.Value<String>("Name")
          )
        )
      );
    }
    private JProperty AbilityPackageToMinifiedJSON(String name, AbilityPackage ablPkg) {
      return AbilityPackageToMinifiedJSON(name, ablPkg, false);
    }
    private static JProperty AbilityTokensToMinifiedJSON(
      Dictionary<Int32, Dictionary<String, Object>> descTokens) {

      if (descTokens == null) return new JProperty(new JProperty("Tokens"));

      JProperty jDescTokens =
        new JProperty(
          "Tokens",
          new JArray(
            descTokens.Select(
              x => new JObject(
                new JProperty("TokenId", x.Key),
                new JProperty(
                  "TokenData",
                  x.Value.ContainsKey("ablParsedDescriptionToken")
                    ? JsonConvert.SerializeObject(x.Value["ablParsedDescriptionToken"])
                    : ""
                ),
                new JProperty(
                  "TokenType",
                  x.Value.ContainsKey("ablDescriptionTokenType")
                    ? x.Value["ablDescriptionTokenType"].ToString().Replace(
                      "ablDescriptionTokenType",
                      ""
                    )
                    : ""
                )
              )
            )
          )
        );

      return jDescTokens;
    }
    private JObject AbilityToMinifiedJSON(Ability abl,
                                          PackageAbility pAbl,
                                          Boolean removeEmptyValues) {

      OutputIcon(abl.Icon);
      List<Int32> ranks = new List<Int32>();
      Boolean scales = false;
      Int32 level;

      if (pAbl != null) {
        level = pAbl.Level;
        ranks = pAbl.Levels;
        scales = pAbl.Scales;
      } else
        level = abl.Level;


      JObject ablObj =
        new JObject(
          new JProperty("Name", new JValue(abl.Name ?? "")),
          new JProperty("Description", new JValue(abl.Description ?? "")),
          new JProperty("Cooldown", new JValue(abl.Cooldown)),
          new JProperty("CastTime", new JValue(abl.CastingTime)),
          new JProperty("ChannelTime", new JValue(abl.ChannelingTime)),
          new JProperty("Range", new JValue((int)(abl.MaxRange * 10))),
          new JProperty("Cost", new JValue((int)(abl.ApCost + abl.EnergyCost + abl.ForceCost))),
          new JProperty("Icon", new JValue(GetIconFilename(abl.Icon ?? "").Replace("'", ""))),
          new JProperty("Level", new JValue(level)),
          new JProperty("IsUtilityPoint", new JValue(abl.Name == null)),
          new JProperty("IsHighlighted", new JValue(!abl.IsPassive)),
          new JProperty("IsHidden", new JValue(abl.IsHidden))
      );

      if (removeEmptyValues) {
        ablObj.Remove("IsUtilityPoint");
        ablObj.Remove("IsHighlighted");
        ablObj.Remove("IsHidden");

        if (ablObj.Value<Single>("Cooldown") == 0.0)
          ablObj.Remove("Cooldown");

        if (ablObj.Value<Single>("CastTime") == 0.0)
          ablObj.Remove("CastTime");

        if (ablObj.Value<Single>("ChannelTime") == 0.0)
          ablObj.Remove("ChannelTime");

        if (ablObj.Value<Int32>("Cost") == 0)
          ablObj.Remove("Cost");

        if (ablObj.Value<Int32>("Range") == 0)
          ablObj.Remove("Range");

        if (ablObj.Value<Int32>("Level") == 1)
          ablObj.Remove("Level");

        if (ablObj.Value<String>("Icon") == "")
          ablObj.Remove("Icon");

        if (ablObj.Value<String>("Name") == ""
            && ablObj.Value<String>("Description") == ""
            && ablObj.Properties().Count() == 2)
          return null;
      }

      if (abl.DescriptionTokens != null)
        ablObj.Add(AbilityTokensToMinifiedJSON(abl.DescriptionTokens));

      if (ranks.Count > 1 && !scales) ablObj.Add(new JProperty("Ranks", new JArray(ranks)));

      return ablObj;
    }
    private JObject AbilityToMinifiedJSON(Ability abl,
                                          PackageAbility pAbl) {
      return AbilityToMinifiedJSON(abl, pAbl, false);
    }
    private JObject AbilityToMinifiedJSON(Ability abl) {
      return AbilityToMinifiedJSON(abl, null);
    }
    private JObject AdvancedClassToMinifiedJSON(AdvancedClass ac,
                                                String icon,
                                                String className) {

      OutputIcon(ac.ClassBackground);
      Dictionary<Double, JObject> util = new Dictionary<Double, JObject>();
      List<JObject> unusedUtilities = new List<JObject>();

      foreach (PackageAbility pAbl in ac.UtilityPkg.PackageAbilities) {
        Ability abl = pAbl.Ability;
        abl.Level = pAbl.Level;
        Double pos = pAbl.UtilityTier + (Double)(pAbl.UtilityPosition / 100.0);
        util.Add(pos, AbilityToMinifiedJSON(abl));
      }

      foreach (PackageTalent pTal in ac.UtilityPkg.PackageTalents) {
        Talent tal = pTal.Talent;
        Double pos = pTal.UtilityTier + (Double)(pTal.UtilityPosition / 100.0);
        JObject jTal = TalentToMinifiedJSON(tal, (Int32)pTal.Level);

        if (pos == 0) unusedUtilities.Add(jTal);
        else util.Add(pos, jTal);
      }

      /*for (int i = 1; i <= 21; i++)
      {
          if (!util.ContainsKey(i))
              util.Add(i, new JObject(
                  new JProperty("Name", new JValue("Unknown")),
                  new JProperty("Description", new JValue("No Utility with this index")),
                  new JProperty("Icon", new JValue("icon"))
                  ));
      }*/

      Boolean available = true;

      /*switch (ac.Name)
      {
          case "Operative":
          case "Scoundrel":
          case "Gunslinger":
              available = false;
              break;
      }*/

      Boolean utilAvailable = available;

      //if (ac.Name == "Sniper")
      //utilAvailable = false;

      JObject acObj =
        new JObject(
          new JProperty("Name", new JValue(ac.Name)),
          new JProperty("Description", new JValue(ac.Description)),
          new JProperty("Icon", new JValue(GetIconFilename(icon))),
          new JProperty("Available", new JValue(available)),
          new JProperty("UtilitiesAvailable", new JValue(utilAvailable)),
          new JProperty("Background", new JValue(GetIconFilename(ac.ClassBackground))),
          new JProperty("JsonPath", new JValue(ac.Name.Replace(' ', '_'))),
          new JProperty(
            "UtilitiesPath",
            new JValue(string.Format("{0}_utilities", ac.Name.Replace(' ', '_')))
          ),
          new JProperty(
            "Disciplines",
            new JArray(
              from d in ac.Disciplines
              orderby d.SortIdx
              select new JObject(
                new JProperty("Name", new JValue(d.Name)),
                new JProperty("Description", new JValue(d.Description)),
                new JProperty("Role", new JValue(d.Role)),
                new JProperty("Icon", new JValue(GetIconFilename(d.Icon))),
                new JProperty(
                  "Available",
                  new JValue(
                    available
                    /*&&
                    (d.Name == "Engineering") ? false : true
                    &&
                    (d.Name == "Marksmanship") ? false : true*/
                  )
                ),
                new JProperty("JsonPath", new JValue(d.Name.Replace(' ', '_'))),
                new JProperty("BaseSkills",
                  new JArray(
                    from b in d.BaseAbilities
                    orderby b.Level
                    select AbilityToMinifiedJSON(b)
                  )
                )
              )
            )
          )
        );
      JObject utilObj =
        new JObject(
          new JProperty(
            "UtilitySkills",
            new JArray(
              from kvp in util
              orderby kvp.Key
              select kvp.Value
            )
          )
        );

      WriteFile(
        utilObj.ToString(
        /*Newtonsoft.Json.Formatting.None*/
        ),
        string.Format(
          "DiscContent\\Data\\{0}_utilities.json",
          ac.Name.Replace(' ', '_')
        ),
        false
      );

      foreach (var dis in ac.Disciplines) {
        WriteFile(
          DisciplineToMinifiedJSON(dis).ToString(
          /*Newtonsoft.Json.Formatting.None*/
          ),
          string.Format(
            "DiscContent\\Data\\{0}.json",
            dis.Name.Replace(' ', '_')
          ),
          false
        );
      }

      // Generate ability json
      JObject acPkgObj =
        new JObject(
          new JProperty(
            "Name",
            new JValue(ac.Name)
          ),
          new JProperty("BaseClassJsonPath", new JValue(className)),
          AbilityPackageToMinifiedJSON(
            "AdvancedClassPackage",
            ac.AdvancedClassPkgs.Where(x => x.Fqn.Contains("base")).First()
          )
        );

      WriteFile(
        acPkgObj.ToString(
        /*Newtonsoft.Json.Formatting.None*/
        ),
        string.Format(
          "DiscContent\\Data\\{0}.json",
          ac.Name.Replace(' ', '_')
        ),
        false
      );

      JObject cPkgObj =
        new JObject(
          new JProperty("Name", new JValue(className)),
          AbilityPackageToMinifiedJSON(
            "BaseClassPackage",
            ac.BaseClassPkgs.Where(x => x.Fqn.Contains("base")).First()
          ),
          AbilityPackageToMinifiedJSON(
            "General",
            ac.BaseClassPkgs.Where(x => x.Fqn.Contains("default")).First()
          )
        );

      className = className.Replace(' ', '_');
      WriteFile(
        cPkgObj.ToString(
        /*Newtonsoft.Json.Formatting.None*/
        ),
        string.Format(
          "DiscContent\\Data\\{0}.json",
          className
        ),
        false
      );

      return acObj;
    }
    private JObject DisciplineToMinifiedJSON(Discipline dis) {
      OutputIcon(dis.Icon);

      Dictionary<Int32, JObject> path = new Dictionary<Int32, JObject>();

      foreach (PackageAbility pAbl in dis.PathAbilities.PackageAbilities) {
        Ability abl = pAbl.Ability;
        abl.Level = pAbl.Level;
        path[abl.Level] = AbilityToMinifiedJSON(abl);
      }

      foreach (PackageTalent pTal in dis.PathAbilities.PackageTalents) {
        Talent tal = pTal.Talent;
        Int32 level = (Int32)pTal.Level;
        path[level] = TalentToMinifiedJSON(tal, level);
      }

      GomObject disUtilityLevelsPrototype = CurrentDom.GetObject("disUtilityLevelsPrototype");
      Dictionary<Int32, Int32> disUtilityLevelsLookup =
        disUtilityLevelsPrototype.Data.ValueOrDefault<Dictionary<Object, Object>>(
          "disUtilityLevelsLookup"
        ).ToDictionary(
          x => Convert.ToInt32(x.Key), x => Convert.ToInt32(x.Value)
        );
      Int32 f = 1;

      foreach (var lvu in disUtilityLevelsLookup) {
        if (lvu.Value == f) {
          Ability uAbl = new Ability {
            Level = lvu.Key,
            IsPassive = true
          };

          path.Add(lvu.Key, AbilityToMinifiedJSON(uAbl));
          f++;
        }
      }

      JObject acObj =
        new JObject(
          new JProperty("Name", new JValue(dis.Name)),
          new JProperty("Description", new JValue(dis.Description)),
          new JProperty("Icon", new JValue(GetIconFilename(dis.Icon))),
          new JProperty("Role", new JValue(dis.Role)),
          new JProperty(
            "DisciplinePath",
            new JArray(
              from kvp in path
              orderby kvp.Key
              select kvp.Value
            )
          )
        );

      return acObj;
    }
    internal void FindValue() {
      ClearProgress();
      LoadData();

      List<GomObject> gomList = CurrentDom.GetObjectsStartingWith("");
      Int32 count = gomList.Count;
      Int32 i = 0;

      WriteFile("", "nodes.txt", false);

      foreach (GomObject gom in gomList) {
        ProgressUpdate(i, count);

        if (gom.Data != null) {
          foreach (var obj in gom.Data.Dictionary) {
            NewMethod(obj.Key, gom.Name);
            NewMethod(obj.Value, gom.Name);
          }
        }

        gom.Unload();
        i++;
      }
    }
    internal static String GenerateDescWithTokens(Ability skill) {
      String retval = skill.Description;

      if (skill.DescriptionTokens == null) return retval;

      for (Int32 i = 0; i < skill.DescriptionTokens.Count; i++) {
        // Int32 id = skill.DescriptionTokens.ElementAt(i).Key;
        // String value = 
        //   skill.DescriptionTokens.ElementAt(i).Value["ablParsedDescriptionToken"].ToString();
        // String type = 
        //   skill.DescriptionTokens.ElementAt(i).Value["ablDescriptionTokenType"].ToString()
        //     .Replace("ablDescriptionTokenType", "");
        KeyValuePair<Int32, Dictionary<String, Object>> curToken =
          skill.DescriptionTokens.ElementAt(i);
        Int32 id = curToken.Key;
        String value = "";

        if (curToken.Value.ContainsKey("ablParsedDescriptionToken")) {
          value = curToken.Value["ablParsedDescriptionToken"].ToString();
        }

        String type = "";

        if (curToken.Value.ContainsKey("ablDescriptionTokenType")) {
          type = curToken.Value["ablDescriptionTokenType"].ToString().Replace(
            "ablDescriptionTokenType",
            ""
          );
        }

        Int32 start = retval.IndexOf("<<" + id);

        if (start == -1) {
          //console.log("didn't find: <<" + id);
          continue;
        }

        //console.log("id" + id + ":" + retval);
        //console.log("Start Index: " + start);
        Int32 end = retval[start..].IndexOf(">>") + 2;
        //console.log("Length: " +length);
        String fullToken = retval.Substring(start, end);
        //console.log("Full: " + fullToken);
        String durationText = "";

        if ((end - start) > 5) {
          _ = new string[] { "", "", "" };

          String partialToken = fullToken[4..^3];
          //console.log("Partial:" + partialToken);
          String[] durationList = partialToken.Replace("%d", "").Split('/').ToArray();
          //console.log(durationList);

          _ = int.TryParse(value.ToString(), out int pValue);

          if (pValue <= 0)
            durationText = durationList[0];
          else if (pValue > 1)
            durationText = durationList[2];
          else
            durationText = durationList[1];

          //console.log(pValue + durationText);
        }
        //console.log(type);

        // Sometimes there's multiple instance of the same token.
        while (retval.IndexOf(fullToken) != -1) {
          switch (type) {
            case "Healing":
            case "Damage":
              retval = retval.Replace(fullToken, GenerateTokenString(value));
              break;
            case "Duration":
              retval = retval.Replace(fullToken, value + durationText);
              break;
            case "Talent":
              retval = retval.Replace(fullToken, value);
              //console.log("replaced '<<" + id + ">>' :" + retval);
              break;
            default:
              //console.log(type);
              retval = retval.Replace(fullToken, "Unknown Token: " + type);
              break;
          }
        }
      }

      return retval;
    }
    internal static String GenerateTokenString(String value) {
      String[] splitTokens = value.Split(';');

      /*
      if (splitTokens.length == 2)
        retval = splitTokens[1] + " to " + splitTokens[0];
      else
      */

      String retval = splitTokens[0];
      String[] tokArray = splitTokens[0].Split(',');

      switch (tokArray[0]) {
        case "damage":
          if (tokArray.Length == 7) {
            Single minp = Single.Parse(tokArray[5]);
            Single maxp = Single.Parse(tokArray[6]);

            if (Single.Parse(tokArray[4]) == 1)
              retval =
              Math.Round(
                Single.Parse(
                  tokArray[4]) * minp
                ) + "-" + Math.Round(
                  Single.Parse(tokArray[4]) * maxp
                );
            else
              retval = Math.Round(Single.Parse(tokArray[4]) * ((minp + maxp) / 2)).ToString();
          } else {
            switch (tokArray[4]) {
              case "w":
                Double min =
                  (Single.Parse(tokArray[11]) + 1.0) * 405
                    + Single.Parse(tokArray[6]) * 1000
                    + Single.Parse(tokArray[7]) * 3185; /*(AmountModifierPercent + 1) * 405 * 0.3 + */
                Double max =
                  (Single.Parse(tokArray[11]) + 1.0) * 607
                    + Single.Parse(tokArray[6]) * 1000
                    + Single.Parse(tokArray[8]) * 3185; /*(AmountModifierPercent + 1) * 607 * 0.3 + */

                // console.log("(" + tokArray[11] + " + 1.0) * 405 + " + tokArray[6] + " * 1000 + " + tokArray[8]  + " * 3185");

                if (Single.Parse(tokArray[5]) == 1)
                  retval =
                    Math.Round(
                      Single.Parse(tokArray[5]) * min
                    )
                    + "-"
                    + Math.Round(
                      Single.Parse(tokArray[5]) * max
                    );
                else
                  retval =
                    Math.Round(
                      Single.Parse(tokArray[5]) * ((min + max) / 2)
                    ).ToString();

                break;
              case "s":
                Double mins =
                  Single.Parse(tokArray[6]) * 1000
                    + Single.Parse(tokArray[7]) * 3185;
                Double maxs =
                  Single.Parse(tokArray[6]) * 1000
                    + Single.Parse(tokArray[8]) * 3185;
                if (Single.Parse(tokArray[5]) == 1)
                  retval =
                    Math.Round(
                      Single.Parse(tokArray[5]) * mins
                    )
                    + "-"
                    + Math.Round(
                      Single.Parse(tokArray[5]) * maxs
                    );
                else
                  retval =
                    Math.Round(
                      Single.Parse(tokArray[5]) * ((mins + maxs) / 2)
                    ).ToString();

                break;
            }
          }

          break;
        case "healing":
          if (tokArray.Length == 5) {
            Double minh = Single.Parse(tokArray[2]) * 1000 + Single.Parse(tokArray[3]) * 14520;
            Double maxh = Single.Parse(tokArray[2]) * 1000 + Single.Parse(tokArray[4]) * 14520;

            if (Single.Parse(tokArray[1]) == 1)
              retval =
              Math.Round(
                Single.Parse(tokArray[1]) * minh
              )
              + "-"
              + Math.Round(
                Single.Parse(tokArray[1]) * maxh
              );
            else
              retval =
                Math.Round(
                  Single.Parse(tokArray[1]) * ((minh + maxh) / 2)
                ).ToString();
          } else {
            Single mina = Single.Parse(tokArray[2]);
            Single maxa = Single.Parse(tokArray[3]);

            if (Single.Parse(tokArray[1]) == 1)
              retval = Math.Round(
                Single.Parse(tokArray[1]) * mina
              )
              + "-"
              + Math.Round(
                Single.Parse(tokArray[1]) * maxa
              );
            else
              retval = Math.Round(
                Single.Parse(tokArray[1]) * ((mina + maxa) / 2)
              ).ToString();
          }

          break;
      }

      return retval;
    }
    /*
    internal void GetAuctionCats() {
      ClearProgress();
      LoadData();

      AuctionCategory.Load(CurrentDom);

      Dictionary<Int64, AuctionCategory> gomList =
        AuctionCategory.AuctionCategoryList;
      Int32 count = gomList.Count;
      Int32 i = 0;

      WriteFile("", "aucCats.txt", false);

      foreach (var gom in gomList) {
        ProgressUpdate(i, count);
        WriteFile(
          String.Join(
            Environment.NewLine,
            Environment.NewLine + gom.Value.Name + " (" + gom.Value.Id + ")",
            String.Join(
              Environment.NewLine,
              gom.Value.SubCategories.Select(
                x => "  " + x.Value.Name + " (" + x.Value.Id + ")"
              ).ToList()
            )
          ),
          "aucCats.txt",
          true
        );

        i++;
      }
    }
    */
    internal void GetCrewSkillData() {
      Clearlist2();
      ClearProgress();
      LoadData();

      //GomLib.Smart smart = new Smart(addtolist2);
      Smart.LinkSchematics(CurrentDom, AddToList2);

      /*
      var prfBundlesTablePrototype = currentDom.GetObject("prfBundlesTablePrototype");
      List<GomObjectData> prfBundlesTable = 
        prfBundlesTablePrototype.Data.ValueOrDefault<List<object>>(
          "prfBundlesTable"
        ).ConvertAll<GomObjectData>(x => (GomObjectData)x);
      List<JObject> bundles = new List<JObject>();

      foreach (var gom in prfBundlesTable) {
        String profession = gom.ValueOrDefault<ScriptEnum>("prfEnum").ToString();
        Int64 min = gom.ValueOrDefault<Int64>("prfBundleMinLevel");
        Int64 max = gom.ValueOrDefault<Int64>("prfBundleMaxLevel");

        List<Schematic> items = new List<Schematic>();
        List<UInt64> idList = 
          gom.ValueOrDefault<List<Object>>("prfBundleSchemList").ConvertAll<ulong>(x => (UInt64)x);
          
        foreach (var id in idList) {
          var schem = currentDom.schematicLoader.Load(id);
          items.Add(schem);
        }

        items = items.OrderBy(x => (x.Item ?? new Item()).Name).ToList();
        JObject bundle = 
          new JObject(
            new JProperty("Profession", profession),
            new JProperty("min", min),
            new JProperty("max", max),
            new JProperty(
              "Schematics", 
              new JArray(items.Select(x => (x.Item ?? new Item()).Name))
            )
          );
        bundles.Add(bundle);
      }

      JObject output = new JObject(new JProperty("Bundles", new JArray(bundles)));
      */

      List<GomObject> itmList = CurrentDom.GetObjectsStartingWith("schem.");

      Dictionary<String, Dictionary<String, List<Schematic>>> professions =
        new Dictionary<String, Dictionary<String, List<Schematic>>>();
      HashSet<UInt64> materialIds = new HashSet<UInt64>();
      Dictionary<String, HashSet<UInt64>> craftedIds = new Dictionary<String, HashSet<UInt64>>();

      foreach (GomObject gom in itmList) {
        Schematic schem = new Schematic();
        CurrentDom.SchematicLoader.Load(schem, gom);

        if (schem.Deprecated) continue;

        String crewskill = schem.CrewSkill.ToString();
        String subtype = "";

        if (!professions.ContainsKey(crewskill)) {
          professions.Add(crewskill, new Dictionary<String, List<Schematic>>());
          craftedIds.Add(crewskill, new HashSet<UInt64>());
        }

        if (schem.MissionCost > 0)
          subtype = "Missions";
        else {
          subtype = schem.SubTypeName;
          craftedIds[crewskill].Add(schem.ItemId);
          materialIds.UnionWith((schem.Materials ?? new Dictionary<UInt64, Int32>()).Keys);
        }

        if (!professions[crewskill].ContainsKey(subtype))
          professions[crewskill].Add(subtype, new List<Schematic>());

        professions[crewskill][subtype].Add(schem);
      }

      JsonSerializerSettings settings = new JsonSerializerSettings {
        Formatting = Formatting.Indented
      };

      JObject output =
        new JObject(
          new JProperty(
            "Professions",
            new JArray(
              from c in professions
              orderby c.Key
              select new JObject(
                new JProperty("Name", Regex.Replace(c.Key, "([a-z]|[A-Z]{2,})([A-Z])", @"$1 $2")),
                new JProperty("JsonPath", c.Key),
                new JProperty(
                  "Subtypes",
                  new JArray(
                    from s in c.Value
                    orderby s.Key
                    select new JObject(
                      new JProperty(
                        "Name",
                        Regex.Replace(s.Key, "([a-z]|[A-Z]{2,})([A-Z])", @"$1 $2")
                      ),
                      new JProperty("Count", s.Value.Count)
                    )
                  )
                )
              )
            )
          )
        );

      WriteFile(output.ToString(), "PrfContent\\Data\\crewskills.json", false);

      foreach (var c in professions) {
        output =
          new JObject(
            from s in c.Value
            select new JProperty(
              Regex.Replace(s.Key, "([a-z]|[A-Z]{2,})([A-Z])", @"$1 $2"),
              new JArray(
                from schem in s.Value
                select SchematicToMinifiedJSON(schem)
              )
            )
          );

        WriteFile(
          output.ToString(Formatting.None),
          String.Format("PrfContent\\Data\\{0}.json", c.Key),
          false
        );
      }

      output =
        new JObject(
          from id in materialIds
          select new JProperty(
            id.ToMaskedBase62(),
            ItemToMinifiedJSON(CurrentDom.ItemLoader.Load(id))
          )
        );

      WriteFile(
        output.ToString(
          Formatting.None
        ),
        "PrfContent\\Data\\prfMaterials.json",
        false
      );

      foreach (var kvp in craftedIds) {
        if (kvp.Value.Count == 0) continue;

        var jDict =
          kvp.Value.Select(
            x => new KeyValuePair<UInt64, JObject>(
              x,
              ItemToMinifiedJSON(CurrentDom.ItemLoader.Load(x))
            )
          );

        output =
          new JObject(
            kvp.Value.Select(
              x => new JProperty(
                x.ToMaskedBase62(),
                ItemToMinifiedJSON(
                  CurrentDom.ItemLoader.Load(x)
                )
              )
            )
          );

        WriteFile(
          output.ToString(Formatting.None),
          string.Format("PrfContent\\Data\\{0}_Items.json", kvp.Key),
          false
        );
      }
    }
    /*
    internal void GetDBOutput() {
      Clearlist2();
      LoadData();
    }
    */
    internal void GetDisciplineCalcData() {
      Clearlist2();
      ClearProgress();
      LoadData();

      GomObject chrAdvancedClassDataPrototype =
        CurrentDom.GetObject("chrAdvancedClassDataPrototype");
      Dictionary<UInt64, List<UInt64>> chrAdvancedClassSetPerClass =
        chrAdvancedClassDataPrototype.Data.ValueOrDefault<Dictionary<Object, Object>>(
          "chrAdvancedClassSetPerClass"
        ).ToDictionary(
          x => (UInt64)x.Key, x => (
            (Dictionary<Object, Object>)x.Value
          ).Keys.ToList().ConvertAll(z => (UInt64)z)
        );
      Dictionary<String, JObject> impClasses = new Dictionary<String, JObject>();
      Dictionary<String, JObject> repClasses = new Dictionary<String, JObject>();
      StringTable nameTable = CurrentDom.StringTable.Find("str.gui.classnames");

      foreach (var baseClass in chrAdvancedClassSetPerClass) {
        GomObject baseClassData = CurrentDom.GetObject(baseClass.Key);
        Int64 baseClassNameId = baseClassData.Data.ValueOrDefault<Int64>("chrClassDataNameId");

        baseClassData.Unload();

        String name = nameTable.GetText(baseClassNameId, "str.gui.classnames");
        String resource = "";

        switch (name) {
          case "Jedi Consular":
          case "Sith Inquisitor":
            resource = "Force";
            break;
          case "Trooper":
            resource = "Ammo";
            break;
          case "Jedi Knight":
            resource = "Focus";
            break;
          case "Sith Warrior":
            resource = "Rage";
            break;
          case "Smuggler":
          case "Imperial Agent":
            resource = "Energy";
            break;
          case "Bounty Hunter":
            resource = "Heat";
            break;
          default:
            Debug.WriteLine("WTF CLASS IS THIS!?!");
            break;
        }

        List<AdvancedClass> acs = new List<AdvancedClass>();

        foreach (UInt64 acId in baseClass.Value) {
          GomObject advClassData = CurrentDom.GetObject(acId);
          AdvancedClass ac = CurrentDom.AdvancedClassLoader.Load(advClassData);
          advClassData.Unload();
          acs.Add(ac);
        }

        acs.Sort((x, y) => string.Compare(x.Name.Replace(" ", ""), y.Name.Replace(" ", "")));

        String icon = "icon";

        switch (name) {
          case "Jedi Knight":
          case "Trooper":
            icon = "republic";
            break;
          case "Jedi Consular":
          case "Smuggler":
            acs.Reverse();
            icon = "republic";
            break;
          case "Sith Inquisitor":
          case "Sith Warrior":
          case "Bounty Hunter":
          case "Imperial Agent":
            icon = "empire";
            break;
        }

        OutputIcon(icon);

        Boolean available = true;

        /*
        switch (name) {
          case "Trooper":
          case "Bounty Hunter":
          case "Sith Inquisitor":
          case "Jedi Consular":
          case "Imperial Agent":
            available = true;
            break;
        }
        */

        JObject classObj =
          new JObject(
            new JProperty("ClassName", new JValue(name)),
            new JProperty("Available", new JValue(available)),
            new JProperty("Icon", new JValue(GetIconFilename(icon))),
            new JProperty("Resource", new JValue(resource)),
            new JProperty(
              "AdvancedClasses",
              new JArray(
                from ac in acs
                // orderby ac.Name
                select AdvancedClassToMinifiedJSON(ac, icon, name)
              )
            )
          );

        switch (name.Split(' ')[0]) {
          case "Jedi":
          case "Smuggler":
          case "Trooper":
            repClasses.Add(name, classObj);
            break;
          case "Sith":
          case "Bounty":
          case "Imperial":
            impClasses.Add(name, classObj);
            break;
          default:
            Debug.WriteLine("WTF CLASS IS THIS!?!");
            break;
        }

      }

      JObject output =
        new JObject(
          new JProperty(
            "Imperial",
            new JArray(
              impClasses["Bounty Hunter"],
              impClasses["Imperial Agent"],
              impClasses["Sith Inquisitor"],
              impClasses["Sith Warrior"]
              /*from c in impClasses
                  orderby c.Value<string>("ClassName")
                  select c*/
            )
          ),
          new JProperty(
            "Republic",
            new JArray(
              repClasses["Trooper"],
              repClasses["Smuggler"],
              repClasses["Jedi Consular"],
              repClasses["Jedi Knight"]
              /*from c in repClasses
              orderby c.Value<string>("ClassName")
              select c*/
            )
          )
        );

      WriteFile(
        output.ToString(
        /*Newtonsoft.Json.Formatting.None*/
        ),
        "DiscContent\\Data\\Classes.json",
        false
      );

      /*
      String path = String.Join("", Config.ExtractPath, "DiscContent\\Data\\");
      foreach (var file in System.IO.Directory.EnumerateFiles(String.Join(path), "*")) {
          CreateGzip(file.Replace(Config.ExtractPath, ""));
          System.IO.File.Delete(file);
      }*/
    }
    /*
    internal void GetitemIds() {
      ClearProgress();
      LoadData();

      List<GomObject> gomList = CurrentDom.GetObjectsStartingWith("itm.");
      Int32 count = gomList.Count;
      Int32 i = 0;

      WriteFile("", "itemIds.txt", false);

      foreach (GomObject gom in gomList) {
        ProgressUpdate(i, count);
        Item itm = CurrentDom.itemLoader.Load(gom);
        WriteFile(
          String.Format(
            "{0}: http://torcommunity.com/db/{1}{2}",
            itm.Name,
            itm.Base62Id,
            Environment.NewLine
          ),
          "itemIds.txt",
          true
        );

        i++;
      }
    }
    */

    internal void GetTorc() {
      Clearlist2();
      FindValue();
      /* TEMP /////////////////////////////////////////////
      var newLines = File.ReadAllLines("i:\\new.txt");
      HashSet<string> newHash = new HashSet<string>();
      newHash.UnionWith(newLines);

      var oldLines = File.ReadAllLines("i:\\old.txt");
      HashSet<string> oldHash = new HashSet<string>();
      oldHash.UnionWith(oldLines);

      newHash.RemoveWhere(x => oldHash.Contains(x));

      StringBuilder t = new StringBuilder();

      t.Append(String.Join(Environment.NewLine, newHash));
      WriteFile(t.ToString(), "unique.txt", false);

      ///////////////////////////////////////// TEMP END */

      LoadData();
      GroupFinder();
      GetDisciplineCalcData();
      GetCrewSkillData();
      OutputTables();
      //getTooltips();
      //getAuctionCats();
      //getitemIds();
      //torheadscanner();
      EnableButtons();
    }
    internal void GroupFinder() {
      ClearProgress();
      LoadData();

      CurrentDom.GroupFinderContentData.Load(0); //dummy to load data

      Int32 count = CurrentDom.GroupFinderContentData.GroupFinderLookup.Count;
      Int32 i = 0;

      WriteFile("", "gfDat.txt", false);

      StringBuilder txter = new StringBuilder();
      Dictionary<GroupFinderTime, String> opsList =
        new Dictionary<GroupFinderTime, String>();

      foreach (var kvp in CurrentDom.GroupFinderContentData.GroupFinderLookup) {
        ProgressUpdate(i, count);

        if (kvp.Value.Times != null) {
          foreach (var time in kvp.Value.Times) {
            opsList.Add(time, kvp.Value.Name);
          }
        }

        i++;
      }

      opsList = opsList.OrderBy(x => x.Key.StartTime.Date).ToDictionary(x => x.Key, x => x.Value);

      foreach (var kvp in opsList) {
        txter.Append(
          String.Format(
            "Start: {0}; End: {1}; Name: {2}{3}",
            kvp.Key.StartTime.ToString(),
            kvp.Key.EndTime.ToString(),
            kvp.Value,
            Environment.NewLine
          )
        );
      }

      WriteFile(txter.ToString(), "gfDat.txt", false);
    }
    private JObject ItemToMinifiedJSON(Item item) {
      if (item != null) OutputIcon(item.Icon, "PrfContent");
      if (item == null) return new JObject();

      JObject jItm =
        new JObject(
          new JProperty("Name", new JValue(item.Name ?? "")),
          new JProperty(
            "Description",
            new JValue(
              (item.Description ?? "").Replace(
                "\r\n", "<br />"
              ).Replace(
                "\n", "<br />"
              ).Replace(
                "\r", "<br />"
              )
            )
          ),
          new JProperty("Quality", new JValue(item.Quality.ToString())),
          new JProperty("Icon", new JValue(GetIconFilename(item.Icon ?? "").Replace("'", ""))),
          new JProperty("BaseLevel", new JValue(item.ItemLevel)),
          new JProperty("CombinedLevel", new JValue(item.CombinedRating))
        );

      if (item.RequiredLevel != 0)
        jItm.Add(new JProperty("MinLevel", new JValue(item.RequiredLevel)));

      if (item.EquipAbilityId != 0)
        jItm.Add(
          new JProperty("EquipAbility", new JValue(GenerateDescWithTokens(item.EquipAbility)))
        );

      if (item.UseAbilityId != 0)
        jItm.Add(new JProperty("UseAbility", new JValue(GenerateDescWithTokens(item.UseAbility))));

      if (item.StatModifiers.ToString() != "Empty List") {
        String list = item.StatModifiers.ToString();
        list = list[0..^1];
        jItm.Add(new JProperty("Stats", new JArray(list.Split(','))));
      }

      // if (item.DisassembleCategory != null)
      jItm.Add(
        new JProperty("DissassembleCategory",
        new JValue(item.DisassembleCategory.ToString()))
      );

      if (item.References.ContainsKey("createdBy")) {
        JArray variants = new JArray();
        if (item.References["createdBy"].Count > 1) {
          // string soindf = "";
        } else {
          SchematicVariation schemvar =
            item.Dom_.SchemVariationLoader.Load(item.References["createdBy"].First());

          if (schemvar.Id != 0) {
            variants =
              new JArray(
                schemvar.VariationPackages.Select(
                  x => new JObject(
                    new JProperty("Name", x.Name),
                    new JProperty(
                      "Stats",
                      new JArray(
                        x.AtrributePercentages.Select(
                          y => string.Format("+{1}% {0}", y.Key, y.Value)
                        ).ToArray()
                      )
                    )
                  )
                ).ToArray()
              );
          }
        }

        jItm.Add(new JProperty("Variations", variants));
      }

      return jItm;
    }
    private static void NewMethod(Object obj, String name) {
      if (obj != null) {
        String type = obj.GetType().ToString();

        switch (type) {
          case "System.Int32":
            if ((Int32)obj == 3495) {
              WriteFile(name + Environment.NewLine, "nodes.txt", true);
            }
            break;
          case "System.Int64":
            if ((Int64)obj == -2305757236622194221 || (Int64)obj == 3495) {
              WriteFile(name + Environment.NewLine, "nodes.txt", true);
            }
            break;
          case "System.Collections.Generic.List`1[System.Int64]":
          case "GomLib.GomObject":
            if (((GomObject)obj).Data != null) {
              foreach (var o in ((GomObject)obj).Data.Dictionary) {
                NewMethod(o.Key, name);
                NewMethod(o.Value, name);
              }
            }
            break;
          case "GomLib.GomObjectData":
            foreach (var ob in ((GomObjectData)obj).Dictionary) {
              NewMethod(ob.Value, name);
            }
            break;
          case "System.Collections.Generic.List`1[System.Object]":
            foreach (var ob in (List<Object>)obj) {
              NewMethod(ob, name);
            }
            break;
          case "System.Collections.Generic.Dictionary`2[System.Object,System.Object]":
            foreach (var ob in (Dictionary<Object, Object>)obj) {
              NewMethod(ob.Key, name);
              NewMethod(ob.Value, name);
            }
            break;
          case "GomLib.DomClass":
          case "System.UInt64":
          case "System.Collections.Generic.List`1[System.Single]":
          case "GomLib.ScriptEnum":
          case "System.Boolean":
          case "System.String":
          case "System.Single":
            break;
          default:
            break;
        }
      }
    }
    private void OutputIcon(String icon) => OutputIcon(icon, "DiscContent");
    internal void OutputTables() {
      FindValue();
      ClearProgress();
      LoadData();

      JsonSerializerSettings settings = new JsonSerializerSettings {
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.Indented
      };

      String json = JsonConvert.SerializeObject(CurrentDom.Data.armorPerLevel.TableData, settings);
      WriteFile(json, "armorPerLevelTable.json", false);

      json = JsonConvert.SerializeObject(CurrentDom.Data.weaponPerLevel.TableData, settings);
      WriteFile(json, "weaponPerLevelTable.json", false);

      ArmorSpec.Load(CurrentDom);
      json = JsonConvert.SerializeObject(ArmorSpec.ArmorSpecList, settings);
      WriteFile(json, "armorSpecTable.json", false);

      WeaponSpec.Load(CurrentDom);
      json = JsonConvert.SerializeObject(WeaponSpec.WeaponSpecList, settings);
      WriteFile(json, "weaponSpecTable.json", false);

      CurrentDom.StatData.ToStat("endurance"); // trick it into loading data
      json = JsonConvert.SerializeObject(CurrentDom.StatData.StatLookup, settings);
      WriteFile(json, "statData.json", false);

      CurrentDom.EnhancementData.ToEnhancement(1); // trick it into loading data
      json = JsonConvert.SerializeObject(CurrentDom.EnhancementData.SlotLookup, settings);
      WriteFile(json, "slotData.json", false);

      // trick it into loading data
      CurrentDom.QuestLoader.Load("qst.location.coruscant.world.enemies_of_the_republic");
      json = JsonConvert.SerializeObject(CurrentDom.QuestLoader.fullCreditRewardsTable, settings);
      WriteFile(json, "fullCreditRewardsTable.json", false);
      json = JsonConvert.SerializeObject(CurrentDom.QuestLoader.experienceTable, settings);
      WriteFile(json, "experienceTable.json", false);
      json = JsonConvert.SerializeObject(CurrentDom.Data.questDifficulty, settings);
      WriteFile(json, "experienceDifficultyMultiplierTable.json", false);
    }
    private static JObject SchematicToMinifiedJSON(Schematic schem) {
      JObject jSchem = JObject.Parse(schem.ToJSON());

      if (schem.MissionCost > 0) {
        jSchem.Remove("CraftingTimeT1");
        jSchem.Remove("CraftingTimeT2");
        jSchem.Remove("CraftingTimeT3");
        jSchem.Remove("Workstation");
        jSchem.Remove("ItemId");
        jSchem.Remove("ItemParentId");
        jSchem.Remove("Materials");
        jSchem.Remove("Subtype");
        jSchem.Remove("ResearchQuantity1");
        jSchem.Remove("ResearchChance1");
        jSchem.Remove("ResearchQuantity2");
        jSchem.Remove("ResearchChance2");
        jSchem.Remove("ResearchQuantity3");
        jSchem.Remove("ResearchChance3");
        jSchem.Remove("TrainingCost");
        jSchem.Remove("DisableDisassemble");
        jSchem.Remove("DisableCritical");
        jSchem.Remove("NameId");
        jSchem.Remove("References");
      } else {
        jSchem.Property("ItemId").Value = jSchem.Property("ItemId").Value.ToString();
        jSchem.Remove("NameId");
        jSchem.Remove("References");
        jSchem.Remove("MissionCost");
        jSchem.Remove("MissionDescriptionId");
        jSchem.Remove("MissionDescription");
        jSchem.Remove("MissionUnlockable");
        jSchem.Remove("MissionLight");
        jSchem.Remove("MissionLightCrit");
        jSchem.Remove("MissionDark");
        jSchem.Remove("MissionDarkCrit");
        jSchem.Remove("MissionFaction");
        jSchem.Remove("MissionYieldDescriptionId");
        jSchem.Remove("MissionYieldDescription");
      }

      return jSchem;
    }
    private static JProperty TalentTokensToMinifiedJSON(List<Single> descTokens) {
      if (descTokens == null) return new JProperty(new JProperty("Tokens"));

      JArray tempArray = new JArray();

      for (Int32 i = 0; i < descTokens.Count; i++) {
        tempArray.Add(
          new JObject(
            new JProperty("TokenId", i + 1),
            new JProperty("TokenData", descTokens[i]),
            new JProperty("TokenType", "Talent")
          )
        );
      }

      return new JProperty("Tokens", tempArray);
    }
    private JObject TalentToMinifiedJSON(Talent tal, Int32 level) {
      OutputIcon(tal.Icon);

      JObject jTal =
        new JObject(
          new JProperty("Name", new JValue(tal.Name ?? "")),
          new JProperty("Description", new JValue(tal.Description ?? "")),
          new JProperty("Icon", new JValue(GetIconFilename(tal.Icon ?? "").Replace("'", ""))),
          new JProperty("Level", new JValue(level)),
          new JProperty("IsUtilityPoint", new JValue(false)),
          new JProperty("IsHighlighted", new JValue(false))
        );

      if (tal.TokenList != null) jTal.Add(TalentTokensToMinifiedJSON(tal.TokenList));

      return jTal;
    }
    /*
    internal void Torheadscanner() {
      //http://www.torhead.com/item/ace+in+the+hole
      //http://www.torhead.com/item/schematic:+[artifact]+microfilament+skill+d-device
      //http://www.torhead.com/item/agent's-birthright-headgear

      ClearProgress();
      LoadData();

      List<GomObject> gomList = CurrentDom.GetObjectsStartingWith("itm.");
      Int32 count = gomList.Count;
      Int32 i = 0;
      Dictionary<String, Item> itemlist =
        new Dictionary<String, Item>();

      foreach (GomObject gom in gomList) {
        ProgressUpdate(i, count);

        Item itm = CurrentDom.itemLoader.Load(gom);

        if (itemlist.ContainsKey(itm.Name)) {
          //add code here to prioritize items based on quality/item level
        } else if (itm.Name != null && itm.Name != "") {
          itemlist.Add(itm.Name, itm);
        }

        i++;
      }

      Addtolist("item list created in memory, scanning torhead");
      Addtolist(string.Format("Found {0} unique names, full scan", itemlist.Count));

      TimeSpan t = TimeSpan.FromSeconds(itemlist.Count / 5);
      String answer = string.Format(
        "will take {0:D2}h:{1:D2}m:{2:D2}s",
        t.Hours,
        t.Minutes,
        t.Seconds
      );

      Addtolist(answer);

      Dictionary<String, String> nametourlmap = new Dictionary<String, String>();

      WriteFile("", "torheadurls.txt", false);
      WriteFile("", "badtorheadurls.txt", false);
      WriteFile("", "torheadurlserrors.txt", false);

      ClearProgress();

      count = itemlist.Count;
      i = 0;

      EnableButtons();

      foreach (var kvp in itemlist) {
        ProgressUpdate(i, count);

        String url = string.Format("http://www.torhead.com/item/{0}", kvp.Key.Replace(" ", "+"));

        HttpWebRequest req = WebRequest.CreateHttp(url);
        req.AllowAutoRedirect = false;

        try {
          HttpWebResponse response = (HttpWebResponse)req.GetResponse();

          if (response.StatusCode != HttpStatusCode.InternalServerError) {
            String loc = response.GetResponseHeader("Location");
            nametourlmap.Add(kvp.Value.Base62Id, loc);

            WriteFile(
              String.Format(
                "{0},{1},http://www.torhead.com{2}{3}",
                kvp.Value.Base62Id,
                url,
                loc,
                Environment.NewLine
              ),
              "torheadurls.txt",
              true
            );

            Addtolist2(url);
          } else
            WriteFile(
              String.Format(
                "{0},{1}{2}",
                kvp.Value.Base62Id,
                url,
                Environment.NewLine
              ),
              "badtorheadurls.txt",
              true
            );
        }
        catch (WebException ex) {
          WriteFile(
            String.Format(
              "{0},{1}{2}",
              kvp.Value.Base62Id,
              url,
              Environment.NewLine
            ),
            "torheadurlserrors.txt",
            true
          );

          Addtolist(string.Format("Error: {0}", ex.Message));

          if (ex.Status == WebExceptionStatus.ProtocolError) {
            Addtolist(
              String.Format(
                "Status Code : {0}",
                ((HttpWebResponse)ex.Response).StatusCode
              )
            );
            Addtolist(
              String.Format(
                "Status Description : {0}",
                ((HttpWebResponse)ex.Response).StatusDescription
              )
            );
          }
        }

        i++;
        Thread.Sleep(200);
      }

      Addtolist("Done scanning torhead!");
    }
    */
  }
}
