using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileFormats;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDXNet.Vertex;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace PugTools {
  // The minimap tiles of an in-game map page, laid over the generated map (the World Browser's counterpart of the
  // Node Browser's "Tiles" mode).
  //
  // A page's original art is ONE image, `<map>_r.dds`. Its minimap is cut into a grid of tiles under `minimaps/`:
  // `<map>_<col>_<row>_r.dds`, column to the right and row downwards, together covering the page's minimap
  // rectangle (mapPageMiniMinCoord .. mapPageMiniMaxCoord). The tiles are drawn exactly like the page image is -- as
  // quads in world coordinates on the top-down map camera -- so they land on the generated map where they belong, and
  // tiles that do not exist (a sparse grid) simply leave the generated map visible.
  internal sealed partial class View_AREA {
    private bool interactiveMapPreferTiles;
    private readonly Dictionary<string, MapTileGrid> mapTileGrids = new Dictionary<string, MapTileGrid>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<MapArtGpu>> mapTileGpu = new Dictionary<string, List<MapArtGpu>>(StringComparer.OrdinalIgnoreCase);

    private struct MapTileGrid {
      public int Columns, Rows;
      public string Prefix;
    }

    public bool InteractiveMapPrefersTiles => interactiveMapPreferTiles;
    public bool InteractiveMapTilesAvailable {
      get {
        MapTileGrid grid = MapTileGridFor(InteractiveMapSelectedPage);
        return grid.Columns > 0 && grid.Rows > 0;
      }
    }
    public bool InteractiveMapTilesActive => mapOpen && interactiveMapPreferTiles && InteractiveMapTilesAvailable;

    public void SetInteractiveMapTiles(bool tiles) {
      interactiveMapPreferTiles = tiles;
      if (mapOpen && !InteractiveMapTravelMode) ApplyInteractiveMapScopeExtents(true);
      else if (mapOpen) { UpdateMapCamera(); InvalidateTemporalHistory(); }
      if (Window is WorldBrowser browser) {
        browser.SetStatusLabel(tiles && !InteractiveMapTilesAvailable
          ? "No minimap tiles exist for this map page."
          : InteractiveMapScopeStatus());
      }
      NotifyInteractiveMapModeChanged();
    }

    private string MapTilePath(string prefix, string mapName, int col, int row) {
      return "/resources/world/" + prefix + "/" + area.Id + "/minimaps/" + mapName + "_" + col.ToString("00") + "_" + row.ToString("00") + "_r.dds";
    }

    private bool MapTileExists(string path) {
      try { using (TorArchive.File file = area?.FindFile(path)) return file != null; } catch { return false; }
    }

    // How many columns and rows of tiles a page has. Counted the way the Node Browser counts them: a row (column) is
    // there when ANY tile of it is, and the scan stops after a run of empty ones, so a sparse grid is not cut short.
    // Both client layouts are tried, the one the area was loaded from first. Cached per page; a page without any
    // tile is remembered as such, so asking again on every frame costs one dictionary lookup.
    private MapTileGrid MapTileGridFor(AreaMapPage page) {
      if (page == null || area == null || String.IsNullOrWhiteSpace(page.MapName)) return default(MapTileGrid);
      if (mapTileGrids.TryGetValue(page.MapName, out MapTileGrid cached)) return cached;

      bool systemGenerated = (area.Path ?? String.Empty).IndexOf("systemgenerated", StringComparison.OrdinalIgnoreCase) >= 0;
      string[] prefixes = systemGenerated ? new[] { "livecontent/systemgenerated", "areas" } : new[] { "areas", "livecontent/systemgenerated" };
      const int maxParts = 99, trailingMisses = 8;
      var result = default(MapTileGrid);
      foreach (string prefix in prefixes) {
        int rows = 0, columns = 0, rowMisses = 0, colMisses = 0;
        for (int row = 0; row < maxParts && rowMisses < trailingMisses; row++) {
          bool found = false;
          for (int col = 0; col < maxParts && !found; col++) found = MapTileExists(MapTilePath(prefix, page.MapName, col, row));
          if (found) { rows = row + 1; rowMisses = 0; } else rowMisses++;
        }
        if (rows == 0) continue;
        for (int col = 0; col < maxParts && colMisses < trailingMisses; col++) {
          bool found = false;
          for (int row = 0; row < rows && !found; row++) found = MapTileExists(MapTilePath(prefix, page.MapName, col, row));
          if (found) { columns = col + 1; colMisses = 0; } else colMisses++;
        }
        if (columns > 0) { result = new MapTileGrid { Columns = columns, Rows = rows, Prefix = prefix }; break; }
      }
      mapTileGrids[page.MapName] = result;
      return result;
    }

    // One textured quad per tile that exists, built the first time the page's tiles are asked for. Orientation and
    // heights follow BuildMapArt: the same V flip as the page image, the same plane above the world.
    private List<MapArtGpu> EnsureMapTiles(AreaMapPage page) {
      if (page == null) return null;
      if (mapTileGpu.TryGetValue(page.MapName ?? String.Empty, out List<MapArtGpu> built)) return built;
      var tiles = new List<MapArtGpu>();
      mapTileGpu[page.MapName ?? String.Empty] = tiles;
      MapTileGrid grid = MapTileGridFor(page);
      if (grid.Columns <= 0 || grid.Rows <= 0) return tiles;

      float minX = Math.Min(page.MiniMapMin.X, page.MiniMapMax.X), maxX = Math.Max(page.MiniMapMin.X, page.MiniMapMax.X);
      float minZ = Math.Min(page.MiniMapMin.Y, page.MiniMapMax.Y), maxZ = Math.Max(page.MiniMapMin.Y, page.MiniMapMax.Y);
      if (maxX - minX < .001f || maxZ - minZ < .001f) return tiles;
      float tileWidth = (maxX - minX) / grid.Columns, tileHeight = (maxZ - minZ) / grid.Rows;
      float y = boundsMax.Y + 2f;
      var n = new Vector3(0, 1, 0);
      var t = new Vector3(1, 0, 0);

      for (int row = 0; row < grid.Rows; row++) {
        for (int col = 0; col < grid.Columns; col++) {
          ShaderResourceView texture = LoadTexture(MapTilePath(grid.Prefix, page.MapName, col, row), true);
          if (texture == null) continue;
          float x0 = minX + col * tileWidth, x1 = x0 + tileWidth, z0 = minZ + row * tileHeight, z1 = z0 + tileHeight;
          var v = new[] {
            new PosNormalTexTan(new Vector3(x0, y, z1), n, new Vector2(0, 1), t),
            new PosNormalTexTan(new Vector3(x1, y, z1), n, new Vector2(1, 1), t),
            new PosNormalTexTan(new Vector3(x1, y, z0), n, new Vector2(1, 0), t),
            new PosNormalTexTan(new Vector3(x0, y, z1), n, new Vector2(0, 1), t),
            new PosNormalTexTan(new Vector3(x1, y, z0), n, new Vector2(1, 0), t),
            new PosNormalTexTan(new Vector3(x0, y, z0), n, new Vector2(0, 0), t)
          };
          var description = new BufferDescription(PosNormalTexTan.Stride * v.Length, ResourceUsage.Immutable, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
          using (var stream = new DataStream(v, false, false))
            tiles.Add(new MapArtGpu { Buffer = new Buffer(Device, stream, description), Count = v.Length, Texture = texture, Name = page.MapName });
        }
      }
      return tiles;
    }

    // The tile quads hold textures from the shared cache, which a texture reload drops: forget them so they are
    // rebuilt from the reloaded textures the next time the tiles are asked for. The grid counts stay valid.
    private void DisposeMapTiles() {
      foreach (List<MapArtGpu> tiles in mapTileGpu.Values) foreach (MapArtGpu g in tiles) g.Dispose();
      mapTileGpu.Clear();
    }

    private void DrawMapTiles(Matrix vp) {
      List<MapArtGpu> tiles = EnsureMapTiles(InteractiveMapSelectedPage);
      if (tiles == null || tiles.Count == 0) return;
      ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
      fx.SetWorld(Matrix.Identity);
      fx.SetViewProj(vp);
      foreach (MapArtGpu g in tiles) {
        if (g.Texture == null || g.Buffer == null) continue;
        fx.SetMapArt(g.Texture, 1f);
        ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(g.Buffer, PosNormalTexTan.Stride, 0));
        fx.MapArt.GetPassByIndex(0).Apply(ImmediateContext);
        ImmediateContext.Draw(g.Count, 0);
      }
      fx.ClearMapArt();
    }

    // The tiles' own rectangle can reach past the generated map, as an authored page can: grow the camera extent to
    // include it, exactly as ExpandMapExtentToIncludePage does for the page image.
    private void ExpandMapExtentToIncludeMiniTiles(AreaMapPage page) {
      if (page == null) return;
      float minX = Math.Min(page.MiniMapMin.X, page.MiniMapMax.X), maxX = Math.Max(page.MiniMapMin.X, page.MiniMapMax.X);
      float minZ = Math.Min(page.MiniMapMin.Y, page.MiniMapMax.Y), maxZ = Math.Max(page.MiniMapMin.Y, page.MiniMapMax.Y);
      if (maxX - minX < .001f || maxZ - minZ < .001f) return;
      mapExtentMinX = Math.Min(mapExtentMinX, minX);
      mapExtentMaxX = Math.Max(mapExtentMaxX, maxX);
      mapExtentMinZ = Math.Min(mapExtentMinZ, minZ);
      mapExtentMaxZ = Math.Max(mapExtentMaxZ, maxZ);
    }

    // Some of the maps are stored as plain 32-bit RGB DDS (X8R8G8B8: red, green and blue masks and NO alpha mask --
    // the two "world map" pages that are mostly SWTOR's green no-data colour around grey terrain are an example). D3DX
    // reads such a file as RGBA with the unused byte, which is 0, as ALPHA: every pixel fully transparent, and the map
    // art composites as nothing. The page has no transparency to preserve, so the file is patched in memory to say
    // what it is -- the spare byte becomes an alpha channel and is set to 255. Anything else is returned untouched.
    private static Stream EnsureOpaqueRgbDds(Stream stream) {
      try {
        if (stream == null || !stream.CanSeek || stream.Length < 128 || stream.Length > Int32.MaxValue) return stream;
        long start = stream.Position;
        var data = new byte[stream.Length];
        stream.Position = 0;
        int read = 0;
        while (read < data.Length) { int n = stream.Read(data, read, data.Length - read); if (n <= 0) break; read += n; }
        stream.Position = start;
        if (read < 128 || data[0] != (byte)'D' || data[1] != (byte)'D' || data[2] != (byte)'S' || data[3] != (byte)' ') return stream;

        // DDS_PIXELFORMAT starts at byte 76: flags, fourCC, bit count, R/G/B/A masks.
        uint flags = BitConverter.ToUInt32(data, 80), bits = BitConverter.ToUInt32(data, 88);
        uint r = BitConverter.ToUInt32(data, 92), g = BitConverter.ToUInt32(data, 96), b = BitConverter.ToUInt32(data, 100), a = BitConverter.ToUInt32(data, 104);
        const uint AlphaPixels = 0x1, FourCc = 0x4, Rgb = 0x40;
        if ((flags & FourCc) != 0 || (flags & Rgb) == 0 || (flags & AlphaPixels) != 0 || bits != 32 || a != 0) return stream;
        // The spare byte must be the top one (X8R8G8B8 / X8B8G8R8); other layouts are not guessed at.
        if ((0xFFFFFFFFu & ~(r | g | b)) != 0xFF000000u) return stream;

        BitConverter.GetBytes(flags | AlphaPixels).CopyTo(data, 80);
        BitConverter.GetBytes(0xFF000000u).CopyTo(data, 104);
        for (int i = 128 + 3; i < data.Length; i += 4) data[i] = 0xFF;
        stream.Dispose();
        return new MemoryStream(data, 0, data.Length, false, true);
      } catch {
        return stream;
      }
    }
  }
}
