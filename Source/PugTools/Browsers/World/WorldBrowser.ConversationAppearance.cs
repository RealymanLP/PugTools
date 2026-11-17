using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GomLib;
using GomLib.Models;

namespace PugTools {
  // `Play Appearance` (port of Jedipedia viewer-webgpu/load/epp.js and cnvPreview.js viewerCnvPlayAppearance).
  //
  // An epp.* node is not an effect. It is a small program: a list of sub-appearances, each with a condition arm and a
  // list of actions, plus root lookups naming which of those subs some LATER event fires (a projectile landing, an
  // animation note). What a scene means by `Play Appearance epp.cine.blasters.weapon_fire_single_notarget` is "run
  // that program on this actor", and what comes out of it for a viewer is a handful of .fxspec casts: a muzzle flash
  // on the weapon, an impact, the sparks off a droid.
  //
  // Only what the world can perform is read: the effects of the arms that pass, from the subs no trigger owns. The
  // projectile and its landing, sounds, grunts and camera shakes are left alone, exactly as the scene's own
  // `Play SFX` and `Play Camera Shake` are.
  public partial class WorldBrowser {
    // The action and condition types this reads, by their numeric id in the epp language.
    private const int WorldEppActionPlayFx = 6, WorldEppActionRoll = 9, WorldEppActionSetVar = 21;
    // Every parameter of every action and condition is stored by VALUE TYPE, in a lookup keyed by a paramId that is
    // global to the whole epp language: `casterBone` is string 22 on a PlayFX and nothing at all on a Roll.
    private const int WorldEppParamFxSpec = 21, WorldEppParamVarName = 5, WorldEppParamFxCaster = 26, WorldEppParamFxTarget = 27,
      WorldEppParamDynamicData = 31, WorldEppParamVarValues = 33;

    // The conditions an arm is gated by, by name (the reader's spelling). The names matter beyond debugging: a condition
    // this cannot answer is taken to PASS unless it is an `IfNot`, which is what makes an if/else-if chain play its
    // first arm rather than none of them.
    private static readonly Dictionary<int, string> WorldEppConditionNames = new Dictionary<int, string> {
      [2] = "IfAlerted", [3] = "IfNotAlerted", [4] = "IfAutoAttacking", [5] = "IfNotAutoAttacking", [6] = "IfCasterBodyType",
      [7] = "IfNotCasterBodyType", [8] = "IfDeathBlow", [9] = "IfNotDeathBlow", [10] = "IfNotExecuted", [11] = "IfHasMoreEnemies",
      [12] = "IfNotHasMoreEnemies", [13] = "IfInCombat", [14] = "IfNotInCombat", [15] = "IfInCover", [16] = "IfNotInCover",
      [17] = "IfEffResult", [18] = "IfNotEffResult", [19] = "IfStealthed", [20] = "IfNotStealthed", [21] = "IfTargetBodyType",
      [22] = "IfNotTargetBodyType", [23] = "IfTargetIsCaster", [24] = "IfNotTargetIsCaster", [25] = "IfWeaponType",
      [26] = "IfNotWeaponType", [27] = "IfVar", [28] = "IfNotVar", [29] = "Else", [30] = "RandomChance", [31] = "IsAlignment",
      [32] = "IsNotAlignment", [33] = "IfTargetExists", [34] = "IfNotTargetExists", [35] = "IsClass", [36] = "IfNotTargetIsLocalShip",
      [37] = "IfTargetIsLocalShip", [38] = "IfCasterBodySize", [39] = "IfNotCasterBodySize", [40] = "IfTargetBodySize",
      [41] = "IfNotTargetBodySize", [42] = "IfMounted", [43] = "IfNotMounted"
    };

    // An .fxspec an appearance hangs off one of the actor's WEAPON slots (caster's or target's primary/secondary melee
    // and ranged -- 15,697 of the 35,763 PlayFX a played appearance fires) asks for the weapon MODEL's bones (a
    // blaster's fx_muzzle). A weapon is drawn as rigid geometry without a skeleton of its own, so the effect would
    // land on the placement origin -- the actor's feet. What stands in is the locator the BODY carries for that slot.
    private static readonly Dictionary<int, string> WorldEppSlotBones = new Dictionary<int, string> {
      [8] = "vfx_hand_right", [9] = "vfx_hand_left", [10] = "vfx_aim01", [11] = "vfx_hand_left",
      [12] = "vfx_hand_right", [13] = "vfx_hand_left", [14] = "vfx_aim01", [15] = "vfx_hand_left"
    };

