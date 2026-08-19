using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FileFormats;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDXNet;
using SlimDXNet.Camera;
using SlimDXNet.FX;
using SlimDXNet.Vertex;
using Buffer = SlimDX.Direct3D11.Buffer;
using MathF = SlimDXNet.MathF;

namespace PugTools {
  class ViewGR2 : D3DPanelApp {
    private Vector3 _cameraPos;
    private Matrix _cMatrix;
    private Boolean _disposed;
    // private Single _flyingCameraSpeed; // = 15.0f;
    private GR2_Effect _fx;
    private Vector3 _globalBoxCenter;
    private Vector3 _globalBoxMax;
    private Vector3 _globalBoxMin;
    // private readonly List<UInt16> _indexList = new List<UInt16>();
    // private readonly List<GR2_Mesh_Vertex_Index> _indices = new List<GR2_Mesh_Vertex_Index>();
    private Point _lastMousePos;
    private Boolean _makeScreenshot;
    private GR2 _model;
    private JBAAnimation _jbaAnimation;
    private JBARig _jbaRig;
    private Dictionary<String, Matrix> _jbaSkinMatrices =
      new Dictionary<String, Matrix>(StringComparer.OrdinalIgnoreCase);
    private Single _jbaTime;
    private Int32 _jbaFrame = -1;
    private Boolean _jbaPlaying;
    private Boolean _jbaLoop = true;
    private Buffer _jbaBoneBuffer;
    private Int32 _jbaBoneVertexCount;
    private GR2_Material _jbaMaterial;
    private readonly LookAtCamera _camera;
    private Single _cameraZoomSpeed; // = 0.40f;
    private Matrix _pMatrix;
    private List<PosNormalTexTan> _vertices; // = new List<PosNormalTexTan>();

