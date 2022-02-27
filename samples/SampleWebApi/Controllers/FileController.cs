using System.Text;
using System.Transactions;
using FileOps.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace SampleWebApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class FileController : ControllerBase
{
    private readonly IFileOpsManager _fileOpsManager;

    public FileController(IFileOpsManager fileOpsManager)
    {
        _fileOpsManager = fileOpsManager;
    }

    [HttpGet(Name = "GenerateRandomFiles")]
    public IActionResult GenerateRandomFiles(bool? throwEx)
    {
        using var scope = new TransactionScope();
        var info1 = Guid.NewGuid().ToString();
        var info2 = Guid.NewGuid().ToString();

        _fileOpsManager.GenerateFile($"/somePath/test1/file-{info1}.txt", Encoding.UTF8.GetBytes(info1));
        _fileOpsManager.GenerateFile($"/somePath/test1/file-{info2}.txt", Encoding.UTF8.GetBytes(info2));

        // Content will be 'info1' for both files
        _fileOpsManager.CopyFile($"/somePath/test1/file-{info1}.txt", $"/somePath/test1/file-{info2}.txt");
                
        if (throwEx == true)
        {
            Thread.Sleep(2000);
            throw new Exception("File operation cancelled");
        }
                
        scope.Complete();

        return Ok();
    }
}