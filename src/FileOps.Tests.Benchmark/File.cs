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

    private string _emptyPath;
    private string _generatedFilePathWithContent;
    private string _generatedFilePathWithContent2;
    private byte[] _content;
    
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
        _emptyPath = GenerateFilePath();
        _generatedFilePathWithContent = GenerateFilePath();
        _fileOpsManager.GenerateFile(_generatedFilePathWithContent, GenerateContent());
        _generatedFilePathWithContent2 = GenerateFilePath();
        _fileOpsManager.GenerateFile(_generatedFilePathWithContent2, GenerateContent());
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
    public void GenerateFile()
    {
        var transactionScope = new TransactionScope();
        _fileOpsManager.GenerateFile(_emptyPath, _content);
        transactionScope.Complete();
        transactionScope.Dispose();
    }
    
    [Benchmark]
    public void DeleteFile()
    {
        var transactionScope = new TransactionScope();
        _fileOpsManager.DeleteFile(_generatedFilePathWithContent);
        transactionScope.Complete();
        transactionScope.Dispose();
    }
    
    [Benchmark]
    public void MoveFile()
    {
        var transactionScope = new TransactionScope();
        _fileOpsManager.MoveFile(_generatedFilePathWithContent, _emptyPath);
        transactionScope.Complete();
        transactionScope.Dispose();
    }
    
    [Benchmark]
    public void CopyFile()
    {
        var transactionScope = new TransactionScope();
        _fileOpsManager.CopyFile(_generatedFilePathWithContent, _emptyPath);
        transactionScope.Complete();
        transactionScope.Dispose();
    }
    
    [Benchmark]
    public async Task CopyFileAsync()
    {
        var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await _fileOpsManager.CopyFileAsync(_generatedFilePathWithContent, _emptyPath);
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