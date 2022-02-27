using System;

namespace FileOps.Abstraction;

public interface IFileOpsManager
{
    public void GenerateDirectory(string path);
    public void MoveDirectory(string sourcePath, string destinationPath);
    public void CopyDirectory(string sourcePath, string destinationPath);
    public void DeleteDirectory(string path);
    public void GenerateFile(string path, byte[] content);
    public void CopyFile(string sourcePath, string destinationPath);
    public void MoveFile(string sourcePath, string destinationPath);
    public void DeleteFile(string path);
    Action OnTransactionPreparing { get; set; }
    Action OnRollback { get; set; }
}