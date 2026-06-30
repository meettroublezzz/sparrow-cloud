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
    /// 文件聚合体
    /// 依赖表：storage_files
    /// 一对多关系：(one)聚合体 -> many 文件
    /// 一对一关系：(one)主文件 -> one 文件
    /// PS：互相依赖
    /// </summary>
    public class StorageFileAggregate : EntityInformation<long>
    {
        /*
         * 介于文件与目录之间，它在用户看来是个文件，也像操作单个文件；
         * 但内部是多个文件或目录聚合在一起的，对外暴露主文件。
         * 
         * 示例场景：
         * 
         * 视频&弹幕&封面（主文件 xxx.mp4 对外）
         * 电影&元数据&字幕&封面（主文件 xxx.mkv 对外）
         * 中文教程&原版教程&注解&Demo（主文件 xxx.pdf 对外）
         */

        /// <summary>
        /// 关联的文件集合
        /// </summary>
        public List<StorageFile> Files { get; set; } = new();

        /// <summary>
        /// 外键：主文件ID
        /// </summary>
        public long MainFileId { get; set; }
        /// <summary>
        /// 主文件
        /// </summary>
        public StorageFile MainFile { get; set; }
    }

    public class StorageFileAggregateConfig : IEntityTypeConfiguration<StorageFileAggregate>
    {
        public void Configure(EntityTypeBuilder<StorageFileAggregate> builder)
        {
            builder.ToTable("storage_file_aggregates");

            // 设置自增主键
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            // ========== 关系1：一对多，聚合包含多个子文件 ==========
            builder.HasMany(a => a.Files)
                   .WithOne(f => f.FileAggregate)
                   .HasForeignKey(f => f.AggregateId)
                   .IsRequired(false) // 文件不属于聚合也能存在，外键可空
                   .OnDelete(DeleteBehavior.SetNull); // 删除聚合时，只清空文件的聚合关联，不删除文件本身

            // ========== 关系2：一对一，聚合引用一个主文件 ==========
            builder.HasOne(a => a.MainFile)
                   // 重点：WithOne() 不传参数，代表文件侧无反向导航
                   .WithOne()
                   .HasForeignKey<StorageFileAggregate>(a => a.MainFileId)
                   .IsRequired() // 主文件必须存在
                   .OnDelete(DeleteBehavior.Restrict); // 删除聚合时，禁止连带删除主文件（文件是独立资源）
            // 唯一约束：保证一一对应
            builder.HasIndex(x => x.MainFileId).IsUnique();
        }
    }
}
