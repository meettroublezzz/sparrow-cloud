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
	/// 文件缩略图 实体
	/// 依赖表：storage_files
	/// 一对一关系：一个文件对应一个缩略图
	/// </summary>
	public class StorageFileThumbnail : EntityIncr<long>
	{
		/// <summary>
		/// 小图二进制数据(128*128)
		/// </summary>
		public required byte[] SmallData { get; set; }

		/// <summary>
		/// 中图二进制数据(512*512)
		/// </summary>
		public required byte[] MediumData { get; set; }

		/// <summary>
		/// 大图二进制数据(1024*1024)
		/// </summary>
		public required byte[] LargeData { get; set; }

		/// <summary>
		/// 关联文件ID（外键）
		/// </summary>
		public long StorageFileId { get; set; }
		// ==============================================
		// 导航属性（一对一）主表
		// ==============================================
		/// <summary>
		/// 关联的文件
		/// </summary>
		public StorageFile? StorageFile { get; set; }
	}

    public class StorageFileThumbnailConfig : IEntityTypeConfiguration<StorageFileThumbnail>
	{
		public void Configure(EntityTypeBuilder<StorageFileThumbnail> builder)
		{
			builder.ToTable("storage_file_thumbnails");

			// ==============================================
			// 一对一关系配置（外键在缩略图表）
			// ==============================================
			builder.HasOne(x => x.StorageFile)
				   .WithOne(x => x.Thumbnail)
				   .HasForeignKey<StorageFileThumbnail>(x => x.StorageFileId)
				   .IsRequired()
				   .OnDelete(DeleteBehavior.Cascade); // 级联删除（文件删除 → 缩略图自动删除）

			// 唯一约束：保证一个文件只有一组缩略图
			builder.HasIndex(x => x.StorageFileId).IsUnique();
		}
	}
}