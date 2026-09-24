using RWCustom;
using System.Text;
using UnityEngine;

namespace AbsoluteFriends;

internal static class Storage
{
    internal static string RootPath => Path.Combine(Custom.RootFolderDirectory(), "absolutefriends");

    internal static void Write(string path, Action<Stream> write)
    {
        string root = NormalizePath(RootPath);
        string destination = Path.GetFullPath(path);
        StringComparison comparison = Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        if (!destination.StartsWith(root + Path.DirectorySeparatorChar, comparison))
        {
            throw new InvalidOperationException("The storage path is outside the Absolute Friends folder");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destination));

        string temporaryPath = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";

        try
        {
            using (FileStream stream = File.Create(temporaryPath))
            {
                write(stream);
                stream.Flush(true);
            }

            if (File.Exists(destination))
            {
                File.Replace(temporaryPath, destination, null);
            }
            else
            {
                File.Move(temporaryPath, destination);
            }
        }
        catch
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

            throw;
        }
    }

    internal static void WriteText(string path, string text)
    {
        Write(path, stream =>
        {
            using StreamWriter writer = new(stream, new UTF8Encoding(false), 1024, true);

            writer.Write(text);
        });
    }

    internal static void Show(string path)
    {
        string destination = Path.GetFullPath(path);
        string folder = NormalizePath(RootPath);
        bool exists = File.Exists(destination);

        Directory.CreateDirectory(RootPath);

        (string command, string arguments) = Application.platform switch
        {
            RuntimePlatform.OSXPlayer or RuntimePlatform.OSXEditor => ("open", exists ? $"-R \"{destination}\"" : $"\"{folder}\""),
            RuntimePlatform.LinuxPlayer or RuntimePlatform.LinuxEditor => ("xdg-open", $"\"{folder}\""),
            _ => ("explorer.exe", exists ? $"/select,\"{destination}\"" : $"\"{folder}\"")
        };

        System.Diagnostics.Process.Start(command, arguments);
    }

    internal static string NormalizePath(string path) => Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
