using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace PugTools {
  internal class Format_FXSPEC {
    private readonly String _dest;
    private readonly List<String> _errors;
    private readonly String _extension;
    private readonly HashSet<String> _resourceFileNames;

    internal HashSet<String> FileNames { get; set; }

    internal Format_FXSPEC(String dest, String ext) {
      _dest = dest;
      _errors = new List<String>();
      _extension = ext;
      _resourceFileNames = new HashSet<String>();
      FileNames = new HashSet<String>();
    }
    internal void ParseFXSPEC(Stream fileStream, String fullFileName) {
      _ = fullFileName[(fullFileName.LastIndexOf('\\') + 1)..];
      _ = fullFileName.Substring(0, fullFileName.LastIndexOf('/'));

      try {
        XmlDocument doc = new XmlDocument();

        doc.Load(fileStream);

        XmlNodeList fileElemList = doc.SelectNodes("//node()[@name='displayName']");

        foreach (XmlNode node in fileElemList) {
          String resource = node.InnerText;
          FileNames.Add(resource + ".fxspec");
        }

        XmlNodeList resourceElemList = doc.SelectNodes("//node()[@name='_fxResourceName']");

        foreach (XmlNode node in resourceElemList) {
          String resource = node.InnerText;

          if (resource.Contains(".prt")) {
            String output =
              "/resources/art/fx/particles/" + resource.Replace('\\', '/').ToLower();
            output = output.Replace("//", "/");
            output =
              output.Replace(
                "/resources/art/fx/particles/art/fx/particles/",
                "/resources/art/fx/particles/"
              );
            _resourceFileNames.Add(output);

          } else if (resource.Contains(".gr2")) {
            String output = "/resources/" + resource.Replace('\\', '/').ToLower();
            output = output.Replace("//", "/");
            _resourceFileNames.Add(output);

          } else if (resource.Contains(".lit")
                     || resource.Contains(".ext")
                     || resource.Contains(".zzp")) {

            String output = "/resources/" + resource.Replace('\\', '/').ToLower();
            output = output.Replace("//", "/");
            _resourceFileNames.Add(output);

          } else if (resource.Contains("Play_")
                     || resource.Contains("play_")
                     || resource.Contains("Stop_")
                     || resource.Contains("stop_")
                     || resource == ""
                     || resource.Contains(".sgt")
                     || resource.Contains(".wav")) {

            continue;
          }
        }

        XmlNodeList projTexElemList = doc.SelectNodes("//node()[@name='_fxProjectionTexture']");

        foreach (XmlNode node in projTexElemList) {
          String resource =
            node.InnerText.Replace(".tiny.dds", "").Replace(".dds", "").Replace(".tex", "");
          String output = "/resources" + resource.Replace('\\', '/').ToLower();
          _resourceFileNames.Add(output + ".dds");
          _resourceFileNames.Add(output + ".tiny.dds");
          _resourceFileNames.Add(output + ".tex");
        }

        XmlNodeList projTex1ElemList =
          doc.SelectNodes("//node()[@name='_fxProjectionTexture_layer1']");

        foreach (XmlNode node in projTex1ElemList) {
          String resource =
            node.InnerText.Replace(".tiny.dds", "").Replace(".dds", "").Replace(".tex", "");
          String output = "/resources" + resource.Replace('\\', '/').ToLower();
          _resourceFileNames.Add(output + ".dds");
          _resourceFileNames.Add(output + ".tiny.dds");
          _resourceFileNames.Add(output + ".tex");
        }

        XmlNodeList texNameElemList = doc.SelectNodes("//node()[@name='_fxTextureName']");

        foreach (XmlNode node in texNameElemList) {
          String resource =
            node.InnerText.Replace(".tiny.dds", "").Replace(".dds", "").Replace(".tex", "");
          String output = "/resources" + resource.Replace('\\', '/').ToLower();
          _resourceFileNames.Add(output + ".dds");
          _resourceFileNames.Add(output + ".tiny.dds");
          _resourceFileNames.Add(output + ".tex");
        }
      }
      catch (Exception ex) {
        _errors.Add("File: " + fullFileName);
        _errors.Add(ex.Message + ":");
        _errors.Add(ex.StackTrace);
        _errors.Add("");
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

      if (_resourceFileNames.Count > 0) {
        StreamWriter outputNames =
          new StreamWriter(
            _dest + "\\File_Names\\" + _extension + "_resource_file_names.txt", false
          );

        foreach (String file in _resourceFileNames) {
          outputNames.Write(file.Replace("\\", "/") + "\r\n");
        }

        outputNames.Close();
        _resourceFileNames.Clear();
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
