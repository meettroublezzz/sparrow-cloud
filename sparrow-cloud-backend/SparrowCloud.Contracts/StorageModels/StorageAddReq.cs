using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Contracts.StorageModels
{
    public class StorageAddReq
    {
        /// <summary>
        /// 文件库名称（创建时将可能作为目录名称）
        /// </summary>
        [StringLength(255)]
        public required string Name { get; set; }

        /// <summary>
        /// 将创建目录的名称
        /// </summary>
        [StringLength(127)]
        public string? DirName { get; set; }

        /// <summary>
        /// 文件库完整路径
        /// </summary>
        public required string FullPath { get; set; }

        /// <summary>
        /// 文件库描述（简短的）
        /// </summary>
        [StringLength(511)]
        public required string Describe { get; set; }
    }

    public enum StorageAddType
    {
        /// <summary>
        /// 创建文件库（不存在目录或空目录，将创建并初始化它）
        /// </summary>
        Create = 1,

        /// <summary>
        /// 附加文件库（已存在的目录）
        /// </summary>
        Attach = 2,
    }
}
