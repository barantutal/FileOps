using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FileOps.Abstraction;

public interface IFileOpsManager
{
    void GenerateDirectory(string path);
    void MoveDirectory(string sourcePath, string destinationPath);
    void CopyDirectory(string sourcePath, string destinationPath);
    Task CopyDirectoryAsync(string sourcePath, string destinationPath);
    void DeleteDirectory(string path);
    void GenerateFile(string path, byte[] content);
    void GenerateFile(string path, IFormFile content);
    Task GenerateFileAsync(string path, byte[] content);
    Task GenerateFileAsync(string path, IFormFile content);
    void CopyFile(string sourcePath, string destinationPath);
    Task CopyFileAsync(string sourcePath, string destinationPath);
    void MoveFile(string sourcePath, string destinationPath);
    void DeleteFile(string path);
    Action OnTransactionPreparing { get; set; }
    Action OnRollback { get; set; }
}