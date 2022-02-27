namespace FileOps.Abstraction;

public interface IFileOpsTransaction : IFileOps
{
    void RollBack();
}