using System.Text;
using System.Transactions;
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
        
        if(Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, true);
        }
        
        Directory.CreateDirectory(_tempPath);
    }
    
    [Fact]
    public void GeneratesFile()
    {
        var path = GenerateFilePath();
        Assert.True(!File.Exists(path));
        
        var transactionScope = new TransactionScope();
        var content = GenerateContent();
        _fileOpsManager.GenerateFile(path, content);
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

    private string GenerateFilePath()
    {
        return Path.Combine(_tempPath, $"{Guid.NewGuid()}.txt");
    }

    private byte[] GenerateContent()
    {
        return Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
    }
}