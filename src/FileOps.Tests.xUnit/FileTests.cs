using System.Text;
using System.Transactions;
using Microsoft.AspNetCore.Http.Internal;
using Xunit;

namespace FileOps.Tests.xUnit;

public class FileTests : IClassFixture<FileOpsManager>
{
    private readonly FileOpsManager _fileOpsManager;
    private readonly string _tempPath;

    public FileTests(FileOpsManager fileOpsManager)
    {
        _fileOpsManager = fileOpsManager;
        _tempPath = Path.Combine(Path.GetTempPath(), "FileOpsManager-tempf-tests");

        if (Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, true);
        }

        Directory.CreateDirectory(_tempPath);
    }

    [Fact]
    public void GeneratesFile()
    {
        var path = GenerateFilePath();
        var content = GenerateContent();

        Assert.True(!File.Exists(path));

        var transactionScope = new TransactionScope();
        _fileOpsManager.GenerateFile(path, content);
        transactionScope.Complete();
        transactionScope.Dispose();

        Assert.True(File.Exists(path));
    }

    [Fact]
    public async Task GeneratesFileAsync()
    {
        var path = GenerateFilePath();
        var content = GenerateContent();

        Assert.True(!File.Exists(path));

        var transactionScope = new TransactionScope();
        await _fileOpsManager.GenerateFileAsync(path, content);
        transactionScope.Complete();
        transactionScope.Dispose();

        Assert.True(File.Exists(path));
    }
    
    [Fact]
    public void GeneratesFormFile()
    {
        var path = GenerateFilePath();

        Assert.True(!File.Exists(path));
        
        var content = GenerateContent();

        using var ms = new MemoryStream(content);
        var formFile = new FormFile(ms, 0, ms.Length, "test-file", Path.GetFileName(path));

        var transactionScope = new TransactionScope();
        _fileOpsManager.GenerateFile(path, formFile);
        transactionScope.Complete();
        transactionScope.Dispose();

        Assert.True(File.Exists(path));
    }

    [Fact]
    public async Task GeneratesFormFileAsync()
    {
        var path = GenerateFilePath();

        Assert.True(!File.Exists(path));

        var content = GenerateContent();

        using var ms = new MemoryStream(content);
        var formFile = new FormFile(ms, 0, ms.Length, "test-file", Path.GetFileName(path));
        
        var transactionScope = new TransactionScope();
        await _fileOpsManager.GenerateFileAsync(path, formFile);
        transactionScope.Complete();
        transactionScope.Dispose();

        Assert.True(File.Exists(path));
    }

    [Fact]
    public void DeletesFile()
    {
        var path = GenerateFilePath();
        var content = GenerateContent();

        Assert.True(!File.Exists(path));

        var transactionScope = new TransactionScope();
        _fileOpsManager.GenerateFile(path, content);
        Assert.True(File.Exists(path));
        _fileOpsManager.DeleteFile(path);
        transactionScope.Complete();
        transactionScope.Dispose();

        Assert.True(!File.Exists(path));
    }

    [Fact]
    public void MovesFile()
    {
        var sourcePath = GenerateFilePath();
        var destinationPath = GenerateFilePath();
        var content = GenerateContent();

        Assert.True(!File.Exists(sourcePath));

        var transactionScope = new TransactionScope();
        _fileOpsManager.GenerateFile(sourcePath, content);
        _fileOpsManager.MoveFile(sourcePath, destinationPath);
        transactionScope.Complete();
        transactionScope.Dispose();

        Assert.True(!File.Exists(sourcePath));
        Assert.True(File.Exists(destinationPath));
    }

    [Fact]
    public void CopiesFile()
    {
        var sourcePath = GenerateFilePath();
        var destinationPath = GenerateFilePath();
        var content = GenerateContent();

        Assert.True(!File.Exists(sourcePath));
        Assert.True(!File.Exists(destinationPath));

        var transactionScope = new TransactionScope();
        _fileOpsManager.GenerateFile(sourcePath, content);
        _fileOpsManager.CopyFile(sourcePath, destinationPath);
        transactionScope.Complete();
        transactionScope.Dispose();

        Assert.True(File.Exists(sourcePath));
        Assert.True(File.Exists(destinationPath));
    }

    [Fact]
    public async Task CopiesFileAsync()
    {
        var sourcePath = GenerateFilePath();
        var destinationPath = GenerateFilePath();
        var content = GenerateContent();

        Assert.True(!File.Exists(sourcePath));
        Assert.True(!File.Exists(destinationPath));

        var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await _fileOpsManager.GenerateFileAsync(sourcePath, content);
        await _fileOpsManager.CopyFileAsync(sourcePath, destinationPath);
        transactionScope.Complete();
        transactionScope.Dispose();

        Assert.True(File.Exists(sourcePath));
        Assert.True(File.Exists(destinationPath));
    }

    private string GenerateFilePath()
    {
        return Path.Combine(_tempPath, $"{Guid.NewGuid()}.txt");
    }

    private byte[] GenerateContent()
    {
        return Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
    }
}