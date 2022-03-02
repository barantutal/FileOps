using System.Transactions;
using BenchmarkDotNet.Attributes;

namespace FileOps.Tests.Benchmark;

[RPlotExporter]
public class DirectoryBenchmark
{
    private FileOpsManager _fileOpsManager;
    private string _tempPath;

    private string _emptyPath;
    private string _directoryPath;
    
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

    [IterationSetup]
    public void IterationSetup()
    {
        _emptyPath = GenerateDirectoryPath();
        _directoryPath = GenerateDirectoryPath();
        _fileOpsManager.GenerateDirectory(_directoryPath);
    }
    
    [IterationCleanup]
    public void IterationCleanup()
    {
        if(Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, true);
        }
        
        Directory.CreateDirectory(_tempPath);
    }
    
    [Benchmark]
    public void GenerateDirectory()
    {
        var transactionScope = new TransactionScope();
        _fileOpsManager.GenerateDirectory(_emptyPath);
        transactionScope.Complete();
        transactionScope.Dispose();
    }
    
    [Benchmark]
    public void DeleteDirectory()
    {
        var transactionScope = new TransactionScope();
        _fileOpsManager.DeleteDirectory(_directoryPath);
        transactionScope.Complete();
        transactionScope.Dispose(); 
    }
    
    [Benchmark]
    public void MoveDirectory()
    {
        var transactionScope = new TransactionScope();
        _fileOpsManager.MoveDirectory(_directoryPath, _emptyPath);
        transactionScope.Complete();
        transactionScope.Dispose();
    }
    
    [Benchmark]
    public void CopyDirectory()
    {
        var transactionScope = new TransactionScope();
        _fileOpsManager.CopyDirectory(_directoryPath, _emptyPath);
        transactionScope.Complete();
        transactionScope.Dispose();
    }
    
    [Benchmark]
    public async Task CopyDirectoryAsync()
    {
        var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await _fileOpsManager.CopyDirectoryAsync(_directoryPath, _emptyPath);
        transactionScope.Complete();
        transactionScope.Dispose();
    }

    private string GenerateDirectoryPath()
    {
        return Path.Combine(_tempPath, Guid.NewGuid().ToString());
    }
}