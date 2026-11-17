using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FileFormats;

namespace PugTools {
  // One additive layer of a body type's look-at stack (port of Jedipedia's `layers` in viewerNpcAnimLookAtLayers).
  //
  // A conversation network lays one SingleFrame per axis over whatever clip an actor is playing: `ad_head_yaw` and
  // `ad_head_pitch`, each only three frames wide -- the head, neck and neck1 bones turned one way, at rest, and turned
  // the other way. The network's own control expression says how an ANGLE in degrees becomes a position in that clip
  // (`Fraction`, 0..1), and which frame that lands on is the turn.
  internal sealed class WorldLookAtLayer {
    public string Axis;   // "yaw" or "pitch"
    public float Weight;  // how much of the layer the graph plays at its defaults
    public WorldNpcAnimationClip Clip;
    public Func<float, float> Fraction;

    // The additive clip's channel for each bone of a skeleton, built once per skeleton: the same bone-name join the
    // body clip itself is posed with.
    private readonly ConditionalWeakTable<object, int[]> bindings = new ConditionalWeakTable<object, int[]>();

    public int[] GetBinding(object skeletonKey, Func<int[]> build) {
      if (skeletonKey == null) return null;
      if (bindings.TryGetValue(skeletonKey, out int[] existing)) return existing;
      int[] created = build();
      bindings.Add(skeletonKey, created);
      return created;
    }
  }

  internal sealed class WorldNpcLookAtRecord {
    public WorldNpcPlacement Target;
    public IReadOnlyList<WorldLookAtLayer> Layers;
  }

  // Who each actor's head is following. Kept beside the placement rather than on it: the placement is shared by
  // every consumer of the type, and only a conversation that says `Set Look At` ever has an entry.
  internal static class WorldNpcLookAt {
    private static readonly ConditionalWeakTable<WorldNpcPlacement, WorldNpcLookAtRecord> records = new ConditionalWeakTable<WorldNpcPlacement, WorldNpcLookAtRecord>();
    private static int any;

    public static void Set(WorldNpcPlacement placement, WorldNpcPlacement target, IReadOnlyList<WorldLookAtLayer> layers) {
      if (placement == null) return;
      records.AddOrUpdate(placement, new WorldNpcLookAtRecord { Target = target, Layers = layers });
      System.Threading.Volatile.Write(ref any, 1);
    }

    public static void Clear(WorldNpcPlacement placement) {
      if (placement != null) records.Remove(placement);
    }

    // The pose loop asks for every skinned NPC every frame; most sessions never name a look-at at all.
    public static bool TryGet(WorldNpcPlacement placement, out WorldNpcLookAtRecord record) {
      record = null;
      if (placement == null || System.Threading.Volatile.Read(ref any) == 0) return false;
      return records.TryGetValue(placement, out record) && record.Target != null && record.Layers != null && record.Layers.Count > 0;
    }
  }

  // The look-at half of a body type's conversation network (port of morpheme.js mphNetworkLookAt and the helpers it
  // is made of: mphNetworkRoot, mphNetworkPlayed, mphNetworkSourceClip, mphControlValue).
  //
  // The network is read, not assumed. Which clips ride over the beat and what angle selects which frame in them is
  // authored per body type in the .mph, in control expressions: `(TargetYawHead - clamp(TargetYawBody, -45, 45))`
  // normalised over -45..45 is one of them. Naming "the yaw clip" from its filename would be right for the humanoid
  // case and wrong for every network whose authors spelled the layers differently.
  internal sealed class WorldLookAtAxis {
    public string ClipName;
    public ViewMPH.MphNode Source;
    public float Weight;
    public string Part;   // "head" or "body"
    public string Axis;   // "yaw" or "pitch"
    public Func<float, float> Fraction;
  }

  internal static class WorldMphLookAt {
    private sealed class Played {
      public ViewMPH.MphNode Node;
      public float Weight;
    }

