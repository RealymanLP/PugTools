using System;
using SlimDX.Direct3D11;
using SlimDXNet.FX;
// using System.Diagnostics;

namespace SlimDXNet {
  public static class InputLayouts {
    public static void InitAll(Device device) {
      /*
      var bl1 = Effects.BasicFX;
      if (bl1 != null) {
          try {
              var passDesc = bl1.Light1Tech.GetPassByIndex(0).Description;
              if (passDesc.Signature != null) PosNormal = new InputLayout(device, passDesc.Signature, InputLayoutDescriptions.PosNormal);
          } catch (Exception ex) {
              Debug.WriteLine(ex.Message );
              PosNormal = null;
          }
          try {
              var passDesc = bl1.Light1Tech.GetPassByIndex(0).Description;
              Basic32 = new InputLayout(device, passDesc.Signature, InputLayoutDescriptions.Basic32);
          } catch (Exception ex) {
              Debug.WriteLine(ex.Message );
              Basic32 = null;
          }
      }
      try {
          var ibl1 = Effects.InstancedBasicFX;
          if (ibl1 != null) {
              var shaderSignature = ibl1.Light1Tech.GetPassByIndex(0).Description.Signature;
              InstancedBasic32 = new InputLayout(device, shaderSignature, InputLayoutDescriptions.InstancedBasic32);
          }
      } catch (Exception ex) {
          Debug.WriteLine(dex.Message + ex.StackTrace);
          InstancedBasic32 = null;
      }
      try {
          var tsl3 = Effects.TreeSpriteFX;
          if (tsl3 != null) {
              var passDesc = tsl3.Light3Tech.GetPassByIndex(0).Description;
              TreePointSprite = new InputLayout(device, passDesc.Signature, InputLayoutDescriptions.TreePointSprite);
          }
      } catch (Exception ex) {
          Debug.WriteLine(ex.Message + ex.StackTrace);
          TreePointSprite = null;
      }
      try {
          var skyTech = Effects.SkyFX;
          if (skyTech != null) {
              var passDesc = skyTech.SkyTech.GetPassByIndex(0).Description;
              Pos = new InputLayout(device, passDesc.Signature, InputLayoutDescriptions.Pos);
          }
      } catch (Exception ex) {
          Debug.WriteLine(ex.Message + ex.StackTrace);
          Pos = null;
      }
      try {
          var tech = Effects.NormalMapFX;
          if (tech != null) {
              var passDesc = tech.Light1Tech.GetPassByIndex(0).Description;
              PosNormalTexTan = new InputLayout(device, passDesc.Signature, InputLayoutDescriptions.PosNormalTexTan);
          }
      } catch (Exception ex) {
          Debug.WriteLine(ex.Message + ex.StackTrace);
          PosNormalTexTan = null;
      }*/
      try {
        var tech = Effects.GR2_FX;
        if (tech != null) {
          var passDesc = tech.Generic.GetPassByIndex(0).Description;
          PosNormalTexTan = new InputLayout(device, passDesc.Signature, InputLayoutDescriptions.PosNormalTexTan);
        }
      }
      catch (Exception ex) {
        System.Diagnostics.Debug.WriteLine(ex.Message + ex.StackTrace);
        PosNormalTexTan = null;
      }
      /*
      try {
          var tech = Effects.TerrainFX;
          if (tech != null) {
              var passDesc = tech.Light1Tech.GetPassByIndex(0).Description;
              TerrainCP = new InputLayout(device, passDesc.Signature, InputLayoutDescriptions.TerrainCP);
          }
      } catch (Exception ex) {
          Debug.WriteLine(ex.Message);
          TerrainCP = null;
      }
      try {
          var tech = Effects.ColorFX;
          if (tech != null) {
              var passDesc = tech.ColorTech.GetPassByIndex(0).Description;
              PosColor = new InputLayout(device, passDesc.Signature, InputLayoutDescriptions.PosColor);
          }
      } catch (Exception ex) {
          Debug.WriteLine(ex.Message);
          PosColor = null;
      }
      try {
          var tech = Effects.BasicFX;
          if (tech != null) {
              var passDesc = tech.Light1SkinnedTech.GetPassByIndex(0).Description;
              PosNormalTexTanSkinned = new InputLayout(device, passDesc.Signature, InputLayoutDescriptions.PosNormalTexTanSkinned);
          } else if ((tech = Effects.NormalMapFX) != null) {
              var passDesc = tech.Light1SkinnedTech.GetPassByIndex(0).Description;
              PosNormalTexTanSkinned = new InputLayout(device, passDesc.Signature, InputLayoutDescriptions.PosNormalTexTanSkinned);
          }
      } catch (Exception ex) {
          Debug.WriteLine(ex.Message);
          PosNormalTexTanSkinned = null;
      }
      try {
          var tech = Effects.InstancedNormalMapFX;
          if (tech != null) {
              var passDesc = tech.Light1Tech.GetPassByIndex(0).Description;
              InstancedPosNormalTexTan = new InputLayout(device, passDesc.Signature, InputLayoutDescriptions.InstancedPosNormalTexTan);
          }
      } catch (Exception ex) {
          Debug.WriteLine(ex.Message + ex.StackTrace);
          InstancedPosNormalTexTan = null;
      }
      try {
          var tech = Effects.FireFX;
          if (tech != null) {
              var passDesc = tech.StreamOutTech.GetPassByIndex(0).Description;
              Particle = new InputLayout(device, passDesc.Signature, InputLayoutDescriptions.Particle);
          }
      } catch (Exception ex) {
          Debug.WriteLine(ex.Message + ex.StackTrace);
          Particle = null;
      }
       */

    }
    public static void DestroyAll() {
      Util.ReleaseCom(ref pos);
      Util.ReleaseCom(ref posNormal);
      Util.ReleaseCom(ref basic32);
      Util.ReleaseCom(ref treePointSprite);
      Util.ReleaseCom(ref instancedBasic32);
      Util.ReleaseCom(ref posNormalTexTan);
      Util.ReleaseCom(ref terrainCP);
      Util.ReleaseCom(ref posColor);
      Util.ReleaseCom(ref posNormalTexTanSkinned);
      Util.ReleaseCom(ref instancedPosNormalTexTan);
      Util.ReleaseCom(ref particle);
    }

