using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PugTools {
  /// <summary>
  /// Extracts resource names from HeroEngine MAG and DYC text specifications.
  /// Both formats are used by every known SWTOR generation.  The values are
  /// candidates only; AssetBrowser validates their complete hash pair before a
  /// name enters the dictionary.
  /// </summary>
  internal sealed class Format_TEXTSPECS {
    private readonly String _destination;
    private readonly String _extension;
    private readonly List<String> _errors = new List<String>();

    internal HashSet<String> FileNames { get; } =
      new HashSet<String>(StringComparer.OrdinalIgnoreCase);

    internal Format_TEXTSPECS(String destination, String extension) {
      _destination = destination;
      _extension = extension;
    }

    internal void Parse(Stream input, String fullFileName) {
      if (input == null) return;
      try {
        String extension = Path.GetExtension(fullFileName ?? String.Empty);
        ViewTextSpecs.TextSpecInfo spec = extension.Equals(".dyc", StringComparison.OrdinalIgnoreCase)
          ? ViewTextSpecs.ParseDyc(input)
          : ViewTextSpecs.ParseMag(input);

        foreach (String gr2 in spec.GrannyReferences) AddResource(gr2, false);
        foreach (ViewTextSpecs.Parameter parameter in spec.Parameters)
          AddParameter(parameter.Key, parameter.Value);

        // Older MAG variants leave keys unknown but still retain full resource
        // paths in comments/values.  Harvest only explicit extensions, never
        // bare words, so malformed beta text cannot cause candidate explosions.
        foreach (Match match in Regex.Matches(spec.FullText ?? String.Empty,
                 @"(?i)(?:/)?(?:resources/)?[a-z0-9_./\\-]+\.(?:gr2|mat|dds|jba|mph|amx|fxspec|dyc|mag|dat|xml)"))
          AddResource(match.Value, false);
      }
      catch (Exception ex) {
        _errors.Add((fullFileName ?? String.Empty) + ": " + ex.GetType().Name + ": " + ex.Message);
      }
    }

    private void AddParameter(String key, String value) {
      if (String.IsNullOrWhiteSpace(key) || String.IsNullOrWhiteSpace(value)) return;
      String name = key.Trim();
      Int32 dot = name.LastIndexOf('.');
      if (dot >= 0) name = name.Substring(dot + 1);
      String normalized = value.Trim().TrimEnd('<').Replace('\\', '/');

      switch (name.ToLowerInvariant()) {
        case "model":
        case "skeleton":
          AddResource("/resources/art/dynamic/spec/" + normalized, false);
          break;
        case "mesh":
        case "animlibraryfqn":
        case "animsharemetadatafqn":
        case "animmetadatafqn":
        case "animnetworkfolder":
          foreach (String part in normalized.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) {
            String candidate = part.Trim();
            if (name.Equals("AnimNetworkFolder", StringComparison.OrdinalIgnoreCase)) {
              candidate = candidate.Trim('/');
              if (candidate.Length > 0) AddResource("/resources/" + candidate + "/mags.mph", false);
            } else AddResource(candidate, true);
          }
          break;
        case "material":
          foreach (String material in normalized.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries))
            AddResource("/resources/art/shaders/materials/" + material + ".mat", false);
          break;
      }
    }

    private void AddResource(String value, Boolean resourceRelative) {
      if (String.IsNullOrWhiteSpace(value)) return;
      String path = value.Trim().TrimEnd('<').Replace('\\', '/');
      while (path.Contains("//")) path = path.Replace("//", "/");
      if (path.StartsWith("resources/", StringComparison.OrdinalIgnoreCase)) path = "/" + path;
      if (!path.StartsWith("/", StringComparison.Ordinal)) {
        path = resourceRelative ? "/resources/" + path.TrimStart('/') : "/resources/" + path;
      }
      FileNames.Add(path.ToLowerInvariant());
    }

    internal void WriteFile() {
      String directory = Path.Combine(_destination, "File_Names");
      Directory.CreateDirectory(directory);
      if (FileNames.Count > 0)
        System.IO.File.WriteAllLines(Path.Combine(directory, _extension + "_file_names.txt"),
                                     FileNames.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
      if (_errors.Count > 0)
        System.IO.File.WriteAllLines(Path.Combine(directory, _extension + "_error_list.txt"), _errors);
    }
  }
}
