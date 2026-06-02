namespace SparrowCloud.Utils;

using System.IO;
using System.IO.Compression;
using System.Text;

/// <summary>
/// .NET8 原生压缩解压工具类（Brotli算法）
/// 两组方法：字符串↔字节 | 字节↔字节
/// </summary>
public static class CompressionHelper
{
    #region 字符串 ↔ 二进制数据（压缩/解压）
    /// <summary>
    /// 字符串 压缩为 二进制数据
    /// </summary>
    /// <param name="text">原始字符串</param>
    /// <returns>压缩后的字节数组</returns>
    public static byte[] CompressStringToBytes(string text)
    {
        if (string.IsNullOrEmpty(text)) return Array.Empty<byte>();
        var bytes = Encoding.UTF8.GetBytes(text);
        return CompressBytes(bytes);
    }

    /// <summary>
    /// 二进制数据 解压为 字符串
    /// </summary>
    /// <param name="compressedBytes">压缩后的字节数组</param>
    /// <returns>原始字符串</returns>
    public static string DecompressBytesToString(byte[] compressedBytes)
    {
        if (compressedBytes == null || compressedBytes.Length == 0) return string.Empty;
        var bytes = DecompressBytes(compressedBytes);
        return Encoding.UTF8.GetString(bytes);
    }
    #endregion

    #region 二进制数据 ↔ 二进制数据（压缩/解压）
    /// <summary>
    /// 二进制数据 压缩为 二进制数据
    /// </summary>
    /// <param name="sourceBytes">原始字节数组</param>
    /// <returns>压缩后的字节数组</returns>
    public static byte[] CompressBytes(byte[] sourceBytes)
    {
        if (sourceBytes == null || sourceBytes.Length == 0) return Array.Empty<byte>();

        using var memoryStream = new MemoryStream();
        using var brotliStream = new BrotliStream(memoryStream, CompressionMode.Compress);
        brotliStream.Write(sourceBytes, 0, sourceBytes.Length);
        brotliStream.Flush();
        return memoryStream.ToArray();
    }

    /// <summary>
    /// 二进制数据 解压为 二进制数据
    /// </summary>
    /// <param name="compressedBytes">压缩后的字节数组</param>
    /// <returns>原始字节数组</returns>
    public static byte[] DecompressBytes(byte[] compressedBytes)
    {
        if (compressedBytes == null || compressedBytes.Length == 0) return Array.Empty<byte>();

        using var compressedStream = new MemoryStream(compressedBytes);
        using var brotliStream = new BrotliStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        brotliStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }
    #endregion
}