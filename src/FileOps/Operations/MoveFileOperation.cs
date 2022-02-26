using System;
using System.IO;
using FileOps.Exceptions;

namespace FileOps.Operations;

public class MoveFileOperation : IFileOpsTransaction
{
    private readonly string _sourcePath;
    private readonly string _destinationPath;
    private readonly string _tempPath;
    private string _backupPath;
    
    public MoveFileOperation(string sourcePath, string destinationPath, string tempPath) 
    {
        _sourcePath = sourcePath;
        _destinationPath = destinationPath;
        _tempPath = tempPath;
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
        
        var backupPath = Path.Combine(_tempPath, Guid.NewGuid() + Path.GetExtension(_sourcePath));
        File.Copy(_sourcePath, backupPath);
        _backupPath = backupPath;
        File.Move(_sourcePath, _destinationPath);
    }

    public void RollBack()
    {
        File.Delete(_destinationPath);
        if (!File.Exists(_sourcePath) && _backupPath != null)
        {
            File.Move(_backupPath, _sourcePath);
        }
    }
}