using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using FileFormats;
using GomLib;
using GomLib.Models;
using SlimDX;
using Room = FileFormats.Room;
using AssetInstance = FileFormats.AssetInstance;

namespace PugTools {
  public partial class WorldBrowser {
    private const ulong WorldConversationRoleSpeaker = 16513498648939006850UL;
    private const ulong WorldConversationRoleListener = 15937209066439308881UL;
    private const ulong WorldConversationRoleSpeakerCc = 10143671182081221108UL;
    private const ulong WorldConversationRoleListenerCc = 3798166423556355225UL;
    private const ulong WorldConversationRolePlayer1 = 13999789947792456739UL;
    private const ulong WorldConversationRolePlayer2 = 13999791047304084950UL;
    private const ulong WorldConversationRolePlayer3 = 13999792146815713161UL;
    private const ulong WorldConversationRolePlayer4 = 13999793246327341372UL;
    private const ulong WorldConversationRoleVoteWinner = 16658114557975260616UL;
    private const ulong WorldConversationRoleVoteLoser1 = 3024371963656054499UL;
    private const ulong WorldConversationRoleVoteLoser2 = 3024373063167682710UL;
    private const ulong WorldConversationRoleVoteLoser3 = 3024374162679310921UL;

    private sealed class WorldConversationStageMarkRef {
      public string Stage;
      public string Mark;
    }

    private sealed class WorldConversationStagingInfo {
      public ulong Group;
      public WorldConversationStageMarkRef Player;
      public readonly Dictionary<ulong, WorldConversationStageMarkRef> Speakers = new Dictionary<ulong, WorldConversationStageMarkRef>();
    }

    private sealed class WorldConversationStageContext {
      public string Fqn;
      public GomObjectData Data;
      public Room Room;
      public Matrix World;
      public float Yaw;
    }

    private sealed class WorldConversationCameraPose {
      public Vector3 Position;
      public Vector3 Look;
      public Vector3 Up = Vector3.UnitY;
      public float? Fov;
    }

    private object worldConversationCinematicCacheDom;
    private readonly Dictionary<string, GomObjectData> worldConversationStageCache = new Dictionary<string, GomObjectData>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, GomObjectData> worldConversationAutoCameraCache = new Dictionary<string, GomObjectData>(StringComparer.OrdinalIgnoreCase);
    // stgCameraOffsetsPrototype: body type -> shot category -> Vector3 (see ApplyWorldConversationAutoCameraHeight).
    private object worldConversationCameraOffsets;
    private bool worldConversationCameraOffsetsLoaded;

    // File-backed conversation diagnostics. Visual Studio's Debug output can become very expensive while the world
    // renderer is running, so the Yalt/Morpheme diagnostics deliberately bypass Debug.WriteLine.
    private static readonly object worldConversationDiagnosticFileLock = new object();
    private static string worldConversationDiagnosticFilePath;
    private static bool worldConversationDiagnosticFileInitialized;

    private static void WorldConversationDiagnosticFileWrite(string line) {
      if (String.IsNullOrWhiteSpace(line)) return;
      lock (worldConversationDiagnosticFileLock) {
        try {
          if (String.IsNullOrWhiteSpace(worldConversationDiagnosticFilePath)) {
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!String.IsNullOrWhiteSpace(local)) {
              string dir = Path.Combine(local, "PugTools");
              Directory.CreateDirectory(dir);
              worldConversationDiagnosticFilePath = Path.Combine(dir, "PugTools-Yalt-Debug.log");
            } else {
              worldConversationDiagnosticFilePath = Path.Combine(Path.GetTempPath(), "PugTools-Yalt-Debug.log");
            }
          }
          if (!worldConversationDiagnosticFileInitialized) {
            File.WriteAllText(worldConversationDiagnosticFilePath,
              "PugTools conversation diagnostics " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + Environment.NewLine);
            worldConversationDiagnosticFileInitialized = true;
          }
          File.AppendAllText(worldConversationDiagnosticFilePath, line + Environment.NewLine);
        } catch {
          try {
            string fallback = Path.Combine(Path.GetTempPath(), "PugTools-Yalt-Debug.log");
            if (!String.Equals(worldConversationDiagnosticFilePath, fallback, StringComparison.OrdinalIgnoreCase)) {
              worldConversationDiagnosticFilePath = fallback;
              if (!worldConversationDiagnosticFileInitialized) {
                File.WriteAllText(fallback,
                  "PugTools conversation diagnostics " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + Environment.NewLine);
                worldConversationDiagnosticFileInitialized = true;
              }
              File.AppendAllText(fallback, line + Environment.NewLine);
            }
          } catch { }
        }
      }
    }

    private void ApplyWorldConversationStaging(Conversation conversation, DialogNode node) {
      if (conversation == null || node == null || panelRender == null) return;
      ClearWorldConversationStaging();
      EnsureWorldConversationAnimationCache();

      // Jedipedia puts the active actor into the conversation stance as soon as the preview takes ownership of it,
      // not only when the node happens to carry a cnvStagePresetMarksNPCs row. Many ordinary dialog nodes have no
      // staging preset at all; the old gate therefore left their speaker in the room-authored static/idle pose and
      // made an otherwise valid cinematic look completely unanimated. Explicit actions below still win afterwards.
      WorldNpcPlacement player = EnsureWorldConversationPlayerPlacement();
      ApplyWorldConversationDefaultStance(player);
      WorldNpcPlacement activeSpeaker = FindWorldConversationSpeakerPlacement(conversation, node);
      ApplyWorldConversationDefaultStance(activeSpeaker);

      WorldConversationStagingInfo staging = ReadWorldConversationStaging(conversation, node.NodeId);
      if (staging == null) return;
      if (player != null && staging.Player != null && !String.IsNullOrWhiteSpace(staging.Player.Stage) && !String.IsNullOrWhiteSpace(staging.Player.Mark) &&
          TryResolveWorldConversationActorMark(staging.Player.Stage, staging.Player.Mark, out Vector3 playerPosition, out float playerYaw, out Room playerRoom)) {
        if (player.ConversationVirtual && playerRoom != null) player.Room = playerRoom;
        WorldConversationDiagnosticFileWrite("CNV staging: node=" + node.NodeId + " PLAYER stage=" + staging.Player.Stage + " mark=" + staging.Player.Mark + " world=" + WorldConversationVec(playerPosition));
        panelRender.SetWorldConversationActorMark(player, playerPosition, playerYaw);
        worldConversationStagedNpcs.Add(player);
      }
      foreach (KeyValuePair<ulong, WorldConversationStageMarkRef> pair in staging.Speakers) {
        WorldConversationStageMarkRef markRef = pair.Value;
        WorldNpcPlacement placement = FindWorldConversationSpeakerPlacement(pair.Key);
        if (placement == null) continue;

        ApplyWorldConversationDefaultStance(placement);
        if (markRef == null || String.IsNullOrWhiteSpace(markRef.Stage) || String.IsNullOrWhiteSpace(markRef.Mark)) continue;
        if (!TryResolveWorldConversationActorMark(markRef.Stage, markRef.Mark, out Vector3 position, out float yaw, out Room markRoom)) {
          WorldConversationDiagnosticFileWrite("CNV staging: node=" + node.NodeId + " SPEAKER " + placement.SourceFqn + " stage=" + markRef.Stage + " mark=" + markRef.Mark + " UNRESOLVED");
          continue;
        }
        if (placement.ConversationVirtual && markRoom != null) placement.Room = markRoom;
        WorldConversationDiagnosticFileWrite("CNV staging: node=" + node.NodeId + " SPEAKER " + placement.SourceFqn + " stage=" + markRef.Stage + " mark=" + markRef.Mark + " world=" + WorldConversationVec(position));
        panelRender.SetWorldConversationActorMark(placement, position, yaw);
        worldConversationStagedNpcs.Add(placement);
      }
      UpdateWorldConversationVfxAnchors(staging);
    }

    private void EnsureWorldConversationAnimationCache() {
      if (ReferenceEquals(worldConversationAnimationCacheAssets, currentAssets)) return;
      worldConversationAnimationCache.Clear();
      worldConversationAnimationCacheAssets = currentAssets;
    }

    private void ApplyWorldConversationDefaultStance(WorldNpcPlacement placement) {
      if (placement == null || panelRender == null || worldConversationAnimatedNpcs.Contains(placement)) return;
      WorldNpcAnimationClip stance = ResolveNpcConversationPostureClip("Normal", placement.BodyType, worldConversationAnimationCache);
      if (stance == null) return;
      panelRender.SetWorldConversationAnimation(placement, stance, true);
      worldConversationAnimatedNpcs.Add(placement);
    }

