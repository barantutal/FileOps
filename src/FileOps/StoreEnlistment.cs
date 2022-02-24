using System;
using System.Collections.Generic;
using System.Transactions;

namespace FileOps;

public class StoreEnlistment : IEnlistmentNotification
{
    private readonly Stack<IFileStoreTransaction> _transactions;
    public Action? TransactionPreparingAction;
    public Action? RollbackAction;

    public StoreEnlistment()
    {
        Transaction.Current?.EnlistVolatile(this, EnlistmentOptions.None);
        _transactions = new Stack<IFileStoreTransaction>();
    }

    public void Add(IFileStoreTransaction storeTransaction)
    {
        _transactions.Push(storeTransaction);
        storeTransaction.Commit();
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
        TransactionPreparingAction?.Invoke();
        preparingEnlistment.Prepared();
        DisposeTransactions();
    }

    public void Rollback(Enlistment enlistment)
    {
        try
        {
            RollbackAction?.Invoke();
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
    
    private void DisposeTransaction(IFileStoreTransaction transaction)
    {
        if (transaction is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}