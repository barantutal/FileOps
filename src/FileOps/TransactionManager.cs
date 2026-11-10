using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using FileOps.Abstraction;

namespace FileOps;

public class TransactionManager : IEnlistmentNotification, ITransactionManager
{
    private readonly Stack<IFileOpsTransaction> _transactions;
    private Transaction _enlistedIn;
    public Action OnTransactionPreparing { get; set; }
    public Action OnRollback { get; set; }

    public TransactionManager()
    {
        _transactions = new Stack<IFileOpsTransaction>();
    }

    public bool TransactionStarted()
    {
        var current = Transaction.Current;

        if (current == null)
        {
            return false;
        }

        // One enlistment per ambient transaction: every file operation used to enlist again, so
        // the transaction manager called Prepare/Commit/Rollback on this instance once per operation.
        if (!ReferenceEquals(_enlistedIn, current))
        {
            current.EnlistVolatile(this, EnlistmentOptions.None);
            _enlistedIn = current;
        }

        return true;
    }

    // An operation joins the rollback stack only once its Commit has succeeded. A refused operation
    // (missing source, existing destination, invalid move) has changed nothing, and rolling it back
    // would act on paths it never touched: delete a destination that already existed, or fail on one
    // that was never created and hide the original refusal behind "Failed during rollback operation".
    public void AddTransaction(IFileOpsTransaction fileOpsTransaction)
    {
        try
        {
            fileOpsTransaction.Commit();
        }
        catch
        {
            DisposeTransaction(fileOpsTransaction);
            throw;
        }

        _transactions.Push(fileOpsTransaction);
    }

    public async Task AddTransactionAsync(IAsyncFileOpsTransaction asyncFileOpsTransaction)
    {
        try
        {
            await asyncFileOpsTransaction.CommitAsync();
        }
        catch
        {
            DisposeTransaction(asyncFileOpsTransaction);
            throw;
        }

        _transactions.Push(asyncFileOpsTransaction);
    }

    public void Prepare(PreparingEnlistment preparingEnlistment)
    {
        // Phase one: vote only. The operations and their backups stay in the stack, because another
        // participant (typically the database connection) can still refuse to commit after this
        // vote, and the transaction manager then calls Rollback here. Disposing the stack at this
        // point left nothing to roll back, so the disk kept the change the database refused.
        OnTransactionPreparing?.Invoke();
        preparingEnlistment.Prepared();
    }

    public void Commit(Enlistment enlistment)
    {
        _enlistedIn = null;
        DisposeTransactions();
        enlistment.Done();
    }

    public void InDoubt(Enlistment enlistment)
    {
        Rollback(enlistment);
    }

    public void Rollback(Enlistment enlistment)
    {
        _enlistedIn = null;

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
        finally
        {
            enlistment.Done();
        }
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