    private void ApplyWorldConversationCinematicAction(Conversation conversation, DialogNode node, WorldConversationCinematicAction action) {
      if (conversation == null || node == null || action == null || action.Ignored || panelRender == null) return;
      string kind = (action.Type ?? String.Empty).Trim();
      if (kind.Length == 0) return;

      if (String.Equals(kind, "Set Camera", StringComparison.OrdinalIgnoreCase) ||
          String.Equals(kind, "Move Camera To Mark", StringComparison.OrdinalIgnoreCase)) {
        if (!TryResolveWorldConversationCameraPose(conversation, node, action.Value, out WorldConversationCameraPose pose)) return;
        Dictionary<string, string> parameters = WorldConversationActionParameters(action.Params);
        ApplyWorldConversationCameraOverrides(pose, parameters);
        WorldConversationDiagnosticFileWrite(WorldConversationCameraDiagnostic(conversation, node, kind, action.Value, pose));
        bool moving = String.Equals(kind, "Move Camera To Mark", StringComparison.OrdinalIgnoreCase);
        float moveSeconds = moving ? WorldConversationParameterSeconds(parameters, "cameramoveduration") : 0f;
        float rotateSeconds = moving ? WorldConversationParameterSeconds(parameters, "camerarotateduration") : 0f;
        float fovSeconds = moving ? WorldConversationParameterSeconds(parameters, "camerafovduration") : 0f;
        string moveType = moving ? WorldConversationParameterText(parameters, "movetype") : null;
        string rotateType = moving ? WorldConversationParameterText(parameters, "rotatetype") : null;
        string fovType = moving ? WorldConversationParameterText(parameters, "fovtype") : null;
        panelRender.SetWorldConversationCamera(pose.Position, pose.Look, pose.Up, pose.Fov,
          moveSeconds, rotateSeconds, fovSeconds, moveType, rotateType, fovType);
        return;
      }

      bool vfxAction = String.Equals(kind, "Play VFX", StringComparison.OrdinalIgnoreCase) ||
        String.Equals(kind, "Stop Character VFX", StringComparison.OrdinalIgnoreCase);
      // Do not synthesize a visible NPC merely because a VFX beat names it. Cinematic-extras nodes are often
      // transform-only markers; if no real/cast body already exists, the VFX path below resolves a staging anchor.
      WorldNpcPlacement placement = FindWorldConversationActionPlacement(conversation, node, action, !vfxAction);
      if (String.Equals(kind, "Set Mark", StringComparison.OrdinalIgnoreCase)) {
        if (placement == null) return;
        string text = (action.Value ?? String.Empty).Trim();
        int split = text.LastIndexOf(':');
        if (split <= 0 || split + 1 >= text.Length) return;
        if (!TryResolveWorldConversationActorMark(text.Substring(0, split), text.Substring(split + 1), out Vector3 position, out float yaw, out Room markRoom)) return;
        if (placement.ConversationVirtual && markRoom != null) placement.Room = markRoom;
        WorldConversationDiagnosticFileWrite("CNV set mark: node=" + (node?.NodeId ?? 0) + " " + placement.SourceFqn + " -> " + text + " world=" + WorldConversationVec(position));
        panelRender.SetWorldConversationActorMark(placement, position, yaw);
        worldConversationStagedNpcs.Add(placement);
        return;
      }

      if (String.Equals(kind, "Hide Actor", StringComparison.OrdinalIgnoreCase)) {
        if (placement == null) return;
        bool hidden = !String.Equals((action.Value ?? String.Empty).Trim(), "false", StringComparison.OrdinalIgnoreCase);
        panelRender.SetWorldConversationActorHidden(placement, hidden);
        worldConversationStagedNpcs.Add(placement);
        return;
      }

      if (String.Equals(kind, "Play VFX", StringComparison.OrdinalIgnoreCase)) {
        if (placement == null) placement = EnsureWorldConversationVfxAnchor(conversation, node, action);
        if (placement != null && !String.IsNullOrWhiteSpace(action.Value)) panelRender.PlayWorldConversationFx(placement, action.Value);
        return;
      }

      if (String.Equals(kind, "Stop Character VFX", StringComparison.OrdinalIgnoreCase)) {
        if (placement == null && !String.IsNullOrWhiteSpace(action.Actor)) worldConversationVfxAnchors.TryGetValue(action.Actor.Trim(), out placement);
        if (placement != null) panelRender.StopWorldConversationFx(placement, action.Value);
        return;
      }

      bool posture = String.Equals(kind, "Set Posture", StringComparison.OrdinalIgnoreCase) || String.Equals(kind, "Snap To Posture", StringComparison.OrdinalIgnoreCase);
      bool animation = posture || String.Equals(kind, "Play Animation", StringComparison.OrdinalIgnoreCase) || String.Equals(kind, "Queue Animation", StringComparison.OrdinalIgnoreCase);
      if (!animation || placement == null || String.IsNullOrWhiteSpace(action.Value)) return;
      EnsureWorldConversationAnimationCache();
      WorldNpcAnimationClip clip = posture
        ? ResolveNpcConversationPostureClip(action.Value.Trim(), placement.BodyType, worldConversationAnimationCache)
        : ResolveNpcConversationAnimationClip(action.Value.Trim(), placement.BodyType, worldConversationAnimationCache);
      if (clip == null) {
        WorldConversationDiagnosticFileWrite("CNV body clip unresolved: " + kind + " '" + action.Value + "' body=" +
          (placement.BodyType ?? "<none>") + " actor=" + (placement.SourceFqn ?? "<virtual>"));
        return;
      }
      WorldConversationDiagnosticFileWrite("CNV body clip: " + kind + " '" + action.Value + "' -> " + clip.Action +
        " body=" + (placement.BodyType ?? "<none>"));
      DebugWorldConversationClipDiagnostics(placement, clip);
      Dictionary<string, string> animationParameters = WorldConversationActionParameters(action.Params);
      float startAt = WorldConversationParameterFloat(animationParameters, "start", out float authoredStart) ? Math.Max(0f, authoredStart) : 0f;
      float speed = WorldConversationParameterFloat(animationParameters, "speed", out float authoredSpeed) && authoredSpeed > 0f ? authoredSpeed : 1f;
      panelRender.SetWorldConversationAnimation(placement, clip, posture, startAt, speed);
      worldConversationAnimatedNpcs.Add(placement);
    }

    private static void DebugWorldConversationClipDiagnostics(WorldNpcPlacement placement, WorldNpcAnimationClip clip) {
      try {
        JBAAnimation animation = clip?.Animation;
        if (animation == null || animation.BoneCount <= 0) return;
        int count = Math.Max(0, animation.BoneCount);
        var samples = new JBATransform[count];
        float sampleTime = animation.Length > .02f ? animation.Length * .5f : 0f;
        animation.SampleInto(sampleTime, samples);
        var rows = new List<Tuple<float, string>>();
        for (int channel = 0; channel < count; channel++) {
          string boneName = null;
          int rigIndex = -1;
          if (animation.BoneNames != null && channel < animation.BoneNames.Count &&
              !String.IsNullOrWhiteSpace(animation.BoneNames[channel]) &&
              !animation.BoneNames[channel].StartsWith("bone_", StringComparison.OrdinalIgnoreCase)) {
            boneName = animation.BoneNames[channel];
          } else if (clip.Rig?.AnimToRig != null && channel < clip.Rig.AnimToRig.Length) {
            rigIndex = clip.Rig.AnimToRig[channel];
            if (rigIndex >= 0 && rigIndex < clip.Rig.Bones.Count) boneName = clip.Rig.Bones[rigIndex].Name;
          }
          if (String.IsNullOrWhiteSpace(boneName)) boneName = "channel#" + channel;
          var t = samples[channel].Translation;
          float length = t.Length();
          string bind = String.Empty;
          if (rigIndex >= 0 && clip.Rig?.Bones != null && rigIndex < clip.Rig.Bones.Count) {
            var b = clip.Rig.Bones[rigIndex].BindTranslation;
            bind = String.Format(CultureInfo.InvariantCulture, " bind=({0:0.###},{1:0.###},{2:0.###})", b.X, b.Y, b.Z);
          }
          string row = String.Format(CultureInfo.InvariantCulture,
            "CNV DIAG ch={0} bone={1} t=({2:0.###},{3:0.###},{4:0.###}) |t|={5:0.###} hasT={6} bindT={7}{8}",
            channel, boneName, t.X, t.Y, t.Z, length, samples[channel].HasTranslation,
            animation.UsesRigBindTranslation(channel), bind);
          rows.Add(Tuple.Create(length, row));
        }
        var block = new List<string> {
          String.Format(CultureInfo.InvariantCulture,
            "CNV DIAG clip={0} network={1} body={2} actor={3} len={4:0.###} fps={5:0.###} channels={6} rig={7} skeleton={8}",
            clip.Action ?? "<none>", clip.Network ?? "<none>", placement?.BodyType ?? "<none>",
            placement?.SourceFqn ?? "<virtual>", animation.Length, animation.FPS, animation.BoneCount,
            clip.Rig?.Source ?? (clip.Rig == null ? "<named-jba>" : "<rig>"), clip.Skeleton?.Count ?? 0)
        };
        foreach (Tuple<float, string> row in rows.OrderByDescending(x => x.Item1).Take(12)) block.Add(row.Item2);
        block.Add(String.Empty.PadRight(72, '-'));
        WorldConversationDiagnosticFileWrite(String.Join(Environment.NewLine, block));
      } catch (Exception ex) {
        WorldConversationDiagnosticFileWrite("CNV DIAG failed: " + ex.Message);
      }
    }

    private WorldNpcPlacement FindWorldConversationActionPlacement(Conversation conversation, DialogNode node, WorldConversationCinematicAction action, bool createCast = true) {
      if (action == null) return null;
      if (!String.IsNullOrWhiteSpace(action.Actor)) {
        string symbolic = action.Actor.Trim().ToLowerInvariant().Replace("_", " ").Replace("-", " ");
        if (symbolic == "speaker" || symbolic == "speaker cc") return FindWorldConversationSpeakerPlacement(conversation, node);
        if (symbolic == "listener" || symbolic == "listener cc")
          return node != null && node.IsPlayerNode ? FindWorldConversationNpcSpeakerPlacement(conversation, node) : EnsureWorldConversationPlayerPlacement();
        if (WorldConversationPlayerActorName(action.Actor)) return EnsureWorldConversationPlayerPlacement();
        WorldNpcPlacement participant = FindWorldConversationActorPlacement(action.Actor);
        if (participant != null) return participant;
        // Hydra toolboxes frequently name members of the scene's cast that are not AREA placements at all. The game
        // creates those actors for the conversation; build the same NPC body on demand for any action that needs one.
        if (createCast && action.Actor.Trim().StartsWith("npc.", StringComparison.OrdinalIgnoreCase)) return EnsureWorldConversationCastPlacement(action.Actor);
      }

      if (action.ActorId == WorldConversationRoleSpeaker || action.ActorId == WorldConversationRoleSpeakerCc)
        return FindWorldConversationSpeakerPlacement(conversation, node);
      if (action.ActorId == WorldConversationRoleListener || action.ActorId == WorldConversationRoleListenerCc)
        return node != null && node.IsPlayerNode ? FindWorldConversationNpcSpeakerPlacement(conversation, node) : EnsureWorldConversationPlayerPlacement();
      if (WorldConversationPlayerRole(action.ActorId)) return EnsureWorldConversationPlayerPlacement();

      // A fair number of old cinematics omit hydObject on speaker-local animation/posture actions. The game resolves
      // those against the active speaker, so do the same instead of silently dropping the beat.
      return action.ActorId == 0 ? FindWorldConversationSpeakerPlacement(conversation, node) : null;
    }

    private WorldConversationStageMarkRef FindWorldConversationStagingMarkForActor(WorldConversationStagingInfo staging, string actorFqn) {
      string wanted = (actorFqn ?? String.Empty).Trim();
      if (staging == null || wanted.Length == 0 || currentDom == null) return null;
      foreach (KeyValuePair<ulong, WorldConversationStageMarkRef> pair in staging.Speakers) {
        string fqn = null;
        try { fqn = WorldResolveGomObject(pair.Key)?.Name; } catch { }
        if (String.IsNullOrWhiteSpace(fqn) && WorldUsesLegacyContent) fqn = WorldLegacyPrototypeName(pair.Key, null);
        if (String.IsNullOrWhiteSpace(fqn)) {
          try { fqn = currentDom.ConversationLoader.LoadSpeaker(pair.Key)?.Fqn; } catch { }
        }
        if (String.Equals((fqn ?? String.Empty).Trim(), wanted, StringComparison.OrdinalIgnoreCase)) return pair.Value;
      }
      return null;
    }

