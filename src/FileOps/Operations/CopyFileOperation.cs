using System.IO;
using FileOps.Exceptions;

namespace FileOps.Operations;

public sealed class CopyFileOperation : IFileOpsTransaction
{
    private readonly string _sourcePath;
    private readonly string _destinationPath;
    private string _directoryPath;
    
    public CopyFileOperation(string sourcePath, string destinationPath) 
    {
        _sourcePath = sourcePath;
        _destinationPath = destinationPath;
    }
    
    public void Commit()
    {
        if (!File.Exists(_sourcePath))
        {
            throw FileOperationException.MissingSourcePathException(_sourcePath);
        }
        
        if (File.Exists(_destinationPath))
        {
            throw FileOperationException.DestinationPathExistsException(_destinationPath);
        }

        var directoryPath = Path.GetDirectoryName(_destinationPath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            _directoryPath = directoryPath;
        }
        
        File.Copy(_sourcePath, _destinationPath);
    }

    public void RollBack()
    {
        File.Delete(_destinationPath);
        
        if (_directoryPath != null)
        {
            Directory.Delete(_directoryPath);
        }
    }
}