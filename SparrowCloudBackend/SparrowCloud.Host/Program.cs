using SparrowCloud.Host.Middlewares;
using SparrowCloud.Models;
using SparrowCloud.Services;

namespace SparrowCloud.Host
{
    public class Program
    {
        public static void Main(string[] args)
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

            // Configure the HTTP request pipeline.

            app.UseMiddleware<HttpMethodOverrideMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.Services.InitSparrowModels();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
