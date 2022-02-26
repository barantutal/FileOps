using System;

namespace FileOps.Exceptions;

public class FileOperationException : Exception
{
    public FileOperationException(string message) : base(message) { }

    public static FileOperationException MissingSourcePathException(string sourcePath)
    {
        return new FileOperationException($"Source path {sourcePath} is empty");
    }
    
    public static FileOperationException DestinationPathExistsException(string destinationPath)
    {
        return new FileOperationException($"Destination path {destinationPath} already exists.");
    }
}