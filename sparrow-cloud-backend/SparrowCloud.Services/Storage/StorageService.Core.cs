using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using SparrowCloud.Models;
using SparrowCloud.Models.Storage;
using SparrowCloud.Utils;
using System.Data;
using System.Diagnostics;

namespace SparrowCloud.Services.Storage
{
    /*
	 * 放置核心处理逻辑
	 */
    public partial class StorageService
    {
        // 批量操作 缓冲区大小
        private const int BatchSize = 131072;

        // 批量操作优化
        private static readonly BulkConfig _bulkConfig = new()
        {
            BatchSize = 4096,
            EnableStreaming = true,
            ConflictOption = EFCore.BulkExtensions.ConflictOption.Ignore,
        };

        /// <summary>
        /// 真实文件（对比数据）
        /// </summary>
        private readonly struct FileComparison
        {
            public readonly string Path { get; init; }

            public readonly long FileLength { get; init; }
            public readonly long CreationTimeTicks { get; init; }
            public readonly long LastWriteTimeTicks { get; init; }
            public readonly long LastAccessTimeTicks { get; init; }
        }
        /// <summary>
        /// 数据库记录（对比数据）
        /// </summary>
        private readonly struct RecordComparison
        {
            public readonly long Id { get; init; }

            public readonly long CreationTimeTicks { get; init; }
            public readonly long LastWriteTimeTicks { get; init; }

            public readonly bool IsDeleted { get; init; }
            public readonly bool IsMissing { get; init; }
        }
        /// <summary>
        /// 数据库记录（临时数据）
        /// </summary>
        private readonly struct RecordTemp
        {
            public readonly long Id { get; init; }

            public readonly long FullPathHash { get; init; }
            public readonly short FullPathLength { get; init; }

            public readonly long CreationTimeTicks { get; init; }
            public readonly long LastWriteTimeTicks { get; init; }

            //public readonly bool IsDeleted { get; init; }
            public readonly bool IsMissing { get; init; }
        }

        /// <summary>
        /// 扫描遍历文件库（幂等）
        /// </summary>
        /// <returns></returns>
        public async Task ScanFilesAsync()
        {
            Stopwatch stopwatchtotal = Stopwatch.StartNew();
            Stopwatch stopwatch = new();
            Console.WriteLine();
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

            stopwatch.Restart();
            // 获取数据库上下文
            using StorageContext db = GetStorageContext();
            // 自动建库建表
            db.Database.EnsureCreated();
            stopwatch.Stop();
            Console.WriteLine($"自动建库建表 Elapsed={stopwatch.Elapsed}");

            #region  批量操作优化
            // 关闭自动变更检测
            db.ChangeTracker.AutoDetectChangesEnabled = false;

            // 查询数据时不追踪
            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            #endregion

            stopwatch.Restart();
            // 数据库内的记录（异步任务）
            var task = GetAllRecords(db);

            // 文件系统的记录
            var files = GetAllFiles();

            // 数据库内的记录
            var records = await task;
            stopwatch.Stop();
            Console.WriteLine($"两组合集查询 Elapsed={stopwatch.Elapsed}");

            stopwatch.Restart();
            // 第一步：以文件系统视角，同步到数据库（新增/更新）
            await SyncFileSystemToDatabaseAsync(db, files, records);
            stopwatch.Stop();
            Console.WriteLine($"第一步end Elapsed={stopwatch.Elapsed}");

            stopwatch.Restart();
            // 第二步：以数据库视角，标记缺失的文件
            await MarkMissingFilesInDatabaseAsync(db, files, records);
            stopwatch.Stop();
            Console.WriteLine($"第二步end Elapsed={stopwatch.Elapsed}");

            stopwatchtotal.Stop();
            Console.WriteLine();
            Console.WriteLine($"Count={files.Count()}, {records.Count()}; Total Elapsed={stopwatchtotal.Elapsed}");
            Console.WriteLine();
        }

        /// <summary>
        /// 从数据库拿到所有记录
        /// </summary>
        /// <returns></returns>
        private async Task<IReadOnlyDictionary<(long PathHash, short PathLength), RecordComparison>> GetAllRecords(StorageContext db)
        {
            var query = db.StorageFiles
                .Where(e => e.Recycled == null);

            int count = await query.CountAsync();

            var dataset = query
                .Select(e => new RecordTemp()
                {
                    Id = e.Id,

                    FullPathHash = e.FullPathHash,
                    FullPathLength = e.FullPathLength,

                    CreationTimeTicks = e.CreationTimeTicks,
                    LastWriteTimeTicks = e.LastWriteTimeTicks,

                    //IsDeleted = (e.Recycled != null),
                    IsMissing = (e.Missing != null),
                })
                .AsAsyncEnumerable();

            var result = new Dictionary<(long PathHash, short PathLength), RecordComparison>(capacity: count);

            await foreach (var record in dataset)
            {
                result.Add((record.FullPathHash, record.FullPathLength), new()
                {
                    Id = record.Id,

                    CreationTimeTicks = record.CreationTimeTicks,
                    LastWriteTimeTicks = record.LastWriteTimeTicks,

                    //IsDeleted = record.IsDeleted,
                    IsMissing = record.IsMissing,
                });
            }

            return result.AsReadOnly();
        }

