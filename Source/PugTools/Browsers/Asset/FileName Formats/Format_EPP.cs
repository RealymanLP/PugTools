using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using GomLib;

namespace PugTools {
  internal class Format_EPP {
    private readonly HashSet<String> _animNames;
    private readonly String _dest;
    private readonly List<String> _errors;
    private readonly String _extension;

    internal HashSet<String> FileNames { get; set; }

    internal Format_EPP(String dest, String ext) {
      _animNames = new HashSet<String>();
      _dest = dest;
      _errors = new List<String>();
      _extension = ext;
      FileNames = new HashSet<String>();
    }
    internal void ParseEPP(Stream fileStream, String fullFileName) {
      _ = fullFileName[(fullFileName.LastIndexOf('\\') + 1)..];
      _ = fullFileName.Substring(0, fullFileName.LastIndexOf('/'));

      try {
        XmlDocument doc = new XmlDocument();
        doc.Load(fileStream);
        XmlNode anode = doc.SelectSingleNode("Appearance");
        String file =
          "/resources/gamedata/"
          + anode.Attributes.GetNamedItem("fqn").InnerText.Replace('.', '/')
          + ".epp";
        FileNames.Add(file);
        XmlNodeList elemList = doc.GetElementsByTagName("fxSpecString");

        foreach (XmlNode node in elemList) {
          String fxspec = node.InnerText;
          fxspec = "/resources/art/fx/fxspec/" + fxspec + ".fxspec";
          FileNames.Add(fxspec);
        }

        elemList = doc.GetElementsByTagName("projectileFXString");

        foreach (XmlNode node in elemList) {
          String fxspec = node.InnerText;
          fxspec = "/resources/art/fx/fxspec/" + fxspec + ".fxspec";
          FileNames.Add(fxspec);
        }

        elemList = doc.GetElementsByTagName("casterAnim");

        foreach (XmlNode node in elemList) {
          String anim = node.InnerText;
          _animNames.Add(anim);
        }

        elemList = doc.GetElementsByTagName("targetAnim");

        foreach (XmlNode node in elemList) {
          String anim = node.InnerText;
          _animNames.Add(anim);
        }
      }
      catch (Exception ex) {
        _errors.Add("File: " + fullFileName);
        _errors.Add(ex.Message + ":");
        _errors.Add(ex.StackTrace);
        _errors.Add("");
      }
    }
    internal void ParseEPPNodes(List<GomObject> eppNodes) {
      foreach (GomObject obj in eppNodes) {
        String slash = obj.Name.ToLower().ToString().Replace('.', '/');
        String epp = "/resources/gamedata/" + slash + ".epp";
        FileNames.Add(epp);
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

      if (_animNames.Count > 0) {
        StreamWriter outputNames =
          new StreamWriter(_dest + "\\File_Names\\" + _extension + "_anim_file_names.txt", false);

        foreach (String file in _animNames) {
          outputNames.Write(file.Replace("\\", "/") + "\r\n");
        }

        outputNames.Close();
        _animNames.Clear();
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
