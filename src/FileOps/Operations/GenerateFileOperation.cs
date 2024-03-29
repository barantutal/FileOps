using System;
using System.IO;
using System.Threading.Tasks;
using FileOps.Abstraction;
using FileOps.Exceptions;

namespace FileOps.Operations;

public class GenerateFileOperation : IFileOps, IAsyncFileOps
{
    private readonly string _path;
    private readonly byte[] _fileContent;
    
    public GenerateFileOperation(string path, byte[] fileContent)
    {
        _path = path;
        _fileContent = fileContent;
    }
    
    public virtual void Commit()
    {
        if (File.Exists(_path))
        {
            throw FileOperationException.DestinationPathExistsException(_path);
        }
        
        File.WriteAllBytes(_path, _fileContent);
        Array.Clear(_fileContent, 0, _fileContent.Length);
    }

    public async Task CommitAsync()
    {
        if (File.Exists(_path))
        {
            throw FileOperationException.DestinationPathExistsException(_path);
        }
        
        await File.WriteAllBytesAsync(_path, _fileContent);
        Array.Clear(_fileContent, 0, _fileContent.Length);
    }
}