    private static InputLayout posNormal;
    private static InputLayout basic32;
    private static InputLayout treePointSprite;
    private static InputLayout instancedBasic32;
    private static InputLayout pos;
    private static InputLayout posNormalTexTan;
    private static InputLayout terrainCP;
    private static InputLayout posColor;
    private static InputLayout posNormalTexTanSkinned;
    private static InputLayout instancedPosNormalTexTan;
    private static InputLayout particle;

    public static InputLayout PosNormal { get => posNormal; set => posNormal = value; }
    public static InputLayout Basic32 { get => basic32; set => basic32 = value; }
    public static InputLayout TreePointSprite { get => treePointSprite; set => treePointSprite = value; }
    public static InputLayout InstancedBasic32 { get => instancedBasic32; set => instancedBasic32 = value; }
    public static InputLayout Pos { get => pos; set => pos = value; }
    public static InputLayout PosNormalTexTan { get => posNormalTexTan; set => posNormalTexTan = value; }
    public static InputLayout TerrainCP { get => terrainCP; set => terrainCP = value; }
    public static InputLayout PosColor { get => posColor; set => posColor = value; }
    public static InputLayout PosNormalTexTanSkinned { get => posNormalTexTanSkinned; set => posNormalTexTanSkinned = value; }
    public static InputLayout InstancedPosNormalTexTan { get => instancedPosNormalTexTan; set => instancedPosNormalTexTan = value; }
    public static InputLayout Particle { get => particle; set => particle = value; }
  }
}