    private sealed class WorldEppItem {
      public string Type;
      public GomObjectData Item;
    }

    private sealed class WorldEppSub {
      public bool Triggered;
      public readonly List<WorldEppItem> Conditions = new List<WorldEppItem>();
      public readonly List<WorldEppItem> Actions = new List<WorldEppItem>();
    }

    // What playing one appearance casts: one .fxspec, with the dynamic data it was given and the body locator that
    // stands in for the weapon slot it names.
    private sealed class WorldEppCast {
      public string Spec;
      public string SlotBone;
      public Dictionary<string, string> DynamicData;
    }

    // Held for the session: a parsed program is a few hundred bytes of plain data and the same handful of packages
    // (the epp.cine.blasters variants alone are a quarter of every Play Appearance in the game) are played by scene
    // after scene. Dropped when the data set changes. A node that does not resolve is remembered as null.
    private readonly Dictionary<string, List<WorldEppSub>> worldEppPlans = new Dictionary<string, List<WorldEppSub>>(StringComparer.OrdinalIgnoreCase);
    private DataObjectModel worldEppPlansDom;

    private List<WorldEppSub> GetWorldEppPlan(string fqn) {
      if (String.IsNullOrWhiteSpace(fqn) || currentDom == null) return null;
      if (!ReferenceEquals(worldEppPlansDom, currentDom)) { worldEppPlans.Clear(); worldEppPlansDom = currentDom; }
      string name = fqn.Trim();
      if (worldEppPlans.TryGetValue(name, out List<WorldEppSub> cached)) return cached;
      List<WorldEppSub> plan = null;
      try {
        GomObject node = WorldResolveGomObject(name);
        if (node?.Data != null) plan = ParseWorldEppPlan(node.Data);
      } catch { }
      worldEppPlans[name] = plan;
      return plan;
    }

    // The program as it is played rather than as it is stored: subs in authored order, each with the arm that gates it
    // and the actions that can be performed. A sub the root lookups name is kept and marked -- it belongs to the
    // appearance and is fired by an event of its own, never by playing it.
    private static List<WorldEppSub> ParseWorldEppPlan(GomObjectData data) {
      var subs = new List<WorldEppSub>();
      foreach (object rawSub in WorldInteractionListEntries(WorldInteractionDataValue(data, "effAppearanceSchedule", "4611686022347910010"))) {
        if (!(rawSub is GomObjectData sub)) continue;
        var parsed = new WorldEppSub {
          Triggered = WorldConversationBool(WorldInteractionDataValue(sub, "effAppearanceSubScheduleHasTrigger", "4611686022647350004"))
        };
        foreach (object raw in WorldInteractionListEntries(WorldInteractionDataValue(sub, "effAppearanceConditions", "4611686048217870035"))) {
          if (!(raw is GomObjectData item)) continue;
          int type = WorldEppNumber(WorldInteractionDataValue(item, "effAppConditionName", "4611686048217870032"));
          parsed.Conditions.Add(new WorldEppItem { Type = WorldEppConditionNames.TryGetValue(type, out string conditionName) ? conditionName : String.Empty, Item = item });
        }
        foreach (object raw in WorldInteractionListEntries(WorldInteractionDataValue(sub, "effAppearanceActions", "4611686048217870034"))) {
          if (!(raw is GomObjectData item)) continue;
          parsed.Actions.Add(new WorldEppItem { Type = WorldEppNumber(WorldInteractionDataValue(item, "effAppActionName", "4611686048217870027")).ToString(CultureInfo.InvariantCulture), Item = item });
        }
        subs.Add(parsed);
      }
      return subs;
    }

    private static int WorldEppNumber(object value) {
      if (value == null) return -1;
      try { return Convert.ToInt32(value, CultureInfo.InvariantCulture); } catch { }
      return Int32.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ? parsed : -1;
    }

