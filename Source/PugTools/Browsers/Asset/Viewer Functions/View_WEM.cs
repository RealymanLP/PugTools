using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NAudio.Vorbis;
using NAudio.Wave;

namespace PugTools {
  internal class ViewWEM {
    private readonly UInt32 _id;
    internal Byte[] Data { get; set; }
    internal Int64 Length { get; set; }
    internal String WemName { get; set; }
    internal Int64 Offset { get; set; }
    internal String OggName { get; set; }
    internal WaveStream Vorbis { get; set; }

    private readonly ProcessStartInfo ww2ogg = new ProcessStartInfo {
      CreateNoWindow = true,
      FileName = @".\Tools\ww2ogg.exe",
      RedirectStandardOutput = true,
      UseShellExecute = false,
      WindowStyle = ProcessWindowStyle.Hidden
    };

    private readonly ProcessStartInfo revorb = new ProcessStartInfo {
      CreateNoWindow = true,
      FileName = @".\Tools\revorb.exe",
      RedirectStandardOutput = true,
      UseShellExecute = false,
      WindowStyle = ProcessWindowStyle.Hidden
    };

    internal ViewWEM(BinaryReader br) {
      _id = br.ReadUInt32();
      Offset = br.ReadUInt32();
      Length = br.ReadUInt32();
      WemName = _id.ToString() + ".wem";
      OggName = _id.ToString() + ".ogg";
    }

    internal ViewWEM(String name = null, Stream inputStream = null) {
      if (name != null) {
        WemName = name.EndsWith(".wem", StringComparison.OrdinalIgnoreCase) ? name : name + ".wem";
        OggName = Path.ChangeExtension(WemName, ".ogg");
      }

      if (inputStream != null) {
        if (inputStream.CanSeek) inputStream.Position = 0;

        using MemoryStream copy = new MemoryStream();
        inputStream.CopyTo(copy);
        Data = copy.ToArray();
      } else {
        Data = Array.Empty<Byte>();
      }
    }

    internal async Task<Boolean> ConvertWEM() {
      try {
        if (Data == null || Data.Length == 0)
          throw new InvalidDataException("WEM file is empty.");

        string tempDirectory = Path.Combine(Environment.CurrentDirectory, "Temp");
        Directory.CreateDirectory(tempDirectory);

        string wemPath = Path.Combine(tempDirectory, WemName);
        string oggPath = Path.Combine(tempDirectory, OggName);

        await File.WriteAllBytesAsync(wemPath, Data);

        ProcessStartInfo ww2 = new ProcessStartInfo {
          CreateNoWindow = true,
          FileName = ww2ogg.FileName,
          Arguments = $"\"{wemPath}\" --pcb \"{Path.Combine(Environment.CurrentDirectory, "Tools", "packed_codebooks_aoTuV_603.bin")}\"",
          RedirectStandardOutput = true,
          UseShellExecute = false,
          WindowStyle = ProcessWindowStyle.Hidden
        };

        using (Process convertWEM = Process.Start(ww2))
        {
          if (convertWEM == null) throw new InvalidOperationException("Could not start ww2ogg.exe.");
          await convertWEM.WaitForExitAsync();
          if (convertWEM.ExitCode != 0)
            throw new InvalidOperationException("ww2ogg failed: " + convertWEM.StandardOutput.ReadToEnd());
        }

        ProcessStartInfo rev = new ProcessStartInfo {
          CreateNoWindow = true,
          FileName = revorb.FileName,
          Arguments = $"\"{oggPath}\"",
          RedirectStandardOutput = true,
          UseShellExecute = false,
          WindowStyle = ProcessWindowStyle.Hidden
        };

        using (Process convertOGG = Process.Start(rev))
        {
          if (convertOGG == null) throw new InvalidOperationException("Could not start revorb.exe.");
          await convertOGG.WaitForExitAsync();
          if (convertOGG.ExitCode != 0)
            throw new InvalidOperationException("revorb failed: " + convertOGG.StandardOutput.ReadToEnd());
        }

        byte[] oggData = await File.ReadAllBytesAsync(oggPath);
        Vorbis?.Dispose();
        Vorbis = new VorbisWaveReader(new MemoryStream(oggData), true);
        return true;
      }
      catch (Exception ex)
      {
        Debug.WriteLine("WEM conversion failed: " + ex);
        Vorbis = null;
        return false;
      }
      finally
      {
        try {
          if (!String.IsNullOrEmpty(WemName))
            File.Delete(Path.Combine(Environment.CurrentDirectory, "Temp", WemName));
          if (!String.IsNullOrEmpty(OggName))
            File.Delete(Path.Combine(Environment.CurrentDirectory, "Temp", OggName));
        } catch { }
      }
    }
  }
}
