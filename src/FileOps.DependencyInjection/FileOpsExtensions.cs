using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileOps.DependencyInjection
{
    public static class FileOpsExtensions
    {
        public static IServiceCollection AddFileOps(this IServiceCollection services)
        {
            services.TryAddTransient<IFileOpsManager, FileOpsManager>();
            return services;
        }
    }
}
