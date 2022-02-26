using System;

namespace FileOps;

public interface IFileInfo
{
    string Name { get; }
    string Path { get; }
    DateTime ModifiedDate { get; }
    DateTime CreatedDate { get; }
    long Size { get; }
}