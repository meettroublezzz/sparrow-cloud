using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Models.Storage
{
    /// <summary>
	/// 文件缩略图（偏图像数据）
	/// 依赖表：storage_files
	/// 一对一关系：(one)缩略图 -> one 文件
	/// </summary>
	public class StorageFileThumbnail : EntityIncr<long>
	{
        /// <summary>
        /// 小图 桶存储引用 (64*64)
        /// </summary>
        public required string SmallInBucket { get; set; }

        /// <summary>
        /// 中图 桶存储引用 (128*128)
        /// </summary>
        public required string MediumInBucket { get; set; }

        /// <summary>
        /// 大图 桶存储引用 (256*256)
        /// </summary>
        public required string LargeInBucket { get; set; }

        /// <summary>
        /// 封面 桶存储引用 (用户自定义)
        /// </summary>
        public string? CoverInBucket { get; set; }

        /// <summary>
        /// 关联文件ID（外键）
        /// </summary>
        public long StorageFileId { get; set; }

		/// <summary>
		/// 关联的文件
		/// </summary>
		public StorageFile StorageFile { get; set; }
	}

    internal class StorageFileThumbnailConfig : IEntityTypeConfiguration<StorageFileThumbnail>
	{
		public void Configure(EntityTypeBuilder<StorageFileThumbnail> builder)
		{
			builder.ToTable("storage_file_thumbnails");

            // ==============================================
            // 一对一关系配置（外键在当前表）
            // ==============================================
            builder.HasOne(x => x.StorageFile)
				   .WithOne(x => x.Thumbnail)
				   .HasForeignKey<StorageFileThumbnail>(x => x.StorageFileId)
				   .IsRequired()
				   .OnDelete(DeleteBehavior.Cascade); // 级联删除（文件删除 → 自动关联删除）
            // 唯一约束：保证一一对应
            builder.HasIndex(x => x.StorageFileId).IsUnique();
		}
	}
}