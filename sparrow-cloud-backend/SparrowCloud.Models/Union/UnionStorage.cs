using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Models.Union
{
    public class UnionStorage : EntityIncr<int>
    {
        /*
         * 一对多关系
         * 
         * 一个用户：多个文件库
         */

        /// <summary>
        /// 文件库名称（任意修改）
        /// </summary>
        [StringLength(255)]
        public required string Name { get; set; }

        /// <summary>
        /// 用户表（外键）
        /// </summary>
        [StringLength(36)]
        public required string UserId { get; set; }

        /// <summary>
        /// 文件库（外键）
        /// </summary>
        [StringLength(36)]
        public required string StorageId { get; set; }

        /// <summary>
        /// 实际路径
        /// </summary>
        [StringLength(4095)]
        public required string RootPath { get; set; }

        /// <summary>
        /// 次序（允许用户手动排序）
        /// </summary>
        public required double Sequence { get; set; }
        
        /// <summary>
        /// 文件库缺失标志位
        /// 正常 = null
        /// </summary>
        public DateTime? Missing { get; set; }

        /// <summary>
        /// 上次扫描时间
        /// 未扫描 = null
        /// </summary>
        public DateTime? LastScan { get; set; }

        /// <summary>
        /// 文件库损坏（理由）
        /// 正常 = null
        /// </summary>
        [StringLength(65535)]
        public string? Damaged { get; set; }

        /// <summary>
        /// 是否被删除（软删除）
        /// 未删除 = null
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// 最后访问时间
        /// </summary>
        public DateTime? LastAccessAt { get; set; }

        /// <summary>
        /// 文件库描述（简短的）
        /// </summary>
        [StringLength(511)]
        public required string Describe { get; set; }
    }

    internal class UnionStorageConfig : IEntityTypeConfiguration<UnionStorage>
    {
        public void Configure(EntityTypeBuilder<UnionStorage> builder)
        {
            builder.ToTable("union_storages");

            builder.HasIndex(e => new
            {
                e.UserId,
                e.StorageId,
            }).IsUnique();

            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.StorageId);
            builder.HasIndex(e => e.Sequence);

            builder.HasIndex(e => e.DeletedAt);
        }
    }
}
