using System.IO;
using System.Threading.Tasks;

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
    
    public static async Task CopyDirectoryAsync(string source, string destination)
    {
        foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(source, destination));
        }

        foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
        {
            await FileHelper.CopyFileAsync(source, newPath.Replace(source, destination));
        }
    }
}