using System;
using System.Collections.Generic;
using System.IO;
using FileOps.Backup;

namespace FileOps.Operations;

public sealed class GenerateFileOperation : IFileOpsTransaction, IDisposable
{
    private readonly string _tempPath;
    private readonly string _path;
    private readonly byte[] _fileContent;
    private string _backupPath;
    private bool _disposed;

    public GenerateFileOperation(string path, byte[] fileContent, string tempPath)
    {
        _path = path;
        _fileContent = fileContent;
        _tempPath = tempPath;
    }
    
    public void Commit()
    {
        if (File.Exists(_path))
        {
            var backupPath = Path.Combine(_tempPath, Guid.NewGuid() + Path.GetExtension(_path));
            File.Move(_path, backupPath);
            _backupPath = backupPath;
        }

        File.WriteAllBytes(_path, _fileContent);
        Array.Clear(_fileContent, 0, _fileContent.Length);
    }

    public void RollBack()
    {
        if (_backupPath == null) return;
        
        File.Delete(_path);
        File.Move(_backupPath, _path);
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
        if (_disposed) return;
        
        _disposed = true;
            
        var backupFolders = new List<string>();
        if (_backupPath != null)
        {
            backupFolders.Add(_backupPath);
        }
            
        ClearBackupHelper.Execute(backupFolders);
    }
}