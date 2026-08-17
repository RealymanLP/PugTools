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
            ResourceUsage.Immutable,
            BindFlags.VertexBuffer,
            CpuAccessFlags.None,
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

          if (piece.matId != -1) {
            _fx.SetDiffuseMap(_model.materials[piece.matId].diffuseSRV);
            _fx.SetGlossMap(_model.materials[piece.matId].glossSRV);
            _fx.SetRotationMap(_model.materials[piece.matId].rotationSRV);
          }

          activeTech.GetPassByIndex(0).Apply(ImmediateContext);

          ImmediateContext.DrawIndexed(
            ((Int32)piece.numPieceFaces) * 3,
            ((Int32)piece.startIndex) * 3,
            0
          );
        }
      }

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

      _cameraPos = new Vector3(
        _globalBoxCenter.X * 2.5F,
        _globalBoxCenter.Y * 2.5F,
        Math.Max(Math.Max(_globalBoxMax.X, _globalBoxMax.Y), _globalBoxMax.Z) * 2.5F
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

      System.Threading.Thread.Sleep(1); // Fix for UI lag. Sleeps the thread for 1 millisecond...
    }
  }
}
