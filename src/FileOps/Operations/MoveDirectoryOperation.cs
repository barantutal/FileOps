using System.IO;
using FileOps.Abstraction;
using FileOps.Exceptions;

namespace FileOps.Operations;

public class MoveDirectoryOperation : IFileOps
{
    private readonly string _sourcePath;
    private readonly string _destinationPath;
    
    public MoveDirectoryOperation(string sourcePath, string destinationPath)
    {
        _sourcePath = sourcePath;
        _destinationPath = destinationPath;
    }
    
    public virtual void Commit()
    {
        if (!Directory.Exists(_sourcePath))
        {
            throw FileOperationException.MissingSourcePathException(_sourcePath);
        }
        
        if (Directory.Exists(_destinationPath))
        {
            throw FileOperationException.DestinationPathExistsException(_destinationPath);
        }

        if (_sourcePath.StartsWith(_destinationPath))
        {
            throw new FileOperationException($"Cannot move child path {_sourcePath} to parent path {_destinationPath}.");
        }
        
        if (_destinationPath.StartsWith(_sourcePath))
        {
            throw new FileOperationException($"Cannot move parent path {_sourcePath} to child path {_destinationPath}.");
        }

        Directory.Move(_sourcePath, _destinationPath);
    }
}