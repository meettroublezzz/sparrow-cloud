using Microsoft.Extensions.DependencyInjection;
using SparrowCloud.Services.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Services
{
    public static class SparrowServiceExtensions
    {
        public static IServiceCollection AddSparrowServices(this IServiceCollection services)
        {
            AddServices(services);
            AddStorage(services);

            return services;
        }

        private static void AddServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var types = assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(ServiceBase).IsAssignableFrom(t));

            foreach (var type in types)
            {
                services.AddScoped(type);
            }
        }

        private static void AddStorage(IServiceCollection services)
        {
            services.AddScoped<StorageManager>();
        }
    }
}
