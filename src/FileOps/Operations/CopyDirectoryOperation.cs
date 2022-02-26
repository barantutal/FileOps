using System.IO;
using FileOps.Exceptions;
using FileOps.Helpers;

namespace FileOps.Operations;

public class CopyDirectoryOperation : IFileOpsTransaction
{
    private readonly string _sourcePath;
    private readonly string _destinationPath;

    public CopyDirectoryOperation(string sourcePath, string destinationPath)
    {
        _sourcePath = sourcePath;
        _destinationPath = destinationPath;
    }
    
    public void Commit()
    {
        if (!Directory.Exists(_sourcePath))
        {
            throw FileOperationException.MissingSourcePathException(_sourcePath);
        }
        
        if (Directory.Exists(_destinationPath))
        {
            throw FileOperationException.DestinationPathExistsException(_destinationPath);
        }
        
        DirectoryHelper.CopyDirectory(_sourcePath, _destinationPath);
    }

    public void RollBack()
    {
        Directory.Delete(_destinationPath, true);
    }
}