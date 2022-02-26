using System;
using System.IO;
using FileOps.Exceptions;

namespace FileOps.Operations;

public sealed class GenerateFileOperation : IFileOpsTransaction
{
    private readonly string _path;
    private readonly byte[] _fileContent;
    private string _directoryPath;
    
    public GenerateFileOperation(string path, byte[] fileContent)
    {
        _path = path;
        _fileContent = fileContent;
    }
    
    public void Commit()
    {
        if (File.Exists(_path))
        {
            throw FileOperationException.DestinationPathExistsException(_path);
        }
        
        var directoryPath = Path.GetDirectoryName(_path);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            _directoryPath = directoryPath;
        }
        
        File.WriteAllBytes(_path, _fileContent);
        Array.Clear(_fileContent, 0, _fileContent.Length);
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