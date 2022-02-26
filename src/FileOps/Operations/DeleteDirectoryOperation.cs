using System;
using System.Collections.Generic;
using System.IO;
using FileOps.Backup;
using FileOps.Exceptions;
using FileOps.Helpers;

namespace FileOps.Operations;

public class DeleteDirectoryOperation : IFileOpsTransaction, IDisposable
{
    private readonly string _tempPath;
    private readonly string _sourcePath;
    private string _sourceBackupPath;
    private bool _disposed;
    
    public DeleteDirectoryOperation(string sourcePath, string tempPath)
    {
        _sourcePath = sourcePath;
        _tempPath = tempPath;
    }
    
    public void Commit()
    {
        if (!Directory.Exists(_sourcePath))
        {
            throw FileOperationException.MissingSourcePathException(_sourcePath);
        }
        
        var sourceBackupFile = Path.Combine(_tempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(_sourceBackupPath);
        DirectoryHelper.CopyDirectory(_sourcePath, _sourceBackupPath);
        _sourceBackupPath = sourceBackupFile;
        Directory.Delete(_sourcePath, true);
    }

    public void RollBack()
    {
        if (!Directory.Exists(_sourcePath))
        {
            DirectoryHelper.CopyDirectory(_sourceBackupPath, _sourcePath);
        }
    }
    
    ~DeleteDirectoryOperation()
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