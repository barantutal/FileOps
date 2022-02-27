using System;
using System.Collections.Generic;
using System.IO;
using FileOps.Abstraction;
using FileOps.Backup;
using FileOps.Operations;

namespace FileOps.Transactions;

public class MoveFileTransaction : MoveFileOperation, IFileOpsTransaction, IDisposable
{
    private readonly string _sourcePath;
    private readonly string _destinationPath;
    private readonly string _tempPath;
    private string _backupPath;
    private string _directoryPath;
    private bool _disposed;
    
    public MoveFileTransaction(string sourcePath, string destinationPath, string tempPath) : base(sourcePath, destinationPath)
    {
        _sourcePath = sourcePath;
        _destinationPath = destinationPath;
        _tempPath = tempPath;
    }
    
    public override void Commit()
    {
        var directoryPath = Path.GetDirectoryName(_destinationPath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            _directoryPath = directoryPath;
        }
        
        var backupPath = Path.Combine(_tempPath, Guid.NewGuid() + Path.GetExtension(_sourcePath));
        File.Copy(_sourcePath, backupPath);
        _backupPath = backupPath;
        
        base.Commit();
    }

    public void RollBack()
    {
        File.Delete(_destinationPath);
        if (!File.Exists(_sourcePath) && _backupPath != null)
        {
            File.Move(_backupPath, _sourcePath);
        }
        
        if (_directoryPath != null)
        {
            Directory.Delete(_directoryPath);
        }
    }
    
    ~MoveFileTransaction()
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