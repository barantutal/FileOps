using System.Transactions;
using FileOps.Abstraction;
using FileOps.Exceptions;
using Xunit;

namespace FileOps.Tests.xUnit;

// A transaction that times out is rolled back on a timer thread while the caller may still be inside
// a file operation. The operation finishes afterwards and must not stay on disk: the rollback that
// would have undone it has already run.
[Collection(BackupFolderCollection.Name)]
public class TransactionTimeoutTests
{
    private readonly string _tempPath;

    public TransactionTimeoutTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "FileOpsManager-timeout-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempPath);
    }

    [Fact]
    public void Operation_FinishingAfterTheTransactionTimedOut_IsUndone()
    {
        var sourcePath = Path.Combine(_tempPath, "01 _Klarenthal");
        var destinationPath = Path.Combine(_tempPath, "01_Klarenthal");
        Directory.CreateDirectory(sourcePath);
        File.WriteAllText(Path.Combine(sourcePath, "a.txt"), "a");
        var manager = new TransactionManager();

        using (new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromSeconds(1)))
        {
            var operation = new SlowMoveDirectory(sourcePath, destinationPath, _tempPath);

            Assert.Throws<TransactionAbortedException>(() => manager.AddTransaction(operation));
            Assert.True(operation.MovedAfterTheAbort);
        }

        Assert.True(File.Exists(Path.Combine(sourcePath, "a.txt")));
        Assert.False(Directory.Exists(destinationPath));
    }

    [Fact]
    public void Operation_StartedAfterTheTransactionTimedOut_IsNotRun()
    {
        var sourcePath = Path.Combine(_tempPath, "source");
        var destinationPath = Path.Combine(_tempPath, "destination");
        Directory.CreateDirectory(sourcePath);
        var fileOps = new FileOpsManager();

        using (new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromSeconds(1)))
        {
            WaitForAbort();

            Assert.ThrowsAny<TransactionException>(() => fileOps.MoveDirectory(sourcePath, destinationPath));
        }

        Assert.True(Directory.Exists(sourcePath));
        Assert.False(Directory.Exists(destinationPath));
    }

    [Fact]
    public void MoveDirectory_TakesNoBackupCopy()
    {
        var backupPath = Path.Combine(_tempPath, "backups");
        var sourcePath = Path.Combine(_tempPath, "source");
        var destinationPath = Path.Combine(_tempPath, "destination");
        Directory.CreateDirectory(backupPath);
        Directory.CreateDirectory(sourcePath);
        File.WriteAllText(Path.Combine(sourcePath, "a.txt"), "a");

        using (new TransactionScope())
        {
            new TransactionManager().AddTransaction(
                new FileOps.Transactions.MoveDirectoryTransaction(sourcePath, destinationPath, backupPath));

            Assert.Empty(Directory.EnumerateFileSystemEntries(backupPath));
        }

        Assert.True(File.Exists(Path.Combine(sourcePath, "a.txt")));
        Assert.False(Directory.Exists(destinationPath));
    }

    [Fact]
    public void MoveDirectory_RolledBack_KeepsBothTreesWhenTheSourceExistsAgain()
    {
        var sourcePath = Path.Combine(_tempPath, "source");
        var destinationPath = Path.Combine(_tempPath, "destination");
        Directory.CreateDirectory(sourcePath);
        File.WriteAllText(Path.Combine(sourcePath, "a.txt"), "a");
        var fileOps = new FileOpsManager();

        var scope = new TransactionScope();
        fileOps.MoveDirectory(sourcePath, destinationPath);
        Directory.CreateDirectory(sourcePath);
        File.WriteAllText(Path.Combine(sourcePath, "b.txt"), "b");

        var exception = Assert.Throws<TransactionException>(() => scope.Dispose());

        Assert.IsType<FileOperationException>(exception.InnerException);
        Assert.True(File.Exists(Path.Combine(sourcePath, "b.txt")));
        Assert.True(File.Exists(Path.Combine(destinationPath, "a.txt")));
    }

    private static void WaitForAbort()
    {
        var transaction = Transaction.Current!;
        var deadline = DateTime.UtcNow.AddSeconds(60);

        while (transaction.TransactionInformation.Status == TransactionStatus.Active && DateTime.UtcNow < deadline)
        {
            Thread.Sleep(50);
        }

        Assert.Equal(TransactionStatus.Aborted, transaction.TransactionInformation.Status);
    }

    // Moves only once the ambient transaction has timed out, like a move that was still busy when
    // the timeout fired.
    private sealed class SlowMoveDirectory : FileOps.Transactions.MoveDirectoryTransaction, IFileOpsTransaction
    {
        public SlowMoveDirectory(string sourcePath, string destinationPath, string tempPath)
            : base(sourcePath, destinationPath, tempPath)
        {
        }

        public bool MovedAfterTheAbort { get; private set; }

        public override void Commit()
        {
            WaitForAbort();
            base.Commit();
            MovedAfterTheAbort = true;
        }
    }
}
