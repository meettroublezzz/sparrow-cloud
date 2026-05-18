using System.IO.Hashing;
using System.Text;

namespace SparrowCloud.Utils
{
    public class HashHelper
    {
        /// <summary>
        /// XXHash64 哈希工具（适配SQLite long类型存储）
        /// </summary>
        public static class XxHash64Utility
        {
            /// <summary>
            /// 计算字符串的 XXHash64 哈希值，返回 long（二进制无损转换）
            /// </summary>
            /// <param name="input">要哈希的字符串</param>
            /// <returns>SQLite兼容的long类型哈希值</returns>
            public static long ComputeHash64(string input)
            {
                // 固定使用 UTF-8 编码（跨系统一致）
                byte[] bytes = Encoding.UTF8.GetBytes(input);

                // 计算官方 XXHash64 (ulong)
                ulong hashUlong = XxHash64.HashToUInt64(bytes);

                // 二进制无损强转 long（适配SQLite存储，不丢失任何数据）
                return unchecked((long)hashUlong);
            }
        }
    }
}