    private WorldNpcPlacement EnsureWorldConversationVfxAnchor(Conversation conversation, DialogNode node, WorldConversationCinematicAction action) {
      string actor = (action?.Actor ?? String.Empty).Trim();
      if (actor.Length == 0 || panelRender == null) return null;
      if (worldConversationVfxAnchors.TryGetValue(actor, out WorldNpcPlacement existing)) {
        PositionWorldConversationVfxAnchor(existing, ReadWorldConversationStaging(conversation, node?.NodeId ?? 0), actor);
        return existing;
      }
      WorldConversationStagingInfo staging = ReadWorldConversationStaging(conversation, node?.NodeId ?? 0);
      WorldConversationStageMarkRef mark = FindWorldConversationStagingMarkForActor(staging, actor);
      if (mark == null || String.IsNullOrWhiteSpace(mark.Stage) || String.IsNullOrWhiteSpace(mark.Mark)) return null;
      if (!TryResolveWorldConversationActorMark(mark.Stage, mark.Mark, out Vector3 position, out float yaw, out Room markRoom)) return null;
      var anchor = new WorldNpcPlacement {
        Room = markRoom ?? WorldConversationAnchorRoom() ?? area?.RoomList?.FirstOrDefault(x => x != null),
        Instance = null,
        SourceFqn = actor,
        Name = actor,
        ShowNameplate = false,
        Scale = 1f,
        ConversationVirtual = true,
        ConversationHidden = false,
        ConversationUnplaced = false,
        ConversationFxAnchor = true
      };
      panelRender.SetWorldConversationActorMark(anchor, position, yaw);
      worldConversationVfxAnchors[actor] = anchor;
      return anchor;
    }

    private void PositionWorldConversationVfxAnchor(WorldNpcPlacement anchor, WorldConversationStagingInfo staging, string actorFqn) {
      if (anchor == null || staging == null || panelRender == null) return;
      WorldConversationStageMarkRef mark = FindWorldConversationStagingMarkForActor(staging, actorFqn);
      if (mark == null || String.IsNullOrWhiteSpace(mark.Stage) || String.IsNullOrWhiteSpace(mark.Mark)) return;
      if (!TryResolveWorldConversationActorMark(mark.Stage, mark.Mark, out Vector3 position, out float yaw, out Room markRoom)) return;
      if (markRoom != null) anchor.Room = markRoom;
      panelRender.SetWorldConversationActorMark(anchor, position, yaw);
    }

    private void UpdateWorldConversationVfxAnchors(WorldConversationStagingInfo staging) {
      if (staging == null || worldConversationVfxAnchors.Count == 0) return;
      foreach (KeyValuePair<string, WorldNpcPlacement> pair in worldConversationVfxAnchors.ToArray())
        PositionWorldConversationVfxAnchor(pair.Value, staging, pair.Key);
    }

    private static bool WorldConversationPlayerRole(ulong role) =>
      role == WorldConversationRolePlayer1 || role == WorldConversationRolePlayer2 || role == WorldConversationRolePlayer3 || role == WorldConversationRolePlayer4 ||
      role == WorldConversationRoleVoteWinner || role == WorldConversationRoleVoteLoser1 || role == WorldConversationRoleVoteLoser2 || role == WorldConversationRoleVoteLoser3;

    private static bool WorldConversationPlayerActorName(string actor) {
      string text = (actor ?? String.Empty).Trim().ToLowerInvariant().Replace("_", " ").Replace("-", " ");
      return text == "player" || text == "player 1" || text == "player 2" || text == "player 3" || text == "player 4" ||
        text == "vote winner" || text == "vote loser" || text == "vote loser 1" || text == "vote loser 2" || text == "vote loser 3";
    }

    private Room WorldConversationAnchorRoom() {
      if (worldConversationPrimaryNpc?.Room != null) return worldConversationPrimaryNpc.Room;
      if (worldConversationPrimarySpn?.Room != null) return worldConversationPrimarySpn.Room;
      return null;
    }

    private bool TryGetWorldConversationAnchor(out Matrix world, out Room room) {
      world = Matrix.Identity;
      room = null;
      if (worldConversationPrimaryNpc != null) {
        room = worldConversationPrimaryNpc.Room;
        if (worldConversationPrimaryNpc.ConversationWorld.HasValue) {
          world = worldConversationPrimaryNpc.ConversationWorld.Value;
          return room != null && WorldConversationFiniteMatrix(world);
        }
        if (worldConversationPrimaryNpc.Instance != null && room != null) {
          try { world = worldConversationPrimaryNpc.Instance.GetAbsoluteTransform(room); return WorldConversationFiniteMatrix(world); } catch { }
        }
      }
      if (worldConversationPrimarySpn?.Instance != null && worldConversationPrimarySpn.Room != null) {
        room = worldConversationPrimarySpn.Room;
        try { world = worldConversationPrimarySpn.Instance.GetAbsoluteTransform(room); return WorldConversationFiniteMatrix(world); } catch { }
      }
      room = null;
      world = Matrix.Identity;
      return false;
    }

    private static bool WorldConversationFiniteMatrix(Matrix value) {
      return !(Single.IsNaN(value.M11) || Single.IsInfinity(value.M11) || Single.IsNaN(value.M12) || Single.IsInfinity(value.M12) ||
        Single.IsNaN(value.M13) || Single.IsInfinity(value.M13) || Single.IsNaN(value.M21) || Single.IsInfinity(value.M21) ||
        Single.IsNaN(value.M22) || Single.IsInfinity(value.M22) || Single.IsNaN(value.M23) || Single.IsInfinity(value.M23) ||
        Single.IsNaN(value.M31) || Single.IsInfinity(value.M31) || Single.IsNaN(value.M32) || Single.IsInfinity(value.M32) ||
        Single.IsNaN(value.M33) || Single.IsInfinity(value.M33) || Single.IsNaN(value.M41) || Single.IsInfinity(value.M41) ||
        Single.IsNaN(value.M42) || Single.IsInfinity(value.M42) || Single.IsNaN(value.M43) || Single.IsInfinity(value.M43));
    }

    private Matrix WorldConversationFallbackStageAnchor(string stageFqn, GomObjectData stageData, Matrix anchorWorld) {
      Vector3 entity = new Vector3(anchorWorld.M41, anchorWorld.M42, anchorWorld.M43);
      WorldConversationStagingInfo staging = ReadWorldConversationStaging(worldConversationPlaybackConversation, worldConversationPlaybackNode?.NodeId ?? 0);
      if (staging == null || stageData == null) {
        Matrix simple = Matrix.Identity; simple.M41 = entity.X; simple.M42 = entity.Y; simple.M43 = entity.Z; return simple;
      }

      WorldConversationStageMarkRef speakerRef = null;
      ulong speakerId = worldConversationPlaybackNode != null && worldConversationPlaybackNode.SpeakerId != 0
        ? worldConversationPlaybackNode.SpeakerId : worldConversationPlaybackConversation?.DefaultSpeakerId ?? 0;
      if (speakerId != 0) staging.Speakers.TryGetValue(speakerId, out speakerRef);
      if (speakerRef == null && staging.Speakers.Count == 1) speakerRef = staging.Speakers.Values.FirstOrDefault();
      if (speakerRef != null && !String.Equals(NormalizeWorldConversationStageFqn(speakerRef.Stage), stageFqn, StringComparison.OrdinalIgnoreCase)) speakerRef = null;
      WorldConversationStageMarkRef playerRef = staging.Player;
      if (playerRef != null && !String.Equals(NormalizeWorldConversationStageFqn(playerRef.Stage), stageFqn, StringComparison.OrdinalIgnoreCase)) playerRef = null;

      object actorMarks = WorldInteractionDataValue(stageData, "stgTemplateActorMarkList_ForPrototype", "4611686042788570002");
      GomObjectData speakerMark = speakerRef == null ? null : WorldConversationNamedObject(actorMarks, speakerRef.Mark);
      GomObjectData playerMark = playerRef == null ? null : WorldConversationNamedObject(actorMarks, playerRef.Mark);
      GomObjectData pinnedMark = speakerMark ?? playerMark;
      if (pinnedMark == null) {
        Matrix simple = Matrix.Identity; simple.M41 = entity.X; simple.M42 = entity.Y; simple.M43 = entity.Z; return simple;
      }

      Vector3 here = SpnDynVector3(WorldInteractionDataValue(pinnedMark, "stgMarkPosition", "4611686024438910010"), Vector3.Zero);
      float yaw = 0f;
      if (speakerMark != null && playerMark != null) {
        Vector3 there = SpnDynVector3(WorldInteractionDataValue(playerMark, "stgMarkPosition", "4611686024438910010"), Vector3.Zero);
        float toward = (float)Math.Atan2(-(there.Z - here.Z), there.X - here.X);
        Vector3 cameraStart = worldConversationOriginCameraPosition ?? (panelRender != null ? panelRender.CurrentCameraPosition : entity + Vector3.UnitZ);
        float wanted = (float)Math.Atan2(-(cameraStart.Z - entity.Z), cameraStart.X - entity.X);
        yaw = wanted - toward;
      }

      Matrix result = Matrix.RotationY(yaw);
      Vector3 turned = Vector3.TransformCoordinate(here, result);
      result.M41 = entity.X - turned.X;
      result.M42 = entity.Y - turned.Y;
      result.M43 = entity.Z - turned.Z;
      return result;
    }

    private static Matrix WorldConversationUprightStageAnchor(Matrix source) {
      Vector3 position = new Vector3(source.M41, source.M42, source.M43);
      float yaw = 0f;
      Vector3 right = new Vector3(source.M11, 0f, source.M13);
      if (right.LengthSquared() > .000001f) yaw = (float)Math.Atan2(-right.Z, right.X);
      Matrix result = Matrix.RotationY(yaw);
      result.M41 = position.X; result.M42 = position.Y; result.M43 = position.Z;
      return result;
    }

    private Matrix WorldConversationPlayerFallbackWorld() {
      if (!TryGetWorldConversationAnchor(out Matrix anchor, out _)) return Matrix.Identity;
      Matrix world = WorldConversationUprightStageAnchor(anchor);
      try {
        // Only a fallback until cnvStagePresetMarksPCs places the player. Keep the stand-in near the clicked NPC or
        // placeable (datacrons included) rather than at world origin for conversations without a PC staging preset.
        Vector3 forward = new Vector3(world.M31, 0f, world.M33);
        if (forward.LengthSquared() < .000001f) forward = Vector3.UnitZ; else forward.Normalize();
        world.M41 += forward.X * 1.5f; world.M43 += forward.Z * 1.5f;
        world = Matrix.RotationY((float)Math.PI) * world;
      } catch { world = Matrix.Identity; }
      return world;
    }

