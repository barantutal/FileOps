using System;

namespace FileOps.Abstraction;

public interface ITransactionManager
{
    void AddTransaction(IFileOpsTransaction fileOpsTransaction);
    bool TransactionStarted();
    Action OnTransactionPreparing { get; set; }
    Action OnRollback { get; set; }
}