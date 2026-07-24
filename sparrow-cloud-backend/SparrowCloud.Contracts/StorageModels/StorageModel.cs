using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Contracts.StorageModels
{
    public class StorageModel : StorageAddReq
    {
        /// <summary>
        /// 文件库ID
        /// </summary>
        public required string StorageId { get; set; }

        /// <summary>
        /// 次序（允许用户手动排序）
        /// </summary>
        public required double Sequence { get; set; }

        /// <summary>
        /// 文件库是否就绪
        /// </summary>
        public bool Ready { get; set; }

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
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

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
        /// 文件库封面
        /// </summary>
        public required string CoverUrl { get; set; }

        /// <summary>
        /// 文件库大小
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 文件库最早创建时间（元数据内的记录）
        /// </summary>
        public DateTime Birthday { get; set; }
    }
}
