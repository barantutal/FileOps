using System.Threading.Tasks;

namespace FileOps.Abstraction;

public interface IAsyncFileOps
{
    Task CommitAsync();
}