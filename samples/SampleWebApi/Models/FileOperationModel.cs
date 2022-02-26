using FileOps;

namespace SampleWebApi.Models;

public record FileOperationModel
{
    public FileOperationModel()
    {
        
    }
    
    public FileOperationModel(string exception)
    {
        Exception = exception;
    }
    
    public string? Exception { get; set; }
    public List<IFileInfo> FileInfos { get; set; } = new();
}