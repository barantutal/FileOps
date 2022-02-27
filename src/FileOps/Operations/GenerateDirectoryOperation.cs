using System.IO;
using FileOps.Abstraction;
using FileOps.Exceptions;

namespace FileOps.Operations;

public class GenerateDirectoryOperation : IFileOps
{
    private readonly string _path;
    
    public GenerateDirectoryOperation(string path)
    {
        _path = path;
    }
    
    public virtual void Commit()
    {
        if (Directory.Exists(_path))
        {
            throw new FileOperationException($"Directory already exists on path {_path}");
        }
        
        Directory.CreateDirectory(_path);
    }
}