using System;

namespace FileOps;

public interface IFileOpsManager
{
    public void GenerateDirectory(string path);
    public void MoveDirectory(string sourcePath, string destinationPath);
    public IFileInfo GenerateFile(string path, byte[] content);
    public IFileInfo CopyFile(string path, string pathToCopy);
    public IFileInfo CopyFile(Guid id, string pathToCopy);
    public void DeleteFile(string path);
}