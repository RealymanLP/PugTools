using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TorArchive;

namespace PugTools {
  internal class Format_GR2 {
    private readonly String _dest;
    private readonly List<String> _errors;
    private readonly String _extension;

    internal HashSet<String> MatNames { get; set; }
    internal Dictionary<String, Archive> MeshNames { get; set; }

    internal Format_GR2(String dest, String ext) {
      _dest = dest;
      _errors = new List<String>();
      _extension = ext;
      MatNames = new HashSet<String>();
      MeshNames = new Dictionary<String, Archive>();
    }

    internal void ParseGR2(Stream fileStream, String fullFileName, Archive arch) {
      List<UInt32> offsetMeshNames = new List<UInt32>();
      List<UInt32> offsetMaterialNames = new List<UInt32>();

      using (BinaryReader br = new BinaryReader(fileStream)) {
        UInt64 header = br.ReadUInt32();

        if (header.ToString("X") != "42574147") {
          _errors.Add("File: " + fullFileName);
          _errors.Add("Invalid header" + header.ToString());
          return;
        } else {
          br.BaseStream.Seek(0x10, SeekOrigin.Begin);
          _ = br.ReadUInt32();
          _ = br.ReadUInt32();
          UInt16 numMeshes = br.ReadUInt16();
          UInt16 numMaterials = br.ReadUInt16();

          br.BaseStream.Seek(0x50, SeekOrigin.Begin);
          _ = br.ReadUInt32();
          UInt32 offsetMeshHeader = br.ReadUInt32();
          UInt32 offsetMaterialNameOffsets = br.ReadUInt32();

          if (numMeshes != 0) {
            br.BaseStream.Seek(offsetMeshHeader, SeekOrigin.Begin);

            for (Int32 i = 0; i < numMeshes; i++) {
              UInt32 offset = br.ReadUInt32();
              br.ReadSingle();
              br.ReadUInt16();
              br.ReadUInt16();
              br.ReadUInt16();
              br.ReadUInt16();
              br.ReadUInt32();
              br.ReadUInt32();
              br.ReadUInt32();
              br.ReadUInt32();
              br.ReadUInt32();
              br.ReadUInt32();
              offsetMeshNames.Add(offset);
            }

            if (offsetMeshNames.Count > 0) {
              foreach (UInt32 i in offsetMeshNames) {
                String meshName = ReadString(fileStream, br, i);

                if (!MeshNames.ContainsKey(meshName))
                  MeshNames.Add(meshName, arch);

                MatNames.Add(meshName);
              }
            }
          }

          if (numMaterials != 0) {
            br.BaseStream.Seek(offsetMaterialNameOffsets, SeekOrigin.Begin);

            for (Int32 i = 0; i < numMaterials; i++) {
              UInt32 offset = br.ReadUInt32();
              offsetMaterialNames.Add(offset);
            }

            if (offsetMaterialNames.Count > 0) {
              foreach (UInt32 i in offsetMaterialNames) {
                MatNames.Add(ReadString(fileStream, br, i));
              }
            }
          }
        }
      }
    }

    internal static String ReadString(Stream fileStream, BinaryReader br, UInt32 offset) {
      Int64 original_position = fileStream.Position;
      fileStream.Position = offset;
      List<Byte> strBytes = new List<Byte>();
      Int32 b;

      while ((b = br.ReadByte()) != 0x00) {
        strBytes.Add((Byte)b);
      }

      fileStream.Position = original_position;

      return Encoding.ASCII.GetString(strBytes.ToArray());
    }

    internal void WriteFile(Boolean _ = false) {
      if (!Directory.Exists(_dest + "\\File_Names"))
        Directory.CreateDirectory(_dest + "\\File_Names");

      if (MeshNames.Count > 0) {
        StreamWriter outputMeshNames =
          new StreamWriter(_dest + "\\File_Names\\" + _extension + "_mesh_file_names.txt", false);

        foreach (KeyValuePair<String, Archive> file in MeshNames) {
          String output = "";

          if (file.Value.FileName.Contains("_dynamic_")) {
            if (file.Key.Contains('_')) {
              String type = file.Key.Split('_').First();
              output += "/resources/art/dynamic/" + type + "/model/" + file.Key + ".gr2\r\n";
              output += "/resources/art/dynamic/" + type + "/model/" + file.Key + ".lod.gr2\r\n";
              output += "/resources/art/dynamic/" + type + "/model/" + file.Key + ".clo\r\n";
            }
          } else {
            /* 
            if (outputAllDirs) {
              foreach (string dir in file.Value.directories) {
                output += dir + "/" + file.Key + ".gr2\r\n";                              
              }
            } else {
            */
            output += file.Key + ".gr2\r\n";
            //}
          }

          output = output.Replace("//", "/");
          outputMeshNames.Write(output);
        }

        outputMeshNames.Close();
        MeshNames.Clear();
      }

      if (MatNames.Count > 0) {
        StreamWriter outputMatNames =
          new StreamWriter(
            _dest + "\\File_Names\\" + _extension + "_material_file_names.txt", false
          );

        foreach (String file in MatNames) {
          outputMatNames.Write("/resources/art/shaders/materials/" + file + ".mat" + "\r\n");
        }

        outputMatNames.Close();
        MatNames.Clear();
      }

      if (_errors.Count > 0) {
        StreamWriter outputErrors =
          new StreamWriter(_dest + "\\File_Names\\" + _extension + "_error_list.txt", false);

        foreach (String error in _errors) {
          outputErrors.Write(error + "\r\n");
        }

        outputErrors.Close();
        _errors.Clear();
      }
    }
  }
}
