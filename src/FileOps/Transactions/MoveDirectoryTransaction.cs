using System.IO;
using FileOps.Abstraction;
using FileOps.Exceptions;
using FileOps.Operations;

namespace FileOps.Transactions;

// A directory move is a rename on one volume, so it is undone by the reverse rename. No backup copy
// of the tree is taken: copying a large folder before every move took longer than the transaction
// timeout, and the move it protected cannot cross volumes in the first place.
public class MoveDirectoryTransaction : MoveDirectoryOperation, IFileOpsTransaction
{
    private readonly string _sourcePath;
    private readonly string _destinationPath;
    private bool _moved;

    // tempPath is unused and kept for source compatibility with callers of the public constructor.
    public MoveDirectoryTransaction(string sourcePath, string destinationPath, string tempPath) : base(sourcePath, destinationPath)
    {
        _sourcePath = sourcePath;
        _destinationPath = destinationPath;
    }

    public override void Commit()
    {
        base.Commit();
        _moved = true;
    }

    public void RollBack()
    {
        if (!_moved || !Directory.Exists(_destinationPath))
        {
            return;
        }

        // Something recreated the source after the move. Merging or deleting either tree would
        // destroy data this transaction does not own, so both are kept and the failure is reported.
        if (Directory.Exists(_sourcePath))
        {
            throw new FileOperationException(
                $"Cannot move {_destinationPath} back to {_sourcePath}: the source path exists again.");
        }

        Directory.Move(_destinationPath, _sourcePath);
    }
}
