using System;
using System.Collections.Generic;
using System.IO;
using FileOps.Backup;
using FileOps.Helpers;

namespace FileOps.Operations;

public sealed class MoveDirectoryOperation : IFileOpsTransaction, IDisposable
{
    private readonly string _tempPath;
    private readonly string _sourceFullPath;
    private readonly string _destinationFullPath;
    private string _sourceBackupPath;
    private string? _destinationBackupPath;
    private bool _disposed;
    private bool _cancelled;
    
    public MoveDirectoryOperation(string sourceFullPath, string destinationFullPath, string tempPath)
    {
        _sourceFullPath = sourceFullPath;
        _destinationFullPath = destinationFullPath;
        _tempPath = tempPath;
    }
    
    public void Commit()
    {
        if (!Directory.Exists(_sourceFullPath))
        {
            _cancelled = true;
            return;
        }
        
        if (_sourceFullPath.StartsWith(_destinationFullPath) || _destinationFullPath.StartsWith(_sourceFullPath))
        {
            _cancelled = true;
            return;
        }
        
        if (Directory.Exists(_destinationFullPath))
        {
            var destinationBackupPath = Path.Combine(_tempPath, Guid.NewGuid().ToString());
            Directory.Move(_destinationFullPath, destinationBackupPath);
            _destinationBackupPath = destinationBackupPath;
        }
        
        var sourceBackupFile = Path.Combine(_tempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(_sourceBackupPath);
        DirectoryHelper.CopyDirectory(_sourceFullPath, _sourceBackupPath);
        _sourceBackupPath = sourceBackupFile;
        Directory.Move(_sourceFullPath, _destinationFullPath);
    }

    public void RollBack()
    {
        if (_cancelled) return;
        
        if (_sourceBackupPath != null && !Directory.Exists(_sourceFullPath))
        {
            Directory.Move(_sourceBackupPath, _sourceFullPath);
        }

        if (_destinationBackupPath != null)
        {
            Directory.Delete(_destinationFullPath, true);
            Directory.Move(_destinationBackupPath, _destinationFullPath);
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