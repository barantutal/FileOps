using System.IO;
using FileOps.Abstraction;
using FileOps.Exceptions;

namespace FileOps.Operations;

public class DeleteDirectoryOperation : IFileOps
{
    private readonly string _sourcePath;
    
    public DeleteDirectoryOperation(string sourcePath)
    {
        _sourcePath = sourcePath;
    }
    
    public virtual void Commit()
    {
        if (!Directory.Exists(_sourcePath))
        {
            throw FileOperationException.MissingSourcePathException(_sourcePath);
        }
        
        Directory.Delete(_sourcePath, true);
    }
}