using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TorArchive {
  public class FileExtension {

    #region Constructors
    public FileExtension() {
      _fileTypes.Add("CWS", "swf");
      _fileTypes.Add("CFX", "gfx");
      _fileTypes.Add("PROT", "node");
      _fileTypes.Add("GAWB", "gr2");
      _fileTypes.Add("SCPT", "scpt");
      _fileTypes.Add("FACE", "fxe");
      _fileTypes.Add("PK", "zip");
      _fileTypes.Add("lua", "lua");
      _fileTypes.Add("DDS", "dds");
      _fileTypes.Add("XSM", "xsm");
      _fileTypes.Add("XAC", "xac");
      _fileTypes.Add("8BPS", "8bps");
      _fileTypes.Add("bdLF", "db");
      _fileTypes.Add("gsLF", "geom");
      _fileTypes.Add("idLF", "diffuse");
      _fileTypes.Add("psLF", "specular");
      _fileTypes.Add("amLF", "mask");
      _fileTypes.Add("ntLF", "tint");
      _fileTypes.Add("lgLF", "glow");
      _fileTypes.Add("Gamebry", "nif");
      _fileTypes.Add("WMPHOTO", "lmp");
      _fileTypes.Add("BKHD", "bnk");
      _fileTypes.Add("AMX", "amx");
      _fileTypes.Add("OLCB", "clo");
      _fileTypes.Add("PNG", "png");
      _fileTypes.Add("; Zo", "zone.txt");
      _fileTypes.Add("RIFF", "riff");
      _fileTypes.Add("WAVE", "wav");
      _fileTypes.Add("\0\0\0\0", "zero.txt");

      xml_types.Add("<Material>", "mat");
      xml_types.Add("<TextureObject", "tex");
      xml_types.Add("<manifest>", "manifest");
      xml_types.Add("<\0n\0o\0d\0e\0W\0C\0l\0a\0s\0s\0e\0s\0", "fxspec");
      xml_types.Add("<\0A\0p\0p\0e\0a\0r\0a\0n\0c\0e", "epp");
      xml_types.Add("<ClothData>", "clo");
      xml_types.Add("<v>", "not");
      xml_types.Add("<Rules>", "rul");
      xml_types.Add("<SurveyInstance>", "svy");
      xml_types.Add("<DataTable>", "tbl");
      xml_types.Add("<TextureObject xmlns", "tex");
      xml_types.Add("<EnvironmentMaterial", "emt");
    }

    #endregion Constructors

    #region Fields
    private readonly Dictionary<String, String> _fileTypes = new Dictionary<String, String>();
    private static readonly FileExtension _instance = new FileExtension();
    public Dictionary<String, String> xml_types = new Dictionary<String, String>();

    #endregion Fields

    #region Methods
    public String GuessExtension(File file) {
      Stream fs = file.Open();
      Byte[] bytes;
      try {
        Int32 size = (Int32)Math.Min(200U, file.FileInfo.UncompressedSize);
        bytes = new Byte[Math.Max(4, size)];
        Int32 total = 0;
        while (total < bytes.Length) {
          Int32 n = fs.Read(bytes, total, bytes.Length - total);
          if (n <= 0) break;
          total += n;
        }
        if (total < bytes.Length) Array.Resize(ref bytes, total);
      } finally {
        fs.Dispose();
      }

      if (bytes.Length < 4) return "unknown";

      if ((bytes[0] == 0x01) && (bytes[1] == 0x00) && (bytes[2] == 0x00)) {
        return "stb";
      }

      if ((bytes[0] == 0x02) && (bytes[1] == 0x00) && (bytes[2] == 0x00)) {
        return "mph";
      }

      if ((bytes[0] == 0x21) && (bytes[1] == 0x0d) && (bytes[2] == 0x0a) && (bytes[3] == 0x21)) {
        String str5 = Encoding.ASCII.GetString(bytes, 0, 64);

        if (str5.Contains("Particle Specification", StringComparison.CurrentCulture)) {
          return "prt";
        } else {
          return "dat";
        }
      }

      if ((bytes[0] == 0) && (bytes[1] == 1) && (bytes[2] == 0)) {
        return "ttf";
      }

      if ((bytes[0] == 10) && (bytes[1] == 5) && (bytes[2] == 1) && (bytes[3] == 8)) {
        return "pcx";
      }

      if ((bytes[0] == 0x38) && (bytes[1] == 0x03) && (bytes[2] == 0x00) && (bytes[3] == 0x00)) {
        return "spt";
      }

      if ((bytes[0] == 0x18) && (bytes[1] == 0x00) && (bytes[2] == 0x00) && (bytes[3] == 0x00)) {
        String strCheckDAT = Encoding.ASCII.GetString(bytes, 4, 22);

        if (strCheckDAT == "AREA_DAT_BINARY_FORMAT" || strCheckDAT == "ROOM_DAT_BINARY_FORMAT") {
          return "dat";
        }
      }

      String str = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
      String str2 = Encoding.ASCII.GetString(bytes, 0, 4);

      foreach (KeyValuePair<String, String> item in _fileTypes) {
        if (str2.Contains(item.Key, StringComparison.CurrentCulture)) {
          if (item.Key == "RIFF") {
            if (Encoding.ASCII.GetString(bytes, 8, 4)
                              .Contains("WAVE", StringComparison.CurrentCulture)) {
              return "wav";
            }
          } else if (item.Key == "lua") {
            if (str.IndexOf("lua") > 50) {
              continue;
            }
          } else if (item.Key == "\0\0\0\0") {
            if (bytes.Length > 0x0b && bytes[0x0b] == 0x41) {
              return "jba";
            }
          }

          return item.Value;
        }
      }

      if (str2.Contains("<", StringComparison.CurrentCulture)) {
        String str4 = Encoding.ASCII.GetString(bytes, 0, 64);

        foreach (KeyValuePair<String, String> item in xml_types) {
          if (str4.Contains(item.Key, StringComparison.CurrentCulture)) {
            return item.Value;
          }
        }

        return "xml";
      }

      String str6;

      if (bytes.Length < 128) {
        str6 = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
      } else {
        str6 = Encoding.ASCII.GetString(bytes, 0, 128);
      }

      if (str6.Contains("[SETTINGS]", StringComparison.CurrentCulture)
          && str6.Contains("gr2", StringComparison.CurrentCulture)) {
        return "dyc";
      }

      if (str.IndexOf("cnv_") >= 1 && str.IndexOf(".wem") >= 1) {
        return "acb";
      }

      Int32 length = str.Split(new Char[] { ',' }, 10).Length;

      if (length >= 10) {
        return "csv";
      } else {
        return "txt";
      }
    }

    #endregion Methods

    #region Properties
    public static FileExtension Instance => _instance;

    #endregion Properties
  }
}