        /// <summary>
        /// 递归扫描所有 文件/目录（排除数据仓库）
        /// </summary>
        private IReadOnlyDictionary<(long PathHash, short PathLength), FileComparison> GetAllFiles()
        {
            var result = new Dictionary<(long PathHash, short PathLength), FileComparison>(capacity: 100000);

            // 规范化路径（统一全路径、去除结尾分隔符，用于匹配排除）
            string excludeFullPath = Path.GetFullPath(_basePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string rootFullPath = Path.GetFullPath(_rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // 迭代式遍历（无栈溢出 + 省内存）
            var dirStack = new Stack<string>();

            // 文件库根目录
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
                        string relativePath = PathHelper.GetNormalizedRelativePath(rootFullPath, file.FullName, isDirectory: false);

                        // 计算哈希与长度
                        long pathHash = HashHelper.ComputeHash64(relativePath);
                        short pathLen = (short)relativePath.Length;

                        // UTC时间戳（跨平台一致）
                        long cTime = file.CreationTimeUtc.Ticks;
                        long wTime = file.LastWriteTimeUtc.Ticks;
                        long aTime = file.LastAccessTimeUtc.Ticks;

                        result.Add((pathHash, pathLen), new()
                        {
                            Path = relativePath,

                            FileLength = file.Length,
                            CreationTimeTicks = cTime,
                            LastWriteTimeTicks = wTime,
                            LastAccessTimeTicks = aTime,
                        });
                    }

                    // ===================== 扫描目录 =====================
                    foreach (var dir in new DirectoryInfo(currentDir).EnumerateDirectories())
                    {
                        string dirFullPath = dir.FullName.TrimEnd(Path.DirectorySeparatorChar);

                        // 跳过排除目录
                        if (dirFullPath.StartsWith(excludeFullPath, StringComparison.OrdinalIgnoreCase))
                            continue;

                        // 🔥 核心：生成【相对根目录】的规范路径
                        string relativePath = PathHelper.GetNormalizedRelativePath(rootFullPath, dir.FullName, isDirectory: true);

                        long pathHash = HashHelper.ComputeHash64(relativePath);
                        short pathLen = (short)relativePath.Length;

                        long cTime = dir.CreationTimeUtc.Ticks;
                        long wTime = dir.LastWriteTimeUtc.Ticks;
                        long aTime = dir.LastAccessTimeUtc.Ticks;

                        result.Add((pathHash, pathLen), new()
                        {
                            Path = relativePath,

                            FileLength = -1,
                            CreationTimeTicks = cTime,
                            LastWriteTimeTicks = wTime,
                            LastAccessTimeTicks = aTime,
                        });

                        // 添加到目录集合，等待下次遍历
                        dirStack.Push(dirFullPath);
                    }
                }
                catch
                {
                    throw;
                }
            }

            return result.AsReadOnly();
        }

        /// <summary>
        /// 以真实文件视角，反查数据库
        /// </summary>
        /// <param name="db"></param>
        /// <param name="files"></param>
        /// <param name="records"></param>
        /// <returns></returns>
        private static async Task SyncFileSystemToDatabaseAsync(StorageContext db, IReadOnlyDictionary<(long PathHash, short PathLength), FileComparison> files, IReadOnlyDictionary<(long PathHash, short PathLength), RecordComparison> records)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            #region 数据库批量操作
            // 待插入缓冲区
            var insertBatch = new List<StorageFile>(capacity: BatchSize);
            // 执行批量插入
            async Task ExecuteBatchInsertAsync(bool final = false)
            {
                if (insertBatch.Count >= BatchSize || final)
                {
                    stopwatch.Stop();
                    Console.WriteLine($"Batch 之前 Elapsed={stopwatch.Elapsed}");

                    stopwatch.Restart();
                    await db.BulkInsertAsync(insertBatch, _bulkConfig);
                    stopwatch.Stop();
                    Console.WriteLine($"Batch Count={insertBatch.Count}; Elapsed={stopwatch.Elapsed}");

                    insertBatch.Clear();
                    stopwatch.Restart();
                }
            }
            #endregion

