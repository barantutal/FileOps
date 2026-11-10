using System.IO;
using FileOps.Abstraction;
using FileOps.Helpers;
using FileOps.Operations;
using Microsoft.AspNetCore.Http;

namespace FileOps.Transactions;

public class GenerateFormFileTransaction : GenerateFormFileOperation, IFileOpsTransaction
{
    private readonly string _path;
    private string _createdDirectoryRoot;
    
    public GenerateFormFileTransaction(string path, IFormFile fileContent) : base(path, fileContent)
    {
        _path = path;
    }
    
    public override void Commit()
    {
        _createdDirectoryRoot = DirectoryHelper.EnsureParentDirectory(_path);
        base.Commit();
    }

    public void RollBack()
    {
        File.Delete(_path);
        DirectoryHelper.RemoveCreatedDirectories(Path.GetDirectoryName(Path.GetFullPath(_path)), _createdDirectoryRoot);
    }
}
