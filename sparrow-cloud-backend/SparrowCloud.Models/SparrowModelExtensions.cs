using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SparrowCloud.Models.Union;

namespace SparrowCloud.Models
{
    /// <summary>
    /// 注册实体类上下文到容器里
    /// </summary>
    public static class SparrowModelExtensions
    {
        /// <summary>
        /// 添加实体类上下文
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSparrowModels(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<UnionContext>(options =>
            {
                // 读取连接字符串
                string path = configuration["SparrowConfigs:MainPath"]!;
                // 数据库文件路径
                path = Path.Combine(path, @"SparrowUnion.db");

                options.UseSqlite($"Data Source={path}; Cache=Shared;");
            });

            return services;
        }

        /// <summary>
        /// 初始化上下文
        /// </summary>
        /// <param name="serviceProvider"></param>
        public static void InitSparrowModels(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var dbStorage = scope.ServiceProvider.GetRequiredService<UnionContext>();
            // 自动建库建表
            dbStorage.Database.EnsureCreated();
        }
    }
}
