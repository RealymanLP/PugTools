using System;
using System.Collections.Generic;
using System.IO;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace FileFormats {
  public class GR2_Mesh {
    public GR2 parent;
    public UInt32 bitFlag2;
    public UInt32 vertexSize;
    public Buffer idxBuffer;
    public List<GR2_Mesh_Bone> meshBones;
    public String meshName;
    public List<GR2_Mesh_Piece> meshPieces;
    public List<GR2_Mesh_Vertex_Index> meshVertIndex;
    public List<GR2_Mesh_Vertex> meshVerts;
    public UInt16 numBones;
    public UInt16 numPieces;
    public UInt32 numVertIndex;
    public UInt32 numVerts;
    public UInt64 offsetMeshBones;
    public UInt64 offsetMeshName;
    public UInt64 offsetMeshPieces;
    public UInt64 offsetMeshVertIndex;
    public UInt64 offsetMeshVerts;
    public Buffer vertBuffer;

    public GR2_Mesh(BinaryReader br, Boolean is64Bit) {
      meshBones = new List<GR2_Mesh_Bone>();
      meshPieces = new List<GR2_Mesh_Piece>();
      meshVertIndex = new List<GR2_Mesh_Vertex_Index>();
      meshVerts = new List<GR2_Mesh_Vertex>();

      offsetMeshName = is64Bit ? br.ReadUInt64() : br.ReadUInt32();
      br.ReadUInt32(); // bitFlag1
      numPieces = br.ReadUInt16();
      numBones = br.ReadUInt16();

      if (is64Bit) {
        bitFlag2 = br.ReadUInt32();
        vertexSize = br.ReadUInt32();
      } else {
        bitFlag2 = br.ReadUInt16();
        vertexSize = br.ReadUInt16();
      }

      numVerts = br.ReadUInt32();
      numVertIndex = br.ReadUInt32();

      offsetMeshVerts = is64Bit ? br.ReadUInt64() : br.ReadUInt32();
      offsetMeshPieces = is64Bit ? br.ReadUInt64() : br.ReadUInt32();
      offsetMeshVertIndex = is64Bit ? br.ReadUInt64() : br.ReadUInt32();
      offsetMeshBones = is64Bit ? br.ReadUInt64() : br.ReadUInt32();

      meshName = FileHelpers.ReadString(br, offsetMeshName);
    }
  }
}
