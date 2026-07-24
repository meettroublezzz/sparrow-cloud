using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Contracts.StorageModels
{
    public class StorageEditReq
    {
        /// <summary>
        /// 文件库名称
        /// </summary>
        [StringLength(255)]
        public required string Name { get; set; }

        /// <summary>
        /// 文件库描述（简短的）
        /// </summary>
        [StringLength(511)]
        public required string Describe { get; set; }
    }
}
