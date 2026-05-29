using Microsoft.EntityFrameworkCore;
using SparrowCloud.Host.Middlewares;
using SparrowCloud.Models;
using SparrowCloud.Models.Intermediate;
using SparrowCloud.Services;
using SparrowCloud.Services.Storage;

namespace SparrowCloud.Host
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    // 忽略循环引用（最常用，解决EF Core导航属性死循环）
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

                    // 驼峰命名
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

                    // 日期格式化
                    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";

                    // 不返回 null 字段
                    //options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });

            builder.Services.AddSparrowModels(builder.Configuration);
            builder.Services.AddSparrowServices();

            var app = builder.Build();

            // 初始化工作
            await InitAsync(app);

            // Configure the HTTP request pipeline.

            app.UseMiddleware<HttpMethodOverrideMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        /// <summary>
        /// 进行初始化工作
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        private static async Task InitAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // 初始化数据库层的上下文
                app.Services.InitSparrowModels();

                var intermediateContext = scope.ServiceProvider.GetRequiredService<IntermediateContext>();

                // 初始化 文件库管理器
                await StorageManager.InitAsync(intermediateContext);
            }
            catch (Exception ex)
            {
                logger.LogError($"Program.InitAsync 初始化工作失败：\n{ex}");

                throw;
            }
        }
    }
}
