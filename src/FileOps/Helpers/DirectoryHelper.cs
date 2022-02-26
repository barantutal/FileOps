using System.IO;

namespace FileOps.Helpers;

public static class DirectoryHelper
{
    public static void CopyDirectory(string source, string destination)
    {
        foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(source, destination));
        }

        foreach (string newPath in Directory.GetFiles(source, "*.*",SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(source, destination), true);
        }
    }
    
    public static string GetRootPathToGenerate(string fullPath)
    {
        var rootPath = Path.GetFullPath(fullPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var parentPath = Path.GetDirectoryName(rootPath);
        while (parentPath != null && !Directory.Exists(parentPath))
        {
            rootPath = parentPath;
            parentPath = Path.GetDirectoryName(rootPath);
        }

        return rootPath;
    }
}