using System;
using System.Collections.Generic;
using System.IO;

namespace PugTools {
  internal class Format_PRT {
    private readonly String _dest;
    private readonly List<String> _errors;
    private readonly String _extension;

    internal HashSet<String> FileNames { get; set; }

    internal Format_PRT(String dest, String ext) {
      _dest = dest;
      _errors = new List<String>();
      _extension = ext;
      FileNames = new HashSet<String>();
    }
    internal void ParsePRT(Stream fileStream, String _) {
      StreamReader reader = new StreamReader(fileStream);
      String line;

      while ((line = reader.ReadLine()) != null) {
        String test = line.Replace("  .", "");
        test = test.Replace("Name=", "");
        test = test.Replace("EmitSpec=", "");
        test = test.Replace("Trail", "");
        test = test.Replace("Texture_Purple", "");
        test = test.Replace("Texture_Blue", "");
        test = test.Replace("Texture_Red", "");
        test = test.Replace("Texture_Green", "");
        test = test.Replace("Texture_White", "");
        test = test.Replace("Texture_Yellow", "");
        test = test.Replace("Texture_Orange", "");
        test = test.Replace("Texture", "/");
        test = test.Replace("GrannyFile", "");
        test = test.Replace("EmitFXSpec=", "");
        test = test.Replace("EmitAtDeathSpec=", "");
        test = test.Replace("=", "");
        test = test.Replace("\\", "/");
        test = test.Replace("//", "/");
        test = test.ToLower();

        if (test.Contains(".prt")) {
          if (!test.Contains("/art/fx/particles/"))
            FileNames.Add("/resources/art/fx/particles/" + test);
          else
            FileNames.Add("/resources" + test);
        } else if (test.Contains(".dds")) {
          FileNames.Add("/resources" + test);
          FileNames.Add("/resources" + test.Replace(".dds", ".tiny.dds"));
          FileNames.Add("/resources" + test.Replace(".dds", ".tex"));
        } else if (test.Contains(".fxspec")) {
          FileNames.Add("/resources" + test);
        } else if (test.Contains(".gr2")) {
          FileNames.Add("/resources" + test);
        }
      }
    }
    internal void WriteFile() {
      if (!Directory.Exists(_dest + "\\File_Names"))
        Directory.CreateDirectory(_dest + "\\File_Names");

      if (FileNames.Count > 0) {
        StreamWriter outputNames =
          new StreamWriter(_dest + "\\File_Names\\" + _extension + "_file_names.txt", false);

        foreach (String file in FileNames) {
          outputNames.Write(file.Replace("\\", "/") + "\r\n");
        }

        outputNames.Close();
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
