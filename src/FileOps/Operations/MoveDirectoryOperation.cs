using System;
using System.Collections.Generic;
using System.IO;
using FileOps.Backup;
using FileOps.Exceptions;
using FileOps.Helpers;

namespace FileOps.Operations;

public sealed class MoveDirectoryOperation : IFileOpsTransaction, IDisposable
{
    private readonly string _tempPath;
    private readonly string _sourcePath;
    private readonly string _destinationPath;
    private string _sourceBackupPath;
    private bool _disposed;
    
    public MoveDirectoryOperation(string sourcePath, string destinationPath, string tempPath)
    {
        _sourcePath = sourcePath;
        _destinationPath = destinationPath;
        _tempPath = tempPath;
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

        if (_sourcePath.StartsWith(_destinationPath))
        {
            throw new FileOperationException($"Cannot move child path {_sourcePath} to parent path {_destinationPath}.");
        }
        
        if (_destinationPath.StartsWith(_sourcePath))
        {
            throw new FileOperationException($"Cannot move parent path {_sourcePath} to child path {_destinationPath}.");
        }

        var sourceBackupFile = Path.Combine(_tempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(_sourceBackupPath);
        DirectoryHelper.CopyDirectory(_sourcePath, _sourceBackupPath);
        _sourceBackupPath = sourceBackupFile;
        Directory.Move(_sourcePath, _destinationPath);
    }

    public void RollBack()
    {
        Directory.Delete(_destinationPath, true);
        if (_sourceBackupPath != null)
        {
            Directory.Move(_sourceBackupPath, _sourcePath);
        }
    }

    ~MoveDirectoryOperation()
    {
        ClearBackups();
    }

    public void Dispose()
    {
        ClearBackups();
        GC.SuppressFinalize(this);
    }

    private void ClearBackups()
    {
        if (_disposed) return;
        
        _disposed = true;
            
        var backupFolders = new List<string>() { _sourceBackupPath };
        ClearBackupHelper.Execute(backupFolders);
    }
}