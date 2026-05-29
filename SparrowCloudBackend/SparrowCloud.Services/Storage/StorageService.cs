using Microsoft.EntityFrameworkCore;
using SparrowCloud.Models.Storage;

namespace SparrowCloud.Services.Storage
{
    /// <summary>
    /// 文件库服务
    /// </summary>
    public partial class StorageService
    {
        #region 相关约定名称
        /// <summary>
        /// 文件库 主数据库 文件名
        /// </summary>
        public const string StorageSqliteFileName = @"StorageSqlite.db";

        /// <summary>
        /// 文件库 回收站目录名称
        /// </summary>
        public const string StorageRecycledName = @"RecycledFiles";
        #endregion

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
        /// 文件库内回收站工作路径
        /// </summary>
        private readonly string _recycledWorkPath;

        /// <summary>
        /// 文件库元数据配置信息
        /// </summary>
        private readonly StorageMetadata _metadata;

        public StorageService(string path, StorageMetadata metadata)
        {
            _metadata = metadata;

            _rootPath = Path.TrimEndingDirectorySeparator(path);

            _basePath = Path.Combine(_rootPath, StorageManager.StandaloneDirectoryName);

            _workPath = Path.Combine(_rootPath, StorageManager.StandaloneDirectoryName, StorageManager.SparrowCloudName);

            _recycledWorkPath = Path.Combine(_workPath, StorageRecycledName);

            // 确保文件夹存在
            Directory.CreateDirectory(_workPath);
            Directory.CreateDirectory(_recycledWorkPath);
        }
        
        /// <summary>
        /// 获取数据库上下文
        /// </summary>
        /// <returns></returns>
        public StorageContext GetStorageContext()
        {
            string filePath = Path.Combine(_workPath, StorageSqliteFileName);

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
