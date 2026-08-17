namespace SlimDXNet.FX {
  using System;

  using SlimDX.Direct3D11;

  public static class Effects {
    public static void InitAll(Device device) {
      try {
        _GR2_FX = new GR2_Effect(device, "Shaders\\GR2.fx");
      }
      catch (Exception ex) {
        System.Diagnostics.Debug.WriteLine(ex.Message);
      }

    }
    public static void DestroyAll() {
      Util.ReleaseCom(ref _GR2_FX);
    }

    private static GR2_Effect _GR2_FX;

    public static GR2_Effect GR2_FX { get => _GR2_FX; }
  }
}
