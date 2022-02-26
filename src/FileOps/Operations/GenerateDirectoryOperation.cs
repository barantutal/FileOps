using System.IO;

namespace FileOps.Operations;

public class GenerateDirectoryOperation : IFileOpsTransaction
{
    private readonly string _fullPath;
    private bool _isDirectoryExists;
    
    public GenerateDirectoryOperation(string fullPath)
    {
        _fullPath = fullPath;
    }
    
    public void Commit()
    {
        if (Directory.Exists(_fullPath))
        {
            _isDirectoryExists = true;
        }
        else
        {
            Directory.CreateDirectory(_fullPath);
        }
    }

    public void RollBack()
    {
        if (!_isDirectoryExists)
        {
            Directory.Delete(_fullPath);
        }
    }
}