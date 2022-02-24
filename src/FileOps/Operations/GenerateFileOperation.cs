using System;
using System.Collections.Generic;
using System.IO;
using FileOps.Backup;

namespace FileOps.Operations;

public sealed class GenerateFileOperation : IFileStoreTransaction, IDisposable
{
    private readonly string _tempPath;
    private readonly string _fullPath;
    private readonly byte[] _fileContent;
    private string? _backupPath;
    private bool _disposed;

    public GenerateFileOperation(string fullPath, byte[] fileContent, string tempPath)
    {
        _fullPath = fullPath;
        _fileContent = fileContent;
        _tempPath = tempPath;
    }
    
    public void Commit()
    {
        if (File.Exists(_fullPath))
        {
            _backupPath = Path.Combine(_tempPath, Guid.NewGuid() + Path.GetExtension(_fullPath));
            File.Copy(_fullPath, _backupPath);
        }

        File.WriteAllBytes(_fullPath, _fileContent);
        Array.Clear(_fileContent, 0, _fileContent.Length);
    }

    public void RollBack()
    {
        File.Delete(_fullPath);

        if (_backupPath != null)
        {
            File.Move(_backupPath, _fullPath);
        }
    }
    
    ~GenerateFileOperation()
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