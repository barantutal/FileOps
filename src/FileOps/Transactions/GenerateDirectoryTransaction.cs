using System.IO;
using FileOps.Abstraction;
using FileOps.Operations;

namespace FileOps.Transactions;

public class GenerateDirectoryTransaction : GenerateDirectoryOperation, IFileOpsTransaction
{
    private readonly string _path;
    
    public GenerateDirectoryTransaction(string path) : base(path)
    {
        _path = path;
    }

    public void RollBack()
    {
        Directory.Delete(_path);
    }
}