    private WorldNpcPlacement EnsureWorldConversationPlayerPlacement() {
      if (worldConversationPlayerNpc != null) {
        if (!worldConversationPlayerNpc.ConversationWorld.HasValue) worldConversationPlayerNpc.ConversationWorld = WorldConversationPlayerFallbackWorld();
        panelRender?.SetWorldConversationVirtualPlayer(worldConversationPlayerNpc);
        return worldConversationPlayerNpc;
      }
      if (currentDom == null || currentAssets == null || panelRender == null) return null;
      try {
        NpcAppearance appearance = null;
        try { appearance = currentDom.AppearanceLoader.Load("npp.npc.staging.pc.bmn") as NpcAppearance; } catch { }
        if (appearance == null) try { appearance = currentDom.AppearanceLoader.Load("npp.npc.default.bmn") as NpcAppearance; } catch { }
        if (appearance == null) return null;
        string bodyType = !String.IsNullOrWhiteSpace(appearance.BodyType) ? appearance.BodyType : "bmn";
        var appearanceCache = new Dictionary<string, List<GR2>>(StringComparer.OrdinalIgnoreCase);
        List<GR2> playerModels = GetNpcAppearanceModels(appearance, appearanceCache, bodyType);
        if (playerModels == null || playerModels.Count == 0) return null;

        Room room = WorldConversationAnchorRoom() ?? worldNpcPlacements?.FirstOrDefault(x => x?.Room != null)?.Room ?? area?.RoomList?.FirstOrDefault(x => x != null);
        if (room == null) return null;
        Matrix world = WorldConversationPlayerFallbackWorld();

        var placement = new WorldNpcPlacement {
          Room = room, Instance = null, SourceFqn = "npp.npc.staging.pc.bmn", Name = "Player", BodyType = bodyType,
          ShowNameplate = false, Scale = 1f, ConversationVirtual = true, ConversationWorld = world,
          Animation = ResolveNpcAnimationClip(null, bodyType, worldConversationAnimationCache)
        };
        foreach (GR2 model in playerModels) placement.Models.Add(model);
        AddNpcClothAssets(placement);
        worldConversationPlayerNpc = placement;
        panelRender.SetWorldConversationVirtualPlayer(placement);
        return placement;
      } catch (Exception ex) {
        System.Diagnostics.Debug.WriteLine("Conversation player stand-in failed: " + ex.Message);
        return null;
      }
    }

    private WorldNpcPlacement EnsureWorldConversationCastPlacement(string fqn, Room roomHint = null) {
      string clean = (fqn ?? String.Empty).Trim();
      if (clean.Length == 0 || !clean.StartsWith("npc.", StringComparison.OrdinalIgnoreCase) || currentDom == null || currentAssets == null || panelRender == null)
        return null;
      if (worldConversationCastNpcs.TryGetValue(clean, out WorldNpcPlacement existing)) {
        panelRender.AddWorldConversationVirtualActor(existing);
        return existing;
      }
      try {
        Room room = roomHint ?? WorldConversationAnchorRoom() ?? area?.RoomList?.FirstOrDefault(x => x != null);
        if (room == null) return null;
        Npc npc = null;
        try { npc = currentDom.NpcLoader.Load(clean); } catch { }
        var appearanceCache = new Dictionary<string, List<GR2>>(StringComparer.OrdinalIgnoreCase);
        var interactionCache = new Dictionary<string, WorldInteractionInfo>(StringComparer.OrdinalIgnoreCase);
        var weaponCache = new Dictionary<string, GR2>(StringComparer.OrdinalIgnoreCase);
        Dictionary<ulong, float> speciesScales = null;
        try { speciesScales = LoadNpcSpeciesScales(); } catch { }
        WorldNpcPlacement placement = npc == null ? null : BuildNpcPlacement(room, null, clean, npc, appearanceCache,
          speciesScales, null, worldConversationAnimationCache, interactionCache, weaponCache);
        if (placement == null && WorldUsesLegacyContent) {
          GomObject node = null;
          try { node = WorldResolveGomObject(clean); } catch { }
          placement = BuildLegacyNpcPlacement(room, null, clean, node, appearanceCache, speciesScales, null,
            worldConversationAnimationCache, interactionCache, weaponCache);
        }
        if (placement == null || placement.Models == null || placement.Models.Count == 0) return null;
        placement.ConversationVirtual = true;
        placement.ConversationUnplaced = true;
        placement.ConversationHidden = true;
        placement.ShowNameplate = false;
        placement.Room = room;
        placement.Instance = null;
        worldConversationCastNpcs[clean] = placement;
        panelRender.AddWorldConversationVirtualActor(placement);
        return placement;
      } catch (Exception ex) {
        WorldConversationDiagnosticFileWrite("CNV virtual cast failed " + clean + ": " + ex.Message);
        return null;
      }
    }

    private WorldNpcPlacement FindWorldConversationSpeakerPlacement(ulong speakerId) {
      if (speakerId == 0) return null;
      string fqn = null;
      try { fqn = WorldResolveGomObject(speakerId)?.Name; } catch { }
      if (String.IsNullOrWhiteSpace(fqn) && WorldUsesLegacyContent)
        fqn = WorldLegacyPrototypeName(speakerId, null);
      if (String.IsNullOrWhiteSpace(fqn)) {
        try { fqn = currentDom?.ConversationLoader.LoadSpeaker(speakerId)?.Fqn; } catch { }
      }
      WorldNpcPlacement placement = FindWorldConversationActorPlacement(fqn);
      if (placement != null) return placement;
      if (WorldConversationMayStandInForPrimary(fqn)) return worldConversationPrimaryNpc;
      return EnsureWorldConversationCastPlacement(fqn);
    }

    // The primary NPC stands in for ONE speaker the area never placed (a conversation started from a variant of the
    // body it names). It must not stand in for a second, different cast member while the primary NPC is itself named
    // by the conversation: both would then be staged on the same body and the later mark (here the pit one of
    // yalts_wife, NPC2) would carry the primary NPC away from the speaker it really is.
    private bool WorldConversationMayStandInForPrimary(string fqn) {
      if (worldConversationPrimaryNpc == null || String.IsNullOrWhiteSpace(fqn)) return false;
      string clean = fqn.Trim();
      if (!String.IsNullOrWhiteSpace(worldConversationPrimaryStandInFqn) &&
          String.Equals(worldConversationPrimaryStandInFqn, clean, StringComparison.OrdinalIgnoreCase)) return true;
      if (!String.IsNullOrWhiteSpace(worldConversationPrimaryStandInFqn)) return false;
      if (WorldConversationNamesPrimaryNpc()) {
        WorldConversationDiagnosticFileWrite("CNV speaker " + clean + " is a different cast member than the primary NPC " + worldConversationPrimaryNpc.SourceFqn + "; no stand-in");
        return false;
      }
      worldConversationPrimaryStandInFqn = clean;
      return true;
    }

    private WorldConversationStageMarkRef WorldConversationPrimarySpeakerMark(WorldConversationStagingInfo staging) {
      string primary = worldConversationPrimaryNpc?.SourceFqn;
      if (staging == null || String.IsNullOrWhiteSpace(primary)) return null;
      foreach (KeyValuePair<ulong, WorldConversationStageMarkRef> pair in staging.Speakers) {
        string fqn = null;
        try { fqn = WorldResolveGomObject(pair.Key)?.Name; } catch { }
        if (String.IsNullOrWhiteSpace(fqn)) { try { fqn = currentDom?.ConversationLoader.LoadSpeaker(pair.Key)?.Fqn; } catch { } }
        if (!String.IsNullOrWhiteSpace(fqn) && String.Equals(fqn.Trim(), primary.Trim(), StringComparison.OrdinalIgnoreCase)) return pair.Value;
      }
      return null;
    }

    private bool WorldConversationNamesPrimaryNpc() {
      string primary = worldConversationPrimaryNpc?.SourceFqn;
      if (String.IsNullOrWhiteSpace(primary) || worldConversationPlaybackConversation == null) return false;
      try {
        // Every speaker the conversation stages, in every group of the tree, not just the current node's.
        foreach (string speakerFqn in WorldConversationAllSpeakerFqns(worldConversationPlaybackConversation))
          if (String.Equals(speakerFqn, primary, StringComparison.OrdinalIgnoreCase)) return true;
      } catch { }
      return false;
    }

    private IEnumerable<string> WorldConversationAllSpeakerFqns(Conversation conversation) {
      var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      GomObject raw = null;
      try {
        raw = conversation.Id != 0 ? WorldResolveGomObject(conversation.Id) : null;
        if (raw == null && !String.IsNullOrWhiteSpace(conversation.Fqn)) raw = WorldResolveGomObject(conversation.Fqn);
      } catch { }
      if (raw?.Data == null) yield break;
      object speakerGroups = WorldInteractionDataValue(raw.Data, "cnvStagePresetMarksNPCs", "4611686068758931193");
      foreach (KeyValuePair<object, object> group in WorldConversationMapEntries(speakerGroups))
        foreach (KeyValuePair<object, object> row in WorldConversationMapEntries(group.Value)) {
          ulong id = WorldInteractionUnsigned(row.Key);
          if (id == 0) continue;
          string fqn = null;
          try { fqn = WorldResolveGomObject(id)?.Name; } catch { }
          if (String.IsNullOrWhiteSpace(fqn)) { try { fqn = currentDom?.ConversationLoader.LoadSpeaker(id)?.Fqn; } catch { } }
          if (!String.IsNullOrWhiteSpace(fqn) && seen.Add(fqn.Trim())) yield return fqn.Trim();
        }
    }

    private WorldConversationStagingInfo ReadWorldConversationStaging(Conversation conversation, long nodeId) {
      if (!TryGetWorldConversationRawData(conversation, nodeId, out GomObjectData root, out GomObjectData nodeData)) return null;
      ulong group = WorldInteractionUnsigned(WorldInteractionDataValue(nodeData, "cnvAssignmentPresetName", "4611686068758931198"));
      if (group == 0) return null;

      var result = new WorldConversationStagingInfo { Group = group };
      object playerGroups = WorldInteractionDataValue(root, "cnvStagePresetMarksPCs", "4611686068758931191");
      object playerGroup = WorldConversationMapValueUnsigned(playerGroups, group);
      GomObjectData playerOuter = WorldConversationFirstObjectData(playerGroup, true);
      result.Player = ReadWorldConversationStageMarkRef(playerOuter);

      object speakerGroups = WorldInteractionDataValue(root, "cnvStagePresetMarksNPCs", "4611686068758931193");
      object speakerGroup = WorldConversationMapValueUnsigned(speakerGroups, group);
      foreach (KeyValuePair<object, object> pair in WorldConversationMapEntries(speakerGroup)) {
        ulong speaker = WorldInteractionUnsigned(pair.Key);
        GomObjectData row = WorldConversationFirstObjectData(pair.Value, false);
        WorldConversationStageMarkRef mark = ReadWorldConversationStageMarkRef(row);
        if (speaker != 0 && mark != null) result.Speakers[speaker] = mark;
      }
      return result;
    }

