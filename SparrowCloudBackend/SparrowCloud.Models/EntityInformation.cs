using MemoryPack;
using SparrowCloud.Models.Storage;
using SparrowCloud.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Models
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

        FileId = 5, // 指向文件库的某个文件
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
        /// 引用桶文件
        /// 无法用文本表达的信息（如种子文件等）
        /// 根据 ReferenceType 给具体解析器处理
        /// </summary>
        [MemoryPackOrder(4)]
        public string? DataInBucket { get; set; }
    }

    /// <summary>
    /// 通用信息字段结构（需要继承类处理自增主键问题）
    /// </summary>
    public class EntityInformation<TKeyType> : EntityBase<TKeyType>
    {
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
                // 维护复用
                references = value;

                // 空集合则直接回写为null
                if (references.Count == 0)
                {
                    ReferencesData = null;
                }
                else
                {
                    ReferencesData = CompressionHelper.CompressBytes(MemoryPackSerializer.Serialize(references));
                }
            }
        }

        /// <summary>
        /// 文件的参考信息（数据库存储字段，二进制数据）
        /// </summary>
        public byte[]? ReferencesData { get; set; }
        #endregion

        /// <summary>
        /// 收藏（喜爱）
        /// </summary>
        public long? FavoritedAt { get; set; }

        /// <summary>
        /// 星级（满十分）半星=1分
        /// </summary>
        public byte? StarLevel { get; set; }
    }
}
