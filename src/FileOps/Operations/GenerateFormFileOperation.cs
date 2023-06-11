using System.IO;
using System.Threading.Tasks;
using FileOps.Abstraction;
using FileOps.Exceptions;
using Microsoft.AspNetCore.Http;

namespace FileOps.Operations;

public class GenerateFormFileOperation : IFileOps, IAsyncFileOps
{
    private readonly string _path;
    private readonly IFormFile _fileContent;
    
    public GenerateFormFileOperation(string path, IFormFile fileContent)
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
        
        using var stream = File.Create(_path);
        _fileContent.CopyTo(stream);
    }

    public async Task CommitAsync()
    {
        if (File.Exists(_path))
        {
            throw FileOperationException.DestinationPathExistsException(_path);
        }

        await using var stream = File.Create(_path);
        await _fileContent.CopyToAsync(stream);
    }
}