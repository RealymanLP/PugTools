using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace TorArchive {
  public static class Extensions {

    #region Fields
    private static readonly Dictionary<Int32, String> globalIdToFqnMap = new();

    #endregion Fields

    #region Methods
    public static Boolean AsBool(this XAttribute el, Boolean defaultValue = false) {
      if (el == null) {
        return defaultValue;
      }

      String val = el.Value;

      if (string.IsNullOrEmpty(val)) {
        return defaultValue;
      }

      if (!bool.TryParse(val, out Boolean result)) {
        return defaultValue;
      }

      return result;
    }
    public static Boolean AsBool(this XElement el, Boolean defaultValue = false) {
      if (el == null) {
        return defaultValue;
      }

      String val = el.Value;

      if (string.IsNullOrEmpty(val)) {
        return defaultValue;
      }

      if (val.ToUpper() == "TRUE") {
        return true;
      }

      if (val.ToUpper() == "FALSE") {
        return false;
      }

      return defaultValue;
    }
    public static Single AsDuration(this XElement el) {
      if (el == null) {
        return 0;
      }

      String durString = (String)el;

      if (string.IsNullOrEmpty(durString)) {
        return 0;
      }

      String[] parts = durString.Split(':');
      Single mult = 1;
      Single result = 0;

      foreach (String part in parts.Reverse()) {
        result += mult * float.Parse(part);
        mult *= 60;
      }

      return result;
    }
    public static Single AsFloat(this XElement el, Single defaultValue = 0f) {
      if (el == null) {
        return defaultValue;
      }

      String val = el.Value;

      if (string.IsNullOrEmpty(val)) {
        return defaultValue;
      }

      if (!float.TryParse(val, out Single result)) {
        return defaultValue;
      }

      return result;
    }
    public static Int32 AsId(this XAttribute el,
                             String fqn,
                             Dictionary<Int32, String> idToFqnMap = null) {

      if (el == null) {
        throw new ArgumentNullException(nameof(el));
      }

      if (idToFqnMap == null) {
        throw new ArgumentNullException(nameof(idToFqnMap));
      }

      String val = el.Value;

      if (string.IsNullOrEmpty(val)) {
        throw new ArgumentException("Attribute has no content!", nameof(el));
      }

      if (!ulong.TryParse(val, out UInt64 result)) {
        throw new ArgumentException("Attribute is not an integer!", nameof(el));
      }

      if ((result & 0xffffffff) != 0) {
        throw new InvalidOperationException("GUID has some of lower 32 bits set for " + fqn);
      }

      Int32 id = (Int32)(result >> 32);

      if (globalIdToFqnMap.ContainsKey(id)) {
        throw new InvalidOperationException($"Duplicate ID for {fqn} and {globalIdToFqnMap[id]}");
      } else {
        globalIdToFqnMap.Add(id, fqn);
      }

      return id;
    }
    public static Int32 AsInt(this XElement el, Int32 defaultValue = 0) {
      if (el == null) {
        return defaultValue;
      }

      String val = el.Value;

      if (string.IsNullOrEmpty(val)) {
        return defaultValue;
      }

      if (!int.TryParse(val, out Int32 result)) {
        return defaultValue;
      }

      return result;
    }
    public static Int32 AsInt(this XAttribute el, Int32 defaultValue = 0) {
      if (el == null) {
        return defaultValue;
      }

      String val = el.Value;

      if (string.IsNullOrEmpty(val)) {
        return defaultValue;
      }

      if (!int.TryParse(val, out Int32 result)) {
        return defaultValue;
      }

      return result;
    }
    public static Int64 AsLong(this XAttribute el, Int64 defaultValue = 0) {
      if (el == null) {
        return defaultValue;
      }

      String val = el.Value;

      if (string.IsNullOrEmpty(val)) {
        return defaultValue;
      }

      if (!long.TryParse(val, out Int64 result)) {
        return defaultValue;
      }

      return result;
    }
    public static Int64 AsLong(this XElement el, Int64 defaultValue = 0) {
      if (el == null) {
        return defaultValue;
      }

      String val = el.Value;

      if (string.IsNullOrEmpty(val)) {
        return defaultValue;
      }

      if (!long.TryParse(val, out Int64 result)) {
        return defaultValue;
      }

      return result;
    }
    public static Int32? AsNullableInt(this XElement el) {
      if (el == null) {
        return null;
      }

      String val = el.Value;

      if (string.IsNullOrEmpty(val)) {
        return null;
      }

      if (!int.TryParse(val, out Int32 result)) {
        return null;
      }

      return result;
    }
    public static void CopyTo(this Stream source, Stream target) {
      const Int32 bufSize = 0x1000;

      Byte[] buf = new Byte[bufSize];
      Int64 totalBytes = 0;
      Int32 bytesRead;

      while ((bytesRead = source.Read(buf, 0, bufSize)) > 0) {
        target.Write(buf, 0, bytesRead);
        totalBytes += bytesRead;
      }
    }
    public static void CopyTo(this Stream input, Stream output, Byte[] buffer) {
      Int32 len;

      while ((len = input.Read(buffer, 0, buffer.Length)) > 0) {
        output.Write(buffer, 0, len);
      }
    }
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action) {
      if (action == null) {
        throw new ArgumentNullException(nameof(action));
      }

      foreach (T item in source) {
        action(item);
      }

      return source;
    }
    public static Int32 ParseDuration(this String str) {
      if (string.IsNullOrEmpty(str)) {
        return 0;
      }

      String[] vals = str.Split(':');
      Int32 mult = 1;
      Int32 seconds = 0;

      for (Int32 i = vals.Length - 1; i >= 0; i--) {
        seconds += mult * int.Parse(vals[i]);
        mult *= 60;
      }

      return seconds;
    }

    #endregion Methods
  }
}
