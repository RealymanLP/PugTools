using System;
using System.Runtime.CompilerServices;

namespace PugTools {
  // One additive clip a conversation beat has hung on an actor (port of Jedipedia's `instance.cnvAdditive`): an `ad_*`
  // nod, glance or smirk. It is a DIFFERENCE and not a pose, so it is kept apart from the actor's body clip, posture and
  // weapon; the pose loop lays it over the body's own pose and it goes away by itself when it has played.
  internal sealed class WorldNpcAdditiveRecord {
    public WorldLookAtLayer Layer;   // the clip, and the bone binding for each skeleton it is read against
    public float Start;              // the renderer clock when the beat fired
    public float Offset;             // authored `start`: how far into the clip the beat begins
    public float Speed = 1f;
  }

  internal static class WorldNpcAdditive {
    private static readonly ConditionalWeakTable<WorldNpcPlacement, WorldNpcAdditiveRecord> records = new ConditionalWeakTable<WorldNpcPlacement, WorldNpcAdditiveRecord>();
    private static int any;

    public static void Set(WorldNpcPlacement placement, WorldNpcAdditiveRecord record) {
      if (placement == null || record == null) return;
      records.AddOrUpdate(placement, record);
      System.Threading.Volatile.Write(ref any, 1);
    }

    public static void Clear(WorldNpcPlacement placement) {
      if (placement != null) records.Remove(placement);
    }

    public static bool TryGet(WorldNpcPlacement placement, out WorldNpcAdditiveRecord record) {
      record = null;
      if (placement == null || System.Threading.Volatile.Read(ref any) == 0) return false;
      return records.TryGetValue(placement, out record) && record?.Layer?.Clip?.Animation != null;
    }
  }
}