    // One parameter of one action, out of the bucket its value type is stored in. A bucket the node left at its default
    // is an empty list rather than an empty lookup, which is why the shape is tested rather than indexed blind.
    private static object WorldEppParam(GomObjectData item, string bucketName, string bucketId, int paramId) {
      Dictionary<object, object> bucket = WorldInteractionMap(WorldInteractionDataValue(item, bucketName, bucketId));
      if (bucket == null) return null;
      string key = paramId.ToString(CultureInfo.InvariantCulture);
      foreach (KeyValuePair<object, object> pair in bucket) if (String.Equals(pair.Key?.ToString(), key, StringComparison.Ordinal)) return pair.Value;
      return null;
    }

    private static string WorldEppString(GomObjectData item, int paramId) {
      return WorldEppParam(item, "effAppStringParams", "4611686048217870042", paramId)?.ToString() ?? String.Empty;
    }

    private static int WorldEppInt(GomObjectData item, int paramId) {
      return WorldEppNumber(WorldEppParam(item, "effAppIntParams", "4611686048217870040", paramId));
    }

    private static List<string> WorldEppStringList(GomObjectData item, int paramId) {
      object value = WorldEppParam(item, "effAppStringListParams", "4611686048217870076", paramId);
      return value == null ? new List<string>() : WorldInteractionListEntries(value).Select(x => x?.ToString() ?? String.Empty).ToList();
    }

    // The dynamic data an action passes the effect: `key=value` pairs. An `@`-key overwrites the .fxspec field it
    // names before anything is placed and a bare one gates whole elements (_fxStartIF).
    private static Dictionary<string, string> WorldEppDynamicData(GomObjectData item) {
      Dictionary<string, string> data = null;
      foreach (string pair in WorldEppStringList(item, WorldEppParamDynamicData)) {
        int split = pair.IndexOf('=');
        if (split <= 0) continue;
        if (data == null) data = new Dictionary<string, string>(StringComparer.Ordinal);
        data[pair.Substring(0, split)] = pair.Substring(split + 1);
      }
      return data;
    }

    // Whether one condition of one arm holds.
    //
    // Three of them can be answered here, and they are the three an appearance uses to talk to ITSELF: `Else` against
    // the client's own latch, and `IfVar`/`IfNotVar` against the variables the appearance's own Roll and SetVar have
    // set. A `RandomChance` is decided the same way every time, so scrubbing a scene twice shows the same shot.
    //
    // Everything else asks about a state the preview does not have (which weapon is drawn, whether the shot crit, the
    // caster's body archetype). Those are taken to PASS and their `IfNot` mirror to fail. It is the answer that costs
    // least when it is wrong: an appearance authored as "blaster arm / Else unarmed arm" plays its blaster arm, with
    // the latch closing the chain after it, so exactly one arm of a chain plays rather than none or all of them.
    private static bool WorldEppConditionPasses(WorldEppItem condition, Dictionary<string, string> env, bool latched) {
      string type = condition.Type ?? String.Empty;
      if (type == "Else") return !latched;
      if (type == "IfVar" || type == "IfNotVar") {
        string name = WorldEppString(condition.Item, WorldEppParamVarName).TrimStart('$');
        List<string> wanted = WorldEppStringList(condition.Item, WorldEppParamVarValues);
        bool has = env.TryGetValue(name, out string held) && wanted.Any(v => String.Equals(v, held, StringComparison.Ordinal));
        return type == "IfVar" ? has : !has;
      }
      if (type == "RandomChance") return true;
      return !type.StartsWith("IfNot", StringComparison.Ordinal);
    }

    // The client keeps one if/else-if latch over the whole appearance: a successful arm closes the chain, a failed
    // non-Else arm opens it, and a failed Else leaves it as it stands. An empty condition list is a successful arm.
    private static bool WorldEppRunSub(WorldEppSub sub, Dictionary<string, string> env, ref bool latched) {
      bool pass = true, sawElse = false;
      foreach (WorldEppItem condition in sub.Conditions) {
        if (condition.Type == "Else") sawElse = true;
        if (!WorldEppConditionPasses(condition, env, latched)) { pass = false; break; }
      }
      if (pass) latched = true;
      else if (!sawElse) latched = false;
      return pass;
    }

