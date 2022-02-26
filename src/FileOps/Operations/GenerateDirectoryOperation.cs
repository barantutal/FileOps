using System.IO;

namespace FileOps.Operations;

public class GenerateDirectoryOperation : IFileOpsTransaction
{
    private readonly string _fullPath;
    private bool _directoryGenerated;
    
    public GenerateDirectoryOperation(string fullPath)
    {
        _fullPath = fullPath;
    }
    
    public void Commit()
    {
        if (Directory.Exists(_fullPath))
        {
            return;
        }
        
        Directory.CreateDirectory(_fullPath);
        _directoryGenerated = true;
    }

    public void RollBack()
    {
        if (_directoryGenerated)
        {
            Directory.Delete(_fullPath);
        }
    }
}