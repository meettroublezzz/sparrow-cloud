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
    /// 文件库所有文件
    /// </summary>
    public class StorageFile : EntityIncr<long>
    {
        #region 跟踪真实文件
        /// <summary>
        /// 完整路径（要求目录必须以 ‘/’ 结尾，文件不能以 ‘/’ 结尾）
        /// 示例：/新建文件夹/ 、/test.txt 、/新建文件夹/new.txt
        /// </summary>
        [MaxLength(4095)]
        public required string FullPath { get; set; }

        /// <summary>
        /// Path拼接所需相对完整的路径
        /// </summary>
        [NotMapped]
        public string RelativeFullPath { get => FullPath.TrimStart('/'); }

        /// <summary>
        /// 完整路径的 XXH64 哈希值
        /// </summary>
        public long FullPathHash { get; set; }

        /// <summary>
        /// 完整路径的 字符串长度
        /// </summary>
        public short FullPathLength { get; set; }

        /// <summary>
        /// 父目录路径的 XXH64 哈希值
        /// 根目录下的项：xxhash64("/")
        /// </summary>
        public long ParentPathHash { get; set; }

        /// <summary>
        /// 父目录路径的 字符串长度
        /// 根目录下的项：ParentPathLength == 1（因为"/"的长度是1）
        /// </summary>
        public short ParentPathLength { get; set; }
        #endregion

        /// <summary>
        /// 文件/文件夹 原始名称（带后缀）
        /// </summary>
        [MaxLength(255)]
        public required string Name { get; set; }

        /// <summary>
        /// 文件/文件夹 扩展名称（仅后缀，不含.）
        /// null = 无后缀
        /// </summary>
        [MaxLength(63)]
        public string? Extension { get; set; }

        /// <summary>
        /// 是否为文件夹
        /// true=文件夹，false=文件
        /// </summary>
        public bool IsDirectory { get; set; }

        #region 真实文件核心信息
        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileLength { get; set; }

        /// <summary>
        /// 文件SHA256哈希值（用于文件去重、完整性校验）
        /// 文件夹 or 懒计算 = null
        /// </summary>
        [MaxLength(64)]
        public string? FileShaHash { get; set; }

        /// <summary>
        /// 文件创建时间（UTC时间的Ticks）
        /// 注意：文件复制/移动时，不同文件系统行为不同
        /// - NTFS：复制时创建时间更新为当前时间，移动时保留原创建时间
        /// - FAT32/exFAT：复制和移动都会更新创建时间为当前时间
        /// </summary>
        public long CreationTimeTicks { get; set; }

        /// <summary>
        /// 非映射属性，用于得到文件 创建时间（仅显示用）
        /// </summary>
        [NotMapped]
        public DateTime CreationTime { get => new DateTime(CreationTimeTicks, DateTimeKind.Utc).ToLocalTime(); }

        /// <summary>
        /// 文件最后修改时间（UTC时间的Ticks）
        /// 这是判断文件是否被修改的**唯一可靠依据**
        /// 所有文件系统都会在文件内容修改时更新此时间
        /// PS：对于云盘如果写文件后，需要根据文件系统的最后修改时间，更新这个值，保持一致性
        /// </summary>
        public long LastWriteTimeTicks { get; set; }

        /// <summary>
        /// 非映射属性，用于得到文件 最后修改时间（仅显示用）
        /// </summary>
        [NotMapped]
        public DateTime LastWriteTime { get => new DateTime(LastWriteTimeTicks, DateTimeKind.Utc).ToLocalTime(); }

        /// <summary>
        /// 首次的值依赖文件系统，后续云盘自己维护
        /// PS：常用于浏览最近文件项
        /// </summary>
        public long LastAccessTimeTicks { get; set; }

        /// <summary>
        /// 非映射属性，用于得到文件 最后修改时间（仅显示用）
        /// </summary>
        [NotMapped]
        public DateTime LastAccessTime { get => new DateTime(LastAccessTimeTicks, DateTimeKind.Utc).ToLocalTime(); }
        #endregion

        /// <summary>
        /// 删除时间（回收站，软删除；清空回收站则真实清理这些文件）
        /// 原理：将要删除的 文件/目录 移动到数据仓库内
        /// </summary>
        public long? DeletedAt { get; set; }

        /// <summary>
        /// 此文件缺失标志位
        /// 正常 = null
        /// </summary>
        public long? Missing { get; set; }

        #region 导航属性
        /// <summary>
        /// 文件额外信息
        /// </summary>
        public virtual StorageFileInformation? Information { get; set; }

        /// <summary>
        /// 文件缩略图
        /// </summary>
        public virtual StorageFileThumbnail? Thumbnail { get; set; }
        #endregion
    }

    public class StorageFilesConfig : IEntityTypeConfiguration<StorageFile>
    {
        public void Configure(EntityTypeBuilder<StorageFile> builder)
        {
            builder.ToTable("storage_files"); 

            // 完整路径索引
            builder
                .HasIndex(e => e.FullPath)
                .IsUnique();

            // 父目录索引
            builder
                .HasIndex(e => new
                {
                    e.ParentPathHash,
                    e.ParentPathLength,
                });

            builder.HasIndex(e => e.Name);
            builder.HasIndex(e => e.Extension);
            builder.HasIndex(e => e.IsDirectory);

            builder.HasIndex(e => e.FileLength);
            builder.HasIndex(e => e.FileShaHash);
            builder.HasIndex(e => new {
                e.FileLength,
                e.FileShaHash,
            });

            builder.HasIndex(e => e.LastAccessTimeTicks);
            builder.HasIndex(e => e.DeletedAt);
            builder.HasIndex(e => e.Missing);
        }
    }
}
