using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;
using SparrowCloud.Models.Storage;
using SparrowCloud.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Services.Storage
{
    public class StorageService
    {
        /// <summary>
        /// 文件库实际根目录路径
        /// </summary>
        private readonly string _rootPath;

        /// <summary>
        /// 文件库内数据仓库路径
        /// </summary>
        private readonly string _basePath;

        /// <summary>
        /// 文件库内主要工作路径
        /// </summary>
        private readonly string _workPath;

        /// <summary>
        /// 文件库元数据配置信息
        /// </summary>
        private readonly StorageMetadata _metadata;

        public StorageService(string path, StorageMetadata metadata)
        {
            _rootPath = Path.TrimEndingDirectorySeparator(path);

            _basePath = Path.Combine(_rootPath, StorageManager.StandaloneDirectoryName);

            _workPath = Path.Combine(_rootPath, StorageManager.StandaloneDirectoryName, StorageManager.SparrowCloudName);

            Console.WriteLine(_workPath);

            _metadata = metadata;
        }

        /// <summary>
        /// 获取数据库上下文
        /// </summary>
        /// <returns></returns>
        public StorageContext GetStorageContext()
        {
            string filePath = Path.Combine(_workPath, @"StorageSqlite.db");

            var options = new DbContextOptionsBuilder<StorageContext>()
                .UseSqlite($"Data Source={filePath}; Cache=Shared;")
                .Options;

            return new StorageContext(options);
        }

        public string RootPath { get => _rootPath; }
        public string BasePath { get => _basePath; }
        public string WorkPath { get => _workPath; }

        /// <summary>
        /// 扫描遍历文件库（核心）
        /// </summary>
        /// <returns></returns>
        public async Task ScanFilesAsync()
        {
            /*
             * 以实际文件为准，数据库为辅！
             * 
             * PS：以下，对于文件/目录，统称为文件。
             * 
             * 先遍历实际文件，此视角下：
             *      如果数据库里不存在此文件，则新增到数据库。
             *      如果存在，且最后修改时间不一致，则更新数据库。
             *      
             *  后遍历数据库，此视角下：
             *      判断此文件实际上是否存在，如果不存在则在数据库里标记为缺失状态。
             *      PS：注意回收站问题，因为移动了，所以没有实际文件；需要先排除已删除的，再单独处理已删除的。
             */
            // 获取数据库上下文并使用using自动释放

            // 确保文件夹存在
            Directory.CreateDirectory(_workPath);
            // 获取数据库上下文
            using StorageContext db = GetStorageContext();
            // 自动建库建表
            db.Database.EnsureCreated();

            #region  批量操作优化
            // 关闭自动变更检测
            db.ChangeTracker.AutoDetectChangesEnabled = false;

            // 查询数据时不追踪
            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            #endregion

            // 数据库内的记录
            List<(long fullPathHash, short fullPathLength, long creationTimeTicks, long lastWriteTimeTicks, long lastAccessTimeTicks, bool missing)> storages = new();

            // 文件系统的记录
            var files = GetAllFiles();

            // 第一步：以文件系统视角，同步到数据库（新增/更新）
            //await SyncFileSystemToDatabaseAsync(dbContext);

            // 第二步：以数据库视角，标记缺失的文件
            //await MarkMissingFilesInDatabaseAsync(dbContext);

            Console.WriteLine();
            Console.WriteLine($"scan-files -> {_rootPath}");
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }
            Console.WriteLine($"count={files.Count};");
        }

        /// <summary>
        /// 递归扫描所有 文件/目录（排除数据仓库）
        /// </summary>
        private List<(string path, long fullPathHash, short fullPathLength, long creationTimeTicks, long lastWriteTimeTicks, long lastAccessTimeTicks)> GetAllFiles()
        {
            var files = new List<(string path, long fullPathHash, short fullPathLength, long creationTimeTicks, long lastWriteTimeTicks, long lastAccessTimeTicks)>();

            // 规范化路径（统一全路径、去除结尾分隔符，用于匹配排除）
            string excludeFullPath = Path.GetFullPath(_basePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string rootFullPath = Path.GetFullPath(_rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // 迭代式遍历（无栈溢出 + 省内存）
            var dirStack = new Stack<string>();
            dirStack.Push(rootFullPath);

            while (dirStack.Count > 0)
            {
                string currentDir = dirStack.Pop();

                // 核心：跳过数据仓库目录 & 所有子目录
                if (currentDir.StartsWith(excludeFullPath, StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    // ===================== 扫描文件 =====================
                    foreach (var file in new DirectoryInfo(currentDir).EnumerateFiles())
                    {
                        // 🔥 核心：生成【相对根目录】的规范路径
                        string relativePath = GetNormalizedRelativePath(rootFullPath, file.FullName, isDirectory: false);

                        // 计算哈希与长度
                        long pathHash = HashHelper.XxHash64Utility.ComputeHash64(relativePath);
                        short pathLen = (short)relativePath.Length;

                        // UTC时间戳（跨平台一致）
                        long cTime = file.CreationTimeUtc.Ticks;
                        long wTime = file.LastWriteTimeUtc.Ticks;
                        long aTime = file.LastAccessTimeUtc.Ticks;

                        files.Add((relativePath, pathHash, pathLen, cTime, wTime, aTime));
                    }

                    // ===================== 扫描目录 =====================
                    foreach (var dir in new DirectoryInfo(currentDir).EnumerateDirectories())
                    {
                        string dirFullPath = dir.FullName.TrimEnd(Path.DirectorySeparatorChar);

                        // 跳过排除目录
                        if (dirFullPath.StartsWith(excludeFullPath, StringComparison.OrdinalIgnoreCase))
                            continue;

                        // 🔥 核心：生成【相对根目录】的规范路径
                        string relativePath = GetNormalizedRelativePath(rootFullPath, dir.FullName, isDirectory: true);

                        long pathHash = HashHelper.XxHash64Utility.ComputeHash64(relativePath);
                        short pathLen = (short)relativePath.Length;

                        long cTime = dir.CreationTimeUtc.Ticks;
                        long wTime = dir.LastWriteTimeUtc.Ticks;
                        long aTime = dir.LastAccessTimeUtc.Ticks;

                        files.Add((relativePath, pathHash, pathLen, cTime, wTime, aTime));
                        dirStack.Push(dirFullPath);
                    }
                }
                catch
                {
                    // 忽略权限/异常目录，保证扫描不中断
                    throw;
                }
            }

            return files;
        }

        /// <summary>
        /// 生成【相对根目录】的标准路径
        /// 规则：/开头，/分隔，目录以/结尾，文件无尾/
        /// 示例：C:\files\img\a.png → /img/a.png
        /// </summary>
        private static string GetNormalizedRelativePath(string rootFullPath, string fullPath, bool isDirectory)
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
                relativePath = relativePath.TrimEnd('/') + "/";
            }
            else
            {
                relativePath = relativePath.TrimEnd('/');
            }

            return relativePath;
        }
    }
}