    private WorldConversationStageMarkRef ReadWorldConversationStageMarkRef(GomObjectData row) {
      if (row == null) return null;
      object rawMark = WorldInteractionDataValue(row, "cnvMark", "4611686068758931197");
      GomObjectData mark = WorldConversationFirstObjectData(rawMark, false) ?? rawMark as GomObjectData;
      if (mark == null) return null;
      string stage = WorldInteractionText(WorldInteractionDataValue(mark, "cnvMarkTemplate", "4611686068758931195"));
      string name = WorldInteractionText(WorldInteractionDataValue(mark, "cnvMarkName", "4611686068758931196"));
      return !String.IsNullOrWhiteSpace(stage) && !String.IsNullOrWhiteSpace(name) ? new WorldConversationStageMarkRef { Stage = stage, Mark = name } : null;
    }

    private bool TryResolveWorldConversationActorMark(string stageFqn, string markName, out Vector3 position, out float yaw) =>
      TryResolveWorldConversationActorMark(stageFqn, markName, out position, out yaw, out _);

    private bool TryResolveWorldConversationActorMark(string stageFqn, string markName, out Vector3 position, out float yaw, out Room room) {
      position = Vector3.Zero; yaw = 0f; room = null;
      if (!TryGetWorldConversationStageContext(stageFqn, out WorldConversationStageContext stage)) return false;
      GomObjectData mark = WorldConversationNamedObject(WorldInteractionDataValue(stage.Data, "stgTemplateActorMarkList_ForPrototype", "4611686042788570002"), markName);
      if (mark == null) return false;
      Vector3 local = SpnDynVector3(WorldInteractionDataValue(mark, "stgMarkPosition", "4611686024438910010"), Vector3.Zero);
      Vector3 rotation = SpnDynVector3(WorldInteractionDataValue(mark, "stgMarkRotation", "4611686024438910011"), Vector3.Zero);
      position = Vector3.TransformCoordinate(local, stage.World);
      yaw = stage.Yaw + DegreesToRadians(rotation.Y);
      room = stage.Room;
      return IsFiniteVector(position);
    }

    private static string WorldConversationVec(Vector3 x) =>
      x.X.ToString("0.000", CultureInfo.InvariantCulture) + "," + x.Y.ToString("0.000", CultureInfo.InvariantCulture) + "," + x.Z.ToString("0.000", CultureInfo.InvariantCulture);

    private string WorldConversationCameraDiagnostic(Conversation conversation, DialogNode node, string kind, string value, WorldConversationCameraPose pose) {
      Func<Vector3, string> v = x => x.X.ToString("0.000", CultureInfo.InvariantCulture) + "," + x.Y.ToString("0.000", CultureInfo.InvariantCulture) + "," + x.Z.ToString("0.000", CultureInfo.InvariantCulture);
      var sb = new System.Text.StringBuilder();
      sb.Append("CNV camera: node=").Append(node?.NodeId ?? 0).Append(' ').Append(kind).Append(" '").Append(value).Append("' pos=").Append(v(pose.Position))
        .Append(" look=").Append(v(pose.Look)).Append(" fov=").Append(pose.Fov.HasValue ? pose.Fov.Value.ToString("0.000", CultureInfo.InvariantCulture) : "-");
      try {
        WorldNpcPlacement speaker = FindWorldConversationSpeakerPlacement(conversation, node);
        if (speaker != null) {
          Matrix w = speaker.ConversationWorld ?? Matrix.Identity;
          sb.Append(" | speaker=").Append(speaker.SourceFqn).Append(" body=").Append(speaker.BodyType ?? "?")
            .Append(" at=").Append(speaker.ConversationWorld.HasValue ? v(new Vector3(w.M41, w.M42, w.M43)) : "authored")
            .Append(" hidden=").Append(speaker.ConversationHidden).Append(" unplaced=").Append(speaker.ConversationUnplaced).Append(" virtual=").Append(speaker.ConversationVirtual);
        } else sb.Append(" | speaker=none");
      } catch { }
      return sb.ToString();
    }

    private bool TryResolveWorldConversationCameraPose(Conversation conversation, DialogNode node, string value, out WorldConversationCameraPose pose) {
      pose = null;
      string text = (value ?? String.Empty).Trim();
      if (text.Length == 0) return false;
      int split = text.LastIndexOf(':');
      if (split > 0 && text.Substring(0, split).IndexOf('.') >= 0) {
        string stageFqn = text.Substring(0, split);
        string markName = text.Substring(split + 1);
        if (!TryGetWorldConversationStageContext(stageFqn, out WorldConversationStageContext stage)) return false;
        GomObjectData camera = WorldConversationNamedObject(WorldInteractionDataValue(stage.Data, "stgTemplateCameraMarkList_ForPrototype", "4611686042788570001"), markName);
        if (camera != null) { pose = WorldConversationPoseFromCameraMark(stage, camera, 0f); return pose != null; }
        GomObjectData auto = WorldConversationNamedObject(WorldInteractionDataValue(stage.Data, "stgTemplateAutoCameraList_ForPrototype", "4611686042788570003"), markName);
        if (auto != null) { pose = WorldConversationPoseFromAutoCamera(stage, auto, ReadWorldConversationStaging(conversation, node?.NodeId ?? 0)); return pose != null; }
        return false;
      }
      return TryResolveWorldConversationAutoShot(conversation, node, text, out pose);
    }

    private bool TryResolveWorldConversationAutoShot(Conversation conversation, DialogNode node, string value, out WorldConversationCameraPose pose) {
      pose = null;
      WorldConversationStagingInfo staging = ReadWorldConversationStaging(conversation, node?.NodeId ?? 0);
      if (staging == null) return false;
      string shotText = (value ?? String.Empty).Trim();
      bool reaction = shotText.StartsWith("reaction.", StringComparison.OrdinalIgnoreCase);
      string shot = shotText;
      if (shot.StartsWith("auto.", StringComparison.OrdinalIgnoreCase)) shot = shot.Substring(5);
      else if (reaction) shot = shot.Substring(9);
      shot = shot.Trim().ToUpperInvariant();
      if (shot.Length == 0) return false;

      WorldConversationStageMarkRef speaker = null;
      ulong speakerId = node != null && node.SpeakerId != 0 ? node.SpeakerId : conversation?.DefaultSpeakerId ?? 0;
      if (speakerId != 0) staging.Speakers.TryGetValue(speakerId, out speaker);
      if (speaker == null && staging.Speakers.Count == 1) speaker = staging.Speakers.Values.First();
      // A player line names no speaker of its own, and a group that also stages a second cast member (here a
      // cinematic_utility body) has more than one. Jedipedia then takes the speaker the conversation was started from
      // (anchorSpeakerId); without it such a shot found no pair of marks and the camera simply stayed where it was.
      if (speaker == null) speaker = WorldConversationPrimarySpeakerMark(staging);
      WorldConversationStageMarkRef player = staging.Player;
      WorldConversationStageMarkRef subject = node != null && node.IsPlayerNode ? player : speaker;
      WorldConversationStageMarkRef other = node != null && node.IsPlayerNode ? speaker : player;
      if (reaction) { WorldConversationStageMarkRef swap = subject; subject = other; other = swap; }
      string stageFqn = subject?.Stage ?? player?.Stage ?? speaker?.Stage ?? staging.Speakers.Values.Select(x => x?.Stage).FirstOrDefault(x => !String.IsNullOrWhiteSpace(x));
      if (String.IsNullOrWhiteSpace(stageFqn) || !TryGetWorldConversationStageContext(stageFqn, out WorldConversationStageContext stage)) return false;

      string subjectMark = subject?.Mark ?? String.Empty;
      string otherMark = other?.Mark ?? String.Empty;
      string suffix = "." + shot.ToLowerInvariant();
      GomObjectData any = null, exact = null;
      object autoTable = WorldInteractionDataValue(stage.Data, "stgTemplateAutoCameraList_ForPrototype", "4611686042788570003");
      foreach (KeyValuePair<object, object> pair in WorldConversationMapEntries(autoTable)) {
        GomObjectData entry = WorldConversationFirstObjectData(pair.Value, false);
        if (entry == null) continue;
        string spec = WorldInteractionText(WorldInteractionDataValue(entry, "stgAutoCameraSpec", "4611686025877970033")) ?? String.Empty;
        if (!spec.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) continue;
        any ??= entry;
        string to = WorldInteractionText(WorldInteractionDataValue(entry, "stgAutoCameraToMark", "4611686025877970031")) ?? String.Empty;
        string from = WorldInteractionText(WorldInteractionDataValue(entry, "stgAutoCameraFromMark", "4611686025877970030")) ?? String.Empty;
        if (String.Equals(to, subjectMark, StringComparison.OrdinalIgnoreCase) && String.Equals(from, otherMark, StringComparison.OrdinalIgnoreCase)) { exact = entry; break; }
      }

      GomObjectData selected = exact;
      string via = exact != null ? "exact" : String.Empty;
      if (selected == null && !String.IsNullOrWhiteSpace(subjectMark) && !String.IsNullOrWhiteSpace(otherMark)) {
        string generated = WorldConversationAutoCameraTemplate(stage, shot, subjectMark, otherMark);
        if (!String.IsNullOrWhiteSpace(generated)) {
          selected = new GomObjectData();
          selected.Dictionary["4611686025877970030"] = otherMark;
          selected.Dictionary["4611686025877970031"] = subjectMark;
          selected.Dictionary["4611686025877970033"] = generated;
          via = "generated";
        }
      }
      selected ??= any;
      if (selected == null) { WorldConversationDiagnosticFileWrite("CNV auto shot: " + shot + " found no entry in " + stageFqn); return false; }
      if (via.Length == 0) via = "any";
      WorldConversationDiagnosticFileWrite("CNV auto shot: node=" + (node?.NodeId ?? 0) + " " + shot + " stage=" + stageFqn + " stageOrigin=" + WorldConversationVec(new Vector3(stage.World.M41, stage.World.M42, stage.World.M43)) +
        " subject=" + (subject?.Stage ?? "-") + ":" + subjectMark + " other=" + (other?.Stage ?? "-") + ":" + otherMark + " via=" + via +
        " to=" + WorldInteractionText(WorldInteractionDataValue(selected, "stgAutoCameraToMark", "4611686025877970031")) +
        " from=" + WorldInteractionText(WorldInteractionDataValue(selected, "stgAutoCameraFromMark", "4611686025877970030")));
      pose = WorldConversationPoseFromAutoCamera(stage, selected, staging);
      return pose != null;
    }

