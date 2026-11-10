using System;
using SlimDX;

namespace PugTools {
  internal sealed partial class View_AREA {
    // A second minimap capture of one world rectangle, for the zoomed-in minimap (WorldBrowser.MiniMapDetail.cs). It
    // goes through the same render path as the main capture; only the extent it fits to is the requested rectangle,
    // and the result is posted as a detail picture instead of replacing the main one.
    private volatile bool miniMapDetailRequested;
    private bool miniMapCaptureIsDetail;
    private float miniMapDetailMinX, miniMapDetailMaxX, miniMapDetailMinZ, miniMapDetailMaxZ;
    private int miniMapDetailToken;
    private float savedExtentMinX, savedExtentMaxX, savedExtentMinZ, savedExtentMaxZ;

    public bool RequestMiniMapDetail(float minX, float maxX, float minZ, float maxZ, int token) {
      // A main capture already waiting wins: it would be taken for this one, and the main picture must not be lost.
      if (miniMapCaptureRequested || mapOpen) return false;
      if (!Single.IsFinite(minX) || !Single.IsFinite(maxX) || !Single.IsFinite(minZ) || !Single.IsFinite(maxZ) || maxX - minX < 1f || maxZ - minZ < 1f) return false;
      miniMapDetailMinX = minX; miniMapDetailMaxX = maxX; miniMapDetailMinZ = minZ; miniMapDetailMaxZ = maxZ;
      miniMapDetailToken = token;
      miniMapCaptureUseAutoScope = false; // keep the page the main picture was taken of
      miniMapCaptureForceFullExtent = false;
      miniMapDetailRequested = true;
      miniMapCaptureRequested = true;
      return true;
    }

    // Called at the end of PrepareMiniMapCaptureScope, once the extent of the page is known.
    private void ApplyMiniMapDetailExtent() {
      miniMapCaptureIsDetail = miniMapDetailRequested;
      miniMapDetailRequested = false;
      if (!miniMapCaptureIsDetail) return;
      savedExtentMinX = mapExtentMinX; savedExtentMaxX = mapExtentMaxX; savedExtentMinZ = mapExtentMinZ; savedExtentMaxZ = mapExtentMaxZ;
      mapExtentMinX = miniMapDetailMinX; mapExtentMaxX = miniMapDetailMaxX;
      mapExtentMinZ = miniMapDetailMinZ; mapExtentMaxZ = miniMapDetailMaxZ;
    }

    // After the capture: the page's own extent is what everything else reads.
    private void FinishMiniMapDetailCapture() {
      if (!miniMapCaptureIsDetail) return;
      mapExtentMinX = savedExtentMinX; mapExtentMaxX = savedExtentMaxX; mapExtentMinZ = savedExtentMinZ; mapExtentMaxZ = savedExtentMaxZ;
      miniMapCaptureIsDetail = false;
    }
  }
}
