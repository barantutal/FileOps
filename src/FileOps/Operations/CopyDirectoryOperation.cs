using System.IO;
using System.Threading.Tasks;
using FileOps.Abstraction;
using FileOps.Exceptions;
using FileOps.Helpers;

namespace FileOps.Operations;

public class CopyDirectoryOperation : IFileOps, IAsyncFileOps
{
    private readonly string _sourcePath;
    private readonly string _destinationPath;

    public CopyDirectoryOperation(string sourcePath, string destinationPath)
    {
        _sourcePath = sourcePath;
        _destinationPath = destinationPath;
    }
    
    public virtual void Commit()
    {
        Prepare();
        DirectoryHelper.CopyDirectory(_sourcePath, _destinationPath);
    }

    public async Task CommitAsync()
    {
        Prepare();
        await DirectoryHelper.CopyDirectoryAsync(_sourcePath, _destinationPath);
    }

    private void Prepare()
    {
        if (!Directory.Exists(_sourcePath))
        {
            throw FileOperationException.MissingSourcePathException(_sourcePath);
        }
        
        if (Directory.Exists(_destinationPath))
        {
            throw FileOperationException.DestinationPathExistsException(_destinationPath);
        }

        Directory.CreateDirectory(_destinationPath);
    }
}