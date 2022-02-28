using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using FileOps.Abstraction;

namespace FileOps;

public class TransactionManager : IEnlistmentNotification, ITransactionManager
{
    private readonly Stack<IFileOpsTransaction> _transactions;
    public Action OnTransactionPreparing { get; set; }
    public Action OnRollback { get; set; }

    public TransactionManager()
    {
        _transactions = new Stack<IFileOpsTransaction>();
    }
    
    public bool TransactionStarted()
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
    
    public async Task AddTransactionAsync(IAsyncFileOpsTransaction asyncFileOpsTransaction)
    {
        _transactions.Push(asyncFileOpsTransaction);
        await asyncFileOpsTransaction.CommitAsync();
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