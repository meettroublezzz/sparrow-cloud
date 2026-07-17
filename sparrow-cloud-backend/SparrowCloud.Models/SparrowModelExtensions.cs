using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SparrowCloud.Models.Union;
using SparrowCloud.Utils;

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
            // 读取默认路径
            string path = configuration["SparrowCloud:DefaultPath"]!;
            // 数据库文件路径
            path = Path.Combine(path, @".__SparrowCloud_Data__");
            // 幂等的自动创建所有不存在的文件夹
            Directory.CreateDirectory(path);

            PathHelper.SetHidden(path);

            string filePath = Path.Combine(path, @"SparrowUnion.db");

            services.AddDbContext<UnionContext>(options =>
            {
                options.UseSqlite($"Data Source={filePath}; Cache=Shared;");
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