    private string WorldConversationAutoCameraTemplate(WorldConversationStageContext stage, string shot, string subjectMark, string otherMark) {
      if (String.Equals(shot, "CLOSE_UP", StringComparison.OrdinalIgnoreCase)) return "cam.auto.close_up.close_up";
      if (String.Equals(shot, "MEDIUM", StringComparison.OrdinalIgnoreCase)) return "cam.auto.medium.medium";
      GomObjectData subject = WorldConversationNamedObject(WorldInteractionDataValue(stage.Data, "stgTemplateActorMarkList_ForPrototype", "4611686042788570002"), subjectMark);
      GomObjectData other = WorldConversationNamedObject(WorldInteractionDataValue(stage.Data, "stgTemplateActorMarkList_ForPrototype", "4611686042788570002"), otherMark);
      if (subject == null || other == null) return null;
      Vector3 here = SpnDynVector3(WorldInteractionDataValue(subject, "stgMarkPosition", "4611686024438910010"), Vector3.Zero);
      Vector3 there = SpnDynVector3(WorldInteractionDataValue(other, "stgMarkPosition", "4611686024438910010"), Vector3.Zero);
      float metres = (here - there).Length() * 10f;
      float[] steps = { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f, 4.5f, 5f, 6f, 7f, 8f, 10f, 12f };
      float step = steps.OrderBy(x => Math.Abs(x - metres)).First();
      int whole = (int)Math.Floor(step);
      int tenth = (int)Math.Round((step - whole) * 10f);
      return "cam.auto." + whole.ToString(CultureInfo.InvariantCulture) + "-" + tenth.ToString(CultureInfo.InvariantCulture) + "." + shot.ToLowerInvariant();
    }

    private WorldConversationCameraPose WorldConversationPoseFromAutoCamera(WorldConversationStageContext stage, GomObjectData entry, WorldConversationStagingInfo staging = null) {
      string fromName = WorldInteractionText(WorldInteractionDataValue(entry, "stgAutoCameraFromMark", "4611686025877970030"));
      string toName = WorldInteractionText(WorldInteractionDataValue(entry, "stgAutoCameraToMark", "4611686025877970031"));
      string specFqn = WorldInteractionText(WorldInteractionDataValue(entry, "stgAutoCameraSpec", "4611686025877970033"));
      if (String.IsNullOrWhiteSpace(fromName) || String.IsNullOrWhiteSpace(toName) || String.IsNullOrWhiteSpace(specFqn)) return null;
      GomObjectData actorTable = WorldInteractionDataValue(stage.Data, "stgTemplateActorMarkList_ForPrototype", "4611686042788570002") as GomObjectData;
      object rawActorTable = actorTable ?? WorldInteractionDataValue(stage.Data, "stgTemplateActorMarkList_ForPrototype", "4611686042788570002");
      GomObjectData from = WorldConversationNamedObject(rawActorTable, fromName);
      GomObjectData to = WorldConversationNamedObject(rawActorTable, toName);
      GomObjectData spec = LoadWorldConversationAutoCamera(specFqn);
      if (from == null || to == null || spec == null) return null;

      Vector3 here = SpnDynVector3(WorldInteractionDataValue(to, "stgMarkPosition", "4611686024438910010"), Vector3.Zero);
      Vector3 away = SpnDynVector3(WorldInteractionDataValue(from, "stgMarkPosition", "4611686024438910010"), Vector3.Zero);
      float frameYaw = (float)Math.Atan2(-(away.Z - here.Z), away.X - here.X);
      Vector3 specPosition = SpnDynVector3(WorldInteractionDataValue(spec, "stgMarkPosition", "4611686024438910010"), Vector3.Zero);
      float cos = (float)Math.Cos(-frameYaw), sin = (float)Math.Sin(-frameYaw);
      Vector3 offset = new Vector3(specPosition.X * cos + specPosition.Z * sin, specPosition.Y, -specPosition.X * sin + specPosition.Z * cos);

      WorldConversationCameraPose pose = WorldConversationPoseFromCameraMark(stage, spec, -frameYaw);
      if (pose == null) return null;
      pose.Position = Vector3.TransformCoordinate(here + offset, stage.World);
      // The game frames an AUTO camera on whoever is in the shot: its height follows the actors' own camera bones.
      ApplyWorldConversationAutoCameraHeight(stage, spec, pose, here, away,
        WorldConversationMarkActor(staging, toName), WorldConversationMarkActor(staging, fromName));
      return pose;
    }

    // ---- Camera height correction for AUTO cameras (port of Jedipedia cnvPreview.js viewerCnvAdjustAutoPose) --------
    //
    // The game applies a per-body-type height correction to an AUTO camera and to nothing else (an authored CAM mark is
    // left exactly where the designer put it). stgCameraOffsetsPrototype maps body type -> shot category -> Vector3; the
    // row's y is minus the height the shot was framed at, and stgStage.GetCharacterCameraOffset adds the height of the
    // actor's own `Camera` bone above its mark. What comes out is how much taller/shorter THIS character is than the
    // body the shot was authored for. Body types without a row take the default rise below.
    //
    // Only the y of a row is applied (taken along the camera's own up axis). The two rows that also carry x/z
    // (astromech, hutt) are NOT applied horizontally yet -- the sign of the camera's right axis in this renderer's
    // handedness has not been verified, and a wrong sign would be worse than none.
    private const string WorldConversationCameraOffsetsNode = "stgCameraOffsetsPrototype";
    private const string WorldConversationCameraOffsetsField = "4611686061669731191";
    private const string WorldConversationCameraCategoryField = "4611686026015590038";
    private const float WorldConversationCameraRiseDefault = -0.18611453f;

    // Who stands on one of the staging's marks: the body a shot is framed on, rather than the spot it is framed at.
    private WorldNpcPlacement WorldConversationMarkActor(WorldConversationStagingInfo staging, string markName) {
      if (staging == null || String.IsNullOrWhiteSpace(markName)) return null;
      if (staging.Player != null && String.Equals(staging.Player.Mark, markName, StringComparison.OrdinalIgnoreCase))
        return worldConversationPlayerNpc;
      foreach (KeyValuePair<ulong, WorldConversationStageMarkRef> pair in staging.Speakers)
        if (pair.Value != null && String.Equals(pair.Value.Mark, markName, StringComparison.OrdinalIgnoreCase))
          return FindWorldConversationSpeakerPlacement(pair.Key);
      return null;
    }

    private object LoadWorldConversationCameraOffsets() {
      EnsureWorldConversationCinematicCaches();
      if (worldConversationCameraOffsetsLoaded) return worldConversationCameraOffsets;
      worldConversationCameraOffsetsLoaded = true;
      try {
        GomObjectData node = WorldResolveGomObject(WorldConversationCameraOffsetsNode)?.Data;
        worldConversationCameraOffsets = WorldInteractionDataValue(node, null, WorldConversationCameraOffsetsField);
      } catch { worldConversationCameraOffsets = null; }
      return worldConversationCameraOffsets;
    }

    private static object WorldConversationMapValue(object map, string key) {
      foreach (KeyValuePair<object, object> pair in WorldConversationMapEntries(map))
        if (String.Equals(pair.Key?.ToString(), key, StringComparison.OrdinalIgnoreCase)) return pair.Value;
      return null;
    }

    // GetCharacterCameraOffset for one actor, along the camera's up axis. `rise` is how far the actor's Camera bone
    // stands above the mark it is on; it travels with the offset because the shoulder and two-shot compare two actors.
    private bool TryGetWorldConversationCameraOffset(WorldNpcPlacement actor, float markWorldY, string category, Vector3 up,
        out Vector3 at, out float rise) {
      at = Vector3.Zero; rise = 0f;
      if (actor == null || panelRender == null || !panelRender.TryGetWorldConversationActorCameraRise(actor, markWorldY, out rise)) return false;
      float rowY = WorldConversationCameraRiseDefault;
      object rows = WorldConversationMapValue(LoadWorldConversationCameraOffsets(), actor.BodyType ?? String.Empty);
      object row = rows != null && !(rows is string) ? WorldConversationMapValue(rows, category) : null;
      if (row != null) rowY = SpnDynVector3(row, new Vector3(0f, WorldConversationCameraRiseDefault, 0f)).Y;
      at = up * (rowY + rise);
      return IsFiniteVector(at);
    }

    private static bool WorldConversationCameraUp(Vector3 look, out Vector3 up) {
      up = Vector3.UnitY;
      if (look.LengthSquared() < .000001f) return false;
      look.Normalize();
      // The camera never rolls, so its up axis is the world's up taken square to the view direction.
      up = Vector3.UnitY - look * Vector3.Dot(Vector3.UnitY, look);
      if (up.LengthSquared() < .000001f) return false;
      up.Normalize();
      return true;
    }

    // A mark's authored pitch is positive looking UP (Look is the negated view direction, so Look.Y = -sin(pitch)).
    // Jedipedia keeps the viewer's pitch, positive downwards, and subtracts the correction from it; that is this
    // adding it to the authored one: an actor taller than the shot was framed for tilts the camera up to them.
    private static void WorldConversationRaisePitch(WorldConversationCameraPose pose, float delta) {
      if (!Single.IsFinite(delta) || pose.Look.LengthSquared() < .000001f) return;
      Vector3 look = pose.Look; look.Normalize();
      float pitch = -(float)Math.Asin(Math.Max(-1f, Math.Min(1f, look.Y))) + delta;
      float yaw = (float)Math.Atan2(look.X, look.Z), cp = (float)Math.Cos(pitch);
      pose.Look = new Vector3((float)Math.Sin(yaw) * cp, -(float)Math.Sin(pitch), (float)Math.Cos(yaw) * cp);
      pose.Look.Normalize();
    }

    // utlMath:IntersectVectorExtentPlane -- the vector along `ray` to the plane through `point` with `normal`.
    private static Vector3 WorldConversationIntersectRay(Vector3 ray, Vector3 normal, Vector3 point) {
      float along = Vector3.Dot(ray, normal);
      return along != 0f ? ray * (Vector3.Dot(point, normal) / along) : Vector3.Zero;
    }

    private void ApplyWorldConversationAutoCameraHeight(WorldConversationStageContext stage, GomObjectData spec, WorldConversationCameraPose pose,
        Vector3 subjectLocal, Vector3 otherLocal, WorldNpcPlacement subjectActor, WorldNpcPlacement otherActor) {
      if (stage == null || spec == null || pose == null || subjectActor == null) return;
      string category = (WorldInteractionText(WorldInteractionDataValue(spec, "stgCameraCategory", WorldConversationCameraCategoryField)) ?? String.Empty).Trim().ToUpperInvariant();
      if (category.Length == 0 || !WorldConversationCameraUp(pose.Look, out Vector3 up)) return;
      // Each actor's height is measured from the mark it stands on; the shot's own geometry on the staging's floor
      // (GetAdjustedCameraParameters zeroes a mark's y for an AUTO camera).
      float subjectY = Vector3.TransformCoordinate(subjectLocal, stage.World).Y;
      float otherY = Vector3.TransformCoordinate(otherLocal, stage.World).Y;
      if (!TryGetWorldConversationCameraOffset(subjectActor, subjectY, category, up, out Vector3 at, out float rise)) return;
      bool haveOther = TryGetWorldConversationCameraOffset(otherActor, otherY, category, up, out Vector3 otherAt, out float otherRise);
      Vector3 subjectFlat = Vector3.TransformCoordinate(new Vector3(subjectLocal.X, 0f, subjectLocal.Z), stage.World);
      Vector3 otherFlat = Vector3.TransformCoordinate(new Vector3(otherLocal.X, 0f, otherLocal.Z), stage.World);
      if (category == "OVER_THE_SHOULDER") { if (haveOther) AdjustWorldConversationShoulder(pose, subjectFlat, otherFlat, at, otherAt, rise, otherRise); return; }
      if (category == "TWO_SHOT") { if (haveOther) AdjustWorldConversationTwoShot(pose, subjectFlat, otherFlat, at.Y, otherAt.Y); return; }
      pose.Position += at;
      WorldConversationDiagnosticFileWrite("CNV camera height: " + category + " body=" + (subjectActor.BodyType ?? "?") +
        " rise=" + rise.ToString("0.0000", CultureInfo.InvariantCulture) + " at=" + at.X.ToString("0.0000", CultureInfo.InvariantCulture) +
        "," + at.Y.ToString("0.0000", CultureInfo.InvariantCulture) + "," + at.Z.ToString("0.0000", CultureInfo.InvariantCulture));
    }

