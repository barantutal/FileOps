using System.IO;
using System.Threading.Tasks;

namespace FileOps.Helpers;

public static class FileHelper
{
    public static async Task CopyFileAsync(string source, string destination)
    {
        var fileInfo = new FileInfo(source);
        await using var reader = new FileStream(source, FileMode.Open, FileAccess.Read);
        await using var writer = new FileStream(destination, FileMode.Create, FileAccess.ReadWrite);
        await reader.CopyToAsync(writer);
        File.SetLastWriteTime(destination, fileInfo.LastWriteTime);
    }
}