using System.IO;

namespace FileOps.Operations;

public class GenerateDirectoryOperation : IFileOpsTransaction
{
    private readonly string _path;
    private bool _directoryGenerated;
    
    public GenerateDirectoryOperation(string path)
    {
        _path = path;
    }
    
    public void Commit()
    {
        if (Directory.Exists(_path))
        {
            return;
        }
        
        Directory.CreateDirectory(_path);
        _directoryGenerated = true;
    }

    public void RollBack()
    {
        if (_directoryGenerated)
        {
            Directory.Delete(_path);
        }
    }
}