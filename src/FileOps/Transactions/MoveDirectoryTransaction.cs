using System;
using System.Collections.Generic;
using System.IO;
using FileOps.Abstraction;
using FileOps.Backup;
using FileOps.Helpers;
using FileOps.Operations;

namespace FileOps.Transactions;

public class MoveDirectoryTransaction : MoveDirectoryOperation, IFileOpsTransaction, IDisposable
{
    private readonly string _tempPath;
    private readonly string _sourcePath;
    private readonly string _destinationPath;
    private string _sourceBackupPath;
    private bool _disposed;
    
    public MoveDirectoryTransaction(string sourcePath, string destinationPath, string tempPath) : base(sourcePath, destinationPath)
    {
        _sourcePath = sourcePath;
        _destinationPath = destinationPath;
        _tempPath = tempPath;
    }
    
    public override void Commit()
    {
        var sourceBackupFile = Path.Combine(_tempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(sourceBackupFile);
        DirectoryHelper.CopyDirectory(_sourcePath, sourceBackupFile);
        _sourceBackupPath = sourceBackupFile;
        
        base.Commit();
    }

    public void RollBack()
    {
        Directory.Delete(_destinationPath, true);
        if (_sourceBackupPath != null)
        {
            Directory.Move(_sourceBackupPath, _sourcePath);
        }
    }

    ~MoveDirectoryTransaction()
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