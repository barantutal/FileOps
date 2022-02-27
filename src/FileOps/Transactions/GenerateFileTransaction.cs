using System.IO;
using FileOps.Abstraction;
using FileOps.Operations;

namespace FileOps.Transactions;

public class GenerateFileTransaction : GenerateFileOperation, IFileOpsTransaction
{
    private readonly string _path;
    private string _directoryPath;
    
    public GenerateFileTransaction(string path, byte[] fileContent) : base(path, fileContent)
    {
        _path = path;
    }
    
    public override void Commit()
    {
        var directoryPath = Path.GetDirectoryName(_path);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            _directoryPath = directoryPath;
        }
        
        base.Commit();
    }

    public void RollBack()
    {
        File.Delete(_path);

        if (_directoryPath != null)
        {
            Directory.Delete(_directoryPath);
        }
    }
}