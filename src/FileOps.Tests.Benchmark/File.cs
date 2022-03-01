using System.Text;
using System.Threading.Channels;
using System.Transactions;
using BenchmarkDotNet.Attributes;

namespace FileOps.Tests.Benchmark;

[RPlotExporter]
public class File
{
    private FileOpsManager _fileOpsManager;
    private string _tempPath;
    
    [GlobalSetup]
    public void Setup()
    {
        _fileOpsManager = new FileOpsManager();
        _tempPath = Path.Combine(Path.GetTempPath(), "FileOpsManager-tempf-tests");

        if(Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, true);
        }
        
        Directory.CreateDirectory(_tempPath);
    }
    
    [Benchmark]
    public void GenerateFile()
    {
        var path = GenerateFilePath();
        var content = GenerateContent();
        
        var transactionScope = new TransactionScope();
        _fileOpsManager.GenerateFile(path, content);
        transactionScope.Complete();
        transactionScope.Dispose();
    }
    
    [Benchmark]
    public void DeleteFile()
    {
        var path = GenerateFilePath();
        var content = GenerateContent();
        
        var transactionScope = new TransactionScope();
        _fileOpsManager.GenerateFile(path, content);
        _fileOpsManager.DeleteFile(path);
        transactionScope.Complete();
        transactionScope.Dispose();
    }
    
    [Benchmark]
    public void MoveFile()
    {
        var sourcePath = GenerateFilePath();
        var destinationPath = GenerateFilePath();
        var content = GenerateContent();
        
        var transactionScope = new TransactionScope();
        _fileOpsManager.GenerateFile(sourcePath, content);
        _fileOpsManager.MoveFile(sourcePath, destinationPath);
        transactionScope.Complete();
        transactionScope.Dispose();
    }
    
    [Benchmark]
    public void CopyFile()
    {
        var sourcePath = GenerateFilePath();
        var destinationPath = GenerateFilePath();
        var content = GenerateContent();

        var transactionScope = new TransactionScope();
        _fileOpsManager.GenerateFile(sourcePath, content);
        _fileOpsManager.CopyFile(sourcePath, destinationPath);
        transactionScope.Complete();
        transactionScope.Dispose();
    }
    
    [Benchmark]
    public async Task CopyFileAsync()
    {
        var sourcePath = GenerateFilePath();
        var destinationPath = GenerateFilePath();
        var content = GenerateContent();

        var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _fileOpsManager.GenerateFile(sourcePath, content);
        await _fileOpsManager.CopyFileAsync(sourcePath, destinationPath);
        transactionScope.Complete();
        transactionScope.Dispose();
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