using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileOps.Helpers;

public static class DirectoryHelper
{
    // Destination paths are built from the path relative to the source. A plain string Replace of
    // the source prefix broke when the source name occurred again deeper in the tree ("data/data/a")
    // and when the source was given with a trailing separator.
    public static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);

        foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
        }

        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            File.Copy(file, Path.Combine(destination, Path.GetRelativePath(source, file)), true);
        }
    }

    public static async Task CopyDirectoryAsync(string source, string destination)
    {
        Directory.CreateDirectory(destination);

        foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
        }

        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            await FileHelper.CopyFileAsync(file, Path.Combine(destination, Path.GetRelativePath(source, file)));
        }
    }

    /// <summary>
    /// Creates the missing parent directories of <paramref name="filePath"/>. Returns the top-most
    /// directory that was created, or null when nothing had to be created. A bare file name has no
    /// directory part; it lives in the current directory, which exists.
    /// </summary>
    public static string EnsureParentDirectory(string filePath)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        var topMostMissing = TopMostMissingDirectory(directory);

        if (topMostMissing != null)
        {
            Directory.CreateDirectory(directory);
        }

        return topMostMissing;
    }

    /// <summary>
    /// The highest directory on the way to <paramref name="directory"/> that does not exist yet, or
    /// null when <paramref name="directory"/> exists. Creating a directory also creates its missing
    /// parents, and a rollback has to remove all of them, not only the leaf.
    /// </summary>
    public static string TopMostMissingDirectory(string directory)
    {
        if (string.IsNullOrEmpty(directory))
        {
            return null;
        }

        var current = Path.GetFullPath(directory);
        if (Directory.Exists(current))
        {
            return null;
        }

        var parent = Path.GetDirectoryName(current);
        while (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
        {
            current = parent;
            parent = Path.GetDirectoryName(current);
        }

        return current;
    }

    /// <summary>
    /// Removes <paramref name="directory"/> and its parents up to and including
    /// <paramref name="createdRoot"/>, as long as they are empty. A directory that received content
    /// from outside the transaction is kept: deleting it would destroy data the transaction never owned.
    /// </summary>
    public static void RemoveCreatedDirectories(string directory, string createdRoot)
    {
        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(createdRoot))
        {
            return;
        }

        var current = Path.GetFullPath(directory);
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(createdRoot));

        while (!string.IsNullOrEmpty(current))
        {
            if (Directory.Exists(current))
            {
                if (Directory.EnumerateFileSystemEntries(current).Any())
                {
                    return;
                }

                Directory.Delete(current);
            }

            if (string.Equals(Path.TrimEndingDirectorySeparator(current), root, StringComparison.Ordinal))
            {
                return;
            }

            current = Path.GetDirectoryName(current);
        }
    }
}
