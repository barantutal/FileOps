namespace FileOps;

public class FileOpsOptions
{
    public string? RootPath { get; set; }
    
    public FileOpsOptions(string rootPath)
    {
        RootPath = rootPath;
    }
}