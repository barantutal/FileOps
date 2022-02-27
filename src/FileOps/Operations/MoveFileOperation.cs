using System.IO;
using FileOps.Abstraction;
using FileOps.Exceptions;

namespace FileOps.Operations;

public class MoveFileOperation : IFileOps
{
    private readonly string _sourcePath;
    private readonly string _destinationPath;

    public MoveFileOperation(string sourcePath, string destinationPath) 
    {
        _sourcePath = sourcePath;
        _destinationPath = destinationPath;
    }
    
    public virtual void Commit()
    {
        if (!File.Exists(_sourcePath))
        {
            throw FileOperationException.MissingSourcePathException(_sourcePath);
        }
        
        if (File.Exists(_destinationPath))
        {
            throw FileOperationException.DestinationPathExistsException(_destinationPath);
        }
        
        File.Move(_sourcePath, _destinationPath);
    }
}