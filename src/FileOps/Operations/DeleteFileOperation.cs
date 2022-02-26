using System;
using System.Collections.Generic;
using System.IO;
using FileOps.Backup;

namespace FileOps.Operations;

public sealed class DeleteFileOperation : IFileOpsTransaction, IDisposable
{
    private readonly string _tempPath;
    private readonly string _fullPath;
    private string? _backupPath;
    private bool _disposed;
    
    public DeleteFileOperation(string fullPath, string tempPath)
    {
        _fullPath = fullPath;
        _tempPath = tempPath;
    }
    
    public void Commit()
    {
        if (File.Exists(_fullPath))
        {
            _backupPath = Path.Combine(_tempPath, Guid.NewGuid() + Path.GetExtension(_fullPath));
            File.Copy(_fullPath, _backupPath);
            File.Delete(_fullPath);
        }
    }

    public void RollBack()
    {
        if (_backupPath != null)
        {
            File.Copy(_backupPath, _fullPath);
        }
    }
    
    ~DeleteFileOperation()
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
            if (_backupPath != null)
            {
                backupFolders.Add(_backupPath);
            }
            
            ClearBackupHelper.Execute(backupFolders);
        }
    }
}