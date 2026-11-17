using System;
using FileFormats;

namespace PugTools {
  // `ad_*` beats (port of Jedipedia cnvPreview.js viewerCnvIsAdditive / viewerCnvPlayAdditive).
  //
  // An `ad_` leaf is what the editor's additive namespace files a glance, nod or smirk under
  // (`cineaction.ad_head_agree_01`; the Tython consular opening alone has 24 such beats). The clip is a DIFFERENCE and
  // not a pose, and playing one as the actor's body clip is what draws the actor as a heap of bones at its own origin.
  // So a beat that names one never touches the body clip, the posture or the face: the clip is hung on the actor for
  // the pose loop to lay over whatever the body is doing, plays once and lets go.
  public partial class WorldBrowser {
    // Read off the name the beat spells, not off the clip it resolves to.
    private static bool WorldConversationIsAdditive(string name) {
      string[] parts = (name ?? String.Empty).Trim().Split(new[] { '.', '\\', '/' }, StringSplitOptions.None);
      return parts.Length > 0 && parts[parts.Length - 1].StartsWith("ad_", StringComparison.OrdinalIgnoreCase);
    }

    private void PlayWorldConversationAdditive(WorldNpcPlacement placement, WorldConversationCinematicAction action) {
      string name = action.Value.Trim();
      WorldNpcAnimationClip clip = ResolveNpcConversationAnimationClip(name, placement.BodyType, worldConversationAnimationCache);
      if (clip?.Animation == null || !(clip.Animation.Length > 0f)) {
        WorldConversationDiagnosticFileWrite("CNV additive unresolved: '" + name + "' body=" + (placement.BodyType ?? "<none>") + " actor=" + (placement.SourceFqn ?? "<virtual>"));
        return;
      }
      var parameters = WorldConversationActionParameters(action.Params);
      float startAt = WorldConversationParameterFloat(parameters, "start", out float authoredStart) ? Math.Max(0f, authoredStart) : 0f;
      float speed = WorldConversationParameterFloat(parameters, "speed", out float authoredSpeed) && authoredSpeed > 0f ? authoredSpeed : 1f;
      panelRender.SetWorldConversationAdditive(placement, clip, startAt, speed);
      WorldConversationDiagnosticFileWrite("CNV additive: '" + name + "' -> " + clip.Action + " (" + clip.Animation.Length.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " s) actor=" + (placement.SourceFqn ?? "<virtual>"));
    }
  }
}
