using System;
using System.Threading.Tasks;

namespace FileOps.Abstraction;

public interface ITransactionManager
{
    void AddTransaction(IFileOpsTransaction fileOpsTransaction);
    Task AddTransactionAsync(IAsyncFileOpsTransaction asyncFileOpsTransaction);
    bool TransactionStarted();
    Action OnTransactionPreparing { get; set; }
    Action OnRollback { get; set; }
}