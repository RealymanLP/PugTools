using System;
using System.Collections.Generic;
using System.Linq;
using SlimDX;

namespace PugTools {
  // The placeables a conversation drives (port of Jedipedia cnvPreview.js viewerCnvDynState / viewerCnvDynPiece).
  //
  // A dyn assembly is every variant of itself at once -- a ship's hull, its collision hull, its door, its landing
  // lights -- and which of them are drawn is the STATE it stands in. A scene drives both halves: it names a state
  // (`Set Dynamic Placeable State`: the ship it launches, the door it opens, the console it blows up) and it turns
  // individual PIECES on and off inside it (`Set Dyn Piece Visible/Invisible`). Both are cumulative, exactly as the
  // game's are.
  //
  // The assembly's own states are shared by every placement of the plc, so a scene never edits them. It gives the ONE
  // placement it drives its own effective state (ConversationDyn), which ActiveSpnDynState hands to every consumer --
  // the draw, the lights, the effects -- in place of the preview clock's choice. Clearing it puts the placement back.
  internal sealed partial class View_AREA {
    private readonly List<WorldSpnPlacement> worldConversationDynPlacements = new List<WorldSpnPlacement>();

    // The placement of a plc a scene names. An area can place the same plc many times (a door type, a console); the one
    // meant is the one standing nearest the scene, which is the NPC the conversation is with, or the camera without one.
    internal WorldSpnPlacement FindWorldConversationSpn(string plcFqn, WorldNpcPlacement near) {
      string wanted = (plcFqn ?? String.Empty).Trim();
      List<WorldSpnPlacement> all = spnPlacements;
      if (wanted.Length == 0 || all == null || all.Count == 0) return null;
      List<WorldSpnPlacement> matches = all.Where(p => p != null && String.Equals(p.SourceFqn, wanted, StringComparison.OrdinalIgnoreCase)).ToList();
      if (matches.Count <= 1) return matches.FirstOrDefault();

      Vector3 origin;
      try {
        if (near != null) { Matrix nearWorld = NpcPlacementBaseWorld(near); origin = new Vector3(nearWorld.M41, nearWorld.M42, nearWorld.M43); }
        else if (camera != null) origin = camera.Position;
        else return null;
      } catch { return null; }

      WorldSpnPlacement best = null;
      float bestDistance = Single.MaxValue;
      foreach (WorldSpnPlacement candidate in matches) {
        try {
          Matrix world = SpnPlacementWorld(candidate, false);
          float distance = (new Vector3(world.M41, world.M42, world.M43) - origin).LengthSquared();
          if (Single.IsFinite(distance) && distance < bestDistance) { best = candidate; bestDistance = distance; }
        } catch { }
      }
      return best;
    }

    // Puts one placement in the state `stateName` with the pieces the scene has switched. A state the assembly does not
    // have, or none, leaves the placement in the one it is already in (the scene's earlier one, or the preview's).
    // `pieces` is row -> visible, 0-based into the assembly's rows, and only what the scene stated.
    internal bool SetWorldConversationDyn(WorldSpnPlacement placement, string stateName, IReadOnlyDictionary<int, bool> pieces) {
      if (placement?.DynStates == null || placement.DynStates.Count == 0) return false;

      WorldSpnDynState baseState = null;
      if (!String.IsNullOrWhiteSpace(stateName))
        baseState = placement.DynStates.FirstOrDefault(x => x != null && String.Equals(x.Name, stateName.Trim(), StringComparison.OrdinalIgnoreCase));
      if (baseState == null) {
        // The state in force right now, without the preview clock cycling it: the scene's own, or the first the
        // assembly authored. ConversationDyn is already a filtered copy, so its name leads back to the shared state.
        string current = placement.ConversationDyn?.Name;
        baseState = placement.DynStates.FirstOrDefault(x => x != null && String.Equals(x.Name, current, StringComparison.OrdinalIgnoreCase)) ?? placement.DynStates[0];
      }
      if (baseState == null) return false;

      var effective = new WorldSpnDynState { Name = baseState.Name, Hidden = baseState.Hidden };
      foreach (WorldSpnDynPart part in baseState.Parts) {
        if (part == null) continue;
        if (pieces != null && part.Row >= 0 && pieces.TryGetValue(part.Row, out bool shown) && !shown) continue;
        effective.Parts.Add(part);
      }
      if (pieces != null) {
        foreach (KeyValuePair<int, bool> piece in pieces) {
          if (!piece.Value || effective.Parts.Any(p => p.Row == piece.Key)) continue;
          // A row the state does not draw, switched on by the scene: built now, from how the template recorded it.
          if (baseState.RowParts == null || !baseState.RowParts.TryGetValue(piece.Key, out Func<WorldSpnDynPart> build)) continue;
          WorldSpnDynPart built = null;
          try { built = build(); } catch { }
          if (built != null) effective.Parts.Add(built);
        }
      }

      placement.ConversationDyn = effective;
      if (!worldConversationDynPlacements.Contains(placement)) worldConversationDynPlacements.Add(placement);
      return true;
    }

    internal void ClearWorldConversationDyn() {
      foreach (WorldSpnPlacement placement in worldConversationDynPlacements) if (placement != null) placement.ConversationDyn = null;
      worldConversationDynPlacements.Clear();
    }
  }
}
