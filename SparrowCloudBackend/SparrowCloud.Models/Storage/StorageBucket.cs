using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Models.Storage
{
    /// <summary>
    /// 桶存储（对象存储简化版）
    /// </summary>
    public class StorageBucket : EntityIncr<long>
    {
        /// <summary>
        /// 桶名称
        /// </summary>
        public required string BucketName { get; set; }

        /// <summary>
        /// 对象名
        /// </summary>
        public required string ObjectName { get; set; }

        public required string Extension { get; set; }

        /// <summary>
        /// 二进制数据（为null表达在文件里）
        /// </summary>
        public byte[]? BinaryData { get; set; }
    }

    public class StorageBucketConfig : IEntityTypeConfiguration<StorageBucket>
    {
        public void Configure(EntityTypeBuilder<StorageBucket> builder)
        {
            builder.ToTable("storage_buckets");

            builder.HasIndex(x => new { x.BucketName, x.ObjectName }).IsUnique();
            builder.HasIndex(x => x.Extension);
        }
    }
}
