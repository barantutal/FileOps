using Xunit;

namespace FileOps.Tests.xUnit;

// Every FileOpsManager keeps its backups in one shared folder under the system temp path. Classes
// that create backups run one after another, so a test that counts that folder sees only its own.
[CollectionDefinition(Name)]
public class BackupFolderCollection
{
    public const string Name = "FileOpsManager backup folder";
}