    // The look-at layers a network lays over the clip it is playing, or null where it has none.
    public static List<WorldLookAtAxis> Parse(ViewMPH.MphNetwork network) {
      if (network == null || network.Nodes.Count == 0) return null;
      Dictionary<uint, ViewMPH.MphNode> byIndex = network.Nodes.GroupBy(x => x.Index).ToDictionary(x => x.Key, x => x.First());
      ViewMPH.MphNode Node(uint index) {
        if (byIndex.TryGetValue(index, out ViewMPH.MphNode exact)) return exact;
        return index < network.Nodes.Count ? network.Nodes[(int)index] : null;
      }

      ViewMPH.MphNode root = Root(network, Node);
      if (root == null) return null;
      var axes = new List<WorldLookAtAxis>();
      foreach (Played entry in PlayedNodes(Node, root.Index, null)) {
        ViewMPH.MphNode node = entry.Node;
        if (node.Type != 109 || node.Flow.Count == 0 || node.Control.Count == 0) continue;
        ViewMPH.MphNode source = SourceClip(Node, node.Flow[0]);
        if (source == null || String.IsNullOrWhiteSpace(source.AnimationName)) continue;

        // What this layer is about comes from the control parameters its expression READS, not from the node's name:
        // two networks that are otherwise identical name theirs Reference_Lookat_Melee_Head2 and Head3.
        var controls = new List<string>();
        void Collect(uint index, HashSet<uint> seen) {
          ViewMPH.MphNode control = Node(index);
          if (control == null || !seen.Add(index)) return;
          if (control.Type == 20) controls.Add(ControlKey(control.Name));
          foreach (uint child in control.Control) Collect(child, seen);
        }
        Collect(node.Control[0], new HashSet<uint>());

        // The head's own parameter is the one the layer is DRIVEN by; the body's is what the expression subtracts.
        string driver = controls.FirstOrDefault(x => x.IndexOf("head", StringComparison.Ordinal) >= 0)
          ?? controls.FirstOrDefault(x => x.IndexOf("body", StringComparison.Ordinal) >= 0);
        if (String.IsNullOrEmpty(driver)) continue;
        uint controlRoot = node.Control[0];
        string capturedDriver = driver;
        axes.Add(new WorldLookAtAxis {
          ClipName = source.AnimationName,
          Source = source,
          Weight = entry.Weight,
          Part = driver.IndexOf("head", StringComparison.Ordinal) >= 0 ? "head" : "body",
          Axis = driver.IndexOf("pitch", StringComparison.Ordinal) >= 0 ? "pitch" : "yaw",
          Fraction = degrees => ControlValue(Node, controlRoot, new Dictionary<string, float> { [capturedDriver] = degrees }, null)
        });
      }
      return axes.Count > 0 ? axes : null;
    }

    // The control parameter's key: the network's own input name, lowercased, without its path or `input_` prefix.
    private static string ControlKey(string name) {
      if (String.IsNullOrWhiteSpace(name)) return String.Empty;
      string[] pieces = name.Split('|');
      string tail = pieces.Length > 0 ? pieces[pieces.Length - 1] : name;
      if (tail.StartsWith("input_", StringComparison.OrdinalIgnoreCase)) tail = tail.Substring(6);
      return tail.Trim().ToLowerInvariant();
    }

    // The node a network is evaluated from: the state machine, or failing that the blend tree, that nothing else
    // reads. Control edges count as reads as well, or the root would go to one of the operators driving the blends.
    private static ViewMPH.MphNode Root(ViewMPH.MphNetwork network, Func<uint, ViewMPH.MphNode> node) {
      var incoming = new Dictionary<uint, int>();
      foreach (ViewMPH.MphNode n in network.Nodes) incoming[n.Index] = 0;
      foreach (ViewMPH.MphNode n in network.Nodes) {
        foreach (uint child in n.Flow) if (node(child) != null) incoming[child] = incoming.TryGetValue(child, out int a) ? a + 1 : 1;
        foreach (uint child in n.Control) if (node(child) != null) incoming[child] = incoming.TryGetValue(child, out int b) ? b + 1 : 1;
      }
      ViewMPH.MphNode root = network.Nodes.FirstOrDefault(x => incoming.TryGetValue(x.Index, out int n) && n == 0 && x.Type == 10);
      if (root != null) return root;
      return network.Nodes.FirstOrDefault(x => incoming.TryGetValue(x.Index, out int n) && n == 0 && x.Type != 20 && x.Type != 111 && x.Type != 112)
        ?? network.Nodes.FirstOrDefault();
    }

