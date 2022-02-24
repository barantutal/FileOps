using System.Collections.Generic;
using System.IO;

namespace FileOps.Backup;

public static class ClearBackupHelper
{
    public static void Execute(List<string> _backupPaths)
    {
        foreach (var backupPath in _backupPaths)
        {
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }

            if (Directory.Exists(backupPath))
            {
                Directory.Delete(backupPath, true);
            }
        }
    }
}