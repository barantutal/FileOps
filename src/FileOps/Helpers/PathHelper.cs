using System;
using System.IO;

namespace FileOps.Helpers;

public static class PathHelper
{
    /// <summary>
    /// True when <paramref name="path"/> lies below <paramref name="root"/>. Compared per path
    /// segment: a plain string prefix took ".../E5234" for a child of ".../E5", so renaming a folder
    /// to a name that starts with its old name was refused as a move into itself.
    /// </summary>
    public static bool IsInside(string path, string root)
    {
        var normalisedPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        var normalisedRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));

        return normalisedPath.StartsWith(
            normalisedRoot + Path.DirectorySeparatorChar,
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }
}
