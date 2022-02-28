using System.IO;
using System.Threading.Tasks;
using FileOps.Abstraction;
using FileOps.Operations;

namespace FileOps.Transactions;

public class CopyFileTransaction : CopyFileOperation, IAsyncFileOpsTransaction
{
    private readonly string _destinationPath;
    private string _directoryPath;
    
    public CopyFileTransaction(string sourcePath, string destinationPath) : base(sourcePath, destinationPath)
    {
        _destinationPath = destinationPath;
    }
    
    public override void Commit()
    {
        PrepareBackup();
        base.Commit();
    }
    
    public override async Task CommitAsync()
    {
        PrepareBackup();
        await base.CommitAsync();
    }

    private void PrepareBackup()
    {
        var directoryPath = Path.GetDirectoryName(_destinationPath);
        if (Directory.Exists(directoryPath)) return;
        
        Directory.CreateDirectory(directoryPath);
        _directoryPath = directoryPath;
    }

    public void RollBack()
    {
        File.Delete(_destinationPath);
        
        if (_directoryPath != null)
        {
            Directory.Delete(_directoryPath);
        }
    }
}