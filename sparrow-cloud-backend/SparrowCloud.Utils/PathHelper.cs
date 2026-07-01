using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Utils
{
    public static class PathHelper
    {

        /// <summary>
        /// 生成【相对根目录】的标准路径
        /// 规则：/开头，/分隔，目录以/结尾，文件无尾/
        /// 示例：C:\files\img\a.png → /img/a.png
        /// </summary>
        public static string GetNormalizedRelativePath(string rootFullPath, string fullPath, bool isDirectory)
        {
            // 1. 计算【相对于根目录】的相对路径
            string relativePath = Path.GetRelativePath(rootFullPath, fullPath);

            // 2. 统一替换为 Linux 分隔符 /
            relativePath = relativePath.Replace('\\', '/');

            // 3. 开头添加 /（强制要求）
            relativePath = $"/{relativePath}";

            // 4. 目录必须以 / 结尾，文件绝对不能以 / 结尾
            if (isDirectory)
            {
                if (!relativePath.EndsWith('/'))
                    relativePath += "/";
            }
            else
            {
                relativePath = relativePath.TrimEnd('/');
            }

            return relativePath;
        }

        /// <summary>
        /// 获取父目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string? GetDirectoryName(string path)
        {
            path = path.TrimEnd('/').TrimEnd('\\');

            string? parentPath = Path.GetDirectoryName(path);

            if (parentPath == null)
                return null;

            // 兼容windows的情况
            return parentPath.Replace('\\', '/');
        }

        /// <summary>
        /// 获取扩展名，不含 '.' 符号
        /// </summary>
        /// <param name="path"></param>
        /// <returns>目录返回 null</returns>
        public static string? GetFileExtensionWithoutDot(string path)
        {
            // 空值 或 路径以目录分隔符结尾 → 判定为目录 → 返回 null
            if (string.IsNullOrWhiteSpace(path) || path.EndsWith('/')
                || path.EndsWith(Path.DirectorySeparatorChar)
                || path.EndsWith(Path.AltDirectorySeparatorChar))
            {
                return null;
            }

            // 获取文件扩展名（带.）
            string extension = Path.GetExtension(path);

            // 无扩展名返回空，有扩展名去掉.返回
            return string.IsNullOrEmpty(extension) ? string.Empty : extension.TrimStart('.');
        }

        /// <summary>
        /// 给目录追加 Hidden 隐藏属性（保留原有属性）
        /// </summary>
        /// <param name="dirPath">目录完整路径</param>
        /// <param name="hidden">设置是否隐藏</param>
        /// <exception cref="DirectoryNotFoundException">目录不存在抛出</exception>
        public static void SetHidden(string dirPath, bool hidden = true)
        {
            if (!Directory.Exists(dirPath))
                throw new DirectoryNotFoundException(dirPath);

            var dir = new DirectoryInfo(dirPath);
            dir.Attributes &= ~FileAttributes.ReadOnly;

            if (hidden)
                dir.Attributes |= FileAttributes.Hidden;
            else
                dir.Attributes &= ~FileAttributes.Hidden;
        }
    }
}
