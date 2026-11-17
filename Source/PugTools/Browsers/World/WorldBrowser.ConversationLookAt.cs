using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileFormats;
using GomLib.Models;
using TorArchive;
using File = TorArchive.File;

namespace PugTools {
  // `Set Look At` (port of Jedipedia cnvPreview.js viewerCnvSetLookAt / viewerCnvLookTarget and
  // load/npc-animation.js viewerNpcAnimLookAtLayers).
  //
  // The action names what one actor's head FOLLOWS: another participant, one of the symbolic roles, DEFAULT for whoever
  // it is talking to, or OFF. The turn itself is not in the tree at all -- it is authored in the body's conversation
  // network as additive layers over the clip the beat plays, with the angle to the target choosing the frame (see
  // WorldMphLookAt). So this side is geometry and bookkeeping: which placement an actor looks at, and the layers its
  // body type brings. The angle is measured per frame by the renderer (NpcEvaluateLookAt), because both ends move.
  public partial class WorldBrowser {
    // One read of the conversation networks per body type, shared by every actor of it. A body type with no look-at
    // stack, or one whose additive clips cannot be loaded, is remembered as null: such an actor simply looks ahead.
    private readonly Dictionary<string, List<WorldLookAtLayer>> worldLookAtLayerCache = new Dictionary<string, List<WorldLookAtLayer>>(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<WorldNpcPlacement> worldConversationLookAtNpcs = new HashSet<WorldNpcPlacement>();

    private List<WorldLookAtLayer> GetWorldLookAtLayers(string bodyType) {
      if (String.IsNullOrWhiteSpace(bodyType) || currentAssets == null) return null;
      if (worldLookAtLayerCache.TryGetValue(bodyType, out List<WorldLookAtLayer> cached)) return cached;
      List<WorldLookAtLayer> layers = null;
      try {
        foreach (NpcAnimationSpec spec in ResolveNpcAnimationSpecs(bodyType)) {
          layers = LoadWorldLookAtLayers(spec, bodyType);
          if (layers != null && layers.Count > 0) break;
        }
      } catch (Exception ex) {
        System.Diagnostics.Debug.WriteLine("Look-at layers failed " + bodyType + ": " + ex.Message);
      }
      if (layers != null && layers.Count == 0) layers = null;
      worldLookAtLayerCache[bodyType] = layers;
      return layers;
    }

    // The first conversation network of the body's folder that has a look-at stack, and its HEAD layers. The body's own
    // layers are an actor turning on the spot, which a scene does with `Set Mark` instead.
    private List<WorldLookAtLayer> LoadWorldLookAtLayers(NpcAnimationSpec spec, string bodyType) {
      var categories = new List<string>();
      void AddCategory(string category) {
        if (!String.IsNullOrWhiteSpace(category) && !categories.Contains(category, StringComparer.OrdinalIgnoreCase)) categories.Add(category.Trim().ToLowerInvariant());
      }
      AddCategory(spec.Category);
      foreach (string category in new[] { "humanoid", "creature", "droid", "npc", "pet" }) AddCategory(category);

      foreach (string category in categories) {
        foreach (string network in WorldConversationAnimationNetworks) {
          string basePath = "/resources/anim/" + category + "/" + spec.Folder + "/";
          using File mphFile = currentAssets.FindFile(basePath + network + ".mph");
          if (mphFile == null) continue;
          try {
            using Stream stream = mphFile.OpenCopyInMemory();
            using var reader = new BinaryReader(stream);
            ViewMPH.MphFileInfo mph = ViewMPH.Parse(reader);
            foreach (ViewMPH.MphSection section in mph.Sections.Where(x => x.Network != null)) {
              List<WorldLookAtAxis> axes = WorldMphLookAt.Parse(section.Network);
              if (axes == null) continue;
              var layers = new List<WorldLookAtLayer>();
              foreach (WorldLookAtAxis axis in axes) {
                if (axis.Part != "head") continue;
                // The node that names the clip also selects which RigToAnimMap poses it on this body, so its exact rig is
                // tried first, as for every conversation clip.
                JBARig exactRig = BuildNpcConversationExactRig(mph, section, axis.Source, axis.ClipName);
                WorldNpcAnimationClip clip = TryLoadNpcAnimationClip(spec, axis.ClipName, network, "lookat:" + axis.ClipName, exactRig)
                  ?? ResolveNpcConversationAnimationClip(axis.ClipName, bodyType, null);
                if (clip?.Animation == null || clip.Animation.FrameCount <= 1 || !(clip.Animation.FPS > 0f)) continue;
                layers.Add(new WorldLookAtLayer { Axis = axis.Axis, Weight = axis.Weight, Clip = clip, Fraction = axis.Fraction });
              }
              if (layers.Count > 0) return layers;
            }
          } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine("Look-at network failed " + basePath + network + ".mph: " + ex.Message);
          }
        }
      }
      return null;
    }