    internal ViewGR2(IntPtr hInstance, Form form, String panelName = "")
      : base(hInstance, panelName) {

      Window = form;
      RenderPanelName = panelName;
      Enable4XMsaa = true;

      ClientHeight = form.Controls.Find(panelName, true).First().Height;
      ClientWidth = form.Controls.Find(panelName, true).First().Width;

      _camera = new LookAtCamera();
      _lastMousePos = new Point();
    }
    private void BuildGeometry() {
      if (_model.numMeshes > 0) {
        foreach (GR2_Mesh mesh in _model.meshes) {
          _vertices = new List<PosNormalTexTan>();

          if (mesh.meshName.Contains("collision")) continue;

          foreach (GR2_Mesh_Vertex vertex in mesh.meshVerts) {
            Vector3 pos = new Vector3(vertex.X, vertex.Y, vertex.Z);
            Vector3 norm = new Vector3(vertex.normX, vertex.normY, vertex.normZ);
            Vector2 texC = new Vector2(vertex.texU, vertex.texV);
            Vector3 tan = new Vector3(vertex.tanX, vertex.tanY, vertex.tanZ);
            _vertices.Add(new PosNormalTexTan(pos, norm, texC, tan));
          }

          BufferDescription vbd = new BufferDescription(
            PosNormalTexTan.Stride * _vertices.Count,
            ResourceUsage.Dynamic,
            BindFlags.VertexBuffer,
            CpuAccessFlags.Write,
            ResourceOptionFlags.None,
            0
          );
          mesh.vertBuffer = new Buffer(
            Device,
            new DataStream(_vertices.ToArray(), false, false),
            vbd
          );
          UInt16[] indexArray = mesh.meshVertIndex.Select(
            GR2_Mesh_Vertex_Index => GR2_Mesh_Vertex_Index.index
          ).ToArray();
          BufferDescription ibd = new BufferDescription(
            sizeof(UInt16) * indexArray.Length,
            ResourceUsage.Immutable,
            BindFlags.IndexBuffer,
            CpuAccessFlags.None,
            ResourceOptionFlags.None,
            0
          );
          mesh.idxBuffer = new Buffer(Device, new DataStream(indexArray, false, false), ibd);
        }
      }
    }
    internal void Clear() {
      StopRender();
      if (_model != null) {
        foreach (var mesh in _model.meshes) {
          Util.ReleaseCom(ref mesh.idxBuffer);
          Util.ReleaseCom(ref mesh.vertBuffer);
        }

        foreach (var mat in _model.materials) {
          Util.ReleaseCom(ref mat.diffuseSRV);
          Util.ReleaseCom(ref mat.rotationSRV);
          Util.ReleaseCom(ref mat.glossSRV);
          Util.ReleaseCom(ref mat.paletteMaskSRV);
          Util.ReleaseCom(ref mat.paletteSRV);
          Util.ReleaseCom(ref mat.complexionSRV);
          Util.ReleaseCom(ref mat.facepaintSRV);
          Util.ReleaseCom(ref mat.ageSRV);
        }
        _model.Dispose();
      }
      Util.ReleaseCom(ref _jbaBoneBuffer);
      _jbaAnimation = null;
      _jbaRig = null;
      _jbaSkinMatrices.Clear();
      _jbaFrame = -1;
      _jbaBoneVertexCount = 0;
      if (_jbaMaterial != null) {
        Util.ReleaseCom(ref _jbaMaterial.diffuseSRV);
        Util.ReleaseCom(ref _jbaMaterial.rotationSRV);
        Util.ReleaseCom(ref _jbaMaterial.glossSRV);
        _jbaMaterial = null;
      }
      if (_vertices != null) _vertices.Clear();
      // _indices.Clear();
      // _indexList.Clear();
    }
    protected override void Dispose(Boolean disposing) {
      if (!_disposed) {
        if (disposing) {
          Window = null;
          if (_model != null) {
            foreach (var mesh in _model.meshes) {
              Util.ReleaseCom(ref mesh.idxBuffer);
              Util.ReleaseCom(ref mesh.vertBuffer);
            }

            foreach (var mat in _model.materials) {
              Util.ReleaseCom(ref mat.diffuseSRV);
              Util.ReleaseCom(ref mat.rotationSRV);
              Util.ReleaseCom(ref mat.glossSRV);
              Util.ReleaseCom(ref mat.paletteMaskSRV);
              Util.ReleaseCom(ref mat.paletteSRV);
              Util.ReleaseCom(ref mat.complexionSRV);
              Util.ReleaseCom(ref mat.facepaintSRV);
              Util.ReleaseCom(ref mat.ageSRV);
            }
            _model.Dispose();
          }

          Util.ReleaseCom(ref _jbaBoneBuffer);
          if (_jbaMaterial != null) {
            Util.ReleaseCom(ref _jbaMaterial.diffuseSRV);
            Util.ReleaseCom(ref _jbaMaterial.rotationSRV);
            Util.ReleaseCom(ref _jbaMaterial.glossSRV);
            _jbaMaterial = null;
          }

          Effects.DestroyAll();
          InputLayouts.DestroyAll();
          RenderStates.DestroyAll();

          _fx.Dispose();
          if (_vertices != null) _vertices.Clear();
          // _indices.Clear();
          // _indexList.Clear();
        }
        _disposed = true;
      }
      base.Dispose(disposing);
    }
    private EffectTechnique ApplyMaterial(
      GR2_Material selectedMaterial,
      EffectTechnique activeTech
    ) {
      if (selectedMaterial == null)
        return activeTech ?? _fx.Generic;

      if (activeTech == null)
        activeTech = _fx.Generic;

      switch (selectedMaterial.derived) {
        case "Eye":
          activeTech = _fx.Eye;
          break;
        case "Garment":
          activeTech = _fx.Garment;
          break;
        case "HairC":
          activeTech = _fx.HairC;
          break;
        case "SkinB":
          activeTech = _fx.SkinB;
          break;
        default:
          activeTech = _fx.Generic;
          break;
      }

      switch (selectedMaterial.alphaMode) {
        case "Test":
          _fx.SetAlphaMode(1);
          break;
        case "Add":
          _fx.SetAlphaMode(2);
          break;
        case "Multiply":
          _fx.SetAlphaMode(3);
          break;
        case "Full":
        case "MultiPassFull":
          _fx.SetAlphaMode(4);
          break;
        default:
          _fx.SetAlphaMode(0);
          break;
      }

      _fx.SetAlphaTestValue(selectedMaterial.alphaTestValue);

      if (selectedMaterial.isTwoSided)
        ImmediateContext.Rasterizer.State = RenderStates.TwoSidedRS;
      else
        ImmediateContext.Rasterizer.State = RenderStates.OneSidedRS;

      // A missing diffuse SRV previously produced a completely black model.
      // The neutral JBA material is preferable to a black silhouette while
      // keeping every other material input intact.
      ShaderResourceView diffuse = selectedMaterial.diffuseSRV;
      if (diffuse == null && _jbaMaterial != null)
        diffuse = _jbaMaterial.diffuseSRV;

      _fx.SetDiffuseMap(diffuse);
      _fx.SetRotationMap(selectedMaterial.rotationSRV);
      _fx.SetGlossMap(selectedMaterial.glossSRV);
      _fx.SetPaletteMap(selectedMaterial.paletteSRV);
      _fx.SetPaletteMaskMap(selectedMaterial.paletteMaskSRV);
      _fx.SetComplexionMap(selectedMaterial.complexionSRV);
      _fx.SetFacepaintMap(selectedMaterial.facepaintSRV);
      _fx.SetAgeMap(selectedMaterial.ageSRV);

      _fx.SetPalette1(selectedMaterial.palette1);
      _fx.SetPalette2(selectedMaterial.palette2);
      _fx.SetPalette1Spec(selectedMaterial.palette1Spec);
      _fx.SetPalette2Spec(selectedMaterial.palette2Spec);
      _fx.SetPalette1MetSpec(selectedMaterial.palette1MetSpec);
      _fx.SetPalette2MetSpec(selectedMaterial.palette2MetSpec);
      _fx.SetFlushTone(selectedMaterial.flushTone);
      _fx.SetFleshBrightness(selectedMaterial.fleshBrightness);

      return activeTech;
    }

