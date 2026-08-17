using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PugTools {
  class Format_AMX {
    private readonly String _dest;
    private readonly List<String> _errors;
    private readonly String _extension;

    internal HashSet<String> FileNames { get; set; }

    internal Format_AMX(String dest, String ext) {
      _dest = dest;
      _errors = new List<String>();
      _extension = ext;
      FileNames = new HashSet<String>();
    }
    internal void ParseAMX(Stream fileStream, String fullFileName) {
      using BinaryReader br = new BinaryReader(fileStream);
      UInt64 header = br.ReadUInt32();

      if (header.ToString("X") != "20584D41") {
        _errors.Add("File: " + fullFileName);
        _errors.Add("Invalid header" + header.ToString());
        return;

      } else {
        br.ReadUInt16(); //unknown
        Boolean stop = false;

        do {
          Byte fileLen = br.ReadByte();

          if (fileLen == 0) {
            stop = true;

          } else {
            Byte[] fileNameBytes = br.ReadBytes(fileLen);
            String fileName = Encoding.ASCII.GetString(fileNameBytes);
            Byte dirLen = br.ReadByte();
            Byte[] dirNameBytes = br.ReadBytes(dirLen);
            String dirName = Encoding.ASCII.GetString(dirNameBytes);
            String fullName =
              ("/resources/anim/" + dirName.Replace('\\', '/') + "/" + fileName).Replace("//", "/");

            //humanoid\bfanew
            //em_wookiee_10

            FileNames.Add(fullName + ".jba");
            FileNames.Add(fullName + ".mph");
            FileNames.Add(fullName + ".mph.amx");

            br.ReadUInt32();
            Byte check = br.ReadByte();

            if (check != 2 && check != 3) stop = true;
          }
        } while (!stop);
      }
    }
    internal void WriteFile(Boolean _ = false) {
      if (!Directory.Exists(_dest + "\\File_Names"))
        Directory.CreateDirectory(_dest + "\\File_Names");

      if (FileNames.Count > 0) {
        StreamWriter outputFileNames =
          new StreamWriter(_dest + "\\File_Names\\" + _extension + "_file_names.txt", false);

        foreach (String file in FileNames) {
          outputFileNames.WriteLine(file);
        }

        outputFileNames.Close();
        FileNames.Clear();
      }

      if (_errors.Count > 0) {
        StreamWriter outputErrors =
          new StreamWriter(_dest + "\\File_Names\\" + _extension + "_error_list.txt", false);

        foreach (String error in _errors) {
          outputErrors.Write(error + "\r\n");
        }

        outputErrors.Close();
        _errors.Clear();
      }
    }
  }
}
