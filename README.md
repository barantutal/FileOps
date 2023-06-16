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

A sample usage with a database transaction.

```cs
var executionStrategy = _context.Database.CreateExecutionStrategy();

var fileOps = new FileOpsManager();

await executionStrategy.ExecuteAsync(async () =>
{
    using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

    var path = "/var/path/file.ext";

    await fileOps.GenerateFileAsync(path, new byte[] {});

    _context.Add(new DbFile() { Path = path });

    await _context.SaveChangesAsync();

    scope.Complete();
});
```    

```cs
public void GenerateRandomFiles()
{
    var fileOpsManager = new FileOpsManager();

    using var scope = new TransactionScope();

    var info = Guid.NewGuid().ToString();

    fileOpsManager.GenerateFile($"/somePath/test/file-{info}.txt", Encoding.UTF8.GetBytes(info));
    fileOpsManager.CopyFile($"/somePath/test/file-{info}.txt", $"/somePath/test/file-{Guid.NewGuid().ToString()}.txt");
             
    scope.Complete();
}
```
