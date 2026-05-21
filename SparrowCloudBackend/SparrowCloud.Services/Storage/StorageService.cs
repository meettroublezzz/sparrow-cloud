using Microsoft.EntityFrameworkCore;
using SparrowCloud.Models.Storage;

namespace SparrowCloud.Services.Storage
{
    /// <summary>
    /// 文件库服务
    /// </summary>
    public partial class StorageService
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

    }
}
