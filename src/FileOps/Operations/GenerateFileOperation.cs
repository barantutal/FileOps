using System;
using System.Collections.Generic;
using System.IO;
using FileOps.Backup;

namespace FileOps.Operations;

public sealed class GenerateFileOperation : IFileOpsTransaction
{
    private readonly string _path;
    private readonly byte[] _fileContent;

    public GenerateFileOperation(string path, byte[] fileContent)
    {
        _path = path;
        _fileContent = fileContent;
    }
    
    public void Commit()
    {
        if (File.Exists(_path)) return;
        
        File.WriteAllBytes(_path, _fileContent);
        Array.Clear(_fileContent, 0, _fileContent.Length);
    }

    public void RollBack()
    {
        // Ignore
    }
}