    // The over-the-shoulder shot does not move with the subject: the camera's own axis is followed out to each actor's
    // plane, both hit points are raised by that actor's own correction, and the camera slides along the line between
    // them back to the plane it started on, then pitches along the line joining the two eyelines.
    private static void AdjustWorldConversationShoulder(WorldConversationCameraPose pose, Vector3 subject, Vector3 other,
        Vector3 subjectAt, Vector3 otherAt, float rise, float otherRise) {
      Vector3 normal = subject - other;
      float span = normal.Length();
      if (!(span > .000001f)) return;
      normal /= span;
      Vector3 origin = pose.Position;
      Vector3 forward = pose.Look; forward.Normalize(); // the line is what matters here, so the sign of Look does not
      if (Vector3.Dot(forward, normal) == 0f) return;
      Vector3 toSubject = origin + WorldConversationIntersectRay(forward, normal, subject - origin) + subjectAt;
      Vector3 toOther = origin + WorldConversationIntersectRay(forward, normal, other - origin) + otherAt;
      Vector3 line = toSubject - toOther;
      if (Vector3.Dot(line, normal) == 0f) return;
      pose.Position = toOther + WorldConversationIntersectRay(line, normal, origin - toOther);
      WorldConversationRaisePitch(pose, (float)Math.Atan2(rise - otherRise, span));
    }

    // The two-shot holds both actors at once, so it does not move: it opens the field of view by as much as the two
    // heights disagree and pitches to the middle of them.
    private static void AdjustWorldConversationTwoShot(WorldConversationCameraPose pose, Vector3 subject, Vector3 other,
        float subjectRaise, float otherRaise) {
      if (!pose.Fov.HasValue || !(pose.Fov.Value > 0f)) return;
      Vector3 origin = pose.Position;
      float tan = (float)Math.Tan(pose.Fov.Value / 2f);
      Func<float, Vector3, float> subtend = (raise, mark) => {
        float flat = (float)Math.Sqrt((origin.X - mark.X) * (origin.X - mark.X) + (origin.Z - mark.Z) * (origin.Z - mark.Z));
        return flat > 0f ? (float)Math.Atan2(raise, (tan * tan + raise * tan / flat + 1f) * flat) : 0f;
      };
      float high = subtend(subjectRaise, subject), low = subtend(otherRaise, other);
      pose.Fov = pose.Fov.Value + Math.Abs(high - low);
      WorldConversationRaisePitch(pose, (high + low) / 2f);
    }

    private WorldConversationCameraPose WorldConversationPoseFromCameraMark(WorldConversationStageContext stage, GomObjectData mark, float extraYaw) {
      if (stage == null || mark == null) return null;
      Vector3 local = SpnDynVector3(WorldInteractionDataValue(mark, "stgMarkPosition", "4611686024438910010"), Vector3.Zero);
      Vector3 rotation = SpnDynVector3(WorldInteractionDataValue(mark, "stgMarkRotation", "4611686024438910011"), Vector3.Zero);
      float pitch = DegreesToRadians(rotation.X);
      float yaw = stage.Yaw + DegreesToRadians(rotation.Y) + extraYaw;
      float cp = (float)Math.Cos(pitch), sp = (float)Math.Sin(pitch);
      // Jedipedia's FlyingCamera returns the actual visible forward vector (yaw 0 = -Z). PugTools' FpsCamera feeds
      // its stored Look vector into a RH projection whose visible direction is -Look -- the same convention already
      // handled by the taxi/space-camera paths. Store the COMPLETE negation of Jedipedia's forward (including pitch),
      // otherwise a correct camera mark looks 180 degrees away horizontally and pitched the wrong way vertically.
      Vector3 look = new Vector3((float)Math.Sin(yaw) * cp, -sp, (float)Math.Cos(yaw) * cp);
      if (!IsFiniteVector(look) || look.LengthSquared() < .000001f) return null;
      look.Normalize();
      float fovDegrees = SpnDynNumber(WorldInteractionDataValue(mark, "stgCameraFOV", "4611686025915590054") ?? WorldInteractionDataValue(mark, "stgCameraFov", null), Single.NaN);
      if (Single.IsNaN(fovDegrees)) {
        // Authored camera marks carry a field of view of 18.5-30 degrees; a camera without one falls back on the
        // viewer's own, much wider, which changes the whole framing. Record what the mark does hold.
        var held = new System.Text.StringBuilder();
        foreach (KeyValuePair<string, object> kv in mark.Dictionary) {
          string text = kv.Value == null ? "null" : kv.Value.ToString();
          held.Append(kv.Key).Append('=').Append(text.Length > 40 ? text.Substring(0, 40) : text).Append("; ");
        }
        WorldConversationDiagnosticFileWrite("CNV camera mark without fov: " + held);
      }
      return new WorldConversationCameraPose {
        Position = Vector3.TransformCoordinate(local, stage.World),
        Look = look,
        Up = Vector3.UnitY,
        Fov = !Single.IsNaN(fovDegrees) && fovDegrees > 0f ? DegreesToRadians(fovDegrees) : (float?)null
      };
    }

    private void ApplyWorldConversationCameraOverrides(WorldConversationCameraPose pose, Dictionary<string, string> parameters) {
      if (pose == null || parameters == null || parameters.Count == 0) return;
      if (WorldConversationParameterFloat(parameters, "overridepositionx", out float x)) pose.Position.X += x;
      if (WorldConversationParameterFloat(parameters, "overridepositiony", out float y)) pose.Position.Y += y;
      if (WorldConversationParameterFloat(parameters, "overridepositionz", out float z)) pose.Position.Z += z;
      if (WorldConversationParameterFloat(parameters, "overridefov", out float fov) && fov > 0f) pose.Fov = DegreesToRadians(fov);

      bool hasPitch = WorldConversationParameterFloat(parameters, "overriderotationx", out float pitchDeg);
      bool hasYaw = WorldConversationParameterFloat(parameters, "overriderotationy", out float yawDeg);
      if (!hasPitch && !hasYaw) return;
      Vector3 look = pose.Look;
      if (look.LengthSquared() < .000001f) return;
      look.Normalize();
      float pitch = hasPitch ? DegreesToRadians(pitchDeg) : -(float)Math.Asin(Math.Max(-1f, Math.Min(1f, look.Y)));
      float yaw = (float)Math.Atan2(look.X, look.Z);
      if (hasYaw) yaw += DegreesToRadians(yawDeg);
      float cp = (float)Math.Cos(pitch);
      pose.Look = new Vector3((float)Math.Sin(yaw) * cp, -(float)Math.Sin(pitch), (float)Math.Cos(yaw) * cp);
      if (pose.Look.LengthSquared() > .000001f) pose.Look.Normalize();
    }

    private Dictionary<string, string> WorldConversationActionParameters(string raw) {
      var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
      if (String.IsNullOrWhiteSpace(raw)) return result;
      foreach (string pair in raw.Split(';')) {
        int split = pair.IndexOf('=');
        if (split <= 0) continue;
        string key = pair.Substring(0, split).Trim();
        if (key.Length == 0) continue;
        result[key] = pair.Substring(split + 1).Trim();
      }
      return result;
    }

    private static float WorldConversationParameterSeconds(Dictionary<string, string> parameters, string key) {
      return WorldConversationParameterFloat(parameters, key, out float seconds) && seconds > 0f ? seconds : 0f;
    }

    private static string WorldConversationParameterText(Dictionary<string, string> parameters, string key) {
      return parameters != null && parameters.TryGetValue(key, out string text) ? (text ?? String.Empty).Trim() : String.Empty;
    }

