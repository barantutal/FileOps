using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using FileOps.Abstraction;

namespace FileOps;

public class TransactionManager : ITransactionManager
{
    // One enlistment, with its own rollback stack, per ambient transaction. A manager is shared
    // between transactions whenever it lives longer than a request (a singleton service, the
    // xUnit class fixture), so a single shared stack mixed the operations of concurrent
    // transactions: a rollback in one request undid the committed files of another, and a
    // transaction re-enlisted every time another one had enlisted in between.
    private readonly Dictionary<Transaction, TransactionEnlistment> _enlistments = new();
    private readonly object _sync = new();
    public Action OnTransactionPreparing { get; set; }
    public Action OnRollback { get; set; }

    public bool TransactionStarted()
    {
        var current = Transaction.Current;

        if (current == null)
        {
            return false;
        }

        GetOrEnlist(current);
        return true;
    }

    // An operation joins the rollback stack only once its Commit has succeeded. A refused operation
    // (missing source, existing destination, invalid move) has changed nothing, and rolling it back
    // would act on paths it never touched: delete a destination that already existed, or fail on one
    // that was never created and hide the original refusal behind "Failed during rollback operation".
    public void AddTransaction(IFileOpsTransaction fileOpsTransaction)
    {
        var enlistment = GetOrEnlist(CurrentTransaction());

        try
        {
            fileOpsTransaction.Commit();
        }
        catch
        {
            DisposeTransaction(fileOpsTransaction);
            throw;
        }

        enlistment.Push(fileOpsTransaction);
    }

    public async Task AddTransactionAsync(IAsyncFileOpsTransaction asyncFileOpsTransaction)
    {
        var enlistment = GetOrEnlist(CurrentTransaction());

        try
        {
            await asyncFileOpsTransaction.CommitAsync();
        }
        catch
        {
            DisposeTransaction(asyncFileOpsTransaction);
            throw;
        }

        enlistment.Push(asyncFileOpsTransaction);
    }

    private static Transaction CurrentTransaction()
    {
        return Transaction.Current
               ?? throw new InvalidOperationException("There is no ambient transaction to add the file operation to.");
    }

    private TransactionEnlistment GetOrEnlist(Transaction transaction)
    {
        lock (_sync)
        {
            if (!_enlistments.TryGetValue(transaction, out var enlistment))
            {
                enlistment = new TransactionEnlistment(this, transaction);
                transaction.EnlistVolatile(enlistment, EnlistmentOptions.None);
                _enlistments[transaction] = enlistment;
            }

            return enlistment;
        }
    }

    private void Forget(Transaction transaction)
    {
        lock (_sync)
        {
            _enlistments.Remove(transaction);
        }
    }

    private static void DisposeTransaction(IFileOpsTransaction transaction)
    {
        if (transaction is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private sealed class TransactionEnlistment : IEnlistmentNotification
    {
        private readonly TransactionManager _owner;
        private readonly Transaction _transaction;
        private readonly Stack<IFileOpsTransaction> _operations = new();

        public TransactionEnlistment(TransactionManager owner, Transaction transaction)
        {
            _owner = owner;
            _transaction = transaction;
        }

        public void Push(IFileOpsTransaction operation)
        {
            lock (_operations)
            {
                _operations.Push(operation);
            }
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            // Phase one: vote only. The operations and their backups stay in the stack, because another
            // participant (typically the database connection) can still refuse to commit after this
            // vote, and the transaction manager then calls Rollback here. Disposing the stack at this
            // point left nothing to roll back, so the disk kept the change the database refused.
            _owner.OnTransactionPreparing?.Invoke();
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            _owner.Forget(_transaction);

            while (TryPop(out var operation))
            {
                DisposeTransaction(operation);
            }

            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            Rollback(enlistment);
        }

        public void Rollback(Enlistment enlistment)
        {
            _owner.Forget(_transaction);
            List<Exception> failures = null;

            try
            {
                _owner.OnRollback?.Invoke();

                // Every operation is rolled back even when an earlier one fails: stopping at the first
                // failure left the remaining operations on disk with no further attempt to undo them.
                while (TryPop(out var operation))
                {
                    try
                    {
                        operation.RollBack();
                    }
                    catch (Exception e)
                    {
                        (failures ??= new List<Exception>()).Add(e);
                    }
                    finally
                    {
                        DisposeTransaction(operation);
                    }
                }
            }
            catch (Exception e)
            {
                (failures ??= new List<Exception>()).Add(e);
            }
            finally
            {
                enlistment.Done();
            }

            if (failures != null)
            {
                throw new TransactionException(
                    "Failed during rollback operation.",
                    failures.Count == 1 ? failures[0] : new AggregateException(failures));
            }
        }

        private bool TryPop(out IFileOpsTransaction operation)
        {
            lock (_operations)
            {
                return _operations.TryPop(out operation);
            }
        }
    }
}
