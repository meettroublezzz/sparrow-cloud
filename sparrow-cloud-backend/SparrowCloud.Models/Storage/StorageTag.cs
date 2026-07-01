using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Models.Storage
{
    /// <summary>
    /// 标签表
    /// </summary>
    public class StorageTag : EntityInformation<int>
    {
        /// <summary>
        /// 标签名称
        /// </summary>
        [MaxLength(127)]
        public required string Name { get; set; }

        /// <summary>
        /// 标签颜色（十六进制，如 #ff0000）前端展示用
        /// </summary>
        [MaxLength(16)]
        public string? Color { get; set; }

        /// <summary>
        /// 是否为系统标签（用户不可删除，不可修改）
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// 使用次数（统计）
        /// </summary>
        public int UsageCount { get; set; }

        /// <summary>
        /// 最近使用时间（统计）
        /// </summary>
        public long UsageTicks { get; set; } = EntityBase.NowTicks;
        [NotMapped]
        public DateTime UsageTime { get => new DateTime(UsageTicks); }

        /// <summary>
        /// 父ID（内键）
        /// </summary>
        public int? TagParentId { get; set; }

        /// <summary>
        /// 关联的文件集合（导航属性）
        /// </summary>
        public List<StorageTagFile> FileTags { get; set; } = new();
    }

    internal class StorageTagConfig : IEntityTypeConfiguration<StorageTag>
    {
        public void Configure(EntityTypeBuilder<StorageTag> builder)
        {
            builder.ToTable("storage_tags");

            // 设置自增主键
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.HasIndex(x => x.TagParentId);

            builder.HasIndex(x => x.Color);
            builder.HasIndex(x => x.UsageTicks);
            builder.HasIndex(x => x.Name).IsUnique();

            builder.HasData(
                new StorageTag
                {
                    Id = 1,

                    Name = "默认",
                    IsSystem = true,
                }
             );
        }
    }
}