    private static bool WorldConversationParameterFloat(Dictionary<string, string> parameters, string key, out float value) {
      value = 0f;
      return parameters != null && parameters.TryGetValue(key, out string text) &&
        Single.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) && !Single.IsNaN(value) && !Single.IsInfinity(value);
    }

    private bool TryGetWorldConversationStageContext(string stageFqn, out WorldConversationStageContext context) {
      context = null;
      string clean = NormalizeWorldConversationStageFqn(stageFqn);
      if (clean.Length == 0 || area?.RoomList == null) return false;
      GomObjectData stageData = LoadWorldConversationStage(clean);
      if (stageData == null) return false;

      Matrix bestWorld = Matrix.Identity;
      Room bestRoom = null;
      bool found = false;
      float bestDistance = Single.MaxValue;
      Vector3 reference = Vector3.Zero;
      bool hasReference = TryGetWorldConversationAnchor(out Matrix anchorWorld, out Room anchorRoom);
      if (hasReference) reference = new Vector3(anchorWorld.M41, anchorWorld.M42, anchorWorld.M43);

      foreach (Room room in area.RoomList) {
        if (room?.InstancesById == null) continue;
        foreach (AssetInstance instance in room.InstancesById.Values) {
          if (instance == null || !area.AssetIdMap.TryGetValue(instance.assetID, out AreaAsset asset) || asset == null) continue;
          if (!String.Equals(NormalizeWorldConversationStageAsset(asset), clean, StringComparison.OrdinalIgnoreCase)) continue;
          Matrix world;
          try { world = instance.GetAbsoluteTransform(room); } catch { continue; }
          float distance = 0f;
          if (hasReference) {
            Vector3 p = new Vector3(world.M41, world.M42, world.M43);
            distance = (p - reference).LengthSquared();
          }
          if (!found || distance < bestDistance) { found = true; bestDistance = distance; bestWorld = world; bestRoom = room; }
        }
      }
      // Datacrons and many other placeable conversations reference an STG template without placing that STG as a
      // room instance. In that case the game instantiates the stage relative to the interaction object. Do the same
      // here so actor marks, animations, Set Camera and Move Camera all get a real world-space frame instead of
      // silently failing and leaving the camera at the point where playback started.
      if (!found && hasReference) {
        found = true;
        bestRoom = anchorRoom;
        bestWorld = WorldConversationFallbackStageAnchor(clean, stageData, anchorWorld);
      }
      if (!found) return false;
      context = new WorldConversationStageContext {
        Fqn = clean,
        Data = stageData,
        Room = bestRoom,
        World = bestWorld,
        Yaw = (float)Math.Atan2(-bestWorld.M13, bestWorld.M11)
      };
      return true;
    }

    private GomObjectData LoadWorldConversationStage(string fqn) {
      EnsureWorldConversationCinematicCaches();
      if (worldConversationStageCache.TryGetValue(fqn, out GomObjectData cached)) return cached;
      GomObjectData data = null;
      try { data = WorldResolveGomObject(fqn)?.Data; } catch { }
      worldConversationStageCache[fqn] = data;
      return data;
    }

    private GomObjectData LoadWorldConversationAutoCamera(string fqn) {
      EnsureWorldConversationCinematicCaches();
      if (worldConversationAutoCameraCache.TryGetValue(fqn, out GomObjectData cached)) return cached;
      GomObjectData data = null;
      try { data = WorldResolveGomObject(fqn)?.Data; } catch { }
      worldConversationAutoCameraCache[fqn] = data;
      return data;
    }

    private void EnsureWorldConversationCinematicCaches() {
      if (ReferenceEquals(worldConversationCinematicCacheDom, currentDom)) return;
      worldConversationCinematicCacheDom = currentDom;
      worldConversationStageCache.Clear();
      worldConversationAutoCameraCache.Clear();
      worldConversationCameraOffsets = null;
      worldConversationCameraOffsetsLoaded = false;
    }

    private static string NormalizeWorldConversationStageAsset(AreaAsset asset) {
      if (asset == null || !String.Equals(asset.Extension, "stg", StringComparison.OrdinalIgnoreCase)) return String.Empty;
      string path = asset.Path ?? String.Empty;
      if (!path.EndsWith(".stg", StringComparison.OrdinalIgnoreCase)) path += ".stg";
      return NormalizeWorldConversationStageFqn(path);
    }

    private static string NormalizeWorldConversationStageFqn(string value) {
      string text = (value ?? String.Empty).Trim().Replace('\\', '/').Trim('/').ToLowerInvariant();
      if (text.EndsWith(".stg", StringComparison.OrdinalIgnoreCase)) text = text.Substring(0, text.Length - 4);
      while (text.StartsWith("gamedata/", StringComparison.OrdinalIgnoreCase) || text.StartsWith("server/", StringComparison.OrdinalIgnoreCase) || text.StartsWith("resources/", StringComparison.OrdinalIgnoreCase)) {
        int slash = text.IndexOf('/');
        text = slash >= 0 ? text.Substring(slash + 1) : String.Empty;
      }
      text = text.Replace('/', '.').Trim('.');
      return text;
    }

    private bool TryGetWorldConversationRawData(Conversation conversation, long nodeId, out GomObjectData root, out GomObjectData nodeData) {
      root = null; nodeData = null;
      if (conversation == null || currentDom == null || nodeId == 0) return false;
      GomObject raw = null;
      try {
        raw = conversation.Id != 0 ? WorldResolveGomObject(conversation.Id) : null;
        if (raw == null && !String.IsNullOrWhiteSpace(conversation.Fqn)) raw = WorldResolveGomObject(conversation.Fqn);
      } catch { }
      root = raw?.Data;
      if (root == null) return false;
      object dialogs = WorldInteractionDataValue(root, "cnvTreeDialogNodes_Prototype", "4611686050212071021");
      foreach (KeyValuePair<object, object> pair in WorldConversationMapEntries(dialogs)) {
        GomObjectData candidate = WorldConversationFirstObjectData(pair.Value, false);
        if (WorldUsesLegacyContent && (candidate == null || WorldInteractionDataValue(candidate, "cnvNodeNumber", "4611686019044571365") == null))
          candidate = WorldConversationFindObjectDataWithField(pair.Value, "cnvNodeNumber", "4611686019044571365");
        if (candidate == null) continue;
        long id = WonkInt64(WorldInteractionDataValue(candidate, "cnvNodeNumber", "4611686019044571365"));
        if (id == 0) id = WonkInt64(pair.Key);
        if (id == nodeId) { nodeData = candidate; return true; }
      }
      return false;
    }

    private static object WorldConversationMapValueUnsigned(object map, ulong wanted) {
      foreach (KeyValuePair<object, object> pair in WorldConversationMapEntries(map))
        if (WorldInteractionUnsigned(pair.Key) == wanted) return pair.Value;
      return null;
    }

    private static IEnumerable<KeyValuePair<object, object>> WorldConversationMapEntries(object map) {
      if (map is GomObjectData gom) {
        foreach (KeyValuePair<string, object> pair in gom.Dictionary) {
          if (String.Equals(pair.Key, "_count", StringComparison.OrdinalIgnoreCase)) continue;
          yield return new KeyValuePair<object, object>(pair.Key, pair.Value);
        }
        yield break;
      }
      if (map is IDictionary dictionary) {
        IDictionaryEnumerator it = dictionary.GetEnumerator();
        while (it.MoveNext()) {
          if (String.Equals(it.Key?.ToString(), "_count", StringComparison.OrdinalIgnoreCase)) continue;
          yield return new KeyValuePair<object, object>(it.Key, it.Value);
        }
        yield break;
      }
      // A few early beta schemas expose a serialized LookupList/List through a non-generic enumerable wrapper rather
      // than IDictionary.  Modern 64-bit data never reaches this branch.  Preserve each element and give it a stable
      // synthetic key; the RED dialog rows carry cnvNodeNumber themselves, so their real id is recovered below.
      if (map is IEnumerable enumerable && map is not string) {
        int index = 1;
        foreach (object value in enumerable) yield return new KeyValuePair<object, object>(index++, value);
      }
    }

    private static GomObjectData WorldConversationFirstObjectData(object value, bool descendOneLevel) {
      if (value is GomObjectData direct) {
        if (!descendOneLevel) return direct;
        object preferred = direct.Dictionary.TryGetValue("1", out object one) ? one : direct.Dictionary.Values.FirstOrDefault();
        return preferred as GomObjectData ?? direct;
      }
      if (value is IDictionary dictionary) {
        object preferred = null;
        if (dictionary.Contains("1")) preferred = dictionary["1"];
        if (preferred == null) {
          IDictionaryEnumerator it = dictionary.GetEnumerator();
          while (it.MoveNext()) { if (!String.Equals(it.Key?.ToString(), "_count", StringComparison.OrdinalIgnoreCase)) { preferred = it.Value; break; } }
        }
        return preferred as GomObjectData;
      }
      return null;
    }


    // RED/Beta occasionally wraps ClassView values in one or more container layers.  The retail parser path above
    // intentionally stays unchanged; this helper is used only by legacy fallbacks and finds the nested ClassView by
    // a field that identifies the structure we are after (dialog node, link node, root node, loc retriever, ...).
    private static GomObjectData WorldConversationFindObjectDataWithField(object value, string fieldName, string numericFieldName, int depth = 0) {
      if (value == null || depth > 6 || value is string) return null;
      if (value is GomObject gomObject) {
        try { return WorldConversationFindObjectDataWithField(gomObject.Data, fieldName, numericFieldName, depth + 1); }
        catch { return null; }
      }
      if (value is GomObjectData data) {
        if (WorldInteractionDataValue(data, fieldName, numericFieldName) != null) return data;
        foreach (KeyValuePair<string, object> pair in data.Dictionary) {
          if (String.Equals(pair.Key, "_count", StringComparison.OrdinalIgnoreCase)) continue;
          GomObjectData found = WorldConversationFindObjectDataWithField(pair.Value, fieldName, numericFieldName, depth + 1);
          if (found != null) return found;
        }
        return null;
      }
      if (value is IDictionary dictionary) {
        foreach (DictionaryEntry pair in dictionary) {
          if (String.Equals(pair.Key?.ToString(), "_count", StringComparison.OrdinalIgnoreCase)) continue;
          GomObjectData found = WorldConversationFindObjectDataWithField(pair.Value, fieldName, numericFieldName, depth + 1);
          if (found != null) return found;
        }
        return null;
      }
      if (value is IEnumerable enumerable) {
        foreach (object item in enumerable) {
          GomObjectData found = WorldConversationFindObjectDataWithField(item, fieldName, numericFieldName, depth + 1);
          if (found != null) return found;
        }
      }
      return null;
    }

    private static IEnumerable<GomObjectData> WorldConversationFindAllObjectDataWithField(object value, string fieldName, string numericFieldName, int depth = 0) {
      if (value == null || depth > 6 || value is string) yield break;
      if (value is GomObject gomObject) {
        GomObjectData root = null;
        try { root = gomObject.Data; } catch { }
        if (root != null)
          foreach (GomObjectData found in WorldConversationFindAllObjectDataWithField(root, fieldName, numericFieldName, depth + 1)) yield return found;
        yield break;
      }
      if (value is GomObjectData data) {
        if (WorldInteractionDataValue(data, fieldName, numericFieldName) != null) {
          yield return data;
          yield break;
        }
        foreach (KeyValuePair<string, object> pair in data.Dictionary) {
          if (String.Equals(pair.Key, "_count", StringComparison.OrdinalIgnoreCase)) continue;
          foreach (GomObjectData found in WorldConversationFindAllObjectDataWithField(pair.Value, fieldName, numericFieldName, depth + 1)) yield return found;
        }
        yield break;
      }
      if (value is IDictionary dictionary) {
        foreach (DictionaryEntry pair in dictionary) {
          if (String.Equals(pair.Key?.ToString(), "_count", StringComparison.OrdinalIgnoreCase)) continue;
          foreach (GomObjectData found in WorldConversationFindAllObjectDataWithField(pair.Value, fieldName, numericFieldName, depth + 1)) yield return found;
        }
        yield break;
      }
      if (value is IEnumerable enumerable) {
        foreach (object item in enumerable)
          foreach (GomObjectData found in WorldConversationFindAllObjectDataWithField(item, fieldName, numericFieldName, depth + 1)) yield return found;
      }
    }

    private static GomObjectData WorldConversationNamedObject(object table, string name) {
      if (String.IsNullOrWhiteSpace(name)) return null;
      foreach (KeyValuePair<object, object> pair in WorldConversationMapEntries(table)) {
        if (!String.Equals(pair.Key?.ToString(), name, StringComparison.OrdinalIgnoreCase)) continue;
        return WorldConversationFirstObjectData(pair.Value, false);
      }
      return null;
    }

    private static float DegreesToRadians(float degrees) => degrees * (float)Math.PI / 180f;
    private static bool IsFiniteVector(Vector3 value) => !Single.IsNaN(value.X) && !Single.IsInfinity(value.X) && !Single.IsNaN(value.Y) && !Single.IsInfinity(value.Y) && !Single.IsNaN(value.Z) && !Single.IsInfinity(value.Z);
  }
}
