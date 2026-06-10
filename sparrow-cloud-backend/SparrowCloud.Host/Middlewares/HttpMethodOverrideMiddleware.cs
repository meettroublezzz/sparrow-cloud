using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SparrowCloud.Host.Middlewares
{
    /// <summary>
    /// 请求方法重写（用于兼容RESTful最佳实践）
    /// </summary>
    public class HttpMethodOverrideMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpMethodOverrideMiddleware> _logger;

        public HttpMethodOverrideMiddleware(RequestDelegate next, ILogger<HttpMethodOverrideMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;

            bool exists = request.Headers.TryGetValue("X-Http-Method-Override", out var head);
            string method = request.Method.ToUpperInvariant();

            if (exists && method == "POST" && !string.IsNullOrWhiteSpace(head))
            {
                // 覆盖请求方法（GET / PUT / DELETE / PATCH 等）
                request.Method = head.ToString().Trim().ToUpperInvariant();
            }

            await _next(context);
        }
    }
}
