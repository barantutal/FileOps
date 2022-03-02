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
public void GenerateRandomFiles()
{
    var fileOpsManager = new FileOpsManager();

    using var scope = new TransactionScope();

    var info1 = Guid.NewGuid().ToString();
    var info2 = Guid.NewGuid().ToString();

    fileOpsManager.GenerateFile($"/somePath/test/file-{info1}.txt", Encoding.UTF8.GetBytes(info1));
    fileOpsManager.GenerateFile($"/somePath/test/file-{info2}.txt", Encoding.UTF8.GetBytes(info2));
    fileOpsManager.CopyFile($"/somePath/test/file-{info1}.txt", $"/somePath/test/file-{Guid.NewGuid().ToString()}.txt");
             
    scope.Complete();
}
```
