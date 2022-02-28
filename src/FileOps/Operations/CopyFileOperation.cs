using System.IO;
using System.Threading.Tasks;
using FileOps.Abstraction;
using FileOps.Exceptions;
using FileOps.Helpers;

namespace FileOps.Operations;

public class CopyFileOperation : IFileOps, IAsyncFileOps
{
    private readonly string _sourcePath;
    private readonly string _destinationPath;
    
    public CopyFileOperation(string sourcePath, string destinationPath) 
    {
        _sourcePath = sourcePath;
        _destinationPath = destinationPath;
    }
    
    public virtual void Commit()
    {
        Prepare();
        File.Copy(_sourcePath, _destinationPath);
    }

    public virtual async Task CommitAsync()
    {
        Prepare();
        await FileHelper.CopyFileAsync(_sourcePath, _destinationPath);
    }

    private void Prepare()
    {
        if (!File.Exists(_sourcePath))
        {
            throw FileOperationException.MissingSourcePathException(_sourcePath);
        }
        
        if (File.Exists(_destinationPath))
        {
            throw FileOperationException.DestinationPathExistsException(_destinationPath);
        }
    }
}