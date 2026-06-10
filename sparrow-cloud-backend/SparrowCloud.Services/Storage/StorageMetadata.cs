using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Services.Storage
{
    /// <summary>
    /// 文件库的元数据
    /// </summary>
    public class StorageMetadata
    {
        /// <summary>
        /// 文件库依赖的云盘版本
        /// </summary>
        public required Version Version { get; set; }

        /// <summary>
        /// 文件库唯一标识
        /// </summary>
        public required string StorageGuid { get; set; }

        /// <summary>
        /// 文件库创建时间
        /// </summary>
        public required DateTime CreateAt { get; set; }

        /// <summary>
        /// 创建者标识（非正式，参考用）
        /// </summary>
        public required string CreatorGuid { get; set; }

        /// <summary>
        /// 创建者名字（非正式，参考用）
        /// </summary>
        public required string CreatorName { get; set; }
    }
}
