using System;
using System.IO;
using System.Threading.Tasks;
using FileOps.Abstraction;
using FileOps.Operations;
using FileOps.Transactions;

namespace FileOps;

public class FileOpsManager : IFileOpsManager
{
    private readonly ITransactionManager _transactionManager;
    private readonly string _tempPath;
    public Action OnTransactionPreparing { get; set; }
    public Action OnRollback { get; set; }
    
    public FileOpsManager()
    {
        _transactionManager = new TransactionManager();
        _tempPath = Path.Combine(Path.GetTempPath(), "FileOpsManager-tempf");
        Directory.CreateDirectory(_tempPath);

        _transactionManager.OnRollback = Rollback;
        _transactionManager.OnTransactionPreparing = Prepare;
    }
    
    public void GenerateDirectory(string path)
    {
        if (_transactionManager.TransactionStarted())
        {
            _transactionManager.AddTransaction(new GenerateDirectoryTransaction(path));
        }
        else
        {
            new GenerateDirectoryOperation(path).Commit();
        }
    }
    
    public void MoveDirectory(string sourcePath, string destinationPath)
    {
        if (_transactionManager.TransactionStarted())
        {
            _transactionManager.AddTransaction( new MoveDirectoryTransaction(sourcePath, destinationPath, _tempPath));
        }
        else
        {
            new MoveDirectoryOperation(sourcePath, destinationPath).Commit();
        }
    }

    public void CopyDirectory(string sourcePath, string destinationPath)
    {
        if (_transactionManager.TransactionStarted())
        {
            _transactionManager.AddTransaction(new CopyDirectoryTransaction(sourcePath, destinationPath));
        }
        else
        {
            new CopyDirectoryOperation(sourcePath, destinationPath).Commit(); 
        }
    }
    
    public async Task CopyDirectoryAsync(string sourcePath, string destinationPath)
    {
        if (_transactionManager.TransactionStarted())
        {
            await _transactionManager.AddTransactionAsync(new CopyDirectoryTransaction(sourcePath, destinationPath));
        }
        else
        {
            await new CopyDirectoryOperation(sourcePath, destinationPath).CommitAsync(); 
        }
    }

    public void DeleteDirectory(string path)
    {
        if (_transactionManager.TransactionStarted())
        {
            _transactionManager.AddTransaction(new DeleteDirectoryTransaction(path, _tempPath));
        }
        else
        {
            new DeleteDirectoryOperation(path).Commit();
        }
    }

    public void GenerateFile(string path, byte[] content)
    {
        if (_transactionManager.TransactionStarted())
        {
            _transactionManager.AddTransaction(new GenerateFileTransaction(path, content));
        }
        else
        {
            new GenerateFileOperation(path, content).Commit();
        }
    }
    
    public void CopyFile(string sourcePath, string destinationPath)
    {
        if (_transactionManager.TransactionStarted())
        {
            _transactionManager.AddTransaction(new CopyFileTransaction(sourcePath, destinationPath));
        }
        else
        {
            new CopyFileOperation(sourcePath, destinationPath).Commit();
        }
    }
    
    public async Task CopyFileAsync(string sourcePath, string destinationPath)
    {
        if (_transactionManager.TransactionStarted())
        {
            await _transactionManager.AddTransactionAsync(new CopyFileTransaction(sourcePath, destinationPath));
        }
        else
        {
            await new CopyFileOperation(sourcePath, destinationPath).CommitAsync();
        }
    }

    public void MoveFile(string sourcePath, string destinationPath)
    {
        if (_transactionManager.TransactionStarted())
        {
            _transactionManager.AddTransaction(new MoveFileTransaction(sourcePath, destinationPath, _tempPath));
        }
        else
        {
            new MoveFileOperation(sourcePath, destinationPath).Commit();
        }
    }

    public void DeleteFile(string path)
    {
        if (_transactionManager.TransactionStarted())
        {
            _transactionManager.AddTransaction(new DeleteFileTransaction(path, _tempPath));
        }
        else
        {
            new DeleteFileOperation(path).Commit();
        }
    }

    public void Prepare()
    {
        OnTransactionPreparing?.Invoke();
    }

    public void Rollback()
    {
        OnRollback?.Invoke();
    }
}