using System;

namespace TorArchive {
  /// <summary>
  /// Struct containing the results of a hash calculation
  /// </summary>
  public struct FileId {

    #region Methods
    public UInt64 AsUInt64() {
      // UInt64 result = Ph;
      // result = (result << 32) | Sh;
      // return result;

      return ((UInt64)Ph << 32) | Sh;
    }
    public static FileId FromFilePath(String filePath, UInt32 seed = 0xDEADBEEF) {
      UInt32 eax, ecx, edx, ebx, esi, edi;

      String s = filePath.ToLower();

      eax = 0; //ecx = edx = ebx = esi = edi = 0;
      ebx = edi = esi = (UInt32)s.Length + seed;

      Int32 i;

      for (i = 0; i + 12 < s.Length; i += 12) {
        edi = (UInt32)((s[i + 7] << 24) | (s[i + 6] << 16) | (s[i + 5] << 8) | s[i + 4]) + edi;
        esi = (UInt32)((s[i + 11] << 24) | (s[i + 10] << 16) | (s[i + 9] << 8) | s[i + 8]) + esi;
        edx = (UInt32)((s[i + 3] << 24) | (s[i + 2] << 16) | (s[i + 1] << 8) | s[i]) - esi;

        edx = (edx + ebx) ^ (esi >> 28) ^ (esi << 4);
        esi += edi;
        edi = (edi - edx) ^ (edx >> 26) ^ (edx << 6);
        edx += esi;
        esi = (esi - edi) ^ (edi >> 24) ^ (edi << 8);
        edi += edx;
        ebx = (edx - esi) ^ (esi >> 16) ^ (esi << 16);
        esi += edi;
        edi = (edi - ebx) ^ (ebx >> 13) ^ (ebx << 19);
        ebx += esi;
        esi = (esi - edi) ^ (edi >> 28) ^ (edi << 4);
        edi += ebx;
      }

      if (s.Length - i > 0) {
        switch (s.Length - i) {
          case 12:
            esi += (UInt32)s[i + 11] << 24;
            goto case 11;
          case 11:
            esi += (UInt32)s[i + 10] << 16;
            goto case 10;
          case 10:
            esi += (UInt32)s[i + 9] << 8;
            goto case 9;
          case 9:
            esi += s[i + 8];
            goto case 8;
          case 8:
            edi += (UInt32)s[i + 7] << 24;
            goto case 7;
          case 7:
            edi += (UInt32)s[i + 6] << 16;
            goto case 6;
          case 6:
            edi += (UInt32)s[i + 5] << 8;
            goto case 5;
          case 5:
            edi += s[i + 4];
            goto case 4;
          case 4:
            ebx += (UInt32)s[i + 3] << 24;
            goto case 3;
          case 3:
            ebx += (UInt32)s[i + 2] << 16;
            goto case 2;
          case 2:
            ebx += (UInt32)s[i + 1] << 8;
            goto case 1;
          case 1:
            ebx += s[i];
            break;
        }

        esi = (esi ^ edi) - ((edi >> 18) ^ (edi << 14));
        ecx = (esi ^ ebx) - ((esi >> 21) ^ (esi << 11));
        edi = (edi ^ ecx) - ((ecx >> 7) ^ (ecx << 25));
        esi = (esi ^ edi) - ((edi >> 16) ^ (edi << 16));
        edx = (esi ^ ecx) - ((esi >> 28) ^ (esi << 4));
        edi = (edi ^ edx) - ((edx >> 18) ^ (edx << 14));
        eax = (esi ^ edi) - ((edi >> 8) ^ (edi << 24));

        // ph = edi;
        // sh = eax;
        return new FileId() { Ph = edi, Sh = eax };
      }
      // ph = esi;
      // sh = eax;
      return new FileId() { Ph = esi, Sh = eax };
    }
    public override String ToString() {
      return AsUInt64().ToString();
    }

    #endregion Methods

    #region Properties
    public UInt32 Ph { get; internal set; }
    public UInt32 Sh { get; internal set; }

    #endregion Properties
  }
}
