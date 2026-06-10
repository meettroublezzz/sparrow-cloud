using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Text;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;

namespace SparrowCloud.Contracts.ResponseModels
{
    /// <summary>
    /// SSE标准响应
    /// </summary>
    public class SSEResponse : IDisposable
    {
        private readonly HttpResponse _httpResponse;
        private readonly StringBuilder _buff = new StringBuilder();

        public SSEResponse(HttpResponse httpResponse)
        {
            _httpResponse = httpResponse;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public async Task InitAsync()
        {
            _httpResponse.Headers.Add("Content-Type", "text/event-stream; charset=utf-8");
            _httpResponse.Headers.Add("Cache-Control", "no-cache");
            _httpResponse.Headers.Add("Connection", "keep-alive");
            _httpResponse.Headers.Add("X-Accel-Buffering", "no"); // 禁用Nginx缓冲
            
            await _httpResponse.Body.FlushAsync();
        }

        /// <summary>
        /// 发送原始sse流（有缓冲区）
        /// </summary>
        /// <param name="event"></param>
        /// <param name="data"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public void Write(string @event, string data, string id = null)
        {
            // 事件类型：event: xxx
            if (!string.IsNullOrEmpty(@event))
            {
                _buff.Append("event: ").Append(@event).Append('\n');
            }

            // 事件ID：id: xxx
            if (!string.IsNullOrEmpty(id))
            {
                _buff.Append("id: ").Append(id.Trim()).Append('\n');
            }

            // 数据：data: xxx
            _buff.Append("data: ").Append(data).Append('\n');

            // 空行表示结束（SSE 协议必须）
            _buff.Append('\n');
        }

        /// <summary>
        /// 真正将数据发送出去
        /// </summary>
        /// <returns></returns>
        public async Task FlushAsync()
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(_buff.ToString());

                await _httpResponse.Body.WriteAsync(bytes, 0, bytes.Length);
                await _httpResponse.Body.FlushAsync();
            }
            catch (Exception ex)
            {
                // 即使连接断开，也继续处理，不中断
            }
            finally
            {
                _buff.Clear();
            }
        }

        /// <summary>
        /// 发送序列化后的sse流数据（有缓冲区）
        /// </summary>
        /// <param name="event"></param>
        /// <param name="data"></param>
        /// <returns>返回json字符串</returns>
        public string WriteSerializing(string @event, object data, string id = null)
        {
            string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings()
            {
                // 驼峰命名
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                // 日期格式化
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
            });

            Write(@event, json, id);

            return json;
        }

        public void Dispose()
        {
            _httpResponse.Body.Dispose();
        }
    }
}
