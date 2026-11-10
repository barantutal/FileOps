using System.Text;
using System.Transactions;
using Xunit;

namespace FileOps.Tests.xUnit;

// A file operation shares its TransactionScope with other participants, typically a database
// connection. Those participants can still fail after FileOps has voted "prepared": a database
// commit may be refused (serialization failure, constraint, lost connection). The disk change
// must then be undone like every other change in the scope.
[Collection(BackupFolderCollection.Name)]
public class TransactionAbortTests
{
    private readonly string _tempPath;

    public TransactionAbortTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "FileOpsManager-abort-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempPath);
    }

    [Fact]
    public void MoveFile_RolledBack_WhenALaterParticipantFailsToPrepare()
    {
        var sourcePath = Seed();
        var destinationPath = GenerateFilePath();
        var fileOps = new FileOpsManager();

        var scope = new TransactionScope();
        fileOps.MoveFile(sourcePath, destinationPath);
        FailingParticipant.Enlist();
        scope.Complete();

        Assert.Throws<TransactionAbortedException>(() => scope.Dispose());

        Assert.True(File.Exists(sourcePath));
        Assert.False(File.Exists(destinationPath));
    }

    [Fact]
    public void MoveFile_RolledBack_WhenAnEarlierParticipantFailsToPrepare()
    {
        var sourcePath = Seed();
        var destinationPath = GenerateFilePath();
        var fileOps = new FileOpsManager();

        var scope = new TransactionScope();
        FailingParticipant.Enlist();
        fileOps.MoveFile(sourcePath, destinationPath);
        scope.Complete();

        Assert.Throws<TransactionAbortedException>(() => scope.Dispose());

        Assert.True(File.Exists(sourcePath));
        Assert.False(File.Exists(destinationPath));
    }

    [Fact]
    public void MoveDirectory_RolledBack_WhenALaterParticipantFailsToPrepare()
    {
        var sourceDirectory = Path.Combine(_tempPath, "source");
        var destinationDirectory = Path.Combine(_tempPath, "destination");
        Directory.CreateDirectory(sourceDirectory);
        File.WriteAllText(Path.Combine(sourceDirectory, "a.txt"), "a");
        var fileOps = new FileOpsManager();

        var scope = new TransactionScope();
        fileOps.MoveDirectory(sourceDirectory, destinationDirectory);
        FailingParticipant.Enlist();
        scope.Complete();

        Assert.Throws<TransactionAbortedException>(() => scope.Dispose());

        Assert.True(File.Exists(Path.Combine(sourceDirectory, "a.txt")));
        Assert.False(Directory.Exists(destinationDirectory));
    }

    [Fact]
    public void SeveralOperations_AllRolledBack_WhenALaterParticipantFailsToPrepare()
    {
        var sourcePath = Seed();
        var movedPath = GenerateFilePath();
        var generatedPath = GenerateFilePath();
        var fileOps = new FileOpsManager();

        var scope = new TransactionScope();
        fileOps.MoveFile(sourcePath, movedPath);
        fileOps.GenerateFile(generatedPath, Encoding.UTF8.GetBytes("new"));
        FailingParticipant.Enlist();
        scope.Complete();

        Assert.Throws<TransactionAbortedException>(() => scope.Dispose());

        Assert.True(File.Exists(sourcePath));
        Assert.False(File.Exists(movedPath));
        Assert.False(File.Exists(generatedPath));
    }

    [Fact]
    public void MoveFile_Committed_ClearsItsBackup()
    {
        var sourcePath = Seed();
        var destinationPath = GenerateFilePath();
        var fileOps = new FileOpsManager();
        var backupRoot = Path.Combine(Path.GetTempPath(), "FileOpsManager-tempf");
        var before = Directory.GetFileSystemEntries(backupRoot).Length;

        var scope = new TransactionScope();
        fileOps.MoveFile(sourcePath, destinationPath);
        scope.Complete();
        scope.Dispose();

        Assert.False(File.Exists(sourcePath));
        Assert.True(File.Exists(destinationPath));
        Assert.Equal(before, Directory.GetFileSystemEntries(backupRoot).Length);
    }

    private string Seed()
    {
        var path = GenerateFilePath();
        File.WriteAllText(path, Guid.NewGuid().ToString());
        return path;
    }

    private string GenerateFilePath()
    {
        return Path.Combine(_tempPath, $"{Guid.NewGuid()}.txt");
    }

    // Stands in for a database connection whose commit is refused: it votes against the
    // transaction in the prepare phase, after or before FileOps has voted.
    private sealed class FailingParticipant : IEnlistmentNotification
    {
        public static void Enlist()
        {
            Transaction.Current!.EnlistVolatile(new FailingParticipant(), EnlistmentOptions.None);
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.ForceRollback(new InvalidOperationException("40001: could not serialize access"));
        }

        public void Commit(Enlistment enlistment) => enlistment.Done();

        public void Rollback(Enlistment enlistment) => enlistment.Done();

        public void InDoubt(Enlistment enlistment) => enlistment.Done();
    }
}
