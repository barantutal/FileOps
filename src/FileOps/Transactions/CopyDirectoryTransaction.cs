using System.IO;
using FileOps.Abstraction;
using FileOps.Operations;

namespace FileOps.Transactions;

public class CopyDirectoryTransaction : CopyDirectoryOperation, IFileOpsTransaction
{
    private readonly string _destinationPath;

    public CopyDirectoryTransaction(string sourcePath, string destinationPath) : base(sourcePath, destinationPath)
    {
        _destinationPath = destinationPath;
    }

    public void RollBack()
    {
        Directory.Delete(_destinationPath, true);
    }
}