    public override void DrawScene() {
      base.DrawScene();

      ImmediateContext.ClearRenderTargetView(RenderTargetView, Color.LightSteelBlue);
      ImmediateContext.ClearDepthStencilView(
        DepthStencilView,
        DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
        1.0F,
        0
      );

      ImmediateContext.InputAssembler.InputLayout = InputLayouts.PosNormalTexTan;
      ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
      ImmediateContext.OutputMerger.BlendState = RenderStates.AlphaToCoverageBS;
      ImmediateContext.Rasterizer.State = RenderStates.OneSidedRS;

      _camera.UpdateViewMatrix();
      _cMatrix = _camera.View;
      _pMatrix = _camera.Proj;

      if (_fx == null) {
        System.Diagnostics.Debug.WriteLine("ViewGR2: DrawScene skipped because GR2 effect is unavailable.");
        SwapChain.Present(1, PresentFlags.None);
        return;
      }

      EffectTechnique activeTech = _fx.Generic;

      // Pose and update the dynamic vertex buffers before drawing the mesh.
      // The previous version rebuilt them from DrawAnimationSkeleton(), after
      // the triangles had already been submitted.
      if (_jbaAnimation != null) {
        Int32 expectedFrame = _jbaAnimation.FPS > 0
    ? Math.Min(
        _jbaAnimation.FrameCount - 1,
        (Int32)Math.Floor(
          _jbaTime * _jbaAnimation.FPS
        )
      )
    : 0;

        if (_jbaPlaying
    || _jbaBoneBuffer == null
    || expectedFrame != _jbaFrame) {

          RebuildAnimationSkeletonBuffer();
        }
      }

      if (Form.ActiveForm != null) {
        if (Util.IsKeyDown(Keys.C)) {
          ImmediateContext.Rasterizer.State = RenderStates.WireframeNoneRS;
        }

        if (Util.IsKeyDown(Keys.PrintScreen)) {
          _makeScreenshot = true;
        }

        if (Util.IsKeyDown(Keys.D1)) {
          activeTech = _fx.filterDiffuseMap;
        }

        if (Util.IsKeyDown(Keys.D2)) {
          activeTech = _fx.filterSpecular;
        }

        if (Util.IsKeyDown(Keys.D3)) {
          activeTech = _fx.filterEmissive;
        }

        // if (Util.IsKeyDown(Keys.D4))
        // {
        //     activeTech = _fx.Light1UberAmbient;
        // }
      }

      Matrix mvMatrix = Matrix.Identity;

      Matrix.Multiply(ref mvMatrix, ref _cMatrix, out mvMatrix);
      Matrix.Multiply(ref mvMatrix, ref _pMatrix, out Matrix wvp);
      Matrix.Invert(ref mvMatrix, out mvMatrix);
      Matrix.Transpose(ref mvMatrix, out mvMatrix);

      _fx.SetWorldMatrix(mvMatrix);
      _fx.SetMvMatrix(wvp);

      if (_model == null) {
        SwapChain.Present(1, PresentFlags.None);
        return;
      }

      foreach (GR2_Mesh mesh in _model.meshes) {
        if (mesh.meshName.Contains("collision")) continue;

        foreach (GR2_Mesh_Piece piece in mesh.meshPieces) {
          ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(
              mesh.vertBuffer,
              PosNormalTexTan.Stride,
              0
            )
          );
          ImmediateContext.InputAssembler.SetIndexBuffer(mesh.idxBuffer, Format.R16_UInt, 0);

          if (piece.matId >= 0 && piece.matId < _model.materials.Count) {
            activeTech = ApplyMaterial(_model.materials[piece.matId], activeTech);
          } else if (_model.materials.Count > 0) {
            activeTech = ApplyMaterial(_model.materials[0], activeTech);
          } else if (_jbaMaterial != null) {
            activeTech = ApplyMaterial(_jbaMaterial, activeTech);
          }

          activeTech.GetPassByIndex(0).Apply(ImmediateContext);

          ImmediateContext.DrawIndexed(
            ((Int32)piece.numPieceFaces) * 3,
            ((Int32)piece.startIndex) * 3,
            0
          );
        }
      }

      //DrawAnimationSkeleton(activeTech);

      SwapChain.Present(1, PresentFlags.None);

      if (_makeScreenshot) {
        MakeScreenshot(ImageFileFormat.Png);
        _makeScreenshot = false;
      }
    }
    public override Boolean Init() {
      if (!base.Init()) return false;

      Effects.InitAll(Device);
      _fx = Effects.GR2_FX;
      if (_fx == null) {
        System.Diagnostics.Debug.WriteLine("ViewGR2: GR2 effect could not be initialized.");
        return false;
      }
      InputLayouts.InitAll(Device);
      RenderStates.InitAll(Device);
      return true;
    }
    internal void LoadModel(GR2 model = null) {
      if (model != null) _model = model;

      _globalBoxMin = new Vector3(
        model.globalBox.minX,
        model.globalBox.minY,
        model.globalBox.minZ
      );
      _globalBoxMax = new Vector3(
        model.globalBox.maxX,
        model.globalBox.maxY,
        model.globalBox.maxZ
      );

      _globalBoxCenter = _globalBoxMin + (_globalBoxMax - _globalBoxMin) / 2;

      Vector3 boxSize = _globalBoxMax - _globalBoxMin;
      Single largestExtent = Math.Max(
        0.01F,
        Math.Max(boxSize.X, Math.Max(boxSize.Y, boxSize.Z))
      );

      _cameraPos = _globalBoxCenter + new Vector3(
        0.0F,
        0.0F,
        largestExtent * 3.6F
      );

      _camera.Reset();
      _camera.Position = _cameraPos;
      _camera.LookAt(_cameraPos, _globalBoxCenter, Vector3.UnitY);

      if (model.numMaterials > 0) {
        foreach (GR2_Material mat in model.materials) {
          mat.ParseMAT(Device);
        }
      }

      BuildGeometry();
    }
    internal void LoadAnimation(JBAAnimation animation, JBARig rig = null) {
      _jbaRig = rig;
      _jbaAnimation = animation;
      _jbaTime = 0.0F;
      _jbaFrame = -1;
      _jbaPlaying = animation != null;
      Util.ReleaseCom(ref _jbaBoneBuffer);
      _jbaBoneVertexCount = 0;

      if (_jbaMaterial == null) {
        try {
          _jbaMaterial = new GR2_Material("all_test_grey_128");
          _jbaMaterial.ParseMAT(Device);
        } catch {
          _jbaMaterial = null;
        }
      }
    }

