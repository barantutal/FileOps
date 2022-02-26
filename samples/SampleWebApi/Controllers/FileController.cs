using System.Text;
using System.Transactions;
using FileOps;
using Microsoft.AspNetCore.Mvc;
using SampleWebApi.Models;

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
    public FileOperationModel GenerateRandomFiles(bool? throwEx)
    {
        var responseModel = new FileOperationModel();
        try
        {
            using var scope = new TransactionScope();
            var info1 = Guid.NewGuid().ToString();
            var info2 = Guid.NewGuid().ToString();

            var file1 = _fileOpsManager.GenerateFile($"/somePath/test1/file-{info1}.txt", Encoding.UTF8.GetBytes(info1));
            var file2 = _fileOpsManager.GenerateFile($"/somePath/test1/file-{info2}.txt", Encoding.UTF8.GetBytes(info2));
                
            responseModel.FileInfos.Add(file1);
            responseModel.FileInfos.Add(file2);

            // Content will be 'info1' for both files
            var copiedFile = _fileOpsManager.CopyFile($"/somePath/test1/file-{info1}.txt", $"/somePath/test1/file-{info2}.txt");
            responseModel.FileInfos.Add(copiedFile);
                
            if (throwEx == true)
            {
                Thread.Sleep(2000);
                throw new Exception("File operation cancelled: " + string.Join(",", responseModel.FileInfos.Select(x => x.Path)));
            }
                
            scope.Complete();
        }
        catch (Exception e)
        {
            return new FileOperationModel(e.Message);
        }

        return responseModel;
    }
}