using System;
using System.Collections.Generic;
using System.IO;
using FileOps.Backup;

namespace FileOps.Operations;

public sealed class CopyFileOperation : IFileOpsTransaction, IDisposable
{
    private readonly string _tempPath;
    private readonly string _fullPath;
    private readonly string _pathToCopy;
    private string? _backupPath;
    private bool _disposed;

    public CopyFileOperation(string fullPath, string pathToCopy, string tempPath) 
    {
        _fullPath = fullPath;
        _pathToCopy = pathToCopy;
        _tempPath = tempPath;
    }
    
    public void Commit()
    {
        if (File.Exists(_pathToCopy))
        {
            _backupPath = Path.Combine(_tempPath, Guid.NewGuid() + Path.GetExtension(_pathToCopy));
            File.Copy(_pathToCopy, _backupPath);
            File.Delete(_pathToCopy);
        }
        
        File.Copy(_fullPath, _pathToCopy);
    }

    public void RollBack()
    {
        File.Delete(_pathToCopy);

        if (_backupPath != null)
        {
            File.Copy(_backupPath, _pathToCopy);
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
            if (_backupPath != null)
            {
                backupFolders.Add(_backupPath);
            }
            
            ClearBackupHelper.Execute(backupFolders);
        }
    }
}