    internal Boolean AnimationPlaying => _jbaPlaying;
    internal Single AnimationTime => _jbaTime;
    internal Single AnimationLength => _jbaAnimation?.Length ?? 0.0F;
    internal Int32 AnimationFrame => _jbaFrame < 0 ? 0 : _jbaFrame;
    internal Int32 AnimationFrameCount => _jbaAnimation?.FrameCount ?? 1;

    internal void PlayAnimation() {
      if (_jbaAnimation == null) return;
      if (_jbaTime >= _jbaAnimation.Length)
        _jbaTime = 0.0F;
      _jbaPlaying = true;
    }

    internal void PauseAnimation() {
      _jbaPlaying = false;
    }

    internal void StopAnimation() {
      _jbaPlaying = false;
      _jbaTime = 0.0F;
      _jbaFrame = -1;
    }

    internal void SetAnimationLoop(Boolean loop) {
      _jbaLoop = loop;
    }

    private static Vector3 BoneBindPosition(GR2_Bone_Skeleton bone) {
      Matrix bind = bone.root;
      try { bind.Invert(); } catch { }
      return new Vector3(bind.M41, bind.M42, bind.M43);
    }

    private void UpdateSkinnedGeometry() {
      if (_model == null || _jbaSkinMatrices.Count == 0)
        return;

      foreach (GR2_Mesh mesh in _model.meshes) {
        if (mesh.vertBuffer == null
            || mesh.meshVerts == null
            || mesh.meshVerts.Count == 0
            || mesh.meshBones == null
            || mesh.meshBones.Count == 0)
          continue;

        Matrix[] palette = new Matrix[mesh.meshBones.Count];
        Int32 mappedPaletteBones = 0;
        for (Int32 i = 0; i < palette.Length; i++) {
          String boneName = CanonicalAnimationBoneName(
            mesh.meshBones[i].boneName ?? String.Empty
          );

          if (_jbaSkinMatrices.TryGetValue(boneName, out Matrix skin)) {
            palette[i] = skin;
            mappedPaletteBones++;
          } else {
            palette[i] = Matrix.Identity;
          }
        }

        if (_jbaPlaying && _jbaFrame <= 1) {
          System.Diagnostics.Debug.WriteLine(
            "JBA mesh palette: "
            + mappedPaletteBones
            + " / "
            + palette.Length
            + " mesh bones mapped for "
            + mesh.meshName
          );
        }

        PosNormalTexTan[] skinned = new PosNormalTexTan[mesh.meshVerts.Count];

        for (Int32 i = 0; i < mesh.meshVerts.Count; i++) {
          GR2_Mesh_Vertex src = mesh.meshVerts[i];

          Vector3 originalPos = new Vector3(src.X, src.Y, src.Z);
          Vector3 originalNormal = new Vector3(
            (src.normX - 127.5F) / 127.5F,
            (src.normY - 127.5F) / 127.5F,
            (src.normZ - 127.5F) / 127.5F
          );

                    Vector3 originalTangent = new Vector3(
            (src.tanX - 127.5F) / 127.5F,
            (src.tanY - 127.5F) / 127.5F,
            (src.tanZ - 127.5F) / 127.5F
          );

          Vector3 pos = Vector3.Zero;
          Vector3 norm = Vector3.Zero;
          Vector3 tan = Vector3.Zero;
          Single totalWeight = 0.0F;

          Single[] weights = {
            src.boneWeight1, src.boneWeight2, src.boneWeight3, src.boneWeight4
          };
          Single[] indices = {
            src.boneIndex1, src.boneIndex2, src.boneIndex3, src.boneIndex4
          };

          for (Int32 w = 0; w < 4; w++) {
            Single weight = weights[w];
            if (weight <= 0.00001F) continue;

            Int32 boneIndex = (Int32)Math.Round(indices[w] * 255.0F);
            if (boneIndex < 0 || boneIndex >= palette.Length) continue;

            Matrix skin = palette[boneIndex];
            pos += Vector3.TransformCoordinate(originalPos, skin) * weight;
            norm += Vector3.TransformNormal(originalNormal, skin) * weight;
            tan += Vector3.TransformNormal(originalTangent, skin) * weight;
            totalWeight += weight;
          }

          if (totalWeight <= 0.00001F) {
            pos = originalPos;
            norm = originalNormal;
            tan = originalTangent;
          } else if (Math.Abs(totalWeight - 1.0F) > 0.0001F) {
            pos /= totalWeight;
            norm /= totalWeight;
            tan /= totalWeight;
          }

          if (norm.LengthSquared() > 0.000001F) norm.Normalize();
          if (tan.LengthSquared() > 0.000001F) tan.Normalize();

          skinned[i] = new PosNormalTexTan(
            pos,
            norm,
            new Vector2(src.texU, src.texV),
            tan
          );
        }

        try {
          DataBox mapped = ImmediateContext.MapSubresource(
            mesh.vertBuffer,
            MapMode.WriteDiscard,
            SlimDX.Direct3D11.MapFlags.None
          );
          mapped.Data.WriteRange(skinned);
          ImmediateContext.UnmapSubresource(mesh.vertBuffer, 0);
        }
        catch (Exception ex) {
          System.Diagnostics.Debug.WriteLine(
            "JBA CPU skinning update failed: " + ex.Message
          );
        }
      }
    }

