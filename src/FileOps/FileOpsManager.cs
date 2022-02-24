using System;
using System.IO;
using System.Transactions;
using FileOps.Operations;

namespace FileOps;

public class FileOpsManager : IFileOpsManager
{
    private readonly string _tempPath;
    private readonly string _rootPath;
    protected StoreEnlistment? _storeEnlistment;
    protected Action? OnTransactionPreparing;
    protected Action? OnRollback;
    
    public FileOpsManager(FileOpsOptions options)
    {
        _rootPath =  options.RootPath ?? "/";
        _tempPath = Path.Combine(Path.GetTempPath(), "StoreManager-tempf");
        Directory.CreateDirectory(_rootPath);
        Directory.CreateDirectory(_tempPath);
    }
    
    public virtual void GenerateDirectory(string path)
    {
        EnlistTransaction(new GenerateDirectoryOperation(GetFullPath(path)));
    }
    
    public virtual void MoveDirectory(string sourcePath, string destinationPath)
    {
        EnlistTransaction(new MoveDirectoryOperation(GetFullPath(sourcePath), GetFullPath(destinationPath), _tempPath));
    }
    
    public virtual IFileInfo GenerateFile(string path, byte[] content)
    {
        var fullPath = GetFullPath(path);
        EnlistTransaction(new GenerateFileOperation(fullPath, content, _tempPath));

        var fileInfo = new System.IO.FileInfo(fullPath);
        return new FileInfo(fileInfo.Name, fileInfo.FullName, DateTime.Now, DateTime.Now, fileInfo.Length);
    }
    
    public virtual IFileInfo CopyFile(string path, string pathToCopy)
    {
        var newFullPath = GetFullPath(pathToCopy);
        EnlistTransaction(new CopyFileOperation(GetFullPath(path), newFullPath, _tempPath));
        
        var fileInfo = new System.IO.FileInfo(newFullPath);
        return new FileInfo(fileInfo.Name, fileInfo.FullName, DateTime.Now, DateTime.Now, fileInfo.Length);
    }

    public virtual IFileInfo CopyFile(Guid id, string pathToCopy)
    {
        return null;
    }

    public virtual void DeleteFile(string path)
    {
        EnlistTransaction(new DeleteFileOperation(GetFullPath(path), _tempPath));
    }
    
    public virtual bool IsTransactionOpen()
    {
        return Transaction.Current != null;
    }

    public virtual string GetFullPath(string path)
    {
        if (path.StartsWith("/"))
        {
            path = path.Substring(1, path.Length - 1);
        }
        
        return Path.Combine(_rootPath, path);
    }

    public void EnlistTransaction(IFileStoreTransaction transaction)
    {
        if (IsTransactionOpen())
        {
            if (_storeEnlistment == null)
            {
                _storeEnlistment = new StoreEnlistment
                {
                    TransactionPreparingAction = Prepare,
                    RollbackAction = Rollback,
                };
            }

            _storeEnlistment.Add(transaction);
        }
        
        transaction.Commit();
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