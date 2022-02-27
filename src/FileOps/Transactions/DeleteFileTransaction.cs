using System;
using System.Collections.Generic;
using System.IO;
using FileOps.Abstraction;
using FileOps.Backup;
using FileOps.Operations;

namespace FileOps.Transactions;

public class DeleteFileTransaction : DeleteFileOperation, IFileOpsTransaction, IDisposable
{
    private readonly string _tempPath;
    private readonly string _path;
    private string? _backupPath;
    private bool _disposed;
    
    public DeleteFileTransaction(string path, string tempPath) : base(path)
    {
        _path = path;
        _tempPath = tempPath;
    }
    
    public override void Commit()
    {
        if (!File.Exists(_path)) return;
        
        _backupPath = Path.Combine(_tempPath, Guid.NewGuid() + Path.GetExtension(_path));
        File.Copy(_path, _backupPath);
        
        base.Commit();
    }

    public void RollBack()
    {
        if (_backupPath != null)
        {
            File.Copy(_backupPath, _path);
        }
    }
    
    ~DeleteFileTransaction()
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