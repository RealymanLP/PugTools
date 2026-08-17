using System;
using System.Collections.Generic;
using System.IO;

namespace PugTools {
  internal class Format_SDEF {
    private readonly String _dest;
    private readonly List<String> _errors;
    private readonly String _extension;
    private readonly HashSet<String> _fileNames;

    internal Int32 Found { get; set; }

    internal Format_SDEF(String dest, String ext) {
      _dest = dest;
      _errors = new List<string>();
      _extension = ext;
      _fileNames = new HashSet<string>();
    }
    internal void ParseSDEF(Stream fileStream) {
      using BinaryReader br = new BinaryReader(fileStream);
      UInt32 header = FileFormats.FileHelpers.ReverseBytes(br.ReadUInt32());

      if (header.ToString("X") != "53444546") return;
      else {
        //read unknown 1 version info??
        br.ReadBytes(4);

        //C9 indicates 2 byte integer
        br.ReadByte();

        //Read 2 byte integer                
        UInt16 count = FileFormats.FileHelpers.ReverseBytes(br.ReadUInt16());

        for (Int32 c = 0; c < count; c++) {
          //CF Idenitifes 8 byte integer
          br.ReadByte();

          //Read the 8 byte integer                    
          UInt64 id = FileFormats.FileHelpers.ReverseBytes(br.ReadUInt64());

          //null seperator
          br.ReadByte();

          //CB identifies a 4 byte integer -- CA identifies a 3 byte integer                    
          Byte cb = br.ReadByte();

          if (cb == 203) {
            //Read the 4 byte integer
            br.ReadBytes(4);
          } else if (cb == 202) {
            //Read the 3 byte integer
            br.ReadBytes(3);
          }

          //null seperator
          br.ReadByte();

          _fileNames.Add("/resources/systemgenerated/compilednative/" + id);
        }
      }

      return;
    }
    internal void WriteFile() {
      if (!Directory.Exists(_dest + "\\File_Names"))
        Directory.CreateDirectory(_dest + "\\File_Names");

      Found = _fileNames.Count;

      if (_fileNames.Count > 0) {
        StreamWriter outputNames =
          new StreamWriter(_dest + "\\File_Names\\" + _extension + "_file_names.txt", false);

        foreach (String file in _fileNames) {
          outputNames.WriteLine(file.Replace("\\", "/"));
        }

        outputNames.Close();
        _fileNames.Clear();
      }

      if (_errors.Count > 0) {
        StreamWriter outputErrors =
          new StreamWriter(_dest + "\\File_Names\\" + _extension + "_error_list.txt", false);

        foreach (String error in _errors) {
          outputErrors.WriteLine(error);
        }

        outputErrors.Close();
        _errors.Clear();
      }
    }
  }
}
