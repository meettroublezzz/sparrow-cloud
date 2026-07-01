using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Models.Storage
{
    /// <summary>
    /// 文件&标签 中间表
    /// </summary>
    public class StorageTagFile : EntityIncr<long>
    {
        public long FileId { get; set; }

        public int TagId { get; set; }

        public StorageTag Tag { get; set; }
        public StorageFile File { get; set; }
    }

    internal class FileJoinTagConfig : IEntityTypeConfiguration<StorageTagFile>
    {
        public void Configure(EntityTypeBuilder<StorageTagFile> builder)
        {
            builder.ToTable("storage_tags_files");

            builder
                .HasOne(ft => ft.File)
                .WithMany(f => f.FileTags)
                .HasForeignKey(ft => ft.FileId)
                .OnDelete(DeleteBehavior.Cascade); // 删除文件自动删标签关联

            builder
                .HasOne(ft => ft.Tag)
                .WithMany(t => t.FileTags)
                .HasForeignKey(ft => ft.TagId)
                .OnDelete(DeleteBehavior.Cascade); // 删除标签自动删文件关联

            builder.HasIndex(x => x.FileId);
            builder.HasIndex(x => new { x.FileId, x.TagId }).IsUnique();
        }
    }
}
