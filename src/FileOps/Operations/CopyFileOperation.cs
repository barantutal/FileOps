using System;
using System.Collections.Generic;
using System.IO;
using FileOps.Backup;

namespace FileOps.Operations;

public sealed class CopyFileOperation : IFileOpsTransaction, IDisposable
{
    private readonly string _tempPath;
    private readonly string _sourcePath;
    private readonly string _destinationPath;
    private string _destinationBackupPath;
    private bool _disposed;

    public CopyFileOperation(string sourcePath, string destinationPath, string tempPath) 
    {
        _sourcePath = sourcePath;
        _destinationPath = destinationPath;
        _tempPath = tempPath;
    }
    
    public void Commit()
    {
        if (File.Exists(_destinationPath))
        {
            var backupPath = Path.Combine(_tempPath, Guid.NewGuid() + Path.GetExtension(_destinationPath));
            File.Move(_destinationPath, backupPath);
            _destinationBackupPath = backupPath;
        }
        
        File.Copy(_sourcePath, _destinationPath);
    }

    public void RollBack()
    {
        if (_destinationBackupPath != null)
        {
            File.Delete(_destinationPath);
            File.Copy(_destinationBackupPath, _destinationPath);
        }
    }
    
    ~CopyFileOperation()
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
        if (!_disposed)
        {
            _disposed = true;
            
            var backupFolders = new List<string>();
            if (_destinationBackupPath != null)
            {
                backupFolders.Add(_destinationBackupPath);
            }
            
            ClearBackupHelper.Execute(backupFolders);
        }
    }
}