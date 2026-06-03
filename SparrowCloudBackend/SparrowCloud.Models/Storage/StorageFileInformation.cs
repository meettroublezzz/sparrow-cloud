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
    /// 参考信息的类型
    /// </summary>
    public enum ReferenceType
    {
        Source = 0, // 源头（文件最初是怎么来的）
        Link = 1, // 链接（各种链接，相关参考文章等）
        Torrent = 2, // 种子文件（文件下载源）
        Magnet = 3, // 磁力链（文件下载源）
        Mmhtml = 4, // 单个离线网页文件（常用于使用教程）
    }

    /// <summary>
    /// 具体的参考信息
    /// </summary>
    [MemoryPackable]
    public partial struct ReferenceItem
    {
        /// <summary>
        /// 唯一ID，用于增删改
        /// </summary>
        [MemoryPackOrder(0)]
        public required string Id { get; set; }

        /// <summary>
        /// 参考类型
        /// </summary>
        [MemoryPackOrder(1)]
        public required ReferenceType Type { get; set; }

        /// <summary>
        /// 参考名称（自定义、网址标题等）
        /// </summary>
        [MemoryPackOrder(2)]
        public required string Title { get; set; }

        /// <summary>
        /// 参考文本信息（文字、链接、磁力链等）
        /// 普通文本，前端悬停时展示
        /// </summary>
        [MemoryPackOrder(3)]
        public string? Text { get; set; }

        /// <summary>
        /// 无法用文本表达的信息（如种子文件等）
        /// 根据 ReferenceType 给具体解析器处理
        /// </summary>
        [MemoryPackOrder(4)]
        public byte[]? Coverall { get; set; }
    }

    /// <summary>
	/// 文件额外信息（偏文本数据）
	/// 依赖表：storage_files
	/// 一对一关系：(one)额外信息 -> one 文件
	/// </summary>
    public class StorageFileInformation : EntityIncr<long>
    {
        /// <summary>
        /// 文件标题（如果有，则显示优先级高于文件名）
        /// </summary>
        [MaxLength(255)]
        public string? Title { get; set; }

        /// <summary>
        /// 备注（可用于文件描述）
        /// 支持md格式（默认‘普通文本’，在字符串开头放置 <!-- this is markdown --> 注释表达是‘Markdown’内容）
        /// </summary>
        [MaxLength(65535)]
        public string? Remark { get; set; }

        #region 参考引用相关处理
        /// <summary>
        /// 当前实例复用同一份数据
        /// </summary>
        [NotMapped]
        private List<ReferenceItem>? references = null;

        /// <summary>
        /// 文件的参考信息（修改集合后，一定要 set 回来）
        /// </summary>
        [NotMapped]
        public List<ReferenceItem> References 
        { 
            get
            {
                if (references == null)
                    references = ReferencesData == null ? [] : MemoryPackSerializer.Deserialize<List<ReferenceItem>>(CompressionHelper.DecompressBytes(ReferencesData));

                return references!;
            } 

            set
            {
                references = value;

                // 空集合则直接回写为null
                if (value.Count == 0)
                {
                    ReferencesData = null;
                }
                else
                {
                    ReferencesData = CompressionHelper.CompressBytes(MemoryPackSerializer.Serialize(value));
                }
            }
        }
        
        /// <summary>
        /// 文件的参考信息（数据库存储字段，二进制数据）
        /// </summary>
        public byte[]? ReferencesData { get; set; }
        #endregion

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
