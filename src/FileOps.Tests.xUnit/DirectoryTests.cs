using System.Text;
using System.Transactions;
using Xunit;

namespace FileOps.Tests.xUnit;

public class DirectoryTests : IClassFixture<FileOpsManager>
{
    private readonly FileOpsManager _fileOpsManager;
    private readonly string _tempPath;
    
    public DirectoryTests(FileOpsManager fileOpsManager)
    {
        _fileOpsManager = fileOpsManager;
        _tempPath = Path.Combine(Path.GetTempPath(), "FileOpsManager-tempf-tests");
        
        if(Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, true);
        }
        
        Directory.CreateDirectory(_tempPath);
    }
    
    [Fact]
    public void GeneratesDirectory()
    {
        var path = GenerateDirectoryPath();
        
        Assert.True(!Directory.Exists(path));
        
        var transactionScope = new TransactionScope();
        _fileOpsManager.GenerateDirectory(path);
        transactionScope.Complete();
        transactionScope.Dispose();
        
        Assert.True(Directory.Exists(path));
    }
    
    [Fact]
    public void DeletesDirectory()
    {
        var path = GenerateDirectoryPath();
        
        Assert.True(!Directory.Exists(path));
        
        var transactionScope = new TransactionScope();
        _fileOpsManager.GenerateDirectory(path);
        _fileOpsManager.DeleteDirectory(path);
        transactionScope.Complete();
        transactionScope.Dispose();
        
        Assert.True(!Directory.Exists(path));
    }
    
    [Fact]
    public void MovesDirectory()
    {
        var sourcePath = GenerateDirectoryPath();
        var destinationPath = GenerateDirectoryPath();

        Assert.True(!Directory.Exists(sourcePath));
        Assert.True(!Directory.Exists(destinationPath));
        
        var transactionScope = new TransactionScope();
        _fileOpsManager.GenerateDirectory(sourcePath);
        _fileOpsManager.MoveDirectory(sourcePath, destinationPath);
        transactionScope.Complete();
        transactionScope.Dispose();
        
        Assert.True(!Directory.Exists(sourcePath));
        Assert.True(Directory.Exists(destinationPath));
    }
    
    [Fact]
    public void CopiesDirectory()
    {
        var sourcePath = GenerateDirectoryPath();
        var destinationPath = GenerateDirectoryPath();

        Assert.True(!Directory.Exists(sourcePath));
        Assert.True(!Directory.Exists(destinationPath));
        
        var transactionScope = new TransactionScope();
        _fileOpsManager.GenerateDirectory(sourcePath);
        _fileOpsManager.CopyDirectory(sourcePath, destinationPath);
        transactionScope.Complete();
        transactionScope.Dispose();
        
        Assert.True(Directory.Exists(sourcePath));
        Assert.True(Directory.Exists(destinationPath));
    }
    
    private string GenerateDirectoryPath()
    {
        return Path.Combine(_tempPath, Guid.NewGuid().ToString());
    }
}