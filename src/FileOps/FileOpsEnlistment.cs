using System;
using System.Collections.Generic;
using System.Transactions;
using FileOps.Abstraction;
using FileOps.Operations;

namespace FileOps;

public class FileOpsEnlistment : IEnlistmentNotification
{
    private readonly Stack<IFileOpsTransaction> _transactions;
    public Action OnTransactionPreparing;
    public Action OnRollback;

    public FileOpsEnlistment()
    {
        Transaction.Current?.EnlistVolatile(this, EnlistmentOptions.None);
        _transactions = new Stack<IFileOpsTransaction>();
    }

    public void Add(IFileOpsTransaction fileOpsTransaction)
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