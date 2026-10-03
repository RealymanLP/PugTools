using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FastTreeView = global::TreeViewFast.Controls.TreeViewFast;

namespace PugTools {
  /// <summary>
  /// Releases very large TreeViewFast hierarchies in small UI-thread slices.
  /// WinForms controls are thread-affine, so disposing a TreeView from the background
  /// cleanup worker is both unsafe and capable of saturating a CPU/memory bus for seconds.
  /// </summary>
  internal static class DeferredTreeCleanup {
    private static readonly Queue<FastTreeView> Queue = new Queue<FastTreeView>();
    private static Boolean _scheduled;

    internal static void Enqueue(FastTreeView tree) {
      if (tree == null || tree.IsDisposed) return;
      lock (Queue) {
        Queue.Enqueue(tree);
        if (_scheduled) return;
        _scheduled = true;
      }
      PostNext();
    }

    private static void PostNext() {
      Tools main = Application.OpenForms.OfType<Tools>()
        .FirstOrDefault(form => form != null && !form.IsDisposed && !form.Disposing);
      if (main == null) {
        lock (Queue) _scheduled = false;
        return;
      }

      try {
        main.BeginInvoke(new Action(ProcessSlice));
      } catch {
        lock (Queue) _scheduled = false;
      }
    }

    private static void ProcessSlice() {
      FastTreeView tree = null;
      lock (Queue) {
        if (Queue.Count > 0) tree = Queue.Peek();
        else {
          _scheduled = false;
          return;
        }
      }

      Boolean done = false;
      try {
        // Keep each UI turn short. The main window can repaint/process input between slices.
        tree.ReleaseNativeHandleForDeferredCleanup();
        done = true;
      } catch {
        done = true;
      }

      if (done) {
        lock (Queue) {
          if (Queue.Count > 0 && ReferenceEquals(Queue.Peek(), tree)) Queue.Dequeue();
        }
        // Do not call Dispose() here. The native handle is already gone; disposing a detached
        // control can synchronously walk the retained managed TreeNode graph again.
      }

      lock (Queue) {
        if (Queue.Count == 0) {
          _scheduled = false;
          return;
        }
      }

      // Yield to the normal WinForms message queue before the next small cleanup slice.
      PostNext();
    }
  }
}
