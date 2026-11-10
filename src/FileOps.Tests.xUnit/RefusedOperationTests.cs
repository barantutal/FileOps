using System.Transactions;
using FileOps.Exceptions;
using Xunit;

namespace FileOps.Tests.xUnit;

// A refused operation has changed nothing on disk. It must surface its own error and must not be
// rolled back with the rest of the scope: undoing it would act on paths it never touched.
[Collection(BackupFolderCollection.Name)]
public class RefusedOperationTests
{
    private readonly string _tempPath;

    public RefusedOperationTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "FileOpsManager-refused-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempPath);
    }

    [Theory]
    [InlineData("E5", "E5234")]
    [InlineData("Schierstein Dotzheim", "Schierstein Dotzheim (Izmir)")]
    public void MoveDirectory_ToANameStartingWithTheOldName_Moves(string oldName, string newName)
    {
        var sourcePath = Path.Combine(_tempPath, oldName);
        var destinationPath = Path.Combine(_tempPath, newName);
        Directory.CreateDirectory(sourcePath);
        File.WriteAllText(Path.Combine(sourcePath, "a.txt"), "a");
        var fileOps = new FileOpsManager();

        using (var scope = new TransactionScope())
        {
            fileOps.MoveDirectory(sourcePath, destinationPath);
            scope.Complete();
        }

        Assert.False(Directory.Exists(sourcePath));
        Assert.True(File.Exists(Path.Combine(destinationPath, "a.txt")));
    }

    [Fact]
    public void MoveDirectory_IntoItsOwnChild_IsRefusedWithItsOwnError()
    {
        var sourcePath = Path.Combine(_tempPath, "E5");
        Directory.CreateDirectory(sourcePath);
        File.WriteAllText(Path.Combine(sourcePath, "a.txt"), "a");
        var fileOps = new FileOpsManager();

        var scope = new TransactionScope();
        Assert.Throws<FileOperationException>(() =>
            fileOps.MoveDirectory(sourcePath, Path.Combine(sourcePath, "child")));

        // Not completed: the scope rolls back, and the refused move must not be part of it.
        scope.Dispose();

        Assert.True(File.Exists(Path.Combine(sourcePath, "a.txt")));
    }

    [Fact]
    public void MoveDirectory_IntoItsOwnChild_IsRefusedOutsideATransaction()
    {
        var sourcePath = Path.Combine(_tempPath, "E5");
        Directory.CreateDirectory(sourcePath);

        Assert.Throws<FileOperationException>(() =>
            new FileOpsManager().MoveDirectory(sourcePath, Path.Combine(sourcePath, "child")));
    }

    [Fact]
    public void RefusedCopyFile_KeepsTheExistingDestination()
    {
        var sourcePath = Seed("source");
        var destinationPath = Seed("existing");
        var fileOps = new FileOpsManager();

        var scope = new TransactionScope();
        Assert.Throws<FileOperationException>(() => fileOps.CopyFile(sourcePath, destinationPath));
        scope.Dispose();

        Assert.Equal("existing", File.ReadAllText(destinationPath));
    }

    [Fact]
    public void RefusedMoveFile_KeepsTheExistingDestination()
    {
        var sourcePath = Seed("source");
        var destinationPath = Seed("existing");
        var fileOps = new FileOpsManager();

        var scope = new TransactionScope();
        Assert.Throws<FileOperationException>(() => fileOps.MoveFile(sourcePath, destinationPath));
        scope.Dispose();

        Assert.Equal("source", File.ReadAllText(sourcePath));
        Assert.Equal("existing", File.ReadAllText(destinationPath));
    }

    [Fact]
    public void RefusedCopyDirectory_KeepsTheExistingDestination()
    {
        var sourcePath = Path.Combine(_tempPath, "source");
        var destinationPath = Path.Combine(_tempPath, "existing");
        Directory.CreateDirectory(sourcePath);
        Directory.CreateDirectory(destinationPath);
        File.WriteAllText(Path.Combine(destinationPath, "keep.txt"), "keep");
        var fileOps = new FileOpsManager();

        var scope = new TransactionScope();
        Assert.Throws<FileOperationException>(() => fileOps.CopyDirectory(sourcePath, destinationPath));
        scope.Dispose();

        Assert.True(File.Exists(Path.Combine(destinationPath, "keep.txt")));
    }

    [Fact]
    public void RefusedOperation_DoesNotStopEarlierOperationsFromRollingBack()
    {
        var movedSource = Seed("moved");
        var movedDestination = Path.Combine(_tempPath, $"{Guid.NewGuid()}.txt");
        var existing = Seed("existing");
        var fileOps = new FileOpsManager();

        var scope = new TransactionScope();
        fileOps.MoveFile(movedSource, movedDestination);
        Assert.Throws<FileOperationException>(() => fileOps.CopyFile(movedDestination, existing));
        scope.Dispose();

        Assert.True(File.Exists(movedSource));
        Assert.False(File.Exists(movedDestination));
        Assert.Equal("existing", File.ReadAllText(existing));
    }

    private string Seed(string content)
    {
        var path = Path.Combine(_tempPath, $"{Guid.NewGuid()}.txt");
        File.WriteAllText(path, content);
        return path;
    }
}
