using System;
using System.IO;
using System.Transactions;
using FileOps.Abstraction;
using FileOps.Operations;
using FileOps.Transactions;

namespace FileOps;

public class FileOpsManager : IFileOpsManager
{
    private readonly string _tempPath;
    protected FileOpsEnlistment? _fileOpsEnlistment;
    protected Action OnTransactionPreparing;
    protected Action OnRollback;
    
    public FileOpsManager()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "FileOpsManager-tempf");
        Directory.CreateDirectory(_tempPath);
    }
    
    public virtual void GenerateDirectory(string path)
    {
        if (IsTransactionOpen())
        {
            _fileOpsEnlistment.Add(new GenerateDirectoryTransaction(path));
        }
        else
        {
            new GenerateDirectoryOperation(path).Commit();
        }
    }
    
    public virtual void MoveDirectory(string sourcePath, string destinationPath)
    {
        if (IsTransactionOpen())
        {
            _fileOpsEnlistment.Add( new MoveDirectoryTransaction(sourcePath, destinationPath, _tempPath));
        }
        else
        {
            new MoveDirectoryOperation(sourcePath, destinationPath).Commit();
        }
    }

    public void CopyDirectory(string sourcePath, string destinationPath)
    {
        if (IsTransactionOpen())
        {
            _fileOpsEnlistment.Add(new CopyDirectoryTransaction(sourcePath, destinationPath));
        }
        else
        {
            new CopyDirectoryOperation(sourcePath, destinationPath).Commit(); 
        }
    }

    public void DeleteDirectory(string path)
    {
        if (IsTransactionOpen())
        {
            _fileOpsEnlistment.Add(new DeleteDirectoryTransaction(path, _tempPath));
        }
        else
        {
            new DeleteDirectoryOperation(path).Commit();
        }
    }

    public virtual void GenerateFile(string path, byte[] content)
    {
        if (IsTransactionOpen())
        {
            _fileOpsEnlistment.Add(new GenerateFileTransaction(path, content));
        }
        else
        {
            new GenerateFileOperation(path, content).Commit();
        }
    }
    
    public virtual void CopyFile(string sourcePath, string destinationPath)
    {
        if (IsTransactionOpen())
        {
            _fileOpsEnlistment.Add(new CopyFileTransaction(sourcePath, destinationPath));
        }
        else
        {
            new CopyFileOperation(sourcePath, destinationPath).Commit();
        }
    }

    public void MoveFile(string sourcePath, string destinationPath)
    {
        if (IsTransactionOpen())
        {
            _fileOpsEnlistment.Add(new MoveFileTransaction(sourcePath, destinationPath, _tempPath));
        }
        else
        {
            new MoveFileOperation(sourcePath, destinationPath).Commit();
        }
    }

    public virtual void DeleteFile(string path)
    {
        if (IsTransactionOpen())
        {
            _fileOpsEnlistment.Add(new DeleteFileTransaction(path, _tempPath));
        }
        else
        {
            new DeleteFileOperation(path).Commit();
        }
    }
    
    public virtual bool IsTransactionOpen()
    {
        if (Transaction.Current != null)
        {
            _fileOpsEnlistment ??= new FileOpsEnlistment
            {
                OnTransactionPreparing = Prepare,
                OnRollback = Rollback,
            };
        }
        
        return Transaction.Current != null;
    }
    
    private void Prepare()
    {
        OnTransactionPreparing?.Invoke();
    }
    
    private void Rollback()
    {
        OnRollback?.Invoke();
    }
}