    private static Matrix PoseToModelMatrix(
      SlimDX.Quaternion morphemeRotation,
      Vector3 morphemeTranslation
    ) {
      SlimDX.Quaternion q = new SlimDX.Quaternion(
        morphemeRotation.X,
        morphemeRotation.Z,
        -morphemeRotation.Y,
        morphemeRotation.W
      );

      Matrix matrix = Matrix.RotationQuaternion(q);
      matrix.M41 = morphemeTranslation.X * 0.001F;
      matrix.M42 = morphemeTranslation.Z * 0.001F;
      matrix.M43 = -morphemeTranslation.Y * 0.001F;
      matrix.M44 = 1.0F;
      return matrix;
    }

    private static Vector3 MorphemeToModel(Vector3 p) {
      // Jedipedia jbaBoneToModelSpace:
      // Morpheme centimetres -> HeroEngine/GR2 metres and axis swap.
      return new Vector3(
        0.001F * p.X,
        0.001F * p.Z,
        -0.001F * p.Y
      );
    }

    private static String CanonicalAnimationBoneName(String name) {
      if (String.Equals(name, "Bip01", StringComparison.OrdinalIgnoreCase))
        return "GOD";

      return name ?? String.Empty;
    }

    private static Matrix InverseMatrix(Matrix matrix) {
      try {
        matrix.Invert();
        return matrix;
      }
      catch {
        return Matrix.Identity;
      }
    }

    private static SlimDX.Quaternion MorphemeRotationToHero(
      System.Numerics.Quaternion q
    ) {
      return new SlimDX.Quaternion(
        q.X,
        q.Z,
        -q.Y,
        q.W
      );
    }

    private static Vector3 MorphemeTranslationToHero(
      System.Numerics.Vector3 p
    ) {
      return new Vector3(
        p.X * 0.001F,
        p.Z * 0.001F,
        -p.Y * 0.001F
      );
    }

    private static Matrix BuildAnimatedLocal(
      Matrix bindLocal,
      JBATransform sample
    ) {
      // Wenn die JBA für diesen Bone keine Translation enthält,
      // behalten wir die Translation der GR2-Bind-Pose.
      Vector3 translation = new Vector3(
    bindLocal.M41,
    bindLocal.M42,
    bindLocal.M43
  );

      if (sample.HasTranslation
          && Single.IsFinite(sample.Translation.X)
          && Single.IsFinite(sample.Translation.Y)
          && Single.IsFinite(sample.Translation.Z)) {

        translation = MorphemeTranslationToHero(sample.Translation);
      }

      System.Numerics.Quaternion nq = sample.Rotation;

      if (!Single.IsFinite(nq.X)
          || !Single.IsFinite(nq.Y)
          || !Single.IsFinite(nq.Z)
          || !Single.IsFinite(nq.W)
          || nq.LengthSquared() <= 0.000001F) {

        return bindLocal;
      }

      nq = System.Numerics.Quaternion.Normalize(nq);

      SlimDX.Quaternion rotation = MorphemeRotationToHero(nq);

      Matrix local = Matrix.RotationQuaternion(rotation);

      local.M41 = translation.X;
      local.M42 = translation.Y;
      local.M43 = translation.Z;
      local.M44 = 1.0F;

      return local;
    }

    private static Vector3 HeroToMorphemeTranslation(Vector3 p) {
      return new Vector3(
        p.X * 1000.0F,
        -p.Z * 1000.0F,
        p.Y * 1000.0F
      );
    }

    private static SlimDX.Quaternion JbaModelSpaceTurn() {
      // Jedipedia MPH_MODEL_SPACE_ROTATION = quat(0, 0, 1, 0).
      // The clip starts below the exporter root, so every newly-driven root
      // gets this 180 degree Morpheme-Z turn once.
      return new SlimDX.Quaternion(0.0F, 0.0F, 1.0F, 0.0F);
    }