    // One control-parameter expression, evaluated with the values supplied by name. A name it does not carry falls
    // back to the parameter's authored default, which is how a network is walked with nothing driving it.
    //
    // The two-input opcodes are Morpheme's own (MR::AttribDataArithmeticOperation). The one-input ones are read off the
    // names the authors gave the nodes: `subtract7` subtracts its constant, `clamp14` bounds a value, and
    // `norm_head_yaw` (45, -45) turns a yaw in degrees into the 0..1 position in a clip whose middle frame is neutral.
    private static float ControlValue(Func<uint, ViewMPH.MphNode> nodeAt, uint index, IReadOnlyDictionary<string, float> values, HashSet<uint> seen) {
      ViewMPH.MphNode node = nodeAt(index);
      if (node == null || (seen != null && seen.Contains(index))) return 0f;
      if (node.Type == 20) {
        return values != null && values.TryGetValue(ControlKey(node.Name), out float supplied) && Single.IsFinite(supplied) ? supplied : node.DefaultValue;
      }
      HashSet<uint> visited = seen ?? new HashSet<uint>();
      visited.Add(index);
      float value = 0f;
      if (node.Type == 111) {
        float a = node.Control.Count > 0 ? ControlValue(nodeAt, node.Control[0], values, visited) : 0f;
        float b = node.Control.Count > 1 ? ControlValue(nodeAt, node.Control[1], values, visited) : 0f;
        switch (node.Operation) {
          case 0: value = a * b; break;
          case 1: value = a + b; break;
          case 2: value = b != 0f ? a / b : 0f; break;
          case 3: value = a - b; break;
          case 4: value = Math.Min(a, b); break;
          case 5: value = Math.Max(a, b); break;
          default: value = a - b; break;
        }
      } else if (node.Type == 112) {
        float input = node.Control.Count > 0 ? ControlValue(nodeAt, node.Control[0], values, visited) : 0f;
        float c0 = node.Constant0, c1 = node.Constant1;
        if (node.Operation == 0) value = input * c0;
        else if (node.Operation == 3) value = input - c0;
        else if (node.Operation == 9) {
          value = c0 == c1 ? 0f : (input - c1) / (c0 - c1);
          value = value < 0f ? 0f : value > 1f ? 1f : value;
        } else if (node.Operation == 10) {
          float low = Math.Min(c0, c1), high = Math.Max(c0, c1);
          value = input < low ? low : input > high ? high : input;
        } else value = input;
      }
      visited.Remove(index);
      return value;
    }

    // Every node the graph actually plays, with the weight it plays at, from `start` down. An ADDITIVE Blend2 plays
    // BOTH of its sources -- source B as the pose and source A as a delta at the blend's weight.
    private static List<Played> PlayedNodes(Func<uint, ViewMPH.MphNode> nodeAt, uint start, IReadOnlyDictionary<string, float> values) {
      var played = new List<Played>();
      void Visit(uint index, float weight, HashSet<uint> seen) {
        ViewMPH.MphNode node = nodeAt(index);
        if (node == null || !(weight > 1e-6f) || seen.Contains(index)) return;
        if (node.Type == 104 || node.Type == 109) { played.Add(new Played { Node = node, Weight = weight }); return; }
        seen.Add(index);
        if (node.Type == 10) {
          ViewMPH.MphStateMachine machine = node.StateMachine;
          if (machine != null && machine.States.Count > 0) {
            ViewMPH.MphState state = machine.DefaultState < machine.States.Count ? machine.States[(int)machine.DefaultState] : machine.States[0];
            Visit(state.Node, weight, seen);
          }
        } else if (node.Type == 401 || node.Type == 402) {
          if (node.To != UInt32.MaxValue) Visit(node.To, weight, seen);
        } else if (node.Type == 101 || node.Type == 107) {
          float blend = Math.Max(0f, Math.Min(1f, ControlValue(nodeAt, node.WeightControl, values, null)));
          uint first = node.Flow.Count > 0 ? node.Flow[0] : UInt32.MaxValue, second = node.Flow.Count > 1 ? node.Flow[1] : UInt32.MaxValue;
          if (node.Additive) {
            Visit(second, weight, seen);
            Visit(first, weight * blend, seen);
          } else {
            Visit(first, weight * (1f - blend), seen);
            Visit(second, weight * blend, seen);
          }
        } else if (node.Weights.Count > 0) {
          float value = ControlValue(nodeAt, node.WeightControl, values, null);
          int best = 0;
          for (int i = 1; i < node.Weights.Count; i++) if (Math.Abs(node.Weights[i] - value) < Math.Abs(node.Weights[best] - value)) best = i;
          if (node.Flow.Count > 0) Visit(node.Flow[Math.Max(0, Math.Min(node.Flow.Count - 1, best))], weight, seen);
        } else if (node.Flow.Count > 0) {
          foreach (uint child in node.Flow) Visit(child, weight / node.Flow.Count, seen);
        }
        seen.Remove(index);
      }
      Visit(start, 1f, new HashSet<uint>());
      return played;
    }

    // The clip one SingleFrame reads its frames out of: the source under it, a plain AnimSource in every conversation
    // network.
    private static ViewMPH.MphNode SourceClip(Func<uint, ViewMPH.MphNode> nodeAt, uint index) {
      Played best = null;
      foreach (Played entry in PlayedNodes(nodeAt, index, null)) {
        if (entry.Node.Type != 104 || String.IsNullOrWhiteSpace(entry.Node.AnimationName)) continue;
        if (best == null || entry.Weight > best.Weight) best = entry;
      }
      return best?.Node;
    }
  }
}
