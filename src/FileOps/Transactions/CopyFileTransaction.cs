using System.IO;
using FileOps.Abstraction;
using FileOps.Operations;

namespace FileOps.Transactions;

public class CopyFileTransaction : CopyFileOperation, IFileOpsTransaction
{
    private readonly string _destinationPath;
    private string _directoryPath;
    
    public CopyFileTransaction(string sourcePath, string destinationPath) : base(sourcePath, destinationPath)
    {
        _destinationPath = destinationPath;
    }
    
    public override void Commit()
    {
        var directoryPath = Path.GetDirectoryName(_destinationPath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            _directoryPath = directoryPath;
        }
        
        base.Commit();
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