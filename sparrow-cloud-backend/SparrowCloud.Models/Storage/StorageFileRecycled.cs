using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Models.Storage
{
    public class StorageFileRecycled : EntityIncr<long>
    {
        /// <summary>
        /// 外键：指向文件表（被删除的文件）
        /// </summary>
        public long FileId { get; set; }

        /// <summary>
        /// 导航属性：被删除的文件
        /// </summary>
        public virtual StorageFile File { get; set; }
    }

    internal class StorageFileRecycledConfig : IEntityTypeConfiguration<StorageFileRecycled>
    {
        public void Configure(EntityTypeBuilder<StorageFileRecycled> builder)
        {
            builder.ToTable("storage_file_recycled");

            // 设置自增主键
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            // ==============================================
            // 一对一关系配置（外键在当前表）
            // ==============================================
            builder.HasOne(x => x.File)
                   .WithOne(x => x.Recycled)
                   .HasForeignKey<StorageFileRecycled>(x => x.FileId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade); // 级联删除（文件删除 → 自动关联删除）
            // 唯一约束：保证一一对应
            builder.HasIndex(x => x.FileId).IsUnique();
        }
    }
}
