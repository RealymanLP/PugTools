using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TorArchive;

namespace FileFormats {
  internal sealed class JBAAppearancePart {
    internal String Model;
    internal String[] Materials;

    internal JBAAppearancePart(String model, params String[] materials) {
      Model = model;
      Materials = materials ?? Array.Empty<String>();
    }
  }

  internal static class JBAAppearance {
    internal static String BodyTypeFromAnimationDirectory(String directory) {
      if (String.IsNullOrWhiteSpace(directory))
        return String.Empty;

      String path = directory.Replace('\\', '/').TrimEnd('/');
      Int32 slash = path.LastIndexOf('/');
      return (slash >= 0 ? path.Substring(slash + 1) : path).ToLowerInvariant();
    }

    internal static List<JBAAppearancePart> GetDefault(String bodyType) {
      bodyType = (bodyType ?? String.Empty).ToLowerInvariant();

      if (bodyType is "bfanew" or "bfbnew" or "bfnnew" or "bfsnew"
          or "bmanew" or "bmfnew" or "bmnnew" or "bmsnew") {
        String bt = bodyType.Substring(0, bodyType.Length - 3);
        String gender = bt.StartsWith("bf", StringComparison.OrdinalIgnoreCase) ? "f" : "m";

        var parts = new List<JBAAppearancePart> {
          new JBAAppearancePart($"/resources/art/dynamic/boot/model/boot_tall_{bt}_archetype.gr2",
            "boot_tall_heavy_bh_a02c01_u"),
          new JBAAppearancePart($"/resources/art/dynamic/bracer/model/bracer_short_{bt}_archetype.gr2",
            "bracer_short_heavy_bh_a02c01_u_v01"),
          new JBAAppearancePart($"/resources/art/dynamic/chest/model/chest_tight_{bt}_archetype.gr2",
            $"chest_tight_light_ge_a33hacker01_{gender}"),
          new JBAAppearancePart($"/resources/art/dynamic/hand/model/hand_gauntlet_{bt}_archetype.gr2",
            "hand_gauntlet_heavy_bh_a02c01_u"),
          new JBAAppearancePart($"/resources/art/dynamic/leg/model/leg_pant_{bt}_archetype.gr2",
            $"leg_pant_light_ge_a33hacker01_{gender}_v02"),
          new JBAAppearancePart($"/resources/art/dynamic/waist/model/waist_belt_{bt}_archetype.gr2",
            "waist_belt_heavy_bh_a02c01_u")
        };

        if (gender == "f") {
          parts.Add(new JBAAppearancePart(
            $"/resources/art/dynamic/head/model/head_human_{bt}_caucasian_a01.gr2",
            "head_human_caucasian_a01c01_f", "eye_human_non_a01_c01"));
          parts.Add(new JBAAppearancePart(
            $"/resources/art/dynamic/hair/model/hair_human_{bt}_non_a01.gr2",
            "hair_human_non_a01_v01_f"));
        } else {
          parts.Add(new JBAAppearancePart(
            $"/resources/art/dynamic/head/model/head_human_{bt}_caucasian_a01.gr2",
            $"head_human_{bt}_caucasian_a01c01", "eye_human_non_a01_c01"));

          if (bt != "bms") {
            String hair = bt == "bmn" ? "a05" : "a04";
            parts.Add(new JBAAppearancePart(
              $"/resources/art/dynamic/hair/model/hair_human_{bt}_non_{hair}.gr2",
              $"hair_human_non_{hair}_v01_m"));
          }
        }

        return parts;
      }

      return bodyType switch {
        "acklay" => One("/resources/art/dynamic/creature/model/acklay_acklay_a01.gr2", "acklay_acklay_a01_v01"),
        "bantha" => One("/resources/art/dynamic/creature/model/bantha_bantha_a01.gr2", "bantha_bantha_a01_v01"),
        "cat" => One("/resources/art/dynamic/creature/model/cat_nexu_a01.gr2", "cat_nexu_a01_v01"),
        "dewback" => One("/resources/art/dynamic/creature/model/dewback_dewback_a01.gr2"),
        "dog" => One("/resources/art/dynamic/creature/model/dog_akk_a01.gr2", "dog_akk_a01_v01"),
        "gree" => One("/resources/art/dynamic/creature/model/gree_gree_a01.gr2", "gree_gree_a01_v01", "gree_gree_eye_a01_v01"),
        "manta" => One("/resources/art/dynamic/creature/model/manta_thranta_a01.gr2", "manta_thranta_a01_v01"),
        "rancor" => One("/resources/art/dynamic/creature/model/rancor_rancor_a01.gr2", "rancor_rancor_a01_v01"),
        "tauntaun" => One("/resources/art/dynamic/creature/model/tauntaun_tauntaun_a01.gr2", "tauntaun_tauntaun_a01_v01"),
        "veractyl" => One("/resources/art/dynamic/creature/model/lizard_veractyl_a01.gr2"),
        "wampa" => One("/resources/art/dynamic/creature/model/wampa_wampa_a01.gr2", "wampa_wampa_a01_v01"),
        "assassin" => One("/resources/art/dynamic/creature/model/assassin_rogue_a01.gr2", "assassin_rogue_a01_v01"),
        "astromech" => One("/resources/art/dynamic/creature/model/astromech_generic_a01.gr2", "astromech_generic_a01_v01"),
        "battle" => One("/resources/art/dynamic/creature/model/battledroid_combat_a01.gr2", "battledroid_combat_a01_v01"),
        "protocol" => One("/resources/art/dynamic/creature/model/protocol_courier_a01.gr2", "protocol_courier_a01_v01"),
        "walker" => One("/resources/art/dynamic/creature/model/walker_atst_a01.gr2", "walker_atst_a01_v01"),
        "hutt" => One("/resources/art/dynamic/creature/model/hutt_hutt_a01.gr2", "hutt_hutt_a01_v01", "eye_hutt_hutt_a01_c01"),
        "ithorian" => One("/resources/art/dynamic/creature/model/ithorian_ithorian_a01.gr2", "ithorian_ithorian_a01_v01", "eye_ithorian_non_a01_v01"),
        _ => new List<JBAAppearancePart>()
      };
    }

