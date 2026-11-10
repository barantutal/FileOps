using System.IO;
using System.Threading.Tasks;
using FileOps.Abstraction;
using FileOps.Helpers;
using FileOps.Operations;

namespace FileOps.Transactions;

public class CopyFileTransaction : CopyFileOperation, IAsyncFileOpsTransaction
{
    private readonly string _destinationPath;
    private string _createdDirectoryRoot;
    
    public CopyFileTransaction(string sourcePath, string destinationPath) : base(sourcePath, destinationPath)
    {
        _destinationPath = destinationPath;
    }
    
    public override void Commit()
    {
        _createdDirectoryRoot = DirectoryHelper.EnsureParentDirectory(_destinationPath);
        base.Commit();
    }
    
    public override async Task CommitAsync()
    {
        _createdDirectoryRoot = DirectoryHelper.EnsureParentDirectory(_destinationPath);
        await base.CommitAsync();
    }

    public void RollBack()
    {
        File.Delete(_destinationPath);
        DirectoryHelper.RemoveCreatedDirectories(Path.GetDirectoryName(Path.GetFullPath(_destinationPath)), _createdDirectoryRoot);
    }
}