    private void RebuildAnimationSkeletonBuffer() {
      Util.ReleaseCom(ref _jbaBoneBuffer);
      _jbaBoneVertexCount = 0;

      if (_jbaAnimation == null
          || Device == null
          || _model == null) {
        return;
      }

      JBAFrame frame = _jbaAnimation.Sample(_jbaTime);
      JBAFrame referenceFrame = _jbaAnimation.Sample(0.0F);
      _jbaFrame = frame.Frame;

      IList<GR2_Bone_Skeleton> skeleton = _model.skeleton_bones;
      if (skeleton == null || skeleton.Count == 0)
        return;

      Dictionary<String, Int32> channelByName =
        new Dictionary<String, Int32>(
          StringComparer.OrdinalIgnoreCase
        );

      if (_jbaRig != null
          && _jbaRig.Bones != null
          && _jbaRig.AnimToRig != null) {

        for (Int32 channel = 0;
             channel < _jbaRig.AnimToRig.Length;
             channel++) {

          Int32 rigIndex = _jbaRig.AnimToRig[channel];
          if (rigIndex < 0 || rigIndex >= _jbaRig.Bones.Count)
            continue;

          String name = CanonicalAnimationBoneName(
            _jbaRig.Bones[rigIndex].Name
          );

          if (!String.IsNullOrWhiteSpace(name))
            channelByName[name] = channel;
        }
      }

      if (_jbaAnimation.BoneNames != null) {
        Int32 namedCount = Math.Min(
          _jbaAnimation.BoneNames.Count,
          frame.Bones.Count
        );

        for (Int32 channel = 0;
             channel < namedCount;
             channel++) {

          String name = _jbaAnimation.BoneNames[channel];

          if (String.IsNullOrWhiteSpace(name)
              || name.StartsWith(
                   "bone_",
                   StringComparison.OrdinalIgnoreCase))
            continue;

          name = CanonicalAnimationBoneName(name);

          if (!channelByName.ContainsKey(name))
            channelByName[name] = channel;
        }
      }

      Int32 count = skeleton.Count;
      Int32[] skeletonToChannel = new Int32[count];
      Int32[] source = new Int32[count];

      for (Int32 i = 0; i < count; i++) {
        String name = CanonicalAnimationBoneName(
          skeleton[i].boneName
        );

        skeletonToChannel[i] =
          channelByName.TryGetValue(name, out Int32 channel)
            && channel >= 0
            && channel < frame.Bones.Count
              ? channel
              : -1;

        if (skeletonToChannel[i] >= 0) {
          source[i] = i;
        }
        else {
          Int32 parent = skeleton[i].parentBoneIndex;
          source[i] =
            parent >= 0 && parent < i
              ? source[parent]
              : -1;
        }
      }

      Matrix[] referenceWorld = new Matrix[count];
      Matrix[] currentWorld = new Matrix[count];
      Boolean[] referenceValid = new Boolean[count];
      Boolean[] currentValid = new Boolean[count];

      SlimDX.Quaternion rootTurn = JbaModelSpaceTurn();
      Matrix rootTurnMatrix = Matrix.RotationQuaternion(rootTurn);

      /*
       * Build the exact same GR2-skeleton pose twice:
       *   - JBA frame 0  -> reference pose
       *   - current JBA frame -> animated pose
       *
       * CPU skinning then applies ONLY the difference between those poses.
       * This deliberately avoids GR2 bind-matrix convention problems.
       *
       * Consequences:
       *   - a blank/constant clip produces identity matrices and cannot
       *     destroy the model;
       *   - frame 0 always preserves the original GR2 mesh;
       *   - later frames still contain the real JBA motion.
       */
      /*
       * Build hierarchy with MATRICES, not quaternion multiplication.
       *
       * This is important in SlimDX because the render path uses DirectX
       * row-vector matrix composition:
       *
       *     world = local * parentWorld
       *
       * The previous version composed parent/local quaternions separately.
       * That is easy to get backwards and produced the remaining twisted
       * limbs even though the overall animation was already recognizable.
       */
      Matrix rootTurnHero =
        PoseToModelMatrix(
          rootTurn,
          Vector3.Zero
        );

      for (Int32 pass = 0; pass < 2; pass++) {
        JBAFrame poseFrame =
          pass == 0 ? referenceFrame : frame;

        Matrix[] targetWorld =
          pass == 0 ? referenceWorld : currentWorld;

        Boolean[] targetValid =
          pass == 0 ? referenceValid : currentValid;

        for (Int32 i = 0; i < count; i++) {
          Int32 channel = skeletonToChannel[i];

          if (channel < 0 || channel >= poseFrame.Bones.Count) {
            targetValid[i] = false;
            continue;
          }

          GR2_Bone_Skeleton bone = skeleton[i];
          JBATransform sample = poseFrame.Bones[channel];

          System.Numerics.Quaternion nq = sample.Rotation;

          if (!Single.IsFinite(nq.X)
              || !Single.IsFinite(nq.Y)
              || !Single.IsFinite(nq.Z)
              || !Single.IsFinite(nq.W)
              || nq.LengthSquared() <= 0.000001F) {
            nq = System.Numerics.Quaternion.Identity;
          }
          else {
            nq = System.Numerics.Quaternion.Normalize(nq);
          }

          SlimDX.Quaternion heroRotation =
            MorphemeRotationToHero(nq);

          Vector3 heroTranslation;

          if (sample.HasTranslation
              && Single.IsFinite(sample.Translation.X)
              && Single.IsFinite(sample.Translation.Y)
              && Single.IsFinite(sample.Translation.Z)) {

            heroTranslation =
              MorphemeTranslationToHero(
                sample.Translation
              );
          }
          else {
            /*
             * No translation channel means "keep the GR2 bind-local
             * translation", not zero.
             */
            heroTranslation = new Vector3(
              bone.parent.M41,
              bone.parent.M42,
              bone.parent.M43
            );
          }

          Matrix localHero =
            Matrix.RotationQuaternion(heroRotation);

          localHero.M41 = heroTranslation.X;
          localHero.M42 = heroTranslation.Y;
          localHero.M43 = heroTranslation.Z;
          localHero.M44 = 1.0F;

          Int32 parent = bone.parentBoneIndex;

          if (parent >= 0
              && parent < i
              && skeletonToChannel[parent] >= 0
              && targetValid[parent]) {

            Matrix parentWorld = targetWorld[parent];

            Matrix.Multiply(
              ref localHero,
              ref parentWorld,
              out targetWorld[i]
            );
          }
          else {
            /*
             * A driven bone below an undriven parent begins a new authored
             * JBA chain. Apply the omitted model-space half-turn exactly once.
             */
            Matrix.Multiply(
              ref localHero,
              ref rootTurnHero,
              out targetWorld[i]
            );
          }

          targetValid[i] = true;
        }
      }

      _jbaSkinMatrices.Clear();

      Vector3[] debugPos = new Vector3[count];
      List<Int32> debugParents = new List<Int32>(count);

      Int32 drivenCount = 0;

      for (Int32 i = 0; i < count; i++) {
        String boneName = CanonicalAnimationBoneName(
          skeleton[i].boneName
        );

        Int32 parent = skeleton[i].parentBoneIndex;
        debugParents.Add(parent);

        if (referenceValid[i] && currentValid[i]) {
          Matrix inverseReference =
            InverseMatrix(referenceWorld[i]);

          Matrix current = currentWorld[i];

          // Row-vector convention:
          // reference * delta = current
          // => delta = inverse(reference) * current
          Matrix.Multiply(
            ref inverseReference,
            ref current,
            out Matrix delta
          );

          // Very short "blank" clips can contain tiny compression noise.
          // Keep near-identity deltas at identity so they cannot visibly
          // deform the mesh.
          Single deltaError =
            Math.Abs(delta.M11 - 1.0F)
            + Math.Abs(delta.M22 - 1.0F)
            + Math.Abs(delta.M33 - 1.0F)
            + Math.Abs(delta.M12)
            + Math.Abs(delta.M13)
            + Math.Abs(delta.M21)
            + Math.Abs(delta.M23)
            + Math.Abs(delta.M31)
            + Math.Abs(delta.M32)
            + Math.Abs(delta.M41)
            + Math.Abs(delta.M42)
            + Math.Abs(delta.M43);

          if (deltaError < 0.0005F)
            delta = Matrix.Identity;

          _jbaSkinMatrices[boneName] = delta;
          drivenCount++;

          debugPos[i] = new Vector3(
            current.M41,
            current.M42,
            current.M43
          );
        }
        else {
          /*
           * Jedipedia-style undriven helpers inherit the nearest driven
           * ancestor's skin delta. With no driven ancestor they stay identity.
           */
          Int32 from = source[i];

          if (from >= 0) {
            String sourceName = CanonicalAnimationBoneName(
              skeleton[from].boneName
            );

            if (_jbaSkinMatrices.TryGetValue(
                  sourceName,
                  out Matrix inherited)) {
              _jbaSkinMatrices[boneName] = inherited;
            }
            else {
              _jbaSkinMatrices[boneName] = Matrix.Identity;
            }

            debugPos[i] = debugPos[from];
          }
          else {
            _jbaSkinMatrices[boneName] = Matrix.Identity;
            debugPos[i] = Vector3.Zero;
          }
        }
      }

      UpdateSkinnedGeometry();

      if (_jbaPlaying) {
        System.Diagnostics.Debug.WriteLine(
          "JBA reference-delta frame="
          + frame.Frame
          + " driven="
          + drivenCount
          + "/"
          + skeleton.Count
          + " channels="
          + frame.Bones.Count
        );
      }

      List<PosNormalTexTan> lines =
        new List<PosNormalTexTan>();

      Vector3 normal = Vector3.UnitY;
      Vector2 tex = Vector2.Zero;
      Vector3 tan = Vector3.UnitX;

      for (Int32 i = 0; i < debugPos.Length; i++) {
        Int32 parent = debugParents[i];

        if (parent < 0 || parent >= debugPos.Length)
          continue;

        lines.Add(
          new PosNormalTexTan(
            debugPos[parent],
            normal,
            tex,
            tan
          )
        );

        lines.Add(
          new PosNormalTexTan(
            debugPos[i],
            normal,
            tex,
            tan
          )
        );
      }

      if (lines.Count == 0)
        return;

      BufferDescription vbd =
        new BufferDescription(
          PosNormalTexTan.Stride * lines.Count,
          ResourceUsage.Immutable,
          BindFlags.VertexBuffer,
          CpuAccessFlags.None,
          ResourceOptionFlags.None,
          0
        );

      _jbaBoneBuffer =
        new Buffer(
          Device,
          new DataStream(
            lines.ToArray(),
            false,
            false
          ),
          vbd
        );

      _jbaBoneVertexCount = lines.Count;
    }

