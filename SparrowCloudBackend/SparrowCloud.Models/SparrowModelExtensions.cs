using Awesome.Net.WritableOptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SparrowCloud.Models.Intermediate;
using SparrowCloud.Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            services.AddDbContext<IntermediateContext>(options =>
            {
                // 读取连接字符串
                string conn = configuration.GetConnectionString("IntermediateContext")!;

                options.UseSqlite(conn);
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

            var dbStorage = scope.ServiceProvider.GetRequiredService<IntermediateContext>();
            // 自动建库建表
            dbStorage.Database.EnsureCreated();
        }
    }
}
