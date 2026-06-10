using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Utils.CustomExtensions
{
    public static class StreamExtension
    {
        /// <summary>
        /// 【同步】读取完整 Stream 转为 byte[]
        /// </summary>
        public static byte[] ToByteArray(this Stream stream)
        {
            // 如果流支持定位，先重置到开头（避免读不到数据）
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// 【异步推荐】读取完整 Stream 转为 byte[]（.NET8 生产环境首选）
        /// </summary>
        public static async Task<byte[]> ToByteArrayAsync(this Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
