using System;

namespace PugTools {
  public static class ObjectExtensions {
    public static String NullSafeToString(this object obj) {
      return obj != null ? obj.ToString() : String.Empty;
    }
  }
}
