using System;
using System.IO;
using System.Text;

namespace PugTools {
  internal static class JBAInfo {
    internal static String Describe(Stream input, String fileName) {
      if (input == null) return "No input stream.";
      input.Position = 0;
      Byte[] data = new Byte[Math.Min(128, (Int32)Math.Max(0, input.Length))];
      Int32 read = 0;
      while (read < data.Length) {
        Int32 n = input.Read(data, read, data.Length - read);
        if (n <= 0) break;
        read += n;
      }
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("SWTOR JBA Animation");
      sb.AppendLine("===================");
      sb.AppendLine($"File: {fileName}");
      sb.AppendLine($"Size: {input.Length:N0} bytes");
      sb.AppendLine();
      sb.AppendLine("Raw header (first 128 bytes):");
      for (Int32 i = 0; i < read; i += 16) {
        Int32 count = Math.Min(16, read - i);
        sb.Append(i.ToString("X4")).Append("  ");
        for (Int32 j = 0; j < 16; j++) {
          sb.Append(j < count ? data[i + j].ToString("X2") : "  ");
          if (j != 15) sb.Append(' ');
        }
        sb.Append("  ");
        for (Int32 j = 0; j < count; j++) {
          Byte b = data[i + j];
          sb.Append(b >= 32 && b < 127 ? (Char)b : '.');
        }
        sb.AppendLine();
      }
      sb.AppendLine();
      sb.AppendLine("JBA Inspector – Phase 1");
      sb.AppendLine("The animation playback reader is deliberately not enabled yet.");
      sb.AppendLine("Current public SWTOR tooling documents JBA import as 32-bit-only;");
      sb.AppendLine("we will validate the exact JBA structure in the current assets before");
      sb.AppendLine("applying animation data to the 64-bit GR2 skeleton renderer.");
      return sb.ToString();
    }
  }
}
