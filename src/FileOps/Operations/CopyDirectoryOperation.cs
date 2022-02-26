using System;
using System.Collections.Generic;
using System.IO;
using FileOps.Backup;
using FileOps.Helpers;

namespace FileOps.Operations;

public class CopyDirectoryOperation : IFileOpsTransaction, IDisposable
{
    private readonly string _sourcePath;
    private readonly string _destinationPath;
    private readonly string _tempPath;
    private string _destinationBackupPath;
    private bool _disposed;

    public CopyDirectoryOperation(string sourcePath, string destinationPath, string tempPath)
    {
        _sourcePath = sourcePath;
        _destinationPath = destinationPath;
        _tempPath = tempPath;
    }
    
    public void Commit()
    {
        if (!Directory.Exists(_sourcePath))
        {
            return;
        }
        
        if (Directory.Exists(_destinationPath))
        {
            var destinationBackupPath = Path.Combine(_tempPath, Guid.NewGuid().ToString());
            Directory.Move(_destinationPath, destinationBackupPath);
            _destinationBackupPath = destinationBackupPath;
        }
        
        DirectoryHelper.CopyDirectory(_sourcePath, _destinationPath);
    }

    public void RollBack()
    {
        if (_destinationBackupPath != null)
        {
            Directory.Delete(_destinationPath, true);
            Directory.Move(_destinationBackupPath, _destinationPath);
        }
    }

    ~CopyDirectoryOperation()
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
            
        var backupFolders = new List<string>(1);
        if (_destinationBackupPath != null)
        {
            backupFolders.Add(_destinationBackupPath);
        }
            
        ClearBackupHelper.Execute(backupFolders);
    }
}