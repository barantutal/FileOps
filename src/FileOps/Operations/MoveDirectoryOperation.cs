using System;
using System.Collections.Generic;
using System.IO;
using FileOps.Backup;
using FileOps.Helpers;

namespace FileOps.Operations;

public sealed class MoveDirectoryOperation : IFileOpsTransaction, IDisposable
{
    private readonly string _tempPath;
    private readonly string _sourcePath;
    private readonly string _destinationPath;
    private string _sourceBackupPath;
    private string _destinationBackupPath;
    private bool _disposed;
    private bool _cancelled;
    
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
            _cancelled = true;
            return;
        }
        
        if (_sourcePath.StartsWith(_destinationPath) || _destinationPath.StartsWith(_sourcePath))
        {
            _cancelled = true;
            return;
        }
        
        if (Directory.Exists(_destinationPath))
        {
            var destinationBackupPath = Path.Combine(_tempPath, Guid.NewGuid().ToString());
            Directory.Move(_destinationPath, destinationBackupPath);
            _destinationBackupPath = destinationBackupPath;
        }
        
        var sourceBackupFile = Path.Combine(_tempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(_sourceBackupPath);
        DirectoryHelper.CopyDirectory(_sourcePath, _sourceBackupPath);
        _sourceBackupPath = sourceBackupFile;
        Directory.Move(_sourcePath, _destinationPath);
    }

    public void RollBack()
    {
        if (_cancelled) return;
        
        if (_sourceBackupPath != null && !Directory.Exists(_sourcePath))
        {
            Directory.Move(_sourceBackupPath, _sourcePath);
        }

        if (_destinationBackupPath != null)
        {
            Directory.Delete(_destinationPath, true);
            Directory.Move(_destinationBackupPath, _destinationPath);
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
        if (_destinationBackupPath != null)
        {
            backupFolders.Add(_destinationBackupPath);
        }
            
        ClearBackupHelper.Execute(backupFolders);
    }
}