    private void DrawAnimationSkeleton(EffectTechnique activeTech) {
      if (_jbaAnimation == null || _model == null || activeTech == null) return;

      if (_jbaBoneBuffer == null || _jbaBoneVertexCount <= 0) return;

      ImmediateContext.InputAssembler.InputLayout = InputLayouts.PosNormalTexTan;
      ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
      ImmediateContext.InputAssembler.SetVertexBuffers(
        0,
        new VertexBufferBinding(_jbaBoneBuffer, PosNormalTexTan.Stride, 0)
      );

      if (_jbaMaterial != null) {
        _fx.SetDiffuseMap(_jbaMaterial.diffuseSRV);
        _fx.SetGlossMap(_jbaMaterial.glossSRV);
        _fx.SetRotationMap(_jbaMaterial.rotationSRV);
      }

      Matrix mvMatrix = Matrix.Identity;
      Matrix.Multiply(ref mvMatrix, ref _cMatrix, out mvMatrix);
      Matrix.Multiply(ref mvMatrix, ref _pMatrix, out Matrix wvp);
      Matrix.Invert(ref mvMatrix, out mvMatrix);
      Matrix.Transpose(ref mvMatrix, out mvMatrix);
      _fx.SetWorldMatrix(mvMatrix);
      _fx.SetMvMatrix(wvp);

      activeTech.GetPassByIndex(0).Apply(ImmediateContext);
      ImmediateContext.Draw(_jbaBoneVertexCount, 0);
      ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
    }

