using System;
using System.Collections.Generic;
using System.IO;
using System.Transactions;
using FileOps.Abstraction;
using FileOps.Operations;
using FileOps.Transactions;

namespace FileOps;

public class FileOpsManager : IFileOpsManager, IEnlistmentNotification
{
    private readonly string _tempPath;
    private readonly Stack<IFileOpsTransaction> _transactions;
    public Action OnTransactionPreparing;
    public Action OnRollback;
    
    public FileOpsManager()
    {
        _transactions = new Stack<IFileOpsTransaction>();
        _tempPath = Path.Combine(Path.GetTempPath(), "FileOpsManager-tempf");
        Directory.CreateDirectory(_tempPath);
    }
    
    public virtual void GenerateDirectory(string path)
    {
        if (TransactionPrepared())
        {
            AddTransaction(new GenerateDirectoryTransaction(path));
        }
        else
        {
            new GenerateDirectoryOperation(path).Commit();
        }
    }
    
    public virtual void MoveDirectory(string sourcePath, string destinationPath)
    {
        if (TransactionPrepared())
        {
            AddTransaction( new MoveDirectoryTransaction(sourcePath, destinationPath, _tempPath));
        }
        else
        {
            new MoveDirectoryOperation(sourcePath, destinationPath).Commit();
        }
    }

    public void CopyDirectory(string sourcePath, string destinationPath)
    {
        if (TransactionPrepared())
        {
            AddTransaction(new CopyDirectoryTransaction(sourcePath, destinationPath));
        }
        else
        {
            new CopyDirectoryOperation(sourcePath, destinationPath).Commit(); 
        }
    }

    public void DeleteDirectory(string path)
    {
        if (TransactionPrepared())
        {
            AddTransaction(new DeleteDirectoryTransaction(path, _tempPath));
        }
        else
        {
            new DeleteDirectoryOperation(path).Commit();
        }
    }

    public virtual void GenerateFile(string path, byte[] content)
    {
        if (TransactionPrepared())
        {
            AddTransaction(new GenerateFileTransaction(path, content));
        }
        else
        {
            new GenerateFileOperation(path, content).Commit();
        }
    }
    
    public virtual void CopyFile(string sourcePath, string destinationPath)
    {
        if (TransactionPrepared())
        {
            AddTransaction(new CopyFileTransaction(sourcePath, destinationPath));
        }
        else
        {
            new CopyFileOperation(sourcePath, destinationPath).Commit();
        }
    }

    public void MoveFile(string sourcePath, string destinationPath)
    {
        if (TransactionPrepared())
        {
            AddTransaction(new MoveFileTransaction(sourcePath, destinationPath, _tempPath));
        }
        else
        {
            new MoveFileOperation(sourcePath, destinationPath).Commit();
        }
    }

    public virtual void DeleteFile(string path)
    {
        if (TransactionPrepared())
        {
            AddTransaction(new DeleteFileTransaction(path, _tempPath));
        }
        else
        {
            new DeleteFileOperation(path).Commit();
        }
    }
    
    public virtual bool TransactionPrepared()
    {
        if (Transaction.Current != null)
        {
            Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
        }
        
        return Transaction.Current != null;
    }
    
    public void AddTransaction(IFileOpsTransaction fileOpsTransaction)
    {
        _transactions.Push(fileOpsTransaction);
        fileOpsTransaction.Commit();
    }

    public void Commit(Enlistment enlistment)
    {
        enlistment.Done();
    }

    public void InDoubt(Enlistment enlistment)
    {
        Rollback(enlistment);
    }

    public void Prepare(PreparingEnlistment preparingEnlistment)
    {
        OnTransactionPreparing?.Invoke();
        preparingEnlistment.Prepared();
        DisposeTransactions();
    }

    public void Rollback(Enlistment enlistment)
    {
        try
        {
            OnRollback?.Invoke();
            while (_transactions.Count > 0)
            {
                var transaction = _transactions.Pop();
                transaction.RollBack();

                DisposeTransaction(transaction);
            }
        }
        catch (Exception e)
        {
            throw new TransactionException("Failed during rollback operation.", e);
        }
        
        enlistment.Done();
    }
    
    private void DisposeTransactions()
    {
        while (_transactions.Count > 0)
        {
            DisposeTransaction(_transactions.Pop());
        }
    }
    
    private void DisposeTransaction(IFileOpsTransaction transaction)
    {
        if (transaction is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}