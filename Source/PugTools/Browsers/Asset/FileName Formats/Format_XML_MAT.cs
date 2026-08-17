using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PugTools {
  internal class Format_XML_MAT {
    private readonly String _dest;
    private readonly List<String> _errors;
    private readonly String _extension;

    internal HashSet<String> AnimNames { get; set; }
    internal HashSet<String> FileNames { get; set; }

    internal Format_XML_MAT(String dest, String ext) {
      _dest = dest;
      _errors = new List<String>();
      _extension = ext;
      AnimNames = new HashSet<String>();
      FileNames = new HashSet<String>();
    }
    private static List<String> AssetReader(XElement childnode) {
      List<String> fileList = new List<String>();

      if (childnode.Element("BaseFile").Value != null) {
        String basefile = childnode.Element("BaseFile").Value;
        Boolean hasBodyTypes = false;
        Boolean bodyTypeT = childnode.Element("BodyTypes") != null;
        Boolean bodyTypet = childnode.Element("Bodytypes") != null;

        if (bodyTypeT) hasBodyTypes = childnode.Element("BodyTypes").HasElements;
        if (bodyTypet) hasBodyTypes = childnode.Element("Bodytypes").HasElements;

        if (hasBodyTypes) {
          IEnumerable<String> bodyTypeList;

          if (bodyTypeT) {
            bodyTypeList = from c in childnode.Element("BodyTypes").Elements()
                           select c.Value;
          } else {
            bodyTypeList = from c in childnode.Element("Bodytypes").Elements()
                           select c.Value;
          }

          if (childnode.Element("BaseFile").Value != "") {
            if (basefile.Contains("[bt]") && hasBodyTypes)
              // Checking if we need to create file names for each bodytype.
              fileList.AddRange(BodyType(bodyTypeList, basefile));
            else {
              if (basefile.Contains("[gen]"))
                // Checking for gender specific file names
                fileList.AddRange(Genderize(basefile));
              else
                fileList.Add("/resources" + basefile);
            }
          }

          IEnumerable<XElement> materials = childnode.Element("Materials").Elements();

          if (materials != null) { // Check for material file names.
            foreach (XElement material in materials) {
              String filename = material.Attribute("filename").Value;

              if (filename.Contains("[bt]") && hasBodyTypes)
                // Checking if we need to create file names for each bodytype.
                fileList.AddRange(BodyType(bodyTypeList, filename));
              else {
                if (filename.Contains("[gen]"))
                  // Checking for gender specific file names
                  fileList.AddRange(Genderize(filename));
                else
                  fileList.Add("/resources" + filename);
              }

              IEnumerable<XElement> matoverrides =
                material.Element("MaterialOverrides").Elements();

              if (matoverrides != null) {
                foreach (XElement over in matoverrides) {
                  String override_filename = over.Attribute("filename").Value;

                  if (override_filename.Contains("[bt]") && hasBodyTypes)
                    // Checking if we need to create file names for each bodytype.
                    fileList.AddRange(BodyType(bodyTypeList, override_filename));
                  else {
                    if (override_filename.Contains("[gen]"))
                      // Checking for gender specific file names
                      fileList.AddRange(Genderize(override_filename));
                    else
                      fileList.Add("/resources" + override_filename);
                  }
                }
              }
            }
          }

          IEnumerable<XElement> attachments = childnode.Element("Attachments").Elements();

          if (attachments != null) { // Check for attachment model file names.
            foreach (XElement attachment in attachments) {
              String filename = attachment.Attribute("filename").Value;

              if (filename.Contains("[bt]"))
                // Checking if we need to create file names for each bodytype.
                fileList.AddRange(BodyType(bodyTypeList, filename));
              else {
                if (filename.Contains("[gen]"))
                  // Checking for gender specific file names
                  fileList.AddRange(Genderize(filename));
                else
                  fileList.Add("/resources" + filename);
              }
            }
          }
        } else {
          if (childnode.Element("BaseFile").Value != "") {
            if (basefile.Contains("[gen]"))
              // Checking for gender specific file names
              fileList.AddRange(Genderize(basefile));
            else
              fileList.Add("/resources" + basefile);
          }

          IEnumerable<XElement> materials = childnode.Element("Materials").Elements();
          if (materials != null) { // Check for material file names.
            foreach (XElement material in materials) {
              String filename = material.Attribute("filename").Value;

              if (filename.Contains("[gen]"))
                // Checking for gender specific file names
                fileList.AddRange(Genderize(filename));
              else
                fileList.Add("/resources" + filename);
            }
          }

          IEnumerable<XElement> attachments = childnode.Element("Attachments").Elements();
          if (attachments != null) { // Check for attachment model file names.
            foreach (XElement attachment in attachments) {
              String filename = attachment.Attribute("filename").Value;

              if (filename.Contains("[gen]"))
                // Checking for gender specific file names
                fileList.AddRange(Genderize(filename));
              else
                fileList.Add("/resources" + filename);
            }
          }
        }
      }

      return fileList;
    }
    private static List<String> BodyType(IEnumerable<String> bodyTypeList, String filename) {
      List<String> fileList = new List<String>();

      foreach (String bodytype in bodyTypeList) {
        String bodyTypeFileName = filename.Replace("[bt]", bodytype);
        fileList.Add("/resources" + bodyTypeFileName);
      }

      return fileList;
    }
    private static List<String> Genderize(String filename) {
      List<String> fileList = new List<String>();
      // Disable "u" to reduce noise in output for analysis, should be turned back on for file 
      // name searching
      List<String> genders = new List<String> { "m", "f", "u" };

      foreach (String gender in genders) {
        String genderFileName = filename.Replace("[gen]", gender);
        fileList.Add("/resources" + genderFileName);
      }

      return fileList;
    }
    private void NodeChecker(XElement node) {
      if (node.HasElements) {
        foreach (XElement childnode in node.Elements()) {
          if (childnode.Name == "input" && childnode.Element("type") != null) {
            // New way of searching for texture file names
            String type = childnode.Element("type").Value;

            if (type == "texture") {
              String textureName = childnode.Element("value").Value;

              if (textureName != null && textureName != "") {
                String scrubbedName =
                  textureName.Replace("////",  "//").Replace("\\art",  "art").Replace(
                    " #", "").Replace("#", "").Replace("+",  "/").Replace(" ", "_");
                FileNames.Add("\\resources\\" + scrubbedName + ".dds");
                FileNames.Add("\\resources\\" + scrubbedName + ".tex");
                FileNames.Add("\\resources\\" + scrubbedName + ".tiny.dds");
                String[] fileName = scrubbedName.Split('\\');
                Int32 startPosition = 0;

                if (scrubbedName.Contains('\\'))
                  startPosition = scrubbedName.LastIndexOf('\\') + 1;

                Int32 length = scrubbedName.Length - startPosition;
                List<Object> tagsToRemove = new List<Object> { "_d", "_n", "_s" };

                if (tagsToRemove.Any(name => scrubbedName.EndsWith(name.ToString())))
                  length -= 2;

                String primaryName = scrubbedName.Substring(startPosition, length);
                FileNames.Add("\\resources\\art\\shaders\\materials\\" + primaryName + ".mat");
              }
            }
            /*
            // Catch types for analysis. Caught the following types: Boolean, uvscale, float,
            // rgba, vector4
            else {
              System.IO.StreamWriter file3 = 
                new System.IO.StreamWriter("c:\\swtor\\types.txt", true);
              file3.WriteLine(type);
              file3.Close();
            }
            */
          }

          IEnumerable<XElement> fxSpecList = childnode.Elements("fxSpecString");
          if (childnode.Name == "AppearanceAction" && fxSpecList.Any()) { //
            foreach (XElement fxSpec in fxSpecList) {
              String fxSpecName = "\\resources\\art\\fx\\fxspec\\" + fxSpec.Value;

              if (!fxSpec.Value.ToLower().EndsWith(".fxspec"))
                fxSpecName += ".fxspec";

              FileNames.Add(fxSpecName);
            }
          }

          if (childnode.Name == "Asset") {
            List<String> assetFilenames = AssetReader(childnode);

            foreach (var name in assetFilenames) {
              String scrubbedName =
                name.Replace("////", "//").Replace(" #", "").Replace("#", "").Replace(
                  "+", "/").Replace(" ", "_");
              FileNames.Add(scrubbedName);
            }
          } else {
            NodeChecker(childnode);
          }
        }
      }
    }
    internal void ParseXML(Stream fileStream, String fullFileName, String baseFolder = null) {
      String fileName = fullFileName[(fullFileName.LastIndexOf('\\') + 1)..];
      String directory = fullFileName.Substring(0, fullFileName.LastIndexOf('/'));

      try {
        if (fileName.Contains("am_")) {
          XDocument doc = XDocument.Load(fileStream);
          String temp = fileName.Split('/').Last();
          String fileNameNoExtension = temp[3..temp.IndexOf('.') ];
          String fullDirectory = "";

          if (baseFolder != null)
            fullDirectory = String.Format("/resources/{0}", baseFolder);
          else
            fullDirectory = directory + '/' + fileNameNoExtension + '/';

          XElement aamElement = doc.Element("aam");

          if (aamElement == null) return;

          XElement actionElement = aamElement.Element("actions");

          if (actionElement != null) {
            IEnumerable<XElement> actionList = actionElement.Elements("action");

            foreach (XElement action in actionList) {
              String actionName = action.Attribute("name").Value;

              if (action.Attribute("actionProvider") != null) {
                String actionProvider = action.Attribute("actionProvider").Value + ".mph";

                if (fullDirectory.Contains("/humanoid/humanoid/")) {
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bfanew/") + actionProvider);
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bfanew/") + actionProvider + ".amx");
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bfbnew/") + actionProvider);
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bfnnew/") + actionProvider);
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bfnnew/") + actionProvider + ".amx");
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bfsnew/") + actionProvider);
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bfsnew/") + actionProvider + ".amx");

                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bmanew/") + actionProvider);
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bmanew/") + actionProvider + ".amx");
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bmfnew/") + actionProvider);
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bmfnew/") + actionProvider + ".amx");
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bmnnew/") + actionProvider);
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bmnnew/") + actionProvider + ".amx");
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bmsnew/") + actionProvider);
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bmsnew/") + actionProvider + ".amx");
                } else {
                  AnimNames.Add(fullDirectory + actionProvider);
                  AnimNames.Add(fullDirectory + actionProvider + ".amx");
                }
              }

              if (action.Attribute("animName") != null) {
                String animationName = action.Attribute("animName").Value;

                if (actionName != animationName) {
                  animationName += ".jba";

                  if (fullDirectory.Contains("/humanoid/humanoid/")) {
                    AnimNames.Add(fullDirectory.Replace(
                      "/humanoid/humanoid/", "/humanoid/bfanew/") + animationName);
                    AnimNames.Add(fullDirectory.Replace(
                      "/humanoid/humanoid/", "/humanoid/bfbnew/") + animationName);
                    AnimNames.Add(fullDirectory.Replace(
                      "/humanoid/humanoid/", "/humanoid/bfnnew/") + animationName);
                    AnimNames.Add(fullDirectory.Replace(
                      "/humanoid/humanoid/", "/humanoid/bfsnew/") + animationName);

                    AnimNames.Add(fullDirectory.Replace(
                      "/humanoid/humanoid/", "/humanoid/bmanew/") + animationName);
                    AnimNames.Add(fullDirectory.Replace(
                      "/humanoid/humanoid/", "/humanoid/bmfnew/") + animationName);
                    AnimNames.Add(fullDirectory.Replace(
                      "/humanoid/humanoid/", "/humanoid/bmnnew/") + animationName);
                    AnimNames.Add(fullDirectory.Replace(
                      "/humanoid/humanoid/", "/humanoid/bmsnew/") + animationName);
                  } else {
                    AnimNames.Add(fullDirectory + animationName);
                  }
                }
              }
              actionName += ".jba";
              if (fullDirectory.Contains("/humanoid/humanoid/")) {
                AnimNames.Add(fullDirectory.Replace(
                  "/humanoid/humanoid/", "/humanoid/bfanew/") + actionName);
                AnimNames.Add(fullDirectory.Replace(
                  "/humanoid/humanoid/", "/humanoid/bfbnew/") + actionName);
                AnimNames.Add(fullDirectory.Replace(
                  "/humanoid/humanoid/", "/humanoid/bfnnew/") + actionName);
                AnimNames.Add(fullDirectory.Replace(
                  "/humanoid/humanoid/", "/humanoid/bfsnew/") + actionName);

                AnimNames.Add(fullDirectory.Replace(
                  "/humanoid/humanoid/", "/humanoid/bmanew/") + actionName);
                AnimNames.Add(fullDirectory.Replace(
                  "/humanoid/humanoid/", "/humanoid/bmfnew/") + actionName);
                AnimNames.Add(fullDirectory.Replace(
                  "/humanoid/humanoid/", "/humanoid/bmnnew/") + actionName);
                AnimNames.Add(fullDirectory.Replace(
                  "/humanoid/humanoid/", "/humanoid/bmsnew/") + actionName);
              } else
                AnimNames.Add(fullDirectory + actionName);
            }
          }

          XElement networkElem = aamElement.Element("networks");

          if (networkElem != null) {
            IEnumerable<XElement> networkList = networkElem.Descendants("literal");

            foreach (XElement network in networkList) {
              String fqnName = network.Attribute("fqn").Value;

              if (fqnName != null) {
                if (fullDirectory.Contains("/humanoid/humanoid/")) {
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bfanew/") + fqnName);
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bfanew/") + fqnName + ".amx");
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bfbnew/") + fqnName);
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bfbnew/") + fqnName + ".amx");
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bfnnew/") + fqnName);
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bfnnew/") + fqnName + ".amx");
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bfsnew/") + fqnName);
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bfsnew/") + fqnName + ".amx");

                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bmanew/") + fqnName);
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bmanew/") + fqnName + ".amx");
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bmfnew/") + fqnName);
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bmfnew/") + fqnName + ".amx");
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bmnnew/") + fqnName);
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bmnnew/") + fqnName + ".amx");
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bmsnew/") + fqnName);
                  AnimNames.Add(fullDirectory.Replace(
                    "/humanoid/humanoid/", "/humanoid/bmsnew/") + fqnName + ".amx");
                } else {
                  AnimNames.Add(fullDirectory + fqnName);
                  AnimNames.Add(fullDirectory + fqnName + ".amx");
                }
              }
            }
          }
        } else {
          XDocument doc = new XDocument();

          try {
            doc = XDocument.Load(fileStream);
          }
          catch (Exception) { // ex) {
            // System.Diagnostics.Debug.WriteLine(ex.Message);
          }

          foreach (XElement node in doc.Elements()) {
            NodeChecker(node);
          }
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

      if (AnimNames.Count > 0) {
        StreamWriter outputAnimNames =
          new StreamWriter(_dest + "\\File_Names\\" + _extension + "_anim_file_names.txt", false);

        foreach (String file in AnimNames) {
          outputAnimNames.Write(file.Replace("\\", "/") + "\r\n");
        }

        outputAnimNames.Close();
        AnimNames.Clear();
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
