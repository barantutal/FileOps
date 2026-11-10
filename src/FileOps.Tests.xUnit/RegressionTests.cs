using System.Text;
using System.Transactions;
using Xunit;

namespace FileOps.Tests.xUnit;

[Collection(BackupFolderCollection.Name)]
public class RegressionTests
{
    private readonly string _tempPath;

    public RegressionTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "FileOpsManager-regression-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempPath);
    }

    [Fact]
    public async Task CopyDirectoryAsync_CopiesTheFilesInside()
    {
        var source = Path.Combine(_tempPath, "source");
        var destination = Path.Combine(_tempPath, "destination");
        Directory.CreateDirectory(Path.Combine(source, "sub"));
        File.WriteAllText(Path.Combine(source, "a.txt"), "a");
        File.WriteAllText(Path.Combine(source, "sub", "b.txt"), "b");

        await new FileOpsManager().CopyDirectoryAsync(source, destination);

        Assert.Equal("a", File.ReadAllText(Path.Combine(destination, "a.txt")));
        Assert.Equal("b", File.ReadAllText(Path.Combine(destination, "sub", "b.txt")));
    }

    [Fact]
    public void DeleteDirectory_RolledBack_RestoresAFlatDirectory()
    {
        var directory = Path.Combine(_tempPath, "flat");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "a.txt"), "a");
        var fileOps = new FileOpsManager();

        var scope = new TransactionScope();
        fileOps.DeleteDirectory(directory);
        Assert.False(Directory.Exists(directory));
        scope.Dispose();

        Assert.Equal("a", File.ReadAllText(Path.Combine(directory, "a.txt")));
    }

    [Fact]
    public void DeleteDirectory_RolledBack_RestoresAnEmptyDirectory()
    {
        var directory = Path.Combine(_tempPath, "empty");
        Directory.CreateDirectory(directory);
        var fileOps = new FileOpsManager();

        var scope = new TransactionScope();
        fileOps.DeleteDirectory(directory);
        scope.Dispose();

        Assert.True(Directory.Exists(directory));
    }

    [Fact]
    public void GenerateFile_DoesNotModifyTheCallersBuffer()
    {
        var content = Encoding.UTF8.GetBytes("hello");
        var fileOps = new FileOpsManager();

        fileOps.GenerateFile(Path.Combine(_tempPath, "1.txt"), content);
        fileOps.GenerateFile(Path.Combine(_tempPath, "2.txt"), content);

        Assert.Equal("hello", File.ReadAllText(Path.Combine(_tempPath, "2.txt")));
    }

    [Fact]
    public void GenerateFile_InATransaction_AcceptsABareFileName()
    {
        var previous = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_tempPath);
        try
        {
            using var scope = new TransactionScope();
            new FileOpsManager().GenerateFile("bare.txt", Encoding.UTF8.GetBytes("x"));
            scope.Complete();
        }
        finally
        {
            Directory.SetCurrentDirectory(previous);
        }

        Assert.True(File.Exists(Path.Combine(_tempPath, "bare.txt")));
    }

    [Fact]
    public void CopyDirectory_WhenASubfolderRepeatsTheSourceName_CopiesIt()
    {
        var previous = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_tempPath);
        try
        {
            Directory.CreateDirectory(Path.Combine("data", "data"));
            File.WriteAllText(Path.Combine("data", "data", "a.txt"), "a");

            new FileOpsManager().CopyDirectory("data", "copy");
        }
        finally
        {
            Directory.SetCurrentDirectory(previous);
        }

        Assert.Equal("a", File.ReadAllText(Path.Combine(_tempPath, "copy", "data", "a.txt")));
    }

    [Fact]
    public void OneManager_TwoConcurrentTransactions_RollBackOnlyTheirOwnOperations()
    {
        var fileOps = new FileOpsManager();
        var committedPath = Path.Combine(_tempPath, "committed.txt");
        var abortedPath = Path.Combine(_tempPath, "aborted.txt");

        using var committedWritten = new ManualResetEventSlim();
        using var abortedWritten = new ManualResetEventSlim();

        var committing = Task.Run(() =>
        {
            using var scope = new TransactionScope();
            fileOps.GenerateFile(committedPath, Encoding.UTF8.GetBytes("keep"));
            committedWritten.Set();
            abortedWritten.Wait();
            scope.Complete();
        });

        var aborting = Task.Run(() =>
        {
            committedWritten.Wait();
            var scope = new TransactionScope();
            fileOps.GenerateFile(abortedPath, Encoding.UTF8.GetBytes("drop"));
            abortedWritten.Set();
            scope.Dispose();
        });

        Task.WaitAll(committing, aborting);

        Assert.True(File.Exists(committedPath));
        Assert.False(File.Exists(abortedPath));
    }

    [Fact]
    public void GenerateFile_RolledBack_RemovesEveryParentDirectoryItCreated()
    {
        var path = Path.Combine(_tempPath, "a", "b", "c", "file.txt");
        var fileOps = new FileOpsManager();

        var scope = new TransactionScope();
        fileOps.GenerateFile(path, Encoding.UTF8.GetBytes("x"));
        Assert.True(File.Exists(path));
        scope.Dispose();

        Assert.False(Directory.Exists(Path.Combine(_tempPath, "a")));
        Assert.True(Directory.Exists(_tempPath));
    }

    [Fact]
    public void GenerateDirectory_RolledBack_RemovesEveryParentDirectoryItCreated()
    {
        var path = Path.Combine(_tempPath, "a", "b", "c");
        var fileOps = new FileOpsManager();

        var scope = new TransactionScope();
        fileOps.GenerateDirectory(path);
        Assert.True(Directory.Exists(path));
        scope.Dispose();

        Assert.False(Directory.Exists(Path.Combine(_tempPath, "a")));
        Assert.True(Directory.Exists(_tempPath));
    }

    [Fact]
    public void GenerateFile_RolledBack_KeepsACreatedDirectoryThatReceivedOtherContent()
    {
        var directory = Path.Combine(_tempPath, "shared");
        var path = Path.Combine(directory, "file.txt");
        var fileOps = new FileOpsManager();

        var scope = new TransactionScope();
        fileOps.GenerateFile(path, Encoding.UTF8.GetBytes("x"));
        File.WriteAllText(Path.Combine(directory, "foreign.txt"), "not ours");
        scope.Dispose();

        Assert.False(File.Exists(path));
        Assert.Equal("not ours", File.ReadAllText(Path.Combine(directory, "foreign.txt")));
    }

    [Fact]
    public void MoveDirectory_RolledBack_RestoresTheSourceTree()
    {
        var source = Path.Combine(_tempPath, "source");
        var destination = Path.Combine(_tempPath, "destination");
        Directory.CreateDirectory(Path.Combine(source, "sub"));
        File.WriteAllText(Path.Combine(source, "a.txt"), "a");
        File.WriteAllText(Path.Combine(source, "sub", "b.txt"), "b");
        var fileOps = new FileOpsManager();

        var scope = new TransactionScope();
        fileOps.MoveDirectory(source, destination);
        Assert.False(Directory.Exists(source));
        scope.Dispose();

        Assert.False(Directory.Exists(destination));
        Assert.Equal("a", File.ReadAllText(Path.Combine(source, "a.txt")));
        Assert.Equal("b", File.ReadAllText(Path.Combine(source, "sub", "b.txt")));
    }
}
