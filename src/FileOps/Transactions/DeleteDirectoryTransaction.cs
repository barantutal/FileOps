using System;
using System.Collections.Generic;
using System.IO;
using FileOps.Abstraction;
using FileOps.Backup;
using FileOps.Helpers;
using FileOps.Operations;

namespace FileOps.Transactions;

public class DeleteDirectoryTransaction : DeleteDirectoryOperation, IFileOpsTransaction, IDisposable
{
    private readonly string _tempPath;
    private readonly string _sourcePath;
    private string _sourceBackupPath;
    private bool _disposed;
    
    public DeleteDirectoryTransaction(string sourcePath, string tempPath) : base(sourcePath)
    {
        _sourcePath = sourcePath;
        _tempPath = tempPath;
    }
    
    public override void Commit()
    {
        var sourceBackupFile = Path.Combine(_tempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(_sourceBackupPath);
        DirectoryHelper.CopyDirectory(_sourcePath, _sourceBackupPath);
        _sourceBackupPath = sourceBackupFile;

        base.Commit();
    }

    public void RollBack()
    {
        if (!Directory.Exists(_sourcePath))
        {
            DirectoryHelper.CopyDirectory(_sourceBackupPath, _sourcePath);
        }
    }
    
    ~DeleteDirectoryTransaction()
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