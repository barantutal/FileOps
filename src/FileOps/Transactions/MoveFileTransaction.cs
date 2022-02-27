using System;
using System.IO;
using FileOps.Abstraction;
using FileOps.Operations;

namespace FileOps.Transactions;

public class MoveFileTransaction : MoveFileOperation, IFileOpsTransaction
{
    private readonly string _sourcePath;
    private readonly string _destinationPath;
    private readonly string _tempPath;
    private string _backupPath;
    private string _directoryPath;
    
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
}