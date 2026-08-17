using System.Runtime.InteropServices;
using SlimDX;

namespace SlimDXNet {
  [StructLayout(LayoutKind.Sequential)]
  public struct DirectionalLight {
    public Color4 Ambient;
    public Color4 Diffuse;
    public Color4 Specular;
    public Vector3 Direction;
    public float Pad;

    private static int stride = Marshal.SizeOf(typeof(DirectionalLight));

    public static System.Int32 Stride { get => stride; set => stride = value; }
  }
  [StructLayout(LayoutKind.Sequential)]
  public struct PointLight {
    public Color4 Ambient;
    public Color4 Diffuse;
    public Color4 Specular;
    public Vector3 Position;
    public Vector3 Attenuation;
    public float Range;
    public float Pad;

    private static int stride = Marshal.SizeOf(typeof(PointLight));

    public static System.Int32 Stride { get => stride; set => stride = value; }
  }
  [StructLayout(LayoutKind.Sequential)]
  public struct SpotLight {
    public Color4 Ambient;
    public Color4 Diffuse;
    public Color4 Specular;
    public Vector3 Position;
    public float Range;
    public Vector3 Direction;
    public float Spot;
    public Vector3 Attenuation;
    public float Pad;

    private static int stride = Marshal.SizeOf(typeof(SpotLight));

    public static System.Int32 Stride { get => stride; set => stride = value; }
  }
  [StructLayout(LayoutKind.Sequential)]
  public struct Material {
    public Color4 Ambient;
    public Color4 Diffuse;
    public Color4 Specular;
    public Color4 Reflect;
    private static int stride = Marshal.SizeOf(typeof(Material));

    public static System.Int32 Stride { get => stride; set => stride = value; }
  }
}
