namespace FileOps;

public interface IFileOpsTransaction
{
    void Commit();
    void RollBack();
}