            foreach (var file in files)
            {
                var fileKey = file.Key;
                var fileValue = file.Value;

                bool exist = records.TryGetValue(fileKey, out var record);

                // 此 文件/目录 不存在则新增到数据库内
                if (!exist)
                {
                    // 其实是相对路径： /a/b/    /a/bbb.txt
                    string fullPath = fileValue.Path!;
                    // 获取上级目录
                    string parentPath = PathHelper.GetDirectoryName(fullPath)!;

                    insertBatch.Add(new StorageFile()
                    {
                        // 主键是自增的，这里只是满足初始化约束
                        Id = default,

                        FullPath = fullPath,

                        FullPathHash = fileKey.PathHash,
                        FullPathLength = fileKey.PathLength,

                        ParentPathHash = HashHelper.ComputeHash64(parentPath),
                        ParentPathLength = (short)parentPath.Length,

                        Name = Path.GetFileName(fullPath),
                        Extension = PathHelper.GetFileExtensionWithoutDot(fullPath),

                        IsDirectory = fullPath.EndsWith('/'),

                        FileLength = fileValue.FileLength,
                        CreationTimeTicks = fileValue.CreationTimeTicks,
                        LastWriteTimeTicks = fileValue.LastWriteTimeTicks,
                        LastAccessTimeTicks = fileValue.LastAccessTimeTicks,
                    });

                    // 拟插入数据
                    await ExecuteBatchInsertAsync();

                    // 下一条
                    continue;
                }

                // 如果是记录为 已删除、缺失 等特殊状态的文件
                if (record.IsDeleted || record.IsMissing)
                {
                    // 暂不处理
                    Console.WriteLine($"反查遍历 -> 缺失找回：{fileValue.Path}");

                    // 下一条
                    continue;
                }

                // 如果存在，且最后修改时间不一致，则更新数据库
                if (fileValue.LastWriteTimeTicks != record.LastWriteTimeTicks)
                {
                    // 暂时用简单办法更新 ... 应该数据量不会很多吧，呆胶布（心虚）
                    await db.StorageFiles
                        .Where(e => e.Id == record.Id)
                        .ExecuteUpdateAsync(e => e
                            .SetProperty(s => s.LastWriteTimeTicks, fileValue.LastWriteTimeTicks)
                            .SetProperty(s => s.FileLength, fileValue.FileLength)
                            .SetProperty(s => s.FileShaHash, (string?)null)
                        );
                    Console.WriteLine($"反查遍历 -> 更新文件：{fileValue.Path}");

                    // 也许应该触发点其他什么
                    // todo

                    // 下一条
                    continue;
                }

                // 创建时间 改变的话，同步一下
                if (fileValue.CreationTimeTicks != record.CreationTimeTicks)
                {
                    // 暂时用简单办法更新 ... 应该数据量不会很多吧，呆胶布（心虚）
                    await db.StorageFiles
                        .Where(e => e.Id == record.Id)
                        .ExecuteUpdateAsync(e => e
                            .SetProperty(s => s.CreationTimeTicks, fileValue.CreationTimeTicks)
                        );

                    // 下一条
                    continue;
                }

                // 数据库已有记录，且真实文件未修改，不管。
            }

            await ExecuteBatchInsertAsync(true);
            stopwatch.Stop();
        }

        /// <summary>
        /// 以数据库视角，修正数据项
        /// </summary>
        /// <param name="db"></param>
        /// <param name="files"></param>
        /// <param name="records"></param>
        /// <returns></returns>
        private static async Task MarkMissingFilesInDatabaseAsync(StorageContext db, IReadOnlyDictionary<(long PathHash, short PathLength), FileComparison> files, IReadOnlyDictionary<(long PathHash, short PathLength), RecordComparison> records)
        {
            foreach (var record in records)
            {
                var recordKey = record.Key;
                var recordValue = record.Value;

                bool exist = files.TryGetValue(recordKey, out var file);

                // 如果数据库里有记录，但真实文件并不存在
                if (!exist)
                {
                    // 如果已经处理过了，则跳过
                    if (recordValue.IsMissing)
                        continue;

                    // 标记为缺失
                    await db.StorageFiles
                        .Where(e => e.Id == recordValue.Id)
                        .ExecuteUpdateAsync(e => e
                            .SetProperty(s => s.Missing, EntityBase.NowTicks)
                        );
                    Console.WriteLine($"数据库遍历 -> 缺失文件：Id={recordValue.Id}; {file.Path}");
                }

                // 存在，其他情况暂时不管
            }
        }
    }
}
