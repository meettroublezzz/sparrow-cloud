using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Utils
{
    public static class DirectoryHelper
    {

        /// <summary>
        /// 获取指定根目录的目录树
        /// </summary>
        /// <param name="rootPath">根目录物理路径</param>
        /// <param name="directOnly">true: 仅返回直接子目录；false: 递归返回全部子目录</param>
        public static List<DirectoryTreeNode> GetDirectoryTree(string rootPath, bool directOnly = false, bool showHide = false)
        {
            if (string.IsNullOrWhiteSpace(rootPath))
                throw new ArgumentException("路径不能为空", nameof(rootPath));

            var rootDir = new DirectoryInfo(rootPath);
            if (!rootDir.Exists)
                throw new DirectoryNotFoundException($"目录不存在: {rootPath}");

            var rootNode = new DirectoryTreeNode
            {
                Label = rootDir.Name,
                Key = rootDir.Name,
                Path = rootDir.FullName,
            };

            // 使用栈进行深度优先遍历（非递归）
            var stack = new Stack<(DirectoryInfo dir, DirectoryTreeNode node)>();
            stack.Push((rootDir, rootNode));

            while (stack.Count > 0)
            {
                var (currentDir, currentNode) = stack.Pop();

                try
                {
                    // 获取当前目录的所有直接子目录
                    var subDirs = currentDir.GetDirectories();

                    foreach (var subDir in subDirs)
                    {
                        if (subDir.Name.StartsWith('.') && !showHide)
                            continue;

                        var childNode = new DirectoryTreeNode
                        {
                            Label = subDir.Name,
                            Key = subDir.Name,
                            Path = subDir.FullName,
                        };

                        currentNode.Children.Add(childNode);

                        // 关键优化：仅当 directOnly == false 时，才将子目录压栈继续遍历
                        if (!directOnly)
                        {
                            stack.Push((subDir, childNode));
                        }
                        // 若 directOnly == true，则不压栈，不再深入
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // 无权限访问，跳过该目录
                    continue;
                }
                catch (PathTooLongException)
                {
                    // 路径过长，跳过
                    continue;
                }
            }

            return rootNode.Children;
        }

        /// <summary>
        /// 获取当前系统下常用且具备完全控制权限的目录列表。
        /// 包含白名单路径（如根目录、用户目录、挂载点等），并校验每个路径的实际权限。
        /// </summary>
        /// <returns>目录条目列表</returns>
        public static List<DirectoryEntry> GetUserDirectories()
        {
            var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var os = GetOperatingSystem();

            if (os == OSPlatform.Windows)
            {
                // 1. 所有固定磁盘驱动器的根目录（如 C:\, D:\）
                foreach (var drive in DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.DriveType == DriveType.Fixed))
                {
                    candidates.Add(drive.RootDirectory.FullName);
                }

                // 2. 当前用户的特殊文件夹
                candidates.Add(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                candidates.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                candidates.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
                candidates.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
                candidates.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos));
                candidates.Add(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                //candidates.Add(Environment.GetFolderPath(Environment.SpecialFolder.Downloads));
            }
            else if (os == OSPlatform.Linux)
            {
                // 1. 根目录（必须包含）
                candidates.Add("/");

                // 2. 常用用户数据目录
                candidates.Add("/home");
                candidates.Add("/mnt");
                candidates.Add("/media");
                candidates.Add("/run/media"); // 某些发行版自动挂载到此

                // 3. 当前用户家目录
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!string.IsNullOrEmpty(home))
                    candidates.Add(home);

                // 4. 可选的额外挂载点（可以根据需要扩展）
                // 例如读取 /etc/mtab 或 /proc/mounts，但这里保持简单
            }
            else if (os == OSPlatform.OSX) // macOS
            {
                // 1. 根目录（必须包含）
                candidates.Add("/");

                // 2. 常用目录
                candidates.Add("/Users");
                candidates.Add("/Volumes");
                candidates.Add("/System/Volumes/Data"); // macOS 10.15+ 数据卷

                // 3. 当前用户家目录
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!string.IsNullOrEmpty(home))
                    candidates.Add(home);
            }
            else
            {
                throw new NotImplementedException("该操作系统平台不被支持");
            }

            var result = new List<DirectoryEntry>();

            foreach (var path in candidates)
            {
                // 检查路径是否存在（目录必须存在）
                if (!Directory.Exists(path))
                    continue;

                // 检查权限（读/写/删）
                bool hasFullControl = CheckFullControl(path);

                // 生成友好的显示名称
                string displayName = GetDisplayName(path, os);

                result.Add(new DirectoryEntry
                {
                    Name = displayName,
                    FullPath = path,
                    HasFullControl = hasFullControl
                });
            }

            return result;
        }

        /// <summary>
        /// 检测当前操作系统
        /// </summary>
        private static OSPlatform GetOperatingSystem()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return OSPlatform.Windows;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return OSPlatform.Linux;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return OSPlatform.OSX;
            return OSPlatform.Create("Unknown");
        }

        /// <summary>
        /// 检查目录是否具有完全控制权限（创建、写入、读取、删除文件）
        /// </summary>
        public static bool CheckFullControl(string directoryPath)
        {
            try
            {
                // 尝试在目录下创建临时文件
                string testFile = Path.Combine(directoryPath, $"~test_{Guid.NewGuid():N}.tmp");

                // 写入
                File.WriteAllText(testFile, "test");

                // 读取
                string content = File.ReadAllText(testFile);

                // 删除
                File.Delete(testFile);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 为路径生成用户友好的显示名称
        /// </summary>
        private static string GetDisplayName(string path, OSPlatform os)
        {
            // 如果是 Windows 驱动器根目录，显示卷标和盘符
            if (os == OSPlatform.Windows)
            {
                try
                {
                    var drive = DriveInfo.GetDrives()
                        .FirstOrDefault(d => d.RootDirectory.FullName.Equals(path, StringComparison.OrdinalIgnoreCase));
                    if (drive != null && drive.IsReady)
                    {
                        string label = string.IsNullOrEmpty(drive.VolumeLabel) ? "本地磁盘" : drive.VolumeLabel;
                        return $"{label} ({drive.Name})";
                    }
                }
                catch { }
            }

            // 如果是当前用户的家目录
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (path.Equals(userProfile, StringComparison.OrdinalIgnoreCase))
                return "用户目录";

            // 如果是 Linux/macOS 根目录
            if (path == "/")
                return "根目录";

            // 如果是常见目录名，直接返回目录名
            try
            {
                var dirInfo = new DirectoryInfo(path);
                return dirInfo.Name;
            }
            catch
            {
                return path; // 最后退路
            }
        }
    }


    /// <summary>
    /// 目录树
    /// </summary>
    public class DirectoryTreeNode
    {
        /// <summary>
        /// 目录展示名称
        /// </summary>
        public required string Label { get; set; }

        /// <summary>
        /// 目录实际名称
        /// </summary>
        public required string Key { get; set; }

        /// <summary>
        /// 完整路径
        /// </summary>
        public required string Path { get; set; }

        /// <summary>
        /// 子目录集合
        /// </summary>
        public List<DirectoryTreeNode> Children { get; set; } = new();
    }

    /// <summary>
    /// 目录条目，包含路径、显示名称和完全控制权限状态
    /// </summary>
    public class DirectoryEntry
    {
        /// <summary>
        /// 显示名称（如 "系统 (C:)" 或 "用户目录"）
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 完整物理路径
        /// </summary>
        public required string FullPath { get; set; }

        /// <summary>
        /// 是否具有完全控制权限（读/写/删除）
        /// </summary>
        public bool HasFullControl { get; set; }
    }

}