    // The role a look-at value or an aimed effect can name: the tree's own spellings of them. Null for any other value.
    private WorldNpcPlacement ResolveWorldConversationRolePlacement(Conversation conversation, DialogNode node, string text, out bool isRole) {
      string symbolic = (text ?? String.Empty).Trim().ToLowerInvariant().Replace("_", " ").Replace("-", " ");
      isRole = true;
      if (symbolic == "speaker" || symbolic == "speaker cc") return FindWorldConversationSpeakerPlacement(conversation, node);
      if (symbolic == "listener" || symbolic == "listener cc")
        return node != null && node.IsPlayerNode ? FindWorldConversationNpcSpeakerPlacement(conversation, node) : EnsureWorldConversationPlayerPlacement();
      isRole = false;
      return null;
    }

    // The placement an actor is to look at. DEFAULT is whoever it is talking to -- the other end of the line, which is
    // the speaker for everyone but the speaker itself.
    private WorldNpcPlacement ResolveWorldConversationLookTarget(Conversation conversation, DialogNode node, string value, WorldNpcPlacement self) {
      string text = (value ?? String.Empty).Trim();
      if (String.Equals(text, "DEFAULT", StringComparison.OrdinalIgnoreCase)) {
        WorldNpcPlacement speaker = FindWorldConversationSpeakerPlacement(conversation, node);
        if (speaker != null && !ReferenceEquals(speaker, self)) return speaker;
        return node != null && node.IsPlayerNode ? FindWorldConversationNpcSpeakerPlacement(conversation, node) : EnsureWorldConversationPlayerPlacement();
      }
      WorldNpcPlacement role = ResolveWorldConversationRolePlacement(conversation, node, text, out bool isRole);
      if (isRole) return role;
      // Anything else is an actor the tree names outright, resolved exactly as a participant naming it would be.
      return FindWorldConversationActorPlacement(text);
    }

    private void ApplyWorldConversationLookAt(Conversation conversation, DialogNode node, WorldConversationCinematicAction action, WorldNpcPlacement placement) {
      // Only a body can turn its head: a placeable, a marker or an actor the preview has no body for is nothing to aim.
      if (placement == null) return;
      string value = (action?.Value ?? String.Empty).Trim();
      WorldNpcPlacement target = value.Length > 0 && !String.Equals(value, "OFF", StringComparison.OrdinalIgnoreCase)
        ? ResolveWorldConversationLookTarget(conversation, node, value, placement) : null;
      if (target == null || ReferenceEquals(target, placement)) {
        WorldNpcLookAt.Clear(placement);
        worldConversationLookAtNpcs.Remove(placement);
        WorldConversationDiagnosticFileWrite("CNV look-at: " + (placement.SourceFqn ?? "<virtual>") + " -> " + (value.Length == 0 || String.Equals(value, "OFF", StringComparison.OrdinalIgnoreCase) ? "off" : "'" + value + "' (no target)"));
        return;
      }
      List<WorldLookAtLayer> layers = GetWorldLookAtLayers(placement.BodyType);
      if (layers == null) {
        WorldNpcLookAt.Clear(placement);
        worldConversationLookAtNpcs.Remove(placement);
        WorldConversationDiagnosticFileWrite("CNV look-at: no look-at layers for body type '" + placement.BodyType + "'");
        return;
      }
      WorldNpcLookAt.Set(placement, target, layers);
      worldConversationLookAtNpcs.Add(placement);
      WorldConversationDiagnosticFileWrite("CNV look-at: " + (placement.SourceFqn ?? "<virtual>") + " -> " + (target.SourceFqn ?? "<virtual>") + " (" + layers.Count + " layer(s))");
    }

    private void RestoreWorldConversationLookAts() {
      foreach (WorldNpcPlacement placement in worldConversationLookAtNpcs.ToArray()) WorldNpcLookAt.Clear(placement);
      worldConversationLookAtNpcs.Clear();
    }
  }
}
