namespace FileOps;

public interface IFileStoreTransaction
{
    void Commit();
    void RollBack();
}