    private static List<JBAAppearancePart> One(String model, params String[] mats) =>
      new List<JBAAppearancePart> { new JBAAppearancePart(model, mats) };

    internal static GR2 LoadComposite(
      Assets assets,
      GR2 skeleton,
      IEnumerable<JBAAppearancePart> parts,
      String name
    ) {
      if (assets == null || skeleton == null)
        return skeleton;

      GR2 composite = new GR2 {
        filename = name ?? "jba_preview",
        transformMatrix = SlimDX.Matrix.Identity,
        scaleMatrix = SlimDX.Matrix.Identity,
        rotationMatrix = SlimDX.Matrix.Identity,
        positionMatrix = SlimDX.Matrix.Identity,
        parentPosMatrix = SlimDX.Matrix.Identity,
        parentRotMatrix = SlimDX.Matrix.Identity
      };

      composite.skeleton_bones.AddRange(skeleton.skeleton_bones);
      composite.numBones = skeleton.numBones;
      composite.globalBox = skeleton.globalBox;

      Boolean gotMesh = false;

      foreach (JBAAppearancePart part in parts ?? Enumerable.Empty<JBAAppearancePart>()) {
        TorArchive.File file = assets.FindFile(part.Model);
        if (file == null) continue;

        GR2 model;
        using (Stream stream = file.OpenCopyInMemory())
        using (BinaryReader br = new BinaryReader(stream))
          model = new GR2(br, Path.GetFileName(part.Model));

        if (model.meshes.Count == 0) {
          model.Dispose();
          continue;
        }

        Boolean hasUsefulEmbeddedMaterials =
          model.materials != null
          && model.materials.Count > 0
          && model.materials.Any(x =>
            x != null
            && !String.IsNullOrWhiteSpace(x.materialName)
            && !String.Equals(
              x.materialName,
              "default",
              StringComparison.OrdinalIgnoreCase
            )
          );

        if (!hasUsefulEmbeddedMaterials && part.Materials.Length > 0) {
          model.materials.Clear();
          foreach (String material in part.Materials)
            model.materials.Add(new GR2_Material(material));
        }

        Int32 materialBase = composite.materials.Count;
        composite.materials.AddRange(model.materials);

        foreach (GR2_Mesh mesh in model.meshes) {
          foreach (GR2_Mesh_Piece piece in mesh.meshPieces) {
            if (piece.matId >= 0 && model.materials.Count > 0) {
              Int32 local = Math.Min(piece.matId, model.materials.Count - 1);
              piece.matId = materialBase + local;
            } else if (piece.matId >= 0) {
              piece.matId = -1;
            }
          }

          composite.meshes.Add(mesh);
        }

        composite.numMeshes += model.numMeshes;
        composite.attachedModels.Add(model);
        gotMesh = true;
      }

      if (!gotMesh) {
        composite.Dispose();
        return skeleton;
      }

      // LoadModel() only parses material files when numMaterials is non-zero.
      // The composite is assembled manually, so keep the legacy count fields in
      // sync with the lists we just populated. Without this the GR2 renders as
      // an untextured/black silhouette even though the material objects exist.
      composite.numMaterials = (ushort)composite.materials.Count;
      composite.numMeshes = (ushort)composite.meshes.Count;

      return composite;
    }
  }
}
