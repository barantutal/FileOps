using FileOps.Abstraction;
using FileOps.Helpers;
using FileOps.Operations;

namespace FileOps.Transactions;

public class GenerateDirectoryTransaction : GenerateDirectoryOperation, IFileOpsTransaction
{
    private readonly string _path;
    private string _createdDirectoryRoot;
    
    public GenerateDirectoryTransaction(string path) : base(path)
    {
        _path = path;
    }

    public override void Commit()
    {
        // Creating "a/b/c" also creates a missing "a" and "a/b"; the rollback removes those too.
        var createdDirectoryRoot = DirectoryHelper.TopMostMissingDirectory(_path);
        base.Commit();
        _createdDirectoryRoot = createdDirectoryRoot;
    }

    public void RollBack()
    {
        DirectoryHelper.RemoveCreatedDirectories(_path, _createdDirectoryRoot);
    }
}
