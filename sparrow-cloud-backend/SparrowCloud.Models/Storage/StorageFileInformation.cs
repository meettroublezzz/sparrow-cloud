using MemoryPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using SparrowCloud.Utils;

namespace SparrowCloud.Models.Storage
{
    /// <summary>
	/// 文件额外信息（偏文本数据）
	/// 依赖表：storage_files
	/// 一对一关系：(one)额外信息 -> one 文件
	/// </summary>
    public class StorageFileInformation : EntityInformation<long>
    {
        /// <summary>
        /// 文件标题（如果有，则显示优先级高于文件名）
        /// </summary>
        [MaxLength(255)]
        public string? Title { get; set; }

        /// <summary>
        /// 关联文件ID（外键）
        /// </summary>
        public long StorageFileId { get; set; }

        /// <summary>
        /// 关联的文件
        /// </summary>
        public StorageFile? StorageFile { get; set; }
    }

    public class StorageFileInformationConfig : IEntityTypeConfiguration<StorageFileInformation>
    {
        public void Configure(EntityTypeBuilder<StorageFileInformation> builder)
        {
            builder.ToTable("storage_file_information");

            // 设置自增主键
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            // ==============================================
            // 一对一关系配置（外键在当前表）
            // ==============================================
            builder.HasOne(x => x.StorageFile)
                   .WithOne(x => x.Information)
                   .HasForeignKey<StorageFileInformation>(x => x.StorageFileId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade); // 级联删除（文件删除 → 自动关联删除）
            // 唯一约束：保证一一对应
            builder.HasIndex(x => x.StorageFileId).IsUnique();
        }
    }
}