    // The first value of a Roll: a scene scrubbed back and forth re-runs its beats, and an appearance that rolled a
    // different arm each time would play a different shot every time the playhead crossed it.
    private static string WorldEppRollValue(string text, List<string> values) {
      if (values != null && values.Count > 0) return values[0];
      string first = (text ?? String.Empty).Split(',')[0];
      int weight = first.LastIndexOf(':');
      return (weight < 0 ? first : first.Substring(0, weight)).Trim();
    }

    // Every .fxspec the arms that pass ask for, in authored order. The variables are the appearance's own and live for
    // the length of one run: a Roll in the first sub is what the IfVar in the third is reading. A PlayProjectile's own
    // .fxspec is not among them: a bolt crosses the room and lands, and casting it at the muzzle would leave it
    // hanging in the caster's hand for as long as the effect runs.
    private static List<WorldEppCast> CastWorldEppPlan(List<WorldEppSub> plan) {
      var casts = new List<WorldEppCast>();
      if (plan == null) return casts;
      var env = new Dictionary<string, string>(StringComparer.Ordinal);
      bool latched = false;
      foreach (WorldEppSub sub in plan) {
        if (sub.Triggered) continue;
        if (!WorldEppRunSub(sub, env, ref latched)) continue;
        foreach (WorldEppItem action in sub.Actions) {
          if (!Int32.TryParse(action.Type, NumberStyles.Integer, CultureInfo.InvariantCulture, out int type)) continue;
          if (type == WorldEppActionSetVar) {
            env[WorldEppString(action.Item, WorldEppParamVarName).TrimStart('$')] = WorldEppString(action.Item, WorldEppParamVarValues);
            continue;
          }
          if (type == WorldEppActionRoll) {
            env[WorldEppString(action.Item, WorldEppParamVarName).TrimStart('$')] =
              WorldEppRollValue(WorldEppString(action.Item, WorldEppParamVarValues), WorldEppStringList(action.Item, WorldEppParamVarValues));
            continue;
          }
          if (type != WorldEppActionPlayFx) continue;
          string spec = WorldEppString(action.Item, WorldEppParamFxSpec);
          if (String.IsNullOrWhiteSpace(spec)) continue;
          int caster = WorldEppInt(action.Item, WorldEppParamFxCaster), target = WorldEppInt(action.Item, WorldEppParamFxTarget);
          casts.Add(new WorldEppCast {
            Spec = spec,
            // One stand-in per cast, because an effect hangs off one placement: the caster's slot where it names one,
            // and the other end's where only that end does -- the shape of a muzzle flash, whose spec bolts its root
            // to the TARGET's fx_muzzle while the appearance points both ends at the same weapon.
            SlotBone = WorldEppSlotBones.TryGetValue(caster, out string casterBone) ? casterBone : (WorldEppSlotBones.TryGetValue(target, out string targetBone) ? targetBone : null),
            DynamicData = WorldEppDynamicData(action.Item)
          });
        }
      }
      return casts;
    }

    // One appearance, played on one actor. An actor the preview has no body for gets the same place an effect is cast
    // from as `Play VFX` gives it. The one action in the game that names an .fxspec outright instead of an appearance
    // is a `Play VFX` in all but its kind.
    private void PlayWorldConversationAppearance(Conversation conversation, DialogNode node, WorldConversationCinematicAction action, WorldNpcPlacement placement) {
      string name = (action?.Value ?? String.Empty).Trim();
      if (name.Length == 0 || panelRender == null) return;
      if (placement == null) placement = EnsureWorldConversationVfxAnchor(conversation, node, action);
      if (placement == null) return;
      if (!name.StartsWith("epp.", StringComparison.OrdinalIgnoreCase)) { panelRender.PlayWorldConversationFx(placement, name); return; }

      List<WorldEppCast> casts = CastWorldEppPlan(GetWorldEppPlan(name));
      WorldConversationDiagnosticFileWrite("CNV appearance: " + name + " -> " + casts.Count + " effect(s)" + (casts.Count == 0 ? "" : ": " + String.Join(", ", casts.Select(c => c.Spec))));
      foreach (WorldEppCast cast in casts)
        panelRender.PlayWorldConversationFx(placement, cast.Spec, null, cast.DynamicData, cast.SlotBone);
    }
  }
}
