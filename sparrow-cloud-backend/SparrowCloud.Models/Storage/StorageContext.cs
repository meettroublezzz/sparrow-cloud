using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Models.Storage
{
    public class StorageContext : DbContext
    {
        public StorageContext(DbContextOptions<StorageContext> options) : base(options)
        {
            var conn = (SqliteConnection)Database.GetDbConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            // sqlite 优化
            cmd.CommandText = EntityBase.SqliteOptimizeCommandText;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 模型创建配置（主键、索引、约束等）
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 获取当前命名空间
            var ns = typeof(StorageContext).Namespace;

            // 只加载：当前程序集 + 同命名空间下的所有配置类
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(), t => t.Namespace == ns);
        }

        public DbSet<StorageConfig> StorageConfigs { get; set; }

        public DbSet<StorageFile> StorageFiles { get; set; }
        public DbSet<StorageFileInformation> StorageFileInformation { get; set; }
        public DbSet<StorageFileThumbnail> StorageFileThumbnails { get; set; }
        public DbSet<StorageFileAggregate> StorageFileAggregates { get; set; }
        public DbSet<StorageFileRecycled> StorageFileRecycled { get; set; }

        public DbSet<StorageTag> StorageTags { get; set; }
        public DbSet<StorageTagFile> StorageTagFile { get; set; }

        public DbSet<StorageBucket> StorageBuckets { get; set; }
    }
}
