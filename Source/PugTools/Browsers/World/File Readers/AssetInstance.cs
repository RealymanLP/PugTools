using System;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip.Compression;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDXNet.Vertex;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace FileFormats {
  public class AssetInstance1 {
    public ulong Id;
    public ulong AssetId;
    public ulong ParentInstanceId;
    public Room Room;
    public Vector3 Position;
    public Vector3 Scale;
    public Vector3 Rotation;
    public Matrix Transform;
    public bool Hidden;

    public AssetInstance1() {
      Id = 0;
      AssetId = 0;
      ParentInstanceId = 0;
      Room = null;
      Position = new Vector3();
      Rotation = new Vector3();
      Scale = new Vector3(1.0f, 1.0f, 1.0f);
      Hidden = false;
    }

    public void CalculateTransform() {
      Transform = Matrix.Scaling(Scale) * Matrix.RotationZ((float)(Rotation.Z * Math.PI / 180.0)) * Matrix.RotationX((float)(Rotation.X * Math.PI / 180.0)) * Matrix.RotationY((float)(Rotation.Y * Math.PI / 180.0)) * Matrix.Translation(Position);
      //this.Transform = Matrix.Translation(this.Position);
    }

    // public Matrix GetAbsoluteTransform()
    // {
    //     ulong pId = ParentInstanceId;
    //     AssetInstance1 p;
    //     Matrix outputTransform = Matrix.Identity;
    //     outputTransform *= Transform;

    //     while (pId != 0)
    //     {
    //         if (!Room.InstancesById.ContainsKey(pId)) { break; }
    //         p = Room.InstancesById[pId];
    //         outputTransform *= p.Transform;
    //         pId = p.ParentInstanceId;
    //     }

    //     return outputTransform;
    // }


    // public Vector3 GetAbsolutePosition()
    // {
    //     ulong pId = ParentInstanceId;
    //     AssetInstance1 p;
    //     Vector3 outputVec = new Vector3();

    //     while (pId != 0)
    //     {
    //         if (!Room.InstancesById.ContainsKey(pId)) { break; }
    //         p = Room.InstancesById[pId];
    //         outputVec.X += p.Position.X;
    //         outputVec.Y += p.Position.Y;
    //         outputVec.Z += p.Position.Z;
    //         pId = p.ParentInstanceId;
    //     }

    //     return outputVec;
    // }

  }

  public class AssetInstance {
    public ulong ID;
    public ulong assetID;
    public ulong parentInstance;

    public bool hasHeightMap;
    public bool hidden;

    private byte[] vertexData;

    public int numFaces;

    public float depth = 64;
    public float height = 64;
    public float width = 64;

    public Area area;

    public Buffer VBO;
    public Buffer IBO;

    public BufferDescription VBD;
    public BufferDescription IBD;

    public DataStream VDS;
    public DataStream IDS;

    private readonly Inflater RawDeflate = new Inflater();

    private void LogHeightmapDiagnostic(string codecUsed, HeightMap heightMap, bool succeeded) {
      try {
        string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "heightmap_debug.log");
        string line = string.Format(
          "id={0} pos=({1:F1},{2:F1},{3:F1}) codec={4} succeeded={5} width={6} depth={7} " +
          "headerBitflag={8} hasHoles={9}\n",
          ID, position.X, position.Y, position.Z, codecUsed, succeeded,
          heightMap != null ? heightMap.width.ToString() : "-",
          heightMap != null ? heightMap.depth.ToString() : "-",
          heightMap != null ? heightMap.headerBitFlag.ToString() : "-",
          heightMap != null ? heightMap.hasHoles.ToString() : "-");
        System.IO.File.AppendAllText(logPath, line);
      } catch {
        // Best-effort diagnostic only.
      }
    }

    public Vector3 position = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 rotation = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 scale = new Vector3(1.0f, 1.0f, 1.0f);

    public Matrix transformMatrix;

    // public VertexBuffer heightMapBuffer;
    // public IndexBuffer heightMapIndex;

    public AssetInstance(ulong ID, ulong assetID, Area area) {
      this.ID = ID;
      this.assetID = assetID;
      this.area = area;
    }

    public void AddProperty(ref BinaryReader br, uint name, uint type) {
      if (name == 0x3C472B4A && type == 0) // .Hidden
      {
        hidden = br.ReadBoolean();
      } else if (name == 0x1B205D23 && type == 4) // .Depth
        {
        depth = br.ReadSingle();
      } else if (name == 0x16B4F247 && type == 4) // .Height
        {
        height = br.ReadSingle();
      } else if (name == 0xD9DDF326 && type == 4) // .Width
        {
        width = br.ReadSingle();
      } else if (name == 0x40865E7F && type == 5) // .ParentInstance
        {
        parentInstance = br.ReadUInt64();
      } else if (name == 0x4F77E269 && type == 6) // .Position
        {
        position.X = br.ReadSingle();
        position.Y = br.ReadSingle();
        position.Z = br.ReadSingle();
      } else if (name == 0x8C64AF1E && type == 6) // .Rotation
        {
        rotation.X = br.ReadSingle();
        rotation.Y = br.ReadSingle();
        rotation.Z = br.ReadSingle();
      } else if (name == 0xB181622A && type == 6) // .Scale
        {
        scale.X = br.ReadSingle();
        scale.Y = br.ReadSingle();
        scale.Z = br.ReadSingle();
      } else if (name == 0xA3AB26AE && type == 9) // .VertexData
        {
        var length = br.ReadUInt32();
        var pos = br.BaseStream.Position;

        if (length > 0) vertexData = br.ReadBytes((int)length);

        // Move to beginning of next property
        br.BaseStream.Seek(pos + length, SeekOrigin.Begin);
      } else if (name == 0xD576D2CF && type == 1) // .Hook
        {
        br.ReadUInt32();
      } else // Unknown
        {
        switch (type) {
          case 0: // 1 x 1 bytes
            br.ReadByte();
            break;
          case 1:
          case 3:
          case 4: // 1 x 4 bytes
            br.ReadUInt32();
            break;
          case 5: // 2 x 4 bytes
            br.ReadUInt64();
            break;
          case 6: // 3 x 4 bytes
            br.ReadUInt32();
            br.ReadUInt32();
            br.ReadUInt32();
            break;
          case 7: // 4 x 4 bytes
            br.ReadSingle(); // R
            br.ReadSingle(); // G
            br.ReadSingle(); // B
            br.ReadSingle(); // A
            break;
          case 8: // String
          case 9: // Binary Data
            br.BaseStream.Seek(br.ReadUInt32(), SeekOrigin.Current);
            break;
        }
      }
    }

    public void ReadHeightMap() {
      if (vertexData != null) {
        BinaryReader br2 = null;
        string codecUsed = "none";

        // Same situation as everywhere else in the 64-bit client: this data may be Zstandard-
        // compressed (magic 28 B5 2F FD) instead of the old zlib/deflate (magic 9C 78 little-endian
        // / 0x9C78 as read below). Check zstd first; fall back to the existing zlib handling.
        bool isZstd = vertexData.Length >= 4 &&
                      vertexData[0] == 0x28 && vertexData[1] == 0xB5 &&
                      vertexData[2] == 0x2F && vertexData[3] == 0xFD;

        if (isZstd) {
          codecUsed = "zstd";
          using var decompressor = new ZstdSharp.Decompressor();
          // maxDecompressedSize=0 is NOT "auto-detect from the frame header" here - ZstdSharp
          // enforces it as a hard cap and throws if the real size exceeds it, even though the
          // frame header does carry the real size (confirmed by the exception message itself
          // reporting it). Heightmap grids are small (tens/low hundreds of KB); 16 MB is a very
          // generous, safe ceiling without hardcoding an exact expected size we don't have here.
          System.Span<byte> span = decompressor.Unwrap(vertexData, 16 * 1024 * 1024);
          br2 = new BinaryReader(new MemoryStream(span.ToArray()));
        } else {
          var magic = BitConverter.ToUInt16(vertexData, 0);

          if (magic == 0x8B1F) {
            codecUsed = "gzip-unsupported";
            System.Diagnostics.Debug.WriteLine("Heightmap uses GZip compression");

            // br.BaseStream.Seek(pos + 14, SeekOrigin.Begin);
            // byte[] bytes = br.ReadBytes((int)length - 18);

            // br.BaseStream.Seek(pos + length, SeekOrigin.Begin);
            // uint bufferSize = br.ReadUInt32();

            // if (bytes[0] == 0x78 && bytes[1] == 0x9C)
            // {
            //     byte[] buffer = new byte[bufferSize];

            //     RawDeflate.SetInput(bytes);
            //     RawDeflate.Inflate(buffer);

            //     br2 = new BinaryReader(new MemoryStream(buffer));
            //     Debug.WriteLine("Heightmap uses Zip compression");
            // }
            // else if (bytes[0] == 0x1f && bytes[1] == 0x8b && bytes[2] == 8)
            // {
            //     Debug.WriteLine("Heightmap uses GZip compression");
            // }
            // else
            // {
            //     Debug.WriteLine("Heightmap uses unknown compression");
            // }
          } else if (magic == 0x9C78) {
            codecUsed = "zlib";
            byte[] buffer = new byte[1048575];

            RawDeflate.SetInput(vertexData);
            RawDeflate.Inflate(buffer);

            br2 = new BinaryReader(new MemoryStream(buffer));
          } else {
            codecUsed = string.Format("unknown-0x{0:X4}", magic);
          }
        }

        if (br2 == null) {
          LogHeightmapDiagnostic(codecUsed, null, false);
          return;
        }

        HeightMap heightMap = new HeightMap(br2);
        LogHeightmapDiagnostic(codecUsed, heightMap, true);

        // Use the HeightMap's own parsed dimensions consistently, not the AssetInstance's width/
        // depth fields (which come from a different source and aren't guaranteed to match the
        // heightmap data actually read - mismatched sizes here were what caused the out-of-bounds
        // access into heightMap.elevation, which is sized [heightMap.depth, heightMap.width]).
        uint hmWidth = heightMap.width;
        uint hmDepth = heightMap.depth;

        VertexPT[] vertices = new VertexPT[(int)(hmWidth * hmDepth)];

        for (int z = 0; z < hmDepth; z++) {
          for (int x = 0; x < hmWidth; x++) {
            float posX = 0.2f * (x - hmWidth / 2);
            float posY = heightMap.elevation[z, x];
            float posZ = 0.2f * (z - hmDepth / 2);

            Vector3 posVec = new Vector3(posX, posY, posZ);
            Vector2 texVec = new Vector2(x % 2, z % 2);

            vertices[(int)(z * hmWidth + x)] = new VertexPT(posVec, texVec);
          }
        }

        VBD = new BufferDescription(VertexPT.Stride * vertices.Length, ResourceUsage.Immutable, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        VDS = new DataStream(vertices, false, false);

        ushort[] indices = new ushort[(int)(hmWidth * hmDepth * 6)];
        numFaces = 0;

        for (int z = 0; z < hmDepth - 1; z++) {
          for (int x = 0; x < hmWidth - 1; x++) {
            if (heightMap.hasHoles && heightMap.CheckNoHole(x, z + 1) && heightMap.CheckNoHole(x + 1, z)) {
              if (heightMap.CheckNoHole(x, z)) {
                indices[numFaces + 0] = (ushort)(z * hmWidth + x); // Top left
                indices[numFaces + 1] = (ushort)((z + 1) * hmWidth + x); // Bottom left
                indices[numFaces + 2] = (ushort)(z * hmWidth + (x + 1)); // Top right

                numFaces += 3;
              }

              if (heightMap.CheckNoHole(x + 1, z + 1)) {
                indices[numFaces + 0] = (ushort)(z * hmWidth + (x + 1)); // Top right
                indices[numFaces + 1] = (ushort)((z + 1) * hmWidth + x); // Bottom left
                indices[numFaces + 2] = (ushort)((z + 1) * hmWidth + (x + 1)); // Bottom right

                numFaces += 3;
              }
            } else {
              indices[numFaces + 0] = (ushort)(z * hmWidth + x); // Top left
              indices[numFaces + 1] = (ushort)((z + 1) * hmWidth + x); // Bottom left
              indices[numFaces + 2] = (ushort)(z * hmWidth + (x + 1)); // Top right
              indices[numFaces + 3] = (ushort)(z * hmWidth + (x + 1)); // Top right
              indices[numFaces + 4] = (ushort)((z + 1) * hmWidth + x); // Bottom left
              indices[numFaces + 5] = (ushort)((z + 1) * hmWidth + (x + 1)); // Bottom right

              numFaces += 6;
            }
          }
        }

        ushort[] indexArray = new ushort[numFaces];
        Array.Copy(indices, indexArray, numFaces);

        IBD = new BufferDescription(sizeof(ushort) * indexArray.Length, ResourceUsage.Immutable, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        IDS = new DataStream(indexArray, false, false);

        hasHeightMap = true;
      }
    }

    public void CalculateTransform() {
      transformMatrix = Matrix.Scaling(scale) *
                        Matrix.RotationZ((float)(rotation.Z * Math.PI / 180.0)) *
                        Matrix.RotationX((float)(rotation.X * Math.PI / 180.0)) *
                        Matrix.RotationY((float)(rotation.Y * Math.PI / 180.0)) *
                        Matrix.Translation(position);
    }

    public Matrix GetAbsoluteTransform(Room room) {
      ulong pId = parentInstance;
      AssetInstance p;
      Matrix outputTransform = Matrix.Identity;
      Matrix.Multiply(ref outputTransform, ref transformMatrix, out outputTransform);
      // outputTransform *= transformMatrix;

      while (pId != 0) {
        if (!room.InstancesById.ContainsKey(pId)) { break; }
        p = room.InstancesById[pId];
        Matrix.Multiply(ref outputTransform, ref p.transformMatrix, out outputTransform);
        // outputTransform *= p.transformMatrix;
        pId = p.parentInstance;
      }

      return outputTransform;
    }
  }
}
