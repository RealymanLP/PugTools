using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PugTools {
  // The minimap is one screenshot of the whole page, taken at the size of the render panel. Zooming in only enlarges a
  // piece of that picture, which is why it turns into blocks. Past the point where the picture no longer holds enough
  // pixels for what is on screen, a second picture is taken of just the visible part (plus a margin so that walking
  // does not need a new one every step), drawn over the first. The first stays underneath as the fallback while the
  // second is being rendered.
  public partial class WorldBrowser {
    private Bitmap miniMapDetailImage;
    private float miniMapDetailMinX, miniMapDetailMaxX, miniMapDetailMinZ, miniMapDetailMaxZ;
    private float miniMapDetailZoom;
    private int miniMapDetailToken;
    private bool miniMapDetailPending;
    private DateTime miniMapDetailLastRequest = DateTime.MinValue;
    private System.Windows.Forms.Timer miniMapDetailTimer;

    private const float MiniMapDetailMargin = .35f;
    private const double MiniMapDetailMinSeconds = 1.2;

    private void ClearMiniMapDetail() {
      miniMapDetailToken++;
      miniMapDetailPending = false;
      if (miniMapDetailImage != null) { miniMapDetailImage.Dispose(); miniMapDetailImage = null; }
    }

    private void DisposeMiniMapDetail() {
      ClearMiniMapDetail();
      if (miniMapDetailTimer != null) { miniMapDetailTimer.Stop(); miniMapDetailTimer.Dispose(); miniMapDetailTimer = null; }
    }

    // Called from every paint of the minimap: cheap, it only makes sure a check is coming.
    private void TouchMiniMapDetail() {
      if (_closing || miniMapImage == null) return;
      if (miniMapDetailTimer == null) {
        miniMapDetailTimer = new System.Windows.Forms.Timer { Interval = 300 };
        miniMapDetailTimer.Tick += (_, __) => { miniMapDetailTimer.Stop(); EvaluateMiniMapDetail(); };
      }
      if (!miniMapDetailTimer.Enabled) miniMapDetailTimer.Start();
    }

    private void EvaluateMiniMapDetail() {
      if (_closing || IsDisposed || panelRender == null || miniMapImage == null || miniMapPicture == null ||
          miniMapPanel == null || !miniMapPanel.Visible || miniMapFollowRefreshPending) return;
      // The original art is a fixed picture of its own: a second render of it gains nothing.
      if (panelRender.MiniMapOriginalArtActive) { if (miniMapDetailImage != null) { ClearMiniMapDetail(); miniMapPicture.Invalidate(); } return; }
      if (miniMapDetailPending && (DateTime.UtcNow - miniMapDetailLastRequest).TotalSeconds < 10) return;
      Rectangle dst = MiniMapImageRectangle();
      if (dst.IsEmpty || miniMapMaxX <= miniMapMinX || miniMapMaxZ <= miniMapMinZ) return;
      MiniMapVisibleWorldBounds(out float visMinX, out float visMaxX, out float visMinZ, out float visMaxZ);
      float visW = visMaxX - visMinX, visH = visMaxZ - visMinZ;
      if (!(visW > .0001f) || !(visH > .0001f)) return;

      // Pixels of picture per world unit: what the screen wants, and what the main picture has.
      float wanted = dst.Width / visW;
      float baseHas = miniMapImage.Width / (miniMapMaxX - miniMapMinX);
      if (baseHas >= wanted * 1.1f) {
        if (miniMapDetailImage != null) { ClearMiniMapDetail(); miniMapPicture.Invalidate(); }
        return;
      }
      bool covers = miniMapDetailImage != null && visMinX >= miniMapDetailMinX && visMaxX <= miniMapDetailMaxX &&
        visMinZ >= miniMapDetailMinZ && visMaxZ <= miniMapDetailMaxZ;
      // Zoomed in further since it was taken: it covers the view, but no longer sharply.
      bool sharp = covers && miniMapZoom <= miniMapDetailZoom * 1.2f;
      if (sharp) return;
      if ((DateTime.UtcNow - miniMapDetailLastRequest).TotalSeconds < MiniMapDetailMinSeconds) { TouchMiniMapDetail(); return; }

      float marginX = visW * MiniMapDetailMargin, marginZ = visH * MiniMapDetailMargin;
      miniMapDetailPending = true;
      miniMapDetailLastRequest = DateTime.UtcNow;
      miniMapDetailZoom = miniMapZoom;
      if (!panelRender.RequestMiniMapDetail(visMinX - marginX, visMaxX + marginX, visMinZ - marginZ, visMaxZ + marginZ, miniMapDetailToken))
        miniMapDetailPending = false;
    }

    // Posted by the renderer thread. A bitmap of null means the capture did not work out.
    internal void PostMiniMapDetailSnapshot(Bitmap bitmap, float minX, float maxX, float minZ, float maxZ, int token) {
      if (_closing || IsDisposed || Disposing) { bitmap?.Dispose(); return; }
      if (InvokeRequired) {
        try { BeginInvoke(new Action<Bitmap, float, float, float, float, int>(PostMiniMapDetailSnapshot), bitmap, minX, maxX, minZ, maxZ, token); }
        catch { bitmap?.Dispose(); }
        return;
      }
      // A picture asked for before the page changed (or the main picture was replaced) belongs to a map that is gone.
      if (token != miniMapDetailToken || miniMapImage == null || bitmap == null) { bitmap?.Dispose(); miniMapDetailPending = false; return; }
      miniMapDetailImage?.Dispose();
      miniMapDetailImage = bitmap;
      miniMapDetailMinX = minX; miniMapDetailMaxX = maxX; miniMapDetailMinZ = minZ; miniMapDetailMaxZ = maxZ;
      miniMapDetailPending = false;
      miniMapPicture?.Invalidate();
    }

    private void DrawMiniMapDetailImage(Graphics graphics, Rectangle destination) {
      if (miniMapDetailImage == null || graphics == null || destination.IsEmpty) return;
      MiniMapVisibleWorldBounds(out float visMinX, out float visMaxX, out float visMinZ, out float visMaxZ);
      float visW = visMaxX - visMinX, visH = visMaxZ - visMinZ;
      if (!(visW > .0001f) || !(visH > .0001f)) return;
      var target = new RectangleF(
        destination.Left + (miniMapDetailMinX - visMinX) / visW * destination.Width,
        destination.Top + (miniMapDetailMinZ - visMinZ) / visH * destination.Height,
        (miniMapDetailMaxX - miniMapDetailMinX) / visW * destination.Width,
        (miniMapDetailMaxZ - miniMapDetailMinZ) / visH * destination.Height);
      if (!(target.Width > 0f) || !(target.Height > 0f) || !target.IntersectsWith(destination)) return;
      GraphicsState state = graphics.Save();
      try {
        graphics.SetClip(destination);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.Half;
        graphics.DrawImage(miniMapDetailImage, target, new RectangleF(0f, 0f, miniMapDetailImage.Width, miniMapDetailImage.Height), GraphicsUnit.Pixel);
      } finally { graphics.Restore(state); }
    }
  }
}
