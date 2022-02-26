using System;
using System.Collections.Generic;
using System.IO;
using FileOps.Backup;

namespace FileOps.Operations;

public sealed class MoveDirectoryOperation : IFileOpsTransaction, IDisposable
{
    private readonly string _tempPath;
    private readonly string _sourceFullPath;
    private readonly string _destinationFullPath;
    private string _sourceBackupPath;
    private string? _destinationBackupPath;
    private bool _disposed;

    public MoveDirectoryOperation(string sourceFullPath, string destinationFullPath, string tempPath)
    {
        _sourceFullPath = sourceFullPath;
        _destinationFullPath = destinationFullPath;
        _tempPath = tempPath;
    }
    
    public void Commit()
    {
        if (Directory.Exists(_destinationFullPath))
        {
            _destinationBackupPath = Path.Combine(_tempPath, Guid.NewGuid().ToString());
            Directory.Move(_destinationFullPath, _destinationBackupPath);
        }
        
        _sourceBackupPath = Path.Combine(_tempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(_sourceBackupPath);
        BackupSourceDirectory();
        Directory.Move(_sourceFullPath, _destinationFullPath);
    }

    public void RollBack()
    {
        if(Directory.Exists(_sourceFullPath))  Directory.Delete(_sourceFullPath, true);
        if(Directory.Exists(_destinationFullPath))  Directory.Delete(_destinationFullPath, true);
        
        Directory.Move(_sourceBackupPath, _sourceFullPath);

        if (_destinationBackupPath != null)
        {
            Directory.Move(_destinationBackupPath, _destinationFullPath);
        }
    }
    
    private void BackupSourceDirectory()
    {
        foreach (string dirPath in Directory.GetDirectories(_sourceFullPath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(_sourceFullPath, _sourceBackupPath));
        }

        foreach (string newPath in Directory.GetFiles(_sourceFullPath, "*.*",SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(_sourceFullPath, _sourceBackupPath), true);
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
        if (!_disposed)
        {
            _disposed = true;
            
            var backupFolders = new List<string>() { _sourceBackupPath };
            if (_destinationBackupPath != null)
            {
                backupFolders.Add(_destinationBackupPath);
            }
            
            ClearBackupHelper.Execute(backupFolders);
        }
    }
}