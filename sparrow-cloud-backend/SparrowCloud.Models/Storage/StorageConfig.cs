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
    /// 文件库内部配置表
    /// </summary>
    public class StorageConfig : EntityInformation<short>
    {
        [StringLength(127)]
        public required string ConfigKey { get; set; }

        public string? ConfigValue { get; set; }
    }

    internal class StorageConfigConfig : IEntityTypeConfiguration<StorageConfig>
    {
        public void Configure(EntityTypeBuilder<StorageConfig> builder)
        {
            builder.ToTable("storage_configs");

            // 设置自增主键
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.HasIndex(x => x.ConfigKey).IsUnique();

            builder.HasData(
                new StorageConfig
                {
                    Id = 1,

                    ConfigKey = "storage_info",
                },

                new StorageConfig
                {
                    Id = 2,

                    ConfigKey = "storage_tags",
                    ConfigValue = "[]",
                }
             );
        }
    }
}
