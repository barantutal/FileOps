using System;

namespace FileOps;

public interface IFileOpsManager
{
    public void GenerateDirectory(string path);
    public void MoveDirectory(string sourcePath, string destinationPath);
    public void CopyDirectory(string sourcePath, string destinationPath);
    public void DeleteDirectory(string path);
    public IFileInfo GenerateFile(string path, byte[] content);
    public IFileInfo CopyFile(string sourcePath, string destinationPath);
    public void DeleteFile(string path);
}