using System;
using SlimDX;
using FileFormats;

namespace PugTools {
  internal sealed partial class View_AREA {
    // cos of half of 0.1 degrees: a rotation whose w is at least this is identity for every purpose here, and a bone
    // left at identity is one the pose loop never has to touch.
    private static readonly float LookAtIdentityW = (float)Math.Cos(0.1 * Math.PI / 360.0);

    // The head turn a placement holds this frame, as additive local rotations by SKELETON bone index (null where the
    // placement is not looking at anything, or the turn is nil). Port of Jedipedia's viewerCnvUpdateLookAt (the angle
    // from the actor's own facing to what it looks at, measured per frame because both ends move) and
    // viewerNpcAnimLookAtDeltas (each layer's clip read BETWEEN two of its frames, the angle choosing where, and the
    // layers composed).
    private System.Numerics.Quaternion?[] NpcEvaluateLookAt(NpcSkinState state, WorldNpcPlacement placement) {
      if (state?.Skeleton == null || !WorldNpcLookAt.TryGet(placement, out WorldNpcLookAtRecord look)) return null;

      float yaw = 0f, pitch = 0f;
      try {
        Matrix from = NpcPlacementBaseWorld(placement), to = NpcPlacementBaseWorld(look.Target);
        float dx = to.M41 - from.M41, dy = to.M42 - from.M42, dz = to.M43 - from.M43;
        float away = (float)Math.Sqrt(dx * dx + dz * dz);
        // A placement faces along its own +Z row, which is what everything else in the world browser calls forward.
        float fx = from.M31, fz = from.M33, facingLength = (float)Math.Sqrt(fx * fx + fz * fz);
        if (facingLength < 1e-6f) return null;
        fx /= facingLength; fz /= facingLength;
        if (away > 1e-6f) {
          // Positive yaw is the target on the actor's RIGHT, which is the sign the network reads TargetYawHead in; the
          // right-hand side of a +Z-forward, +Y-up placement is (fz, 0, -fx).
          float ahead = dx * fx + dz * fz, right = dx * fz - dz * fx;
          yaw = (float)(Math.Atan2(right, ahead) * 180.0 / Math.PI);
          // Positive DOWNWARDS, the sign the network reads TargetPitchHead in.
          pitch = (float)(-Math.Atan2(dy, away) * 180.0 / Math.PI);
        }
      } catch { return null; }

      int count = state.Skeleton.Count;
      System.Numerics.Quaternion?[] result = null;
      foreach (WorldLookAtLayer layer in look.Layers) {
        JBAAnimation animation = layer.Clip?.Animation;
        if (animation == null || animation.FrameCount <= 1 || !(animation.FPS > 0f) || animation.BoneCount <= 0) continue;
        float fraction = layer.Fraction(layer.Axis == "pitch" ? pitch : yaw);
        if (!Single.IsFinite(fraction)) continue;
        float frame = Math.Max(0f, Math.Min(1f, fraction)) * (animation.FrameCount - 1);

        WorldLookAtLayer captured = layer;
        int[] binding = layer.GetBinding(state.Skeleton, () => {
          NpcBuildChannelBinding(animation, captured.Clip.Rig, state.Skeleton, out int[] map, out bool[] _);
          return map;
        });
        if (binding == null) continue;
        var samples = new JBATransform[animation.BoneCount];
        animation.SampleInto(frame / animation.FPS, samples);

        for (int i = 0; i < count && i < binding.Length; i++) {
          int channel = binding[i];
          if (channel < 0 || channel >= samples.Length) continue;
          System.Numerics.Quaternion rotation = NpcJbaSampleQuaternion(samples[channel]);
          if (layer.Weight < 1f) rotation = System.Numerics.Quaternion.Slerp(System.Numerics.Quaternion.Identity, rotation, Math.Max(0f, layer.Weight));
          if (Math.Abs(rotation.W) >= LookAtIdentityW) continue;
          if (result == null) result = new System.Numerics.Quaternion?[count];
          // Deltas compose rather than replace: the later layer goes on the parent side of the earlier one.
          result[i] = result[i].HasValue ? NpcJbaQuatMultiply(rotation, result[i].Value) : rotation;
        }
      }
      return result;
    }
  }
}
