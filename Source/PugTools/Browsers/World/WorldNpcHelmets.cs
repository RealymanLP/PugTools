using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using FileFormats;

namespace PugTools {
  // Headgear a conversation can take off (`Hide Helmet`), port of Jedipedia's per-mesh npcHelmet flag and
  // cnvHideHelmet instance flag (viewer/load/npc.js, viewerNpcMeshHiddenOnInstance).
  //
  // The helmet is the models of the appearance's FACE slot (appSlotFace: helmets, masks, goggles), not of the head slot
  // that holds the character's own head underneath. Appearance models are cached and shared by every placement wearing
  // that appearance, while only some placements have their helmet off -- so "this model is headgear" is remembered on
  // the model, and "this placement has taken its headgear off" on the placement. Neither needs a field on the shared
  // GR2/placement classes, which keeps the flags out of every other consumer of those types.
  internal static class WorldNpcHelmets {
    private static readonly object Marker = new object();
    private static readonly ConditionalWeakTable<GR2, object> helmetModels = new ConditionalWeakTable<GR2, object>();
    private static readonly ConditionalWeakTable<WorldNpcPlacement, object> hiddenPlacements = new ConditionalWeakTable<WorldNpcPlacement, object>();
    // Set the first time any placement hides its helmet and never cleared: keeps the per-model lookups out of the
    // NPC draw loop for every session that never plays a conversation with a Hide Helmet beat.
    private static int anyHidden;

    public static void MarkModel(GR2 model) {
      if (model != null) helmetModels.AddOrUpdate(model, Marker);
    }

    public static bool IsHelmetModel(GR2 model) {
      return model != null && helmetModels.TryGetValue(model, out _);
    }

    // A body with no headgear model has nothing to take off.
    public static bool HasHelmet(WorldNpcPlacement placement) {
      if (placement?.Models == null) return false;
      try { return placement.Models.ToArray().Any(IsHelmetModel); } catch { return false; }
    }

    public static void SetHidden(WorldNpcPlacement placement, bool hidden) {
      if (placement == null) return;
      if (hidden) {
        hiddenPlacements.AddOrUpdate(placement, Marker);
        Volatile.Write(ref anyHidden, 1);
      } else {
        hiddenPlacements.Remove(placement);
      }
    }

    public static bool IsHidden(WorldNpcPlacement placement) {
      return placement != null && hiddenPlacements.TryGetValue(placement, out _);
    }

    // True when this model of this placement is headgear the scene has taken off and must not be drawn or picked.
    public static bool IsDrawSuppressed(WorldNpcPlacement placement, GR2 model) {
      if (Volatile.Read(ref anyHidden) == 0) return false;
      return IsHelmetModel(model) && IsHidden(placement);
    }
  }
}
