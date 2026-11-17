using System;
using System.Collections.Generic;
using FileFormats;

namespace PugTools {
  internal sealed partial class View_AREA {
    // How long an additive delta takes to hand the bones it holds back to the body's own clip. Most of these end near
    // identity on their own -- a nod comes back to where it started -- but a long suspicious glance held to its last
    // frame would let the head go in a single frame without this.
    private const float ConversationAdditiveEaseSeconds = 0.2f;

    private readonly List<WorldNpcPlacement> worldConversationAdditivePlacements = new List<WorldNpcPlacement>();

    internal void SetWorldConversationAdditive(WorldNpcPlacement placement, WorldNpcAnimationClip clip, float startAt, float speed) {
      if (placement == null || clip?.Animation == null) return;
      WorldNpcAdditive.Set(placement, new WorldNpcAdditiveRecord {
        Layer = new WorldLookAtLayer { Axis = "additive", Weight = 1f, Clip = clip },
        Start = elapsed,
        Offset = Single.IsNaN(startAt) || Single.IsInfinity(startAt) ? 0f : Math.Max(0f, startAt),
        Speed = Single.IsNaN(speed) || Single.IsInfinity(speed) || speed <= 0f ? 1f : speed
      });
      if (!worldConversationAdditivePlacements.Contains(placement)) worldConversationAdditivePlacements.Add(placement);
    }

    internal void ClearWorldConversationAdditives() {
      foreach (WorldNpcPlacement placement in worldConversationAdditivePlacements) WorldNpcAdditive.Clear(placement);
      worldConversationAdditivePlacements.Clear();
    }

    // The additive clip a placement is playing OVER its own, sampled for the frame it stands on (port of
    // viewerNpcAnimAdditiveDeltas). Played ONCE and then over: a body clip holds its last frame because the next beat
    // moves the actor on, but a delta held that way would leave the head turned for the rest of the conversation.
    private System.Numerics.Quaternion?[] NpcEvaluateConversationAdditive(NpcSkinState state, WorldNpcPlacement placement) {
      if (state?.Skeleton == null || !WorldNpcAdditive.TryGet(placement, out WorldNpcAdditiveRecord record)) return null;
      JBAAnimation animation = record.Layer.Clip.Animation;
      float since = Math.Max(0f, (elapsed - record.Start) * Math.Max(.0001f, record.Speed) + record.Offset);
      if (!(animation.Length > 0f) || since >= animation.Length) {
        WorldNpcAdditive.Clear(placement);
        return null;
      }
      if (animation.FrameCount < 1 || !(animation.FPS > 0f) || animation.BoneCount <= 0) return null;

      float frame = Math.Min(animation.FrameCount - 1, since * animation.FPS);
      float left = (animation.Length - since) / ConversationAdditiveEaseSeconds;
      float weight = left < 1f ? left : 1f;

      WorldLookAtLayer layer = record.Layer;
      int[] binding = layer.GetBinding(state.Skeleton, () => {
        NpcBuildChannelBinding(animation, layer.Clip.Rig, state.Skeleton, out int[] map, out bool[] _);
        return map;
      });
      if (binding == null) return null;
      var samples = new JBATransform[animation.BoneCount];
      animation.SampleInto(frame / animation.FPS, samples);

      int count = state.Skeleton.Count;
      System.Numerics.Quaternion?[] result = null;
      for (int i = 0; i < count && i < binding.Length; i++) {
        int channel = binding[i];
        if (channel < 0 || channel >= samples.Length) continue;
        System.Numerics.Quaternion rotation = NpcJbaSampleQuaternion(samples[channel]);
        if (weight < 1f) rotation = System.Numerics.Quaternion.Slerp(System.Numerics.Quaternion.Identity, rotation, Math.Max(0f, weight));
        // Bones the clip leaves at identity -- most of them -- are left out, so the pose loop never touches them.
        if (Math.Abs(rotation.W) >= LookAtIdentityW) continue;
        if (result == null) result = new System.Numerics.Quaternion?[count];
        result[i] = rotation;
      }
      return result;
    }

    // Two additive layers composed (port of mphMergeAdditiveRotations): `extra` goes on the parent side of `baseDeltas`,
    // so the head turn rides over the nod rather than under it.
    private static System.Numerics.Quaternion?[] NpcMergeAdditiveDeltas(System.Numerics.Quaternion?[] baseDeltas, System.Numerics.Quaternion?[] extra) {
      if (baseDeltas == null) return extra;
      if (extra == null) return baseDeltas;
      int count = Math.Max(baseDeltas.Length, extra.Length);
      var merged = new System.Numerics.Quaternion?[count];
      for (int i = 0; i < count; i++) {
        System.Numerics.Quaternion? a = i < baseDeltas.Length ? baseDeltas[i] : null;
        System.Numerics.Quaternion? b = i < extra.Length ? extra[i] : null;
        merged[i] = a.HasValue && b.HasValue ? NpcJbaQuatMultiply(b.Value, a.Value) : (b ?? a);
      }
      return merged;
    }
  }
}
