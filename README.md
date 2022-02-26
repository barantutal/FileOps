# FileOps
FileOps is a transactional file operations library.

### Configuration

You can configure in your Startup.cs:

```cs
public void ConfigureServices(IServiceCollection services)
{
    //......

    services.AddFileOps();
}
```

```cs
private readonly IFileOpsManager _fileOpsManager;

public MyService(IFileOpsManager fileOpsManager)
{
    _fileOpsManager = fileOpsManager;
}
```

Or you can directly create a new instance:

```cs
var fileOpsManager = new FileOpsManager();
```

### Usage

```cs
public FileController(IFileOpsManager fileOpsManager)
{
    _fileOpsManager = fileOpsManager;
}

public void GenerateRandomFiles()
{
    using var scope = new TransactionScope();

    var info1 = Guid.NewGuid().ToString();
    var info2 = Guid.NewGuid().ToString();

    var file1 = _fileOpsManager.GenerateFile($"/somePath/test1/file-{info1}.txt", Encoding.UTF8.GetBytes(info1));
    var file2 = _fileOpsManager.GenerateFile($"/somePath/test1/file-{info2}.txt", Encoding.UTF8.GetBytes(info2));
                    
    // Content will be 'info1' for both files
    var copiedFile = _fileOpsManager.CopyFile($"/somePath/test1/file-{info1}.txt", $"/somePath/test1/file-{info2}.txt");
             
    scope.Complete();
}
```
