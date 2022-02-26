using System;
using System.IO;
using System.Transactions;
using FileOps.Operations;

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
        EnlistTransaction(new GenerateDirectoryOperation(path));
    }
    
    public virtual void MoveDirectory(string sourcePath, string destinationPath)
    {
        EnlistTransaction(new MoveDirectoryOperation(sourcePath, destinationPath, _tempPath));
    }
    
    public virtual IFileInfo GenerateFile(string path, byte[] content)
    {
        EnlistTransaction(new GenerateFileOperation(path, content, _tempPath));

        var fileInfo = new System.IO.FileInfo(path);
        return new FileInfo(fileInfo.Name, fileInfo.FullName, DateTime.Now, DateTime.Now, fileInfo.Length);
    }
    
    public virtual IFileInfo CopyFile(string path, string pathToCopy)
    {
        EnlistTransaction(new CopyFileOperation(path, pathToCopy, _tempPath));
        
        var fileInfo = new System.IO.FileInfo(pathToCopy);
        return new FileInfo(fileInfo.Name, fileInfo.FullName, DateTime.Now, DateTime.Now, fileInfo.Length);
    }
    
    public virtual void DeleteFile(string path)
    {
        EnlistTransaction(new DeleteFileOperation(path, _tempPath));
    }
    
    public virtual bool IsTransactionOpen()
    {
        return Transaction.Current != null;
    }

    public void EnlistTransaction(IFileOpsTransaction transaction)
    {
        if (IsTransactionOpen())
        {
            _fileOpsEnlistment ??= new FileOpsEnlistment
            {
                TransactionPreparingAction = Prepare,
                RollbackAction = Rollback,
            };

            _fileOpsEnlistment.Add(transaction);
        }
        else
        {
            transaction.Commit();
        }
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