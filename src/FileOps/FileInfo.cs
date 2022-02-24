using System;

namespace FileOps;

public class FileInfo : IFileInfo
{
    public Guid Id { get; set; }
    public string Name { get; }
    public string Path { get; }
    public DateTime ModifiedDate { get; }
    public DateTime CreatedDate { get; }
    public long Size { get; }
    
    public FileInfo(string name, string path, DateTime modifiedDate, DateTime createDate, long size)
    {
        Id = Guid.NewGuid();
        Name = name;
        Path = path;
        ModifiedDate = modifiedDate;
        CreatedDate = createDate;
        Size = size;
    }
}