    internal void MakeScreenshot(ImageFileFormat format) {
      try {
        String filename = Tools.PrepExtractPath(
          _model.filename
          + '-'
          + DateTime.Now.ToString("yyyyMMddHHmmss")
          + '.'
          + format.ToString().ToLower()
        );

        Texture2DDescription outputDesc = new Texture2DDescription {
          Width = ClientWidth,
          Height = ClientHeight,
          MipLevels = 1,
          ArraySize = 1,
          Format = Format.R8G8B8A8_UNorm,
          SampleDescription = new SampleDescription(1, 0),
          Usage = ResourceUsage.Default,
          BindFlags = BindFlags.None,
          CpuAccessFlags = CpuAccessFlags.None,
        };
        Texture2D outputFile = new Texture2D(Device, outputDesc);
        Texture2D BackBuffer = SlimDX.Direct3D11.Resource.FromSwapChain<Texture2D>(SwapChain, 0);

        ImmediateContext.ResolveSubresource(BackBuffer, 0, outputFile, 0, Format.R8G8B8A8_UNorm);
        Texture2D.ToFile(ImmediateContext, outputFile, format, filename);
        Util.ReleaseCom(ref outputFile);
        ((AssetBrowser)Window).StatusLabel1Text("Screenshot Completed");
      }
      catch (Exception ex) {
        System.Diagnostics.Debug.WriteLine(ex.ToString());
      }
    }
    protected override void OnMouseDown(Object sender, MouseEventArgs mEvnt) {
      _lastMousePos = mEvnt.Location;
      Window.Controls.Find(RenderPanelName, true).First().Capture = true;
    }
    protected override void OnMouseMove(Object sender, MouseEventArgs mEvnt) {
      if (mEvnt.Button == MouseButtons.Left) {
        Single yDelta = MathF.ToRadians(0.4F * (mEvnt.Y - _lastMousePos.Y));
        Single xDelta = -MathF.ToRadians(0.4F * (mEvnt.X - _lastMousePos.X));

        if (Util.IsKeyDown(Keys.LShiftKey)) {
          xDelta = MathF.ToRadians(0.05F * (mEvnt.X - _lastMousePos.X));
          yDelta = MathF.ToRadians(0.05F * (mEvnt.Y - _lastMousePos.Y));

          _camera.Strafe(-xDelta * _camera.Radius);
          _camera.Fly(yDelta * _camera.Radius);

        } else {
          _camera.Pitch(yDelta);
          _camera.Yaw(-xDelta);
        }
      } else if (mEvnt.Button == MouseButtons.Right) {
        Single xDelta = MathF.ToRadians(0.05F * (mEvnt.X - _lastMousePos.X));
        Single yDelta = MathF.ToRadians(0.05F * (mEvnt.Y - _lastMousePos.Y));

        _camera.Strafe(-xDelta * _camera.Radius);
        _camera.Fly(yDelta * _camera.Radius);
      }

      _lastMousePos = mEvnt.Location;
    }
    protected override void OnMouseUp(Object sender, MouseEventArgs mEvnt) {
      Window.Controls.Find(RenderPanelName, true).First().Capture = true;
    }
    protected override void OnMouseWheel(Object sender, MouseEventArgs mEvnt) {
      Double zoom = -mEvnt.Delta * SystemInformation.MouseWheelScrollLines;

      _cameraZoomSpeed = !Util.IsKeyDown(Keys.ShiftKey) ? 0.00025F : 0.000025F;

      while (zoom != 0) {
        _camera.Zoom(zoom < 0 ? -_cameraZoomSpeed : _cameraZoomSpeed);
        zoom = Math.Truncate(zoom * 750) / 1000;
      }
    }
    public override void OnResize() {
      base.OnResize();
      _camera.SetLens(0.25F * MathF.PI, AspectRatio, 0.001F, 1000.0F);
    }
    public override void UpdateScene(Single dt) {
      base.UpdateScene(dt);

      if (Form.ActiveForm != null) {
        if (Util.IsKeyDown(Keys.R)) {
          _camera.Reset();
          _camera.Position = _cameraPos;
          _camera.LookAt(_cameraPos, _globalBoxCenter, Vector3.UnitY);
        }

        if (Util.IsKeyDown(Keys.Oemplus)) {
          // _flyingCameraSpeed = 15.0F;
          _cameraZoomSpeed = 0.20F;
        }

        if (Util.IsKeyDown(Keys.OemMinus)) {
          // _flyingCameraSpeed = 2.5F;
          _cameraZoomSpeed = 0.05F;
        }

        // if (Util.IsKeyDown(Keys.PageUp)) _camera.Zoom(-_flyingCameraSpeed * dt);

        // if (Util.IsKeyDown(Keys.PageDown)) _camera.Zoom(_flyingCameraSpeed * dt);
      }

      if (_jbaPlaying && _jbaAnimation != null && _jbaAnimation.Length > 0) {
        _jbaTime += dt;

        if (_jbaTime >= _jbaAnimation.Length) {
          if (_jbaLoop) {
            _jbaTime %= _jbaAnimation.Length;
          } else {
            _jbaTime = _jbaAnimation.Length;
            _jbaPlaying = false;
          }
        }
      }

      System.Threading.Thread.Sleep(1); // Fix for UI lag. Sleeps the thread for 1 millisecond...
    }
  }
}
