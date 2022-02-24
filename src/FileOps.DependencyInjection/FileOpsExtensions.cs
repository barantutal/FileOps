using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileOps.DependencyInjection
{
    public static class FileOpsExtensions
    {
        public static IServiceCollection AddFileStore(this IServiceCollection services)
        {
            return services.AddFileOps(null);
        }

        public static IServiceCollection AddFileOps(this IServiceCollection services, Action<FileOpsOptions>? configure)
        {
            var fileStoreOptions = new FileOpsOptions("/");
            configure?.Invoke(fileStoreOptions);

            services.TryAddTransient<IFileOpsManager>(x => ActivatorUtilities.CreateInstance<FileOpsManager>(x, fileStoreOptions));
            return services;
        }
    }
}
