using System.IO;
using FileOps.Abstraction;

namespace FileOps.Operations;

public class DeleteFileOperation : IFileOps
{
    private readonly string _path;

    public DeleteFileOperation(string path)
    {
        _path = path;
    }

    public virtual void Commit()
    {
        File.Delete(_path);
    }
}