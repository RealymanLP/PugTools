using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using GomLib;
using SlimDX;
using SlimDX.Direct3D11;
using TorArchive;
using File = TorArchive.File;
using ShaderResourceView = SlimDX.Direct3D11.ShaderResourceView;

namespace FileFormats {
  public class GR2_Material {
    public String ageDDS;
    public ShaderResourceView ageSRV;
    public Boolean alphaClip;
    public String alphaMode;
    public Single alphaTestValue;
    public String complexionDDS;
    public ShaderResourceView complexionSRV;
    public String derived;
    public String diffuseDDS;
    public ShaderResourceView diffuseSRV;
    public String facepaintDDS;
    public ShaderResourceView facepaintSRV;
    public Single fleshBrightness;
    public Vector4 flushTone;
    // private Vector4 glassParams;
    public String glossDDS;
    public ShaderResourceView glossSRV;
    public Boolean isTwoSided;
    public String materialName;
    public Vector4 palette1;
    public Vector4 palette1MetSpec;
    public Vector4 palette1Spec;
    public String palette1XML;
    public Vector4 palette2;
    public Vector4 palette2MetSpec;
    public Vector4 palette2Spec;
    public String palette2XML;
    public String paletteDDS;
    public String paletteMaskDDS;
    public ShaderResourceView paletteMaskSRV;
    public ShaderResourceView paletteSRV;
    public Boolean parsed;
    // private String polytype;
    public String rotationDDS;
    public ShaderResourceView rotationSRV;
    public Boolean useEmissive;
    // private Boolean useReflection;
    // private String visibility;

    public GR2_Material(String materialName) {
      this.materialName = materialName;
    }

    public GR2_Material(BinaryReader br, Boolean is64Bit) {
      UInt64 offsetMaterialName = is64Bit ? br.ReadUInt64() : br.ReadUInt32();
      materialName = FileHelpers.ReadString(br, offsetMaterialName);
    }

    private static void FileToShaderResource(ref Device device,
                                             File file,
                                             ref ShaderResourceView srv) {

      if (file != null && device != null) {
        /*
        using Stream textureStream = file.OpenCopyInMemory();
        using MemoryStream textureMS = new MemoryStream();
        textureStream.CopyTo(textureMS);
        srv = ShaderResourceView.FromMemory(device, textureMS.ToArray());
        */

        using Stream textureStream = file.OpenCopyInMemory();
        srv = ShaderResourceView.FromStream(device, textureStream, (Int32)textureStream.Length);

      } else {
        return;
      }
    }

    private static void FileToShaderResource(ref Device device,
                                             String value,
                                             ref ShaderResourceView srv) {

      Assets curAssets = AssetHandler.Instance.GetCurrentAssets();
      using File file = curAssets.FindFile(value);

      FileToShaderResource(ref device, file, ref srv);
    }

    public void ParseMAT(Device device, List<GR2_Material> parentMaterials = null) {
      String materialFileName = "/resources/art/shaders/materials/" + materialName + ".mat";
      Assets currentAssets = AssetHandler.Instance.GetCurrentAssets();

      try {
        if (palette1XML != null) {
          File palette1File = currentAssets.FindFile(palette1XML);

          if (palette1File != null) {
            using Stream palette1Stream = palette1File.OpenCopyInMemory();
            XmlDocument p1XmlDoc = new XmlDocument();
            p1XmlDoc.Load(palette1Stream);

            Vector4 metSpec = FileHelpers.StringToVec4(
              p1XmlDoc.DocumentElement.SelectSingleNode("/Palette/Metallicspecular").InnerText
            );
            Vector4 spec = FileHelpers.StringToVec4(
              p1XmlDoc.DocumentElement.SelectSingleNode("/Palette/Specular").InnerText
            );
            Single hue = Single.Parse(
              p1XmlDoc.DocumentElement.SelectSingleNode("/Palette/Hue").InnerText
            );
            Single bright = Single.Parse(
              p1XmlDoc.DocumentElement.SelectSingleNode("/Palette/Brightness").InnerText
            );
            Single saturation = Single.Parse(
              p1XmlDoc.DocumentElement.SelectSingleNode("/Palette/Saturation").InnerText
            );
            Single contrast = Single.Parse(
              p1XmlDoc.DocumentElement.SelectSingleNode("/Palette/Contrast").InnerText
            );

            palette1 = new Vector4(hue, saturation, bright, contrast);
            palette1MetSpec = metSpec;
            palette1Spec = spec;

            palette1File.Dispose();
          }
        }

        if (palette2XML != null) {
          File palette2File = currentAssets.FindFile(palette2XML);
          if (palette2File != null) {
            using Stream palette2Stream = palette2File.OpenCopyInMemory();
            XmlDocument p2XmlDoc = new XmlDocument();
            p2XmlDoc.Load(palette2Stream);

            Vector4 metSpec = FileHelpers.StringToVec4(
              p2XmlDoc.DocumentElement.SelectSingleNode("/Palette/Metallicspecular").InnerText
            );
            Vector4 spec = FileHelpers.StringToVec4(
              p2XmlDoc.DocumentElement.SelectSingleNode("/Palette/Specular").InnerText
            );
            Single hue = Single.Parse(
              p2XmlDoc.DocumentElement.SelectSingleNode("/Palette/Hue").InnerText
            );
            Single bright = Single.Parse(
              p2XmlDoc.DocumentElement.SelectSingleNode("/Palette/Brightness").InnerText
            );
            Single saturation = Single.Parse(
              p2XmlDoc.DocumentElement.SelectSingleNode("/Palette/Saturation").InnerText
            );
            Single contrast = Single.Parse(
              p2XmlDoc.DocumentElement.SelectSingleNode("/Palette/Contrast").InnerText
            );

            palette2 = new Vector4(hue, saturation, bright, contrast);
            palette2MetSpec = metSpec;
            palette2Spec = spec;

            palette2File.Dispose();
          }
        }
      }
      catch (Exception) { }

      File materialFile = currentAssets.FindFile(materialFileName);

      if (materialFile == null) {
        materialFile = currentAssets.FindFile(
          materialFileName.Replace("_m_", "_u_").Replace("_f_", "_u_")
        );

        // if (materialFile != null)
        //     materialFileName = materialFileName.Replace("_m_", "_u_").Replace("_f_", "_u_");
        // materialFile = currentAssets.FindFile(materialFileName.Replace("_u_", "_f_"));
        // if (materialFile != null)
        //     materialFileName = materialFileName.Replace("_u_", "_f_");
        // materialFile = currentAssets.FindFile(materialFileName.Replace("_u_", "_m_"));
        // if (materialFile != null)
        //     _ = materialFileName.Replace("_u_", "_m_");
      }

      if (materialFile != null) {
        using Stream materialStream = materialFile.OpenCopyInMemory();
        XmlDocument material = new XmlDocument();
        material.Load(materialStream);

        derived = material.SelectSingleNode("/Material/Derived").InnerText;
        // polytype = material.SelectSingleNode("/Material/PolyType").InnerText;
        // visibility = material.SelectSingleNode("/Material/Visibility").InnerText;

        String alphaMode = material.SelectSingleNode("/Material/AlphaMode").InnerText;
        String alphaTestValue = material.SelectSingleNode("/Material/AlphaTestValue").InnerText;

        if (alphaMode != "None") {
          alphaClip = true;
          this.alphaMode = alphaMode;
        }

        this.alphaTestValue = Single.Parse(alphaTestValue);
        isTwoSided = material.SelectSingleNode("/Material/IsTwoSided").InnerText == "True";
        XmlNodeList nodeList = material.SelectNodes("/Material/input");

        foreach (XmlNode node in nodeList) {
          String semantic = node["semantic"].InnerText;
          String value = node["value"].InnerText.Replace("\\", "/");

          if (semantic == "DiffuseMap") {
            diffuseDDS = ("/resources/" + value.Replace("\\", "/") + ".dds").Replace("//", "/");
            File diffuseFile = currentAssets.FindFile(diffuseDDS);

            if (diffuseFile != null && device != null) {
              FileToShaderResource(ref device, diffuseFile, ref diffuseSRV);
            } else {
              diffuseDDS = "/resources/art/defaultassets/blue.dds";
              FileToShaderResource(ref device, diffuseDDS, ref diffuseSRV);
            }
          } else if (semantic == "RotationMap1") {
            rotationDDS = ("/resources/" + value + ".dds").Replace("//", "/");
            FileToShaderResource(ref device, rotationDDS, ref rotationSRV);
          } else if (semantic == "GlossMap") {
            glossDDS = ("/resources/" + value + ".dds").Replace("//", "/");
            FileToShaderResource(ref device, glossDDS, ref glossSRV);
          } else if (semantic == "UsesEmissive") {
            useEmissive = Convert.ToBoolean(value);
          }

          if (derived == "Garment" || derived == "GarmentScrolling" || derived == "SkinB"
              || derived == "HairC" || derived == "Eye") {

            if (semantic == "PaletteMap") {
              paletteDDS = ("/resources/" + value + ".dds").Replace("//", "/");
              FileToShaderResource(ref device, paletteDDS, ref paletteSRV);
            } else if (semantic == "PaletteMaskMap") {
              paletteMaskDDS = ("/resources/" + value + ".dds").Replace("//", "/");
              FileToShaderResource(ref device, paletteMaskDDS, ref paletteMaskSRV);
            } else if (semantic == "palette1") {
              if (palette1 == new Vector4())
                palette1 = FileHelpers.StringToVec4(value);
            } else if (semantic == "palette2") {
              if (palette2 == new Vector4())
                palette2 = FileHelpers.StringToVec4(value);
            } else if (semantic == "palette1Specular") {
              palette1Spec = FileHelpers.StringToVec4(value);
            } else if (semantic == "palette2Specular") {
              palette2Spec = FileHelpers.StringToVec4(value);
            } else if (semantic == "palette1MetallicSpecular") {
              palette1MetSpec = FileHelpers.StringToVec4(value);
            } else if (semantic == "palette2MetallicSpecular") {
              palette2MetSpec = FileHelpers.StringToVec4(value);
            }
          }

          if (derived == "SkinB") {
            if (semantic == "ComplexionMap") {
              complexionDDS = ("/resources/" + value + ".dds").Replace("//", "/");
              FileToShaderResource(ref device, complexionDDS, ref complexionSRV);
            } else if (semantic == "FacepaintMap") {
              facepaintDDS = ("/resources/" + value + ".dds").Replace("//", "/");
              FileToShaderResource(ref device, facepaintDDS, ref facepaintSRV);
            } else if (semantic == "AgeMap") {
              ageDDS = ("/resources/" + value + ".dds").Replace("//", "/");
              FileToShaderResource(ref device, ageDDS, ref ageSRV);
            } else if (semantic == "FlushTone") {
              if (flushTone == new Vector4())
                flushTone = FileHelpers.StringToVec4(value);
            } else if (semantic == "FleshBrightness") {
              if (fleshBrightness == 0)
                fleshBrightness = Single.Parse(value);
            }
          }

          /*
          if (derived == "Glass") {
            if (semantic == "UsesReflection") {
              useReflection = Convert.ToBoolean(value);
            } else if (semantic == "GlassParams") {
              glassParams = FileHelpers.StringToVec4(value);
            }
          }
          */
        }

        if (palette1.X == 0 && palette1.Y == 0.5 && palette1.Z == 0 && palette1.W == 1
            && parentMaterials != null) {

          if (parentMaterials[0] != null) {
            palette1 = parentMaterials[0].palette1;
            palette1MetSpec = parentMaterials[0].palette1MetSpec;
            palette1Spec = parentMaterials[0].palette1Spec;
            palette2 = parentMaterials[0].palette2;
            palette2MetSpec = parentMaterials[0].palette2MetSpec;
            palette2Spec = parentMaterials[0].palette2Spec;
          }
        }
      } else {
        diffuseDDS = "/resources/art/defaultassets/blue.dds";
        FileToShaderResource(ref device, diffuseDDS, ref diffuseSRV);
      }

      parsed = true;
    }

    public void SetComplexionMap(Device device, String complexionPath) {
      complexionDDS = "/resources" + complexionPath;
      FileToShaderResource(ref device, complexionDDS, ref complexionSRV);
    }

    public void SetDynamicColor(GomObject dynObj, Int32 paletteNum = 0) {
      Single hue = dynObj.Data.ValueOrDefault<Single>("appPaletteHue", 0);
      Single saturation = dynObj.Data.ValueOrDefault("appPaletteSaturation", 0.5F);
      Single brightness = dynObj.Data.ValueOrDefault<Single>("appPaletteBrightness", 0);
      Single contrast = dynObj.Data.ValueOrDefault("appPaletteContrast", 1.0F);

      Vector4 palette = new Vector4(hue, saturation, brightness, contrast);
      GomObjectData specData = (GomObjectData)dynObj.Data.Dictionary["appPaletteSpecular"];
      Vector4 specular = new Vector4(
        (Single)specData.Dictionary["r"], (Single)specData.Dictionary["g"],
        (Single)specData.Dictionary["b"], (Single)specData.Dictionary["a"]
      );
      GomObjectData metSpecData =
        (GomObjectData)dynObj.Data.Dictionary["appPaletteMetallicSpecular"];
      Vector4 metallicSpecular = new Vector4(
        (Single)metSpecData.Dictionary["r"], (Single)metSpecData.Dictionary["g"],
        (Single)metSpecData.Dictionary["b"], (Single)metSpecData.Dictionary["a"]
      );

      if (paletteNum != 0) {
        if (paletteNum == 1) {
          palette1 = palette;
          palette1MetSpec = metallicSpecular;
          palette1Spec = specular;
        }

        if (paletteNum == 2) {
          palette2 = palette;
          palette2MetSpec = metallicSpecular;
          palette2Spec = specular;
        }
      } else {
        palette1 = palette;
        palette1MetSpec = metallicSpecular;
        palette1Spec = specular;
        palette2 = palette;
        palette2MetSpec = metallicSpecular;
        palette2Spec = specular;
      }
    }

    public void SetFacepaintMap(Device device, String facepaintPath) {
      facepaintDDS = "/resources" + facepaintPath;
      FileToShaderResource(ref device, facepaintDDS, ref facepaintSRV